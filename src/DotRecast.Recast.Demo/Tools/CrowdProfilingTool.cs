/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using DotRecast.Detour.QueryResults;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using ImGuiNET;
using Silk.NET.Windowing;
using static DotRecast.Recast.Demo.Draw.DebugDraw;

namespace DotRecast.Recast.Demo.Tools;

public class CrowdProfilingTool
{
    private readonly Func<CrowdAgentParams> agentParamsSupplier;
    private int expandSimOptions = 1;
    private int expandCrowdOptions = 1;
    private int agents = 1000;
    private int randomSeed = 270;
    private int numberOfZones = 4;
    private float zoneRadius = 20f;
    private float percentMobs = 80f;
    private float percentTravellers = 15f;
    private int pathQueueSize = 32;
    private int maxIterations = 300;
    private Crowd crowd;
    private NavMesh navMesh;
    private CrowdConfig config;
    private FRand rnd;
    private readonly List<FindRandomPointResult> zones = new();
    private long crowdUpdateTime;

    public CrowdProfilingTool(Func<CrowdAgentParams> agentParamsSupplier)
    {
        this.agentParamsSupplier = agentParamsSupplier;
    }

    public void Layout()
    {
        ImGui.Text("Simulation Options");
        ImGui.Separator();
        ImGui.SliderInt("Agents", ref agents, 0, 10000);
        ImGui.SliderInt("Random Seed", ref randomSeed, 0, 1024);
        ImGui.SliderInt("Number of Zones", ref numberOfZones, 0, 10);
        ImGui.SliderFloat("Zone Radius", ref zoneRadius, 0, 100, "%.0f");
        ImGui.SliderFloat("Mobs %", ref percentMobs, 0, 100, "%.0f");
        ImGui.SliderFloat("Travellers %", ref percentTravellers, 0, 100, "%.0f");
        ImGui.NewLine();

        ImGui.Text("Crowd Options");
        ImGui.Separator();
        ImGui.SliderInt("Path Queue Size", ref pathQueueSize, 0, 1024);
        ImGui.SliderInt("Max Iterations", ref maxIterations, 0, 4000);
        ImGui.NewLine();

        if (ImGui.Button("Start"))
        {
            if (navMesh != null)
            {
                rnd = new FRand(randomSeed);
                CreateCrowd();
                CreateZones();
                NavMeshQuery navquery = new NavMeshQuery(navMesh);
                QueryFilter filter = new DefaultQueryFilter();
                for (int i = 0; i < agents; i++)
                {
                    float tr = rnd.Frand();
                    AgentType type = AgentType.MOB;
                    float mobsPcnt = percentMobs / 100f;
                    if (tr > mobsPcnt)
                    {
                        tr = rnd.Frand();
                        float travellerPcnt = percentTravellers / 100f;
                        if (tr > travellerPcnt)
                        {
                            type = AgentType.VILLAGER;
                        }
                        else
                        {
                            type = AgentType.TRAVELLER;
                        }
                    }

                    Vector3f? pos = null;
                    switch (type)
                    {
                        case AgentType.MOB:
                            pos = GetMobPosition(navquery, filter);
                            break;
                        case AgentType.VILLAGER:
                            pos = GetVillagerPosition(navquery, filter);
                            break;
                        case AgentType.TRAVELLER:
                            pos = GetVillagerPosition(navquery, filter);
                            break;
                    }

                    if (pos != null)
                    {
                        AddAgent(pos.Value, type);
                    }
                }
            }
        }

        ImGui.Text("Times");
        ImGui.Separator();
        if (crowd != null)
        {
            ImGui.Text($"Max time to enqueue request: {crowd.Telemetry().MaxTimeToEnqueueRequest()} s");
            ImGui.Text($"Max time to find path: {crowd.Telemetry().MaxTimeToFindPath()} s");
            List<Tuple<string, long>> timings = crowd.Telemetry()
                .ExecutionTimings()
                .Select(e => Tuple.Create(e.Key, e.Value))
                .OrderBy(x => x.Item2)
                .ToList();

            foreach (Tuple<string, long> e in timings)
            {
                ImGui.Text($"{e.Item1}: {e.Item2 / 1_000} us");
            }

            ImGui.Text($"Update Time: {crowdUpdateTime} ms");
        }
    }

    private Vector3f? GetMobPosition(NavMeshQuery navquery, QueryFilter filter)
    {
        Result<FindRandomPointResult> result = navquery.FindRandomPoint(filter, rnd);
        if (result.Succeeded())
        {
            return result.result.GetRandomPt();
        }

        return null;
    }

    private Vector3f? GetVillagerPosition(NavMeshQuery navquery, QueryFilter filter)
    {
        if (0 < zones.Count)
        {
            int zone = (int)(rnd.Frand() * zones.Count);
            Result<FindRandomPointResult> result = navquery.FindRandomPointWithinCircle(zones[zone].GetRandomRef(),
                zones[zone].GetRandomPt(), zoneRadius, filter, rnd);
            if (result.Succeeded())
            {
                return result.result.GetRandomPt();
            }
        }

        return null;
    }

    private void CreateZones()
    {
        zones.Clear();
        QueryFilter filter = new DefaultQueryFilter();
        NavMeshQuery navquery = new NavMeshQuery(navMesh);
        for (int i = 0; i < numberOfZones; i++)
        {
            float zoneSeparation = zoneRadius * zoneRadius * 16;
            for (int k = 0; k < 100; k++)
            {
                Result<FindRandomPointResult> result = navquery.FindRandomPoint(filter, rnd);
                if (result.Succeeded())
                {
                    bool valid = true;
                    foreach (FindRandomPointResult zone in zones)
                    {
                        if (RecastMath.VDistSqr(zone.GetRandomPt(), result.result.GetRandomPt()) < zoneSeparation)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        zones.Add(result.result);
                        break;
                    }
                }
            }
        }
    }

    private void CreateCrowd()
    {
        crowd = new Crowd(config, navMesh, __ => new DefaultQueryFilter(SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
            SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED, new float[] { 1f, 10f, 1f, 1f, 2f, 1.5f }));

        ObstacleAvoidanceParams option = new ObstacleAvoidanceParams(crowd.GetObstacleAvoidanceParams(0));
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

    public void Update(float dt)
    {
        long startTime = FrequencyWatch.Ticks;
        if (crowd != null)
        {
            crowd.Config().pathQueueSize = pathQueueSize;
            crowd.Config().maxFindPathIterations = maxIterations;
            crowd.Update(dt, null);
        }

        long endTime = FrequencyWatch.Ticks;
        if (crowd != null)
        {
            NavMeshQuery navquery = new NavMeshQuery(navMesh);
            QueryFilter filter = new DefaultQueryFilter();
            foreach (CrowdAgent ag in crowd.GetActiveAgents())
            {
                if (NeedsNewTarget(ag))
                {
                    AgentData agentData = (AgentData)ag.option.userData;
                    switch (agentData.type)
                    {
                        case AgentType.MOB:
                            MoveMob(navquery, filter, ag, agentData);
                            break;
                        case AgentType.VILLAGER:
                            MoveVillager(navquery, filter, ag, agentData);
                            break;
                        case AgentType.TRAVELLER:
                            MoveTraveller(navquery, filter, ag, agentData);
                            break;
                    }
                }
            }
        }

        crowdUpdateTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
    }

    private void MoveMob(NavMeshQuery navquery, QueryFilter filter, CrowdAgent ag, AgentData agentData)
    {
        // Move somewhere
        Result<FindNearestPolyResult> nearestPoly = navquery.FindNearestPoly(ag.npos, crowd.GetQueryExtents(), filter);
        if (nearestPoly.Succeeded())
        {
            Result<FindRandomPointResult> result = navquery.FindRandomPointAroundCircle(nearestPoly.result.GetNearestRef(),
                agentData.home, zoneRadius * 2f, filter, rnd);
            if (result.Succeeded())
            {
                crowd.RequestMoveTarget(ag, result.result.GetRandomRef(), result.result.GetRandomPt());
            }
        }
    }

    private void MoveVillager(NavMeshQuery navquery, QueryFilter filter, CrowdAgent ag, AgentData agentData)
    {
        // Move somewhere close
        Result<FindNearestPolyResult> nearestPoly = navquery.FindNearestPoly(ag.npos, crowd.GetQueryExtents(), filter);
        if (nearestPoly.Succeeded())
        {
            Result<FindRandomPointResult> result = navquery.FindRandomPointAroundCircle(nearestPoly.result.GetNearestRef(),
                agentData.home, zoneRadius * 0.2f, filter, rnd);
            if (result.Succeeded())
            {
                crowd.RequestMoveTarget(ag, result.result.GetRandomRef(), result.result.GetRandomPt());
            }
        }
    }

    private void MoveTraveller(NavMeshQuery navquery, QueryFilter filter, CrowdAgent ag, AgentData agentData)
    {
        // Move to another zone
        List<FindRandomPointResult> potentialTargets = new();
        foreach (FindRandomPointResult zone in zones)
        {
            if (RecastMath.VDistSqr(zone.GetRandomPt(), ag.npos) > zoneRadius * zoneRadius)
            {
                potentialTargets.Add(zone);
            }
        }

        if (0 < potentialTargets.Count)
        {
            potentialTargets.Shuffle();
            crowd.RequestMoveTarget(ag, potentialTargets[0].GetRandomRef(), potentialTargets[0].GetRandomPt());
        }
    }

    private bool NeedsNewTarget(CrowdAgent ag)
    {
        if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
            || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
        {
            return true;
        }

        if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VALID)
        {
            float dx = ag.targetPos.x - ag.npos.x;
            float dy = ag.targetPos.y - ag.npos.y;
            float dz = ag.targetPos.z - ag.npos.z;
            return dx * dx + dy * dy + dz * dz < 0.3f;
        }

        return false;
    }

    public void Setup(float maxAgentRadius, NavMesh nav)
    {
        navMesh = nav;
        if (nav != null)
        {
            config = new CrowdConfig(maxAgentRadius);
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();
        dd.DepthMask(false);
        if (crowd != null)
        {
            foreach (CrowdAgent ag in crowd.GetActiveAgents())
            {
                float radius = ag.option.radius;
                Vector3f pos = ag.npos;
                dd.DebugDrawCircle(pos.x, pos.y, pos.z, radius, DuRGBA(0, 0, 0, 32), 2.0f);
            }

            foreach (CrowdAgent ag in crowd.GetActiveAgents())
            {
                AgentData agentData = (AgentData)ag.option.userData;

                float height = ag.option.height;
                float radius = ag.option.radius;
                Vector3f pos = ag.npos;

                int col = DuRGBA(220, 220, 220, 128);
                if (agentData.type == AgentType.TRAVELLER)
                {
                    col = DuRGBA(100, 160, 100, 128);
                }

                if (agentData.type == AgentType.VILLAGER)
                {
                    col = DuRGBA(120, 80, 160, 128);
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                    col = DuLerpCol(col, DuRGBA(255, 255, 32, 128), 128);
                else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                    col = DuLerpCol(col, DuRGBA(255, 64, 32, 128), 128);
                else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                    col = DuRGBA(255, 32, 16, 128);
                else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                    col = DuLerpCol(col, DuRGBA(64, 255, 0, 128), 128);

                dd.DebugDrawCylinder(pos.x - radius, pos.y + radius * 0.1f, pos.z - radius, pos.x + radius, pos.y + height,
                    pos.z + radius, col);
            }
        }

        dd.DepthMask(true);
    }

    private CrowdAgent AddAgent(Vector3f p, AgentType type)
    {
        CrowdAgentParams ap = agentParamsSupplier.Invoke();
        ap.userData = new AgentData(type, p);
        return crowd.AddAgent(p, ap);
    }

    public enum AgentType
    {
        VILLAGER,
        TRAVELLER,
        MOB,
    }

    private class AgentData
    {
        public readonly AgentType type;
        public readonly Vector3f home = new Vector3f();

        public AgentData(AgentType type, Vector3f home)
        {
            this.type = type;
            this.home = home;
        }
    }

    public void UpdateAgentParams(int updateFlags, int obstacleAvoidanceType, float separationWeight)
    {
        if (crowd != null)
        {
            foreach (CrowdAgent ag in crowd.GetActiveAgents())
            {
                CrowdAgentParams option = new CrowdAgentParams();
                option.radius = ag.option.radius;
                option.height = ag.option.height;
                option.maxAcceleration = ag.option.maxAcceleration;
                option.maxSpeed = ag.option.maxSpeed;
                option.collisionQueryRange = ag.option.collisionQueryRange;
                option.pathOptimizationRange = ag.option.pathOptimizationRange;
                option.queryFilterType = ag.option.queryFilterType;
                option.userData = ag.option.userData;
                option.updateFlags = updateFlags;
                option.obstacleAvoidanceType = obstacleAvoidanceType;
                option.separationWeight = separationWeight;
                crowd.UpdateAgentParameters(ag, option);
            }
        }
    }
}
