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
using System.Linq;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using DotRecast.Recast.DemoTool.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.DemoTool;
using DotRecast.Recast.DemoTool.Tools;
using ImGuiNET;
using static DotRecast.Recast.Demo.Draw.DebugDraw;

namespace DotRecast.Recast.Demo.Tools;

public class CrowdProfilingTool
{
    private readonly Func<DtCrowdAgentParams> agentParamsSupplier;
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
    private DtCrowd crowd;
    private DtNavMesh navMesh;
    private DtCrowdConfig config;
    private FRand rnd;
    private readonly List<DtPolyPoint> _polyPoints = new();
    private long crowdUpdateTime;

    public CrowdProfilingTool(Func<DtCrowdAgentParams> agentParamsSupplier)
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

        if (ImGui.Button("Start Crowd Profiling"))
        {
            if (navMesh != null)
            {
                rnd = new FRand(randomSeed);
                CreateCrowd();
                CreateZones();
                DtNavMeshQuery navquery = new DtNavMeshQuery(navMesh);
                IDtQueryFilter filter = new DtQueryDefaultFilter();
                for (int i = 0; i < agents; i++)
                {
                    float tr = rnd.Next();
                    CrowdAgentType type = CrowdAgentType.MOB;
                    float mobsPcnt = percentMobs / 100f;
                    if (tr > mobsPcnt)
                    {
                        tr = rnd.Next();
                        float travellerPcnt = percentTravellers / 100f;
                        if (tr > travellerPcnt)
                        {
                            type = CrowdAgentType.VILLAGER;
                        }
                        else
                        {
                            type = CrowdAgentType.TRAVELLER;
                        }
                    }

                    RcVec3f? pos = null;
                    switch (type)
                    {
                        case CrowdAgentType.MOB:
                            pos = GetMobPosition(navquery, filter);
                            break;
                        case CrowdAgentType.VILLAGER:
                            pos = GetVillagerPosition(navquery, filter);
                            break;
                        case CrowdAgentType.TRAVELLER:
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
            var timings = crowd.Telemetry()
                .ToExecutionTimings();

            foreach (var rtt in timings)
            {
                ImGui.Text($"{rtt.Key}: {rtt.Micros} us");
            }

            ImGui.Text($"Update Time: {crowdUpdateTime} ms");
        }
    }

    private RcVec3f? GetMobPosition(DtNavMeshQuery navquery, IDtQueryFilter filter)
    {
        var status = navquery.FindRandomPoint(filter, rnd, out var randomRef, out var randomPt);
        if (status.Succeeded())
        {
            return randomPt;
        }

        return null;
    }

    private RcVec3f? GetVillagerPosition(DtNavMeshQuery navquery, IDtQueryFilter filter)
    {
        if (0 < _polyPoints.Count)
        {
            int zone = (int)(rnd.Next() * _polyPoints.Count);
            var status = navquery.FindRandomPointWithinCircle(_polyPoints[zone].refs, _polyPoints[zone].pt, zoneRadius, filter, rnd,
                out var randomRef, out var randomPt);
            if (status.Succeeded())
            {
                return randomPt;
            }
        }

        return null;
    }

    private void CreateZones()
    {
        _polyPoints.Clear();
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        DtNavMeshQuery navquery = new DtNavMeshQuery(navMesh);
        for (int i = 0; i < numberOfZones; i++)
        {
            float zoneSeparation = zoneRadius * zoneRadius * 16;
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
        crowd = new DtCrowd(config, navMesh, __ => new DtQueryDefaultFilter(SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
            SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED, new float[] { 1f, 10f, 1f, 1f, 2f, 1.5f }));

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

    public void Update(float dt)
    {
        long startTime = RcFrequency.Ticks;
        if (crowd != null)
        {
            crowd.Config().pathQueueSize = pathQueueSize;
            crowd.Config().maxFindPathIterations = maxIterations;
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
                    CrowdAgentData crowAgentData = (CrowdAgentData)ag.option.userData;
                    switch (crowAgentData.type)
                    {
                        case CrowdAgentType.MOB:
                            MoveMob(navquery, filter, ag, crowAgentData);
                            break;
                        case CrowdAgentType.VILLAGER:
                            MoveVillager(navquery, filter, ag, crowAgentData);
                            break;
                        case CrowdAgentType.TRAVELLER:
                            MoveTraveller(navquery, filter, ag, crowAgentData);
                            break;
                    }
                }
            }
        }

        crowdUpdateTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
    }

    private void MoveMob(DtNavMeshQuery navquery, IDtQueryFilter filter, DtCrowdAgent ag, CrowdAgentData crowAgentData)
    {
        // Move somewhere
        var status = navquery.FindNearestPoly(ag.npos, crowd.GetQueryExtents(), filter, out var nearestRef, out var nearestPt, out var _);
        if (status.Succeeded())
        {
            status = navquery.FindRandomPointAroundCircle(nearestRef, crowAgentData.home, zoneRadius * 2f, filter, rnd,
                out var randomRef, out var randomPt);
            if (status.Succeeded())
            {
                crowd.RequestMoveTarget(ag, randomRef, randomPt);
            }
        }
    }

    private void MoveVillager(DtNavMeshQuery navquery, IDtQueryFilter filter, DtCrowdAgent ag, CrowdAgentData crowAgentData)
    {
        // Move somewhere close
        var status = navquery.FindNearestPoly(ag.npos, crowd.GetQueryExtents(), filter, out var nearestRef, out var nearestPt, out var _);
        if (status.Succeeded())
        {
            status = navquery.FindRandomPointAroundCircle(nearestRef, crowAgentData.home, zoneRadius * 0.2f, filter, rnd,
                out var randomRef, out var randomPt);
            if (status.Succeeded())
            {
                crowd.RequestMoveTarget(ag, randomRef, randomPt);
            }
        }
    }

    private void MoveTraveller(DtNavMeshQuery navquery, IDtQueryFilter filter, DtCrowdAgent ag, CrowdAgentData crowAgentData)
    {
        // Move to another zone
        List<DtPolyPoint> potentialTargets = new();
        foreach (var zone in _polyPoints)
        {
            if (RcVec3f.DistSqr(zone.pt, ag.npos) > zoneRadius * zoneRadius)
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

    public void Setup(float maxAgentRadius, DtNavMesh nav)
    {
        navMesh = nav;
        if (nav != null)
        {
            config = new DtCrowdConfig(maxAgentRadius);
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();
        dd.DepthMask(false);
        if (crowd != null)
        {
            foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
            {
                float radius = ag.option.radius;
                RcVec3f pos = ag.npos;
                dd.DebugDrawCircle(pos.x, pos.y, pos.z, radius, DuRGBA(0, 0, 0, 32), 2.0f);
            }

            foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
            {
                CrowdAgentData crowAgentData = (CrowdAgentData)ag.option.userData;

                float height = ag.option.height;
                float radius = ag.option.radius;
                RcVec3f pos = ag.npos;

                int col = DuRGBA(220, 220, 220, 128);
                if (crowAgentData.type == CrowdAgentType.TRAVELLER)
                {
                    col = DuRGBA(100, 160, 100, 128);
                }

                if (crowAgentData.type == CrowdAgentType.VILLAGER)
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

    private DtCrowdAgent AddAgent(RcVec3f p, CrowdAgentType type)
    {
        DtCrowdAgentParams ap = agentParamsSupplier.Invoke();
        ap.userData = new CrowdAgentData(type, p);
        return crowd.AddAgent(p, ap);
    }

    public void UpdateAgentParams(int updateFlags, int obstacleAvoidanceType, float separationWeight)
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
                option.updateFlags = updateFlags;
                option.obstacleAvoidanceType = obstacleAvoidanceType;
                option.separationWeight = separationWeight;
                crowd.UpdateAgentParameters(ag, option);
            }
        }
    }
}