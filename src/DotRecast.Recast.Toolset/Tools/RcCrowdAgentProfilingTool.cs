using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Core.Collections;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using DotRecast.Recast.Toolset.Builder;

namespace DotRecast.Recast.Toolset.Tools
{
    public class RcCrowdAgentProfilingTool : IRcToolable
    {
        private RcCrowdAgentProfilingToolConfig _cfg;

        private DtCrowdConfig _crowdCfg;
        private DtCrowd crowd;
        private readonly DtCrowdAgentConfig _agCfg;

        private DtNavMesh navMesh;

        private RcRand rnd;
        private readonly List<DtPolyPoint> _polyPoints;
        private long crowdUpdateTime;

        public RcCrowdAgentProfilingTool()
        {
            _cfg = new RcCrowdAgentProfilingToolConfig();
            _agCfg = new DtCrowdAgentConfig();
            _polyPoints = new List<DtPolyPoint>();
        }

        public string GetName()
        {
            return "Crowd Agent Profiling";
        }

        public RcCrowdAgentProfilingToolConfig GetToolConfig()
        {
            return _cfg;
        }

        public DtCrowdAgentConfig GetCrowdConfig()
        {
            return _agCfg;
        }

        public DtCrowd GetCrowd()
        {
            return crowd;
        }

        public void Setup(float maxAgentRadius, DtNavMesh nav)
        {
            navMesh = nav;
            if (nav != null)
            {
                _crowdCfg = new DtCrowdConfig(maxAgentRadius);
            }
        }

        private DtCrowdAgentParams GetAgentParams(float agentRadius, float agentHeight, float agentMaxAcceleration, float agentMaxSpeed)
        {
            DtCrowdAgentParams ap = new DtCrowdAgentParams();
            ap.radius = agentRadius;
            ap.height = agentHeight;
            ap.maxAcceleration = agentMaxAcceleration;
            ap.maxSpeed = agentMaxSpeed;
            ap.collisionQueryRange = ap.radius * 12.0f;
            ap.pathOptimizationRange = ap.radius * 30.0f;
            ap.updateFlags = _agCfg.GetUpdateFlags();
            ap.obstacleAvoidanceType = _agCfg.obstacleAvoidanceType;
            ap.separationWeight = _agCfg.separationWeight;
            return ap;
        }

        private DtStatus GetMobPosition(DtNavMeshQuery navquery, IDtQueryFilter filter, out RcVec3f randomPt)
        {
            return navquery.FindRandomPoint(filter, rnd, out var randomRef, out randomPt);
        }

        private DtStatus GetVillagerPosition(DtNavMeshQuery navquery, IDtQueryFilter filter, out RcVec3f randomPt)
        {
            randomPt = RcVec3f.Zero;

            if (0 >= _polyPoints.Count)
                return DtStatus.DT_FAILURE;

            int zone = (int)(rnd.Next() * _polyPoints.Count);
            return navquery.FindRandomPointWithinCircle(_polyPoints[zone].refs, _polyPoints[zone].pt, _cfg.zoneRadius, filter, rnd,
                out var randomRef, out randomPt);
        }

        private void CreateZones()
        {
            _polyPoints.Clear();
            IDtQueryFilter filter = new DtQueryDefaultFilter();
            DtNavMeshQuery navquery = new DtNavMeshQuery(navMesh);
            for (int i = 0; i < _cfg.numberOfZones; i++)
            {
                float zoneSeparation = _cfg.zoneRadius * _cfg.zoneRadius * 16;
                for (int k = 0; k < 100; k++)
                {
                    var status = navquery.FindRandomPoint(filter, rnd, out var randomRef, out var randomPt);
                    if (status.Succeeded())
                    {
                        bool valid = true;
                        foreach (var zone in _polyPoints)
                        {
                            if (RcVec3f.DistSqr(zone.pt, randomPt) < zoneSeparation)
                            {
                                valid = false;
                                break;
                            }
                        }

                        if (valid)
                        {
                            _polyPoints.Add(new DtPolyPoint(randomRef, randomPt));
                            break;
                        }
                    }
                }
            }
        }

        private void CreateCrowd()
        {
            crowd = new DtCrowd(_crowdCfg, navMesh, __ => new DtQueryDefaultFilter(
                SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
                SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED,
                new float[] { 1f, 10f, 1f, 1f, 2f, 1.5f })
            );

            DtObstacleAvoidanceParams option = new DtObstacleAvoidanceParams(crowd.GetObstacleAvoidanceParams(0));
            // Low (11)
            option.velBias = 0.5f;
            option.adaptiveDivs = 5;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 1;
            crowd.SetObstacleAvoidanceParams(0, option);
            // Medium (22)
            option.velBias = 0.5f;
            option.adaptiveDivs = 5;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 2;
            crowd.SetObstacleAvoidanceParams(1, option);
            // Good (45)
            option.velBias = 0.5f;
            option.adaptiveDivs = 7;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 3;
            crowd.SetObstacleAvoidanceParams(2, option);
            // High (66)
            option.velBias = 0.5f;
            option.adaptiveDivs = 7;
            option.adaptiveRings = 3;
            option.adaptiveDepth = 3;
            crowd.SetObstacleAvoidanceParams(3, option);
        }

        public void StartProfiling(float agentRadius, float agentHeight, float agentMaxAcceleration, float agentMaxSpeed)
        {
            if (null == navMesh)
                return;

            rnd = new RcRand(_cfg.randomSeed);
            CreateCrowd();
            CreateZones();
            DtNavMeshQuery navquery = new DtNavMeshQuery(navMesh);
            IDtQueryFilter filter = new DtQueryDefaultFilter();
            for (int i = 0; i < _cfg.agents; i++)
            {
                float tr = rnd.Next();
                RcCrowdAgentType type = RcCrowdAgentType.MOB;
                float mobsPcnt = _cfg.percentMobs / 100f;
                if (tr > mobsPcnt)
                {
                    tr = rnd.Next();
                    float travellerPcnt = _cfg.percentTravellers / 100f;
                    if (tr > travellerPcnt)
                    {
                        type = RcCrowdAgentType.VILLAGER;
                    }
                    else
                    {
                        type = RcCrowdAgentType.TRAVELLER;
                    }
                }

                var status = DtStatus.DT_FAILURE;
                var randomPt = RcVec3f.Zero;
                switch (type)
                {
                    case RcCrowdAgentType.MOB:
                        status = GetMobPosition(navquery, filter, out randomPt);
                        break;
                    case RcCrowdAgentType.VILLAGER:
                        status = GetVillagerPosition(navquery, filter, out randomPt);
                        break;
                    case RcCrowdAgentType.TRAVELLER:
                        status = GetVillagerPosition(navquery, filter, out randomPt);
                        break;
                }

                if (status.Succeeded())
                {
                    AddAgent(randomPt, type, agentRadius, agentHeight, agentMaxAcceleration, agentMaxSpeed);
                }
            }
        }

        public void Update(float dt)
        {
            long startTime = RcFrequency.Ticks;
            if (crowd != null)
            {
                crowd.Config().pathQueueSize = _cfg.pathQueueSize;
                crowd.Config().maxFindPathIterations = _cfg.maxIterations;
                crowd.Update(dt, null);
            }

            long endTime = RcFrequency.Ticks;
            if (crowd != null)
            {
                DtNavMeshQuery navquery = new DtNavMeshQuery(navMesh);
                IDtQueryFilter filter = new DtQueryDefaultFilter();
                foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
                {
                    if (NeedsNewTarget(ag))
                    {
                        RcCrowdAgentData crowAgentData = (RcCrowdAgentData)ag.option.userData;
                        switch (crowAgentData.type)
                        {
                            case RcCrowdAgentType.MOB:
                                MoveMob(navquery, filter, ag, crowAgentData);
                                break;
                            case RcCrowdAgentType.VILLAGER:
                                MoveVillager(navquery, filter, ag, crowAgentData);
                                break;
                            case RcCrowdAgentType.TRAVELLER:
                                MoveTraveller(navquery, filter, ag, crowAgentData);
                                break;
                        }
                    }
                }
            }

            crowdUpdateTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
        }

        private void MoveMob(DtNavMeshQuery navquery, IDtQueryFilter filter, DtCrowdAgent ag, RcCrowdAgentData crowAgentData)
        {
            // Move somewhere
            var status = navquery.FindNearestPoly(ag.npos, crowd.GetQueryExtents(), filter, out var nearestRef, out var nearestPt, out var _);
            if (status.Succeeded())
            {
                status = navquery.FindRandomPointAroundCircle(nearestRef, crowAgentData.home, _cfg.zoneRadius * 2f, filter, rnd,
                    out var randomRef, out var randomPt);
                if (status.Succeeded())
                {
                    crowd.RequestMoveTarget(ag, randomRef, randomPt);
                }
            }
        }

        private void MoveVillager(DtNavMeshQuery navquery, IDtQueryFilter filter, DtCrowdAgent ag, RcCrowdAgentData crowAgentData)
        {
            // Move somewhere close
            var status = navquery.FindNearestPoly(ag.npos, crowd.GetQueryExtents(), filter, out var nearestRef, out var nearestPt, out var _);
            if (status.Succeeded())
            {
                status = navquery.FindRandomPointAroundCircle(nearestRef, crowAgentData.home, _cfg.zoneRadius * 0.2f, filter, rnd,
                    out var randomRef, out var randomPt);
                if (status.Succeeded())
                {
                    crowd.RequestMoveTarget(ag, randomRef, randomPt);
                }
            }
        }

        private void MoveTraveller(DtNavMeshQuery navquery, IDtQueryFilter filter, DtCrowdAgent ag, RcCrowdAgentData crowAgentData)
        {
            // Move to another zone
            List<DtPolyPoint> potentialTargets = new List<DtPolyPoint>();
            foreach (var zone in _polyPoints)
            {
                if (RcVec3f.DistSqr(zone.pt, ag.npos) > _cfg.zoneRadius * _cfg.zoneRadius)
                {
                    potentialTargets.Add(zone);
                }
            }

            if (0 < potentialTargets.Count)
            {
                potentialTargets.Shuffle();
                crowd.RequestMoveTarget(ag, potentialTargets[0].refs, potentialTargets[0].pt);
            }
        }

        private bool NeedsNewTarget(DtCrowdAgent ag)
        {
            if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
            {
                return true;
            }

            if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VALID)
            {
                float dx = ag.targetPos.X - ag.npos.X;
                float dy = ag.targetPos.Y - ag.npos.Y;
                float dz = ag.targetPos.Z - ag.npos.Z;
                return dx * dx + dy * dy + dz * dz < 0.3f;
            }

            return false;
        }

        private DtCrowdAgent AddAgent(RcVec3f p, RcCrowdAgentType type, float agentRadius, float agentHeight, float agentMaxAcceleration, float agentMaxSpeed)
        {
            DtCrowdAgentParams ap = GetAgentParams(agentRadius, agentHeight, agentMaxAcceleration, agentMaxSpeed);
            ap.userData = new RcCrowdAgentData(type, p);
            return crowd.AddAgent(p, ap);
        }

        public void UpdateAgentParams()
        {
            if (crowd != null)
            {
                foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
                {
                    DtCrowdAgentParams option = new DtCrowdAgentParams();
                    option.radius = ag.option.radius;
                    option.height = ag.option.height;
                    option.maxAcceleration = ag.option.maxAcceleration;
                    option.maxSpeed = ag.option.maxSpeed;
                    option.collisionQueryRange = ag.option.collisionQueryRange;
                    option.pathOptimizationRange = ag.option.pathOptimizationRange;
                    option.queryFilterType = ag.option.queryFilterType;
                    option.userData = ag.option.userData;
                    option.updateFlags = _agCfg.GetUpdateFlags();
                    option.obstacleAvoidanceType = _agCfg.obstacleAvoidanceType;
                    option.separationWeight = _agCfg.separationWeight;
                    crowd.UpdateAgentParameters(ag, option);
                }
            }
        }

        public long GetCrowdUpdateTime()
        {
            return crowdUpdateTime;
        }
    }
}