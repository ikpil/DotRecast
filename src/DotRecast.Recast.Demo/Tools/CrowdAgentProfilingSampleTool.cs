/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Tools;
using ImGuiNET;
using Serilog;
using static DotRecast.Recast.Demo.Draw.DebugDraw;

namespace DotRecast.Recast.Demo.Tools;

public class CrowdAgentProfilingSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<CrowdAgentProfilingSampleTool>();

    private DemoSample _sample;
    private DtNavMesh m_nav;

    private readonly RcCrowdAgentProfilingTool _tool;


    public CrowdAgentProfilingSampleTool()
    {
        _tool = new();
    }

    public void Layout()
    {
        var cfg = _tool.GetCrowdConfig();
        var prevOptimizeVis = cfg.optimizeVis;
        var prevOptimizeTopo = cfg.optimizeTopo;
        var prevAnticipateTurns = cfg.anticipateTurns;
        var prevObstacleAvoidance = cfg.obstacleAvoidance;
        var prevSeparation = cfg.separation;
        var prevObstacleAvoidanceType = cfg.obstacleAvoidanceType;
        var prevSeparationWeight = cfg.separationWeight;

        ImGui.Text("Options");
        ImGui.Separator();
        ImGui.Checkbox("Optimize Visibility", ref cfg.optimizeVis);
        ImGui.Checkbox("Optimize Topology", ref cfg.optimizeTopo);
        ImGui.Checkbox("Anticipate Turns", ref cfg.anticipateTurns);
        ImGui.Checkbox("Obstacle Avoidance", ref cfg.obstacleAvoidance);
        ImGui.SliderInt("Avoidance Quality", ref cfg.obstacleAvoidanceType, 0, 3);
        ImGui.Checkbox("Separation", ref cfg.separation);
        ImGui.SliderFloat("Separation Weight", ref cfg.separationWeight, 0f, 20f, "%.2f");
        ImGui.NewLine();

        if (prevOptimizeVis != cfg.optimizeVis || prevOptimizeTopo != cfg.optimizeTopo
                                               || prevAnticipateTurns != cfg.anticipateTurns
                                               || prevObstacleAvoidance != cfg.obstacleAvoidance
                                               || prevSeparation != cfg.separation
                                               || prevObstacleAvoidanceType != cfg.obstacleAvoidanceType
                                               || !prevSeparationWeight.Equals(cfg.separationWeight))
        {
            _tool.UpdateAgentParams();
        }

        var toolCfg = _tool.GetToolConfig();

        ImGui.Text("Simulation Options");
        ImGui.Separator();
        ImGui.SliderInt("Agents", ref toolCfg.agents, 0, 10000);
        ImGui.SliderInt("Random Seed", ref toolCfg.randomSeed, 0, 1024);
        ImGui.SliderInt("Number of Zones", ref toolCfg.numberOfZones, 0, 10);
        ImGui.SliderFloat("Zone Radius", ref toolCfg.zoneRadius, 0, 100, "%.0f");
        ImGui.SliderFloat("Mobs %", ref toolCfg.percentMobs, 0, 100, "%.0f");
        ImGui.SliderFloat("Travellers %", ref toolCfg.percentTravellers, 0, 100, "%.0f");
        ImGui.NewLine();

        ImGui.Text("Crowd Options");
        ImGui.Separator();
        ImGui.SliderInt("Path Queue Size", ref toolCfg.pathQueueSize, 0, 1024);
        ImGui.SliderInt("Max Iterations", ref toolCfg.maxIterations, 0, 4000);
        ImGui.NewLine();

        if (ImGui.Button("Start Crowd Profiling"))
        {
            var settings = _sample.GetSettings();
            _tool.StartProfiling(settings.agentRadius, settings.agentHeight, settings.agentMaxAcceleration, settings.agentMaxSpeed);
        }

        ImGui.Text("Times");
        ImGui.Separator();

        var crowd = _tool.GetCrowd();
        if (crowd != null)
        {
            ImGui.Text($"Max time to enqueue request: {crowd.Telemetry().MaxTimeToEnqueueRequest()} s");
            ImGui.Text($"Max time to find path: {crowd.Telemetry().MaxTimeToFindPath()} s");
            var timings = crowd.Telemetry().ToExecutionTimings();
            foreach (var rtt in timings)
            {
                ImGui.Text($"{rtt.Key}: {rtt.Micros} us");
            }

            ImGui.Text($"Current Update Time: {_tool.GetCrowdUpdateTime()} ms");
            ImGui.Text($"Sampling Update Time: {_tool.GetCrowdUpdateSamplingTime()} ms");
            ImGui.Text($"Avg Update Time: {_tool.GetCrowdUpdateAvgTime()} ms");
            ImGui.Text($"Max Update Time: {_tool.GetCrowdUpdateMaxTime()} ms");
            ImGui.Text($"Min Update Time: {_tool.GetCrowdUpdateMinTime()} ms");
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();
        dd.DepthMask(false);

        var crowd = _tool.GetCrowd();
        if (crowd != null)
        {
            foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
            {
                float radius = ag.option.radius;
                RcVec3f pos = ag.npos;
                dd.DebugDrawCircle(pos.X, pos.Y, pos.Z, radius, DuRGBA(0, 0, 0, 32), 2.0f);
            }

            foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
            {
                RcCrowdAgentData crowAgentData = (RcCrowdAgentData)ag.option.userData;

                float height = ag.option.height;
                float radius = ag.option.radius;
                RcVec3f pos = ag.npos;

                int col = DuRGBA(220, 220, 220, 128);
                if (crowAgentData.type == RcCrowdAgentType.TRAVELLER)
                {
                    col = DuRGBA(100, 160, 100, 128);
                }

                if (crowAgentData.type == RcCrowdAgentType.VILLAGER)
                {
                    col = DuRGBA(120, 80, 160, 128);
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                    col = DuLerpCol(col, DuRGBA(255, 255, 32, 128), 128);
                else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                    col = DuLerpCol(col, DuRGBA(255, 64, 32, 128), 128);
                else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                    col = DuRGBA(255, 32, 16, 128);
                else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                    col = DuLerpCol(col, DuRGBA(64, 255, 0, 128), 128);

                dd.DebugDrawCylinder(pos.X - radius, pos.Y + radius * 0.1f, pos.Z - radius, pos.X + radius, pos.Y + height,
                    pos.Z + radius, col);
            }
        }

        dd.DepthMask(true);
    }

    public void SetSample(DemoSample sample)
    {
        _sample = sample;
    }

    public void OnSampleChanged()
    {
        var geom = _sample.GetInputGeom();
        var settings = _sample.GetSettings();
        var navMesh = _sample.GetNavMesh();

        if (navMesh != null && m_nav != navMesh)
        {
            m_nav = navMesh;
            _tool.Setup(settings.agentRadius, m_nav);
        }
    }

    public IRcToolable GetTool()
    {
        return _tool;
    }


    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        //throw new NotImplementedException();
    }


    public void HandleUpdate(float dt)
    {
        _tool.Update(dt);
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
        //throw new NotImplementedException();
    }
}