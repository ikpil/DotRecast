/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class CrowdSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<CrowdSampleTool>();

    private DemoSample _sample;
    private readonly RcCrowdTool _tool;

    private DtNavMesh m_nav;

    private RcCrowdToolMode m_mode = RcCrowdToolMode.CREATE;
    private int m_modeIdx = RcCrowdToolMode.CREATE.Idx;

    private int _expandSelectedDebugDraw = 1;
    private bool _showCorners = true;
    private bool _showCollisionSegments = true;
    private bool _showPath = true;
    private bool _showVO = true;
    private bool _showOpt = true;
    private bool _showNeis = true;

    private int _expandDebugDraw = 0;
    private bool _showLabels = true;
    private bool _showGrid = false;
    private bool _showNodes = true;
    private bool _showPerfGraph = true;
    private bool _showDetailAll = true;

    public CrowdSampleTool()
    {
        _tool = new();
    }

    public void Layout()
    {
        ImGui.Text($"Crowd Tool Mode");
        ImGui.Separator();
        RcCrowdToolMode previousToolMode = m_mode;
        ImGui.RadioButton(RcCrowdToolMode.CREATE.Label, ref m_modeIdx, RcCrowdToolMode.CREATE.Idx);
        ImGui.RadioButton(RcCrowdToolMode.MOVE_TARGET.Label, ref m_modeIdx, RcCrowdToolMode.MOVE_TARGET.Idx);
        ImGui.RadioButton(RcCrowdToolMode.SELECT.Label, ref m_modeIdx, RcCrowdToolMode.SELECT.Idx);
        ImGui.RadioButton(RcCrowdToolMode.TOGGLE_POLYS.Label, ref m_modeIdx, RcCrowdToolMode.TOGGLE_POLYS.Idx);
        ImGui.NewLine();

        if (previousToolMode.Idx != m_modeIdx)
        {
            m_mode = RcCrowdToolMode.Values[m_modeIdx];
        }

        var crowdCfg = _tool.GetCrowdConfig();
        var prevOptimizeVis = crowdCfg.optimizeVis;
        var prevOptimizeTopo = crowdCfg.optimizeTopo;
        var prevAnticipateTurns = crowdCfg.anticipateTurns;
        var prevObstacleAvoidance = crowdCfg.obstacleAvoidance;
        var prevSeparation = crowdCfg.separation;
        var prevObstacleAvoidanceType = crowdCfg.obstacleAvoidanceType;
        var prevSeparationWeight = crowdCfg.separationWeight;

        ImGui.Text("Options");
        ImGui.Separator();
        ImGui.Checkbox("Optimize Visibility", ref crowdCfg.optimizeVis);
        ImGui.Checkbox("Optimize Topology", ref crowdCfg.optimizeTopo);
        ImGui.Checkbox("Anticipate Turns", ref crowdCfg.anticipateTurns);
        ImGui.Checkbox("Obstacle Avoidance", ref crowdCfg.obstacleAvoidance);
        ImGui.SliderInt("Avoidance Quality", ref crowdCfg.obstacleAvoidanceType, 0, 3);
        ImGui.Checkbox("Separation", ref crowdCfg.separation);
        ImGui.SliderFloat("Separation Weight", ref crowdCfg.separationWeight, 0f, 20f, "%.2f");
        ImGui.NewLine();

        if (prevOptimizeVis != crowdCfg.optimizeVis || prevOptimizeTopo != crowdCfg.optimizeTopo
                                                    || prevAnticipateTurns != crowdCfg.anticipateTurns
                                                    || prevObstacleAvoidance != crowdCfg.obstacleAvoidance
                                                    || prevSeparation != crowdCfg.separation
                                                    || prevObstacleAvoidanceType != crowdCfg.obstacleAvoidanceType
                                                    || !prevSeparationWeight.Equals(crowdCfg.separationWeight))
        {
            _tool.UpdateAgentParams();
        }


        ImGui.Text("Selected Debug Draw");
        ImGui.Separator();
        ImGui.Checkbox("Show Corners", ref _showCorners);
        ImGui.Checkbox("Show Collision Segs", ref _showCollisionSegments);
        ImGui.Checkbox("Show Path", ref _showPath);
        ImGui.Checkbox("Show VO", ref _showVO);
        ImGui.Checkbox("Show Path Optimization", ref _showOpt);
        ImGui.Checkbox("Show Neighbours", ref _showNeis);
        ImGui.NewLine();

        ImGui.Text("Debug Draw");
        ImGui.Separator();
        ImGui.Checkbox("Show Proximity Grid", ref _showGrid);
        ImGui.Checkbox("Show Nodes", ref _showNodes);
        ImGui.Text($"Update Time: {_tool.GetCrowdUpdateTime()} ms");
    }

    public unsafe void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();
        var settings = _sample.GetSettings();
        float rad = settings.agentRadius;

        var crowd = _tool.GetCrowd();
        if (crowd == null)
            return;

        var nav = crowd.GetNavMesh();
        if (nav == null)
            return;

        var cfg = _tool.GetCrowdConfig();
        var agentDebug = _tool.GetCrowdAgentDebugInfo();
        var agentTrails = _tool.GetCrowdAgentTrails();
        var moveTargetRef = _tool.GetMoveTargetRef();
        var moveTargetPos = _tool.GetMoveTargetPos();

        if (_showNodes && crowd.GetPathQueue() != null)
        {
            var navquery = crowd.GetNavMeshQuery();
            if (navquery != null)
            {
                dd.DebugDrawNavMeshNodes(navquery);
            }
        }

        dd.DepthMask(false);

        // Draw paths
        if (_showPath)
        {
            foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
            {
                if (!_showDetailAll && ag != agentDebug.agent)
                    continue;

                var path = ag.corridor.GetPath();
                int npath = ag.corridor.GetPathCount();
                for (int j = 0; j < npath; ++j)
                {
                    dd.DebugDrawNavMeshPoly(nav, path[j], DuRGBA(255, 255, 255, 24));
                }
            }
        }

        if (moveTargetRef != 0)
            dd.DebugDrawCross(moveTargetPos.X, moveTargetPos.Y + 0.1f, moveTargetPos.Z, rad, DuRGBA(255, 255, 255, 192), 2.0f);

        // Occupancy grid.
        if (_showGrid)
        {
            float gridy = -float.MaxValue;
            foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
            {
                RcVec3f pos = ag.corridor.GetPos();
                gridy = Math.Max(gridy, pos.Y);
            }

            gridy += 1.0f;

            DtProximityGrid grid = crowd.GetGrid();
            if (null != grid)
            {
                dd.Begin(QUADS);
                float cs = grid.GetCellSize();
                foreach (var (combinedKey, count) in grid.GetItemCounts())
                {
                    DtProximityGrid.DecomposeKey(combinedKey, out var x, out var y);
                    if (count != 0)
                    {
                        int col = DuRGBA(128, 0, 0, Math.Min(count * 40, 255));
                        dd.Vertex(x * cs, gridy, y * cs, col);
                        dd.Vertex(x * cs, gridy, y * cs + cs, col);
                        dd.Vertex(x * cs + cs, gridy, y * cs + cs, col);
                        dd.Vertex(x * cs + cs, gridy, y * cs, col);
                    }
                }

                dd.End();
            }
        }

        // Trail
        foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
        {
            RcCrowdAgentTrail trail = agentTrails[ag.idx];
            RcVec3f pos = ag.npos;

            dd.Begin(LINES, 3.0f);
            RcVec3f prev = new RcVec3f();
            float preva = 1;
            prev = pos;
            for (int j = 0; j < RcCrowdAgentTrail.AGENT_MAX_TRAIL - 1; ++j)
            {
                int idx = (trail.htrail + RcCrowdAgentTrail.AGENT_MAX_TRAIL - j) % RcCrowdAgentTrail.AGENT_MAX_TRAIL;
                int v = idx * 3;
                float a = 1 - j / (float)RcCrowdAgentTrail.AGENT_MAX_TRAIL;
                dd.Vertex(prev.X, prev.Y + 0.1f, prev.Z, DuRGBA(0, 0, 0, (int)(128 * preva)));
                dd.Vertex(trail.trail[v], trail.trail[v + 1] + 0.1f, trail.trail[v + 2], DuRGBA(0, 0, 0, (int)(128 * a)));
                preva = a;
                prev = RcVec.Create(trail.trail, v);
            }

            dd.End();
        }

        // Corners & co
        foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
        {
            if (_showDetailAll == false && ag != agentDebug.agent)
                continue;

            float radius = ag.option.radius;
            RcVec3f pos = ag.npos;

            if (_showCorners)
            {
                if (0 < ag.ncorners)
                {
                    dd.Begin(LINES, 2.0f);
                    for (int j = 0; j < ag.ncorners; ++j)
                    {
                        RcVec3f va = j == 0 ? pos : ag.corners[j - 1].pos;
                        RcVec3f vb = ag.corners[j].pos;
                        dd.Vertex(va.X, va.Y + radius, va.Z, DuRGBA(128, 0, 0, 192));
                        dd.Vertex(vb.X, vb.Y + radius, vb.Z, DuRGBA(128, 0, 0, 192));
                    }

                    if ((ag.corners[ag.ncorners - 1].flags
                         & DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        RcVec3f v = ag.corners[ag.ncorners - 1].pos;
                        dd.Vertex(v.X, v.Y, v.Z, DuRGBA(192, 0, 0, 192));
                        dd.Vertex(v.X, v.Y + radius * 2, v.Z, DuRGBA(192, 0, 0, 192));
                    }

                    dd.End();

                    if (cfg.anticipateTurns)
                    {
                        /*                  float dvel[3], pos[3];
                         CalcSmoothSteerDirection(ag.pos, ag.cornerVerts, ag.ncorners, dvel);
                         pos.x = ag.pos.x + dvel.x;
                         pos.y = ag.pos.y + dvel.y;
                         pos.z = ag.pos.z + dvel.z;

                         float off = ag.radius+0.1f;
                         float[] tgt = &ag.cornerVerts.x;
                         float y = ag.pos.y+off;

                         dd.Begin(DU_DRAW_LINES, 2.0f);

                         dd.Vertex(ag.pos.x,y,ag.pos.z, DuRGBA(255,0,0,192));
                         dd.Vertex(pos.x,y,pos.z, DuRGBA(255,0,0,192));

                         dd.Vertex(pos.x,y,pos.z, DuRGBA(255,0,0,192));
                         dd.Vertex(tgt.x,y,tgt.z, DuRGBA(255,0,0,192));

                         dd.End();*/
                    }
                }
            }

            if (_showCollisionSegments)
            {
                RcVec3f center = ag.boundary.GetCenter();
                dd.DebugDrawCross(center.X, center.Y + radius, center.Z, 0.2f, DuRGBA(192, 0, 128, 255), 2.0f);
                dd.DebugDrawCircle(center.X, center.Y + radius, center.Z, ag.option.collisionQueryRange, DuRGBA(192, 0, 128, 128), 2.0f);

                dd.Begin(LINES, 3.0f);
                for (int j = 0; j < ag.boundary.GetSegmentCount(); ++j)
                {
                    int col = DuRGBA(192, 0, 128, 192);
                    var s = ag.boundary.GetSegment(j);
                    //RcVec3f s0 = s[0];
                    //RcVec3f s3 = s[1];
                    RcVec3f s3 = new RcVec3f(s.s[3 + 0], s.s[3 + 1], s.s[3 + 2]);
                    RcVec3f s0 = new RcVec3f(s.s[0 + 0], s.s[0 + 1], s.s[0 + 2]);
                    if (DtUtils.TriArea2D(pos, s0, s3) < 0.0f)
                        col = DuDarkenCol(col);

                    dd.AppendArrow(s0.X, s0.Y + 0.2f, s0.Z, s3.X, s3.Z + 0.2f, s3.Z, 0.0f, 0.3f, col);
                }

                dd.End();
            }

            if (_showNeis)
            {
                dd.DebugDrawCircle(pos.X, pos.Y + radius, pos.Z, ag.option.collisionQueryRange, DuRGBA(0, 192, 128, 128),
                    2.0f);

                dd.Begin(LINES, 2.0f);
                for (int j = 0; j < ag.nneis; ++j)
                {
                    DtCrowdAgent nei = ag.neis[j].agent;
                    if (nei != null)
                    {
                        dd.Vertex(pos.X, pos.Y + radius, pos.Z, DuRGBA(0, 192, 128, 128));
                        dd.Vertex(nei.npos.X, nei.npos.Y + radius, nei.npos.Z, DuRGBA(0, 192, 128, 128));
                    }
                }

                dd.End();
            }

            if (_showOpt)
            {
                dd.Begin(LINES, 2.0f);
                dd.Vertex(agentDebug.optStart.X, agentDebug.optStart.Y + 0.3f, agentDebug.optStart.Z,
                    DuRGBA(0, 128, 0, 192));
                dd.Vertex(agentDebug.optEnd.X, agentDebug.optEnd.Y + 0.3f, agentDebug.optEnd.Z, DuRGBA(0, 128, 0, 192));
                dd.End();
            }
        }

        // Agent cylinders.
        foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
        {
            float radius = ag.option.radius;
            RcVec3f pos = ag.npos;

            int col = DuRGBA(0, 0, 0, 32);
            if (agentDebug.agent == ag)
                col = DuRGBA(255, 0, 0, 128);

            dd.DebugDrawCircle(pos.X, pos.Y, pos.Z, radius, col, 2.0f);
        }

        foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
        {
            float height = ag.option.height;
            float radius = ag.option.radius;
            RcVec3f pos = ag.npos;

            int col = DuRGBA(220, 220, 220, 128);
            if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                col = DuLerpCol(col, DuRGBA(128, 0, 255, 128), 32);
            else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                col = DuLerpCol(col, DuRGBA(128, 0, 255, 128), 128);
            else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                col = DuRGBA(255, 32, 16, 128);
            else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                col = DuLerpCol(col, DuRGBA(64, 255, 0, 128), 128);

            dd.DebugDrawCylinder(pos.X - radius, pos.Y + radius * 0.1f, pos.Z - radius, pos.X + radius, pos.Y + height,
                pos.Z + radius, col);
        }

        if (_showVO)
        {
            foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
            {
                if (_showDetailAll == false && ag != agentDebug.agent)
                    continue;

                // Draw detail about agent sela
                DtObstacleAvoidanceDebugData vod = agentDebug.vod;

                float dx = ag.npos.X;
                float dy = ag.npos.Y + ag.option.height;
                float dz = ag.npos.Z;

                dd.DebugDrawCircle(dx, dy, dz, ag.option.maxSpeed, DuRGBA(255, 255, 255, 64), 2.0f);

                dd.Begin(QUADS);
                for (int j = 0; j < vod.GetSampleCount(); ++j)
                {
                    RcVec3f p = vod.GetSampleVelocity(j);
                    float sr = vod.GetSampleSize(j);
                    float pen = vod.GetSamplePenalty(j);
                    float pen2 = vod.GetSamplePreferredSidePenalty(j);
                    int col = DuLerpCol(DuRGBA(255, 255, 255, 220), DuRGBA(128, 96, 0, 220), (int)(pen * 255));
                    col = DuLerpCol(col, DuRGBA(128, 0, 0, 220), (int)(pen2 * 128));
                    dd.Vertex(dx + p.X - sr, dy, dz + p.Z - sr, col);
                    dd.Vertex(dx + p.X - sr, dy, dz + p.Z + sr, col);
                    dd.Vertex(dx + p.X + sr, dy, dz + p.Z + sr, col);
                    dd.Vertex(dx + p.X + sr, dy, dz + p.Z - sr, col);
                }

                dd.End();
            }
        }

        // Velocity stuff.
        foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
        {
            float radius = ag.option.radius;
            float height = ag.option.height;
            RcVec3f pos = ag.npos;
            RcVec3f vel = ag.vel;
            RcVec3f dvel = ag.dvel;

            int col = DuRGBA(220, 220, 220, 192);
            if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                col = DuLerpCol(col, DuRGBA(128, 0, 255, 192), 48);
            else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                col = DuLerpCol(col, DuRGBA(128, 0, 255, 192), 128);
            else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                col = DuRGBA(255, 32, 16, 192);
            else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                col = DuLerpCol(col, DuRGBA(64, 255, 0, 192), 128);

            dd.DebugDrawCircle(pos.X, pos.Y + height, pos.Z, radius, col, 2.0f);

            dd.DebugDrawArrow(pos.X, pos.Y + height, pos.Z, pos.X + dvel.X, pos.Y + height + dvel.Y, pos.Z + dvel.Z,
                0.0f, 0.4f, DuRGBA(0, 192, 255, 192), agentDebug.agent == ag ? 2.0f : 1.0f);

            dd.DebugDrawArrow(pos.X, pos.Y + height, pos.Z, pos.X + vel.X, pos.Y + height + vel.Y, pos.Z + vel.Z, 0.0f,
                0.4f, DuRGBA(0, 0, 0, 160), 2.0f);
        }

        dd.DepthMask(true);
    }

    public IRcToolable GetTool()
    {
        return _tool;
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
            _tool.Setup(settings.agentRadius, navMesh);
        }
    }

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        var crowd = _tool.GetCrowd();
        if (crowd == null)
        {
            return;
        }

        if (m_mode == RcCrowdToolMode.CREATE)
        {
            if (shift)
            {
                // Delete
                DtCrowdAgent ahit = _tool.HitTestAgents(s, p);
                if (ahit != null)
                {
                    _tool.RemoveAgent(ahit);
                }
            }
            else
            {
                // Add
                var settings = _sample.GetSettings();
                _tool.AddAgent(p, settings.agentRadius, settings.agentHeight, settings.agentMaxAcceleration, settings.agentMaxSpeed);
            }
        }
        else if (m_mode == RcCrowdToolMode.MOVE_TARGET)
        {
            _tool.SetMoveTarget(p, shift);
        }
        else if (m_mode == RcCrowdToolMode.SELECT)
        {
            // Highlight
            DtCrowdAgent ahit = _tool.HitTestAgents(s, p);
            _tool.HighlightAgent(ahit);
        }
        else if (m_mode == RcCrowdToolMode.TOGGLE_POLYS)
        {
            DtNavMesh nav = _sample.GetNavMesh();
            DtNavMeshQuery navquery = _sample.GetNavMeshQuery();
            if (nav != null && navquery != null)
            {
                IDtQueryFilter filter = new DtQueryDefaultFilter();
                RcVec3f halfExtents = crowd.GetQueryExtents();
                navquery.FindNearestPoly(p, halfExtents, filter, out var refs, out var nearestPt, out var _);
                if (refs != 0)
                {
                    var status = nav.GetPolyFlags(refs, out var f);
                    if (status.Succeeded())
                    {
                        nav.SetPolyFlags(refs, f ^ SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED);
                    }
                }
            }
        }
    }


    public void HandleUpdate(float dt)
    {
        _tool.Update(dt);
    }


    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}