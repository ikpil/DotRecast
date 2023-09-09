/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
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
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using DotRecast.Detour.Crowd.Tracking;
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
    private readonly CrowdOption _option = new CrowdOption();
    private DtNavMesh m_nav;
    private DtCrowd crowd;
    private readonly DtCrowdAgentDebugInfo _agentDebug = new DtCrowdAgentDebugInfo();

    private readonly Dictionary<long, CrowdAgentTrail> m_trails = new();
    private RcVec3f m_targetPos;
    private long m_targetRef;
    private CrowdToolMode m_mode = CrowdToolMode.CREATE;
    private int m_modeIdx = CrowdToolMode.CREATE.Idx;
    private long crowdUpdateTime;

    private int _expandSelectedDebugDraw = 1;
    private bool _showCorners;
    private bool _showCollisionSegments;
    private bool _showPath;
    private bool _showVO;
    private bool _showOpt;
    private bool _showNeis;

    private int _expandDebugDraw = 0;
    private bool _showLabels;
    private bool _showGrid;
    private bool _showNodes;
    private bool _showPerfGraph;
    private bool _showDetailAll;

    public CrowdSampleTool()
    {
        _agentDebug.vod = new DtObstacleAvoidanceDebugData(2048);
        _tool = new();
    }

    public void Layout()
    {
        ImGui.Text($"Crowd Tool Mode");
        ImGui.Separator();
        CrowdToolMode previousToolMode = m_mode;
        ImGui.RadioButton(CrowdToolMode.CREATE.Label, ref m_modeIdx, CrowdToolMode.CREATE.Idx);
        ImGui.RadioButton(CrowdToolMode.MOVE_TARGET.Label, ref m_modeIdx, CrowdToolMode.MOVE_TARGET.Idx);
        ImGui.RadioButton(CrowdToolMode.SELECT.Label, ref m_modeIdx, CrowdToolMode.SELECT.Idx);
        ImGui.RadioButton(CrowdToolMode.TOGGLE_POLYS.Label, ref m_modeIdx, CrowdToolMode.TOGGLE_POLYS.Idx);
        ImGui.NewLine();

        if (previousToolMode.Idx != m_modeIdx)
        {
            m_mode = CrowdToolMode.Values[m_modeIdx];
        }

        var prevOptimizeVis = _option.optimizeVis;
        var prevOptimizeTopo = _option.optimizeTopo;
        var prevAnticipateTurns = _option.anticipateTurns;
        var prevObstacleAvoidance = _option.obstacleAvoidance;
        var prevSeparation = _option.separation;
        var prevObstacleAvoidanceType = _option.obstacleAvoidanceType;
        var prevSeparationWeight = _option.separationWeight;

        ImGui.Text("Options");
        ImGui.Separator();
        ImGui.Checkbox("Optimize Visibility", ref _option.optimizeVis);
        ImGui.Checkbox("Optimize Topology", ref _option.optimizeTopo);
        ImGui.Checkbox("Anticipate Turns", ref _option.anticipateTurns);
        ImGui.Checkbox("Obstacle Avoidance", ref _option.obstacleAvoidance);
        ImGui.SliderInt("Avoidance Quality", ref _option.obstacleAvoidanceType, 0, 3);
        ImGui.Checkbox("Separation", ref _option.separation);
        ImGui.SliderFloat("Separation Weight", ref _option.separationWeight, 0f, 20f, "%.2f");
        ImGui.NewLine();

        if (prevOptimizeVis != _option.optimizeVis || prevOptimizeTopo != _option.optimizeTopo
                                                   || prevAnticipateTurns != _option.anticipateTurns
                                                   || prevObstacleAvoidance != _option.obstacleAvoidance
                                                   || prevSeparation != _option.separation
                                                   || prevObstacleAvoidanceType != _option.obstacleAvoidanceType
                                                   || !prevSeparationWeight.Equals(_option.separationWeight))
        {
            UpdateAgentParams();
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
        ImGui.Text($"Update Time: {crowdUpdateTime} ms");
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

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                    col = DuLerpCol(col, DuRGBA(255, 255, 32, 128), 128);
                else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                    col = DuLerpCol(col, DuRGBA(255, 64, 32, 128), 128);
                else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                    col = DuRGBA(255, 32, 16, 128);
                else if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                    col = DuLerpCol(col, DuRGBA(64, 255, 0, 128), 128);

                dd.DebugDrawCylinder(pos.x - radius, pos.y + radius * 0.1f, pos.z - radius, pos.x + radius, pos.y + height,
                    pos.z + radius, col);
            }
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

            DtCrowdConfig config = new DtCrowdConfig(settings.agentRadius);
            crowd = new DtCrowd(config, navMesh, __ => new DtQueryDefaultFilter(
                SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
                SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED,
                new float[] { 1f, 10f, 1f, 1f, 2f, 1.5f })
            );

            // Setup local avoidance option to different qualities.
            // Use mostly default settings, copy from dtCrowd.
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
    }

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        if (crowd == null)
        {
            return;
        }

        if (m_mode == CrowdToolMode.CREATE)
        {
            if (shift)
            {
                // Delete
                DtCrowdAgent ahit = HitTestAgents(s, p);
                if (ahit != null)
                {
                    RemoveAgent(ahit);
                }
            }
            else
            {
                // Add
                AddAgent(p);
            }
        }
        else if (m_mode == CrowdToolMode.MOVE_TARGET)
        {
            SetMoveTarget(p, shift);
        }
        else if (m_mode == CrowdToolMode.SELECT)
        {
            // Highlight
            DtCrowdAgent ahit = HitTestAgents(s, p);
            HighlightAgent(ahit);
        }
        else if (m_mode == CrowdToolMode.TOGGLE_POLYS)
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

    private void RemoveAgent(DtCrowdAgent agent)
    {
        crowd.RemoveAgent(agent);
        if (agent == _agentDebug.agent)
        {
            _agentDebug.agent = null;
        }
    }

    private void AddAgent(RcVec3f p)
    {
        DtCrowdAgentParams ap = GetAgentParams();
        DtCrowdAgent ag = crowd.AddAgent(p, ap);
        if (ag != null)
        {
            if (m_targetRef != 0)
                crowd.RequestMoveTarget(ag, m_targetRef, m_targetPos);

            // Init trail
            if (!m_trails.TryGetValue(ag.idx, out var trail))
            {
                trail = new CrowdAgentTrail();
                m_trails.Add(ag.idx, trail);
            }

            for (int i = 0; i < CrowdAgentTrail.AGENT_MAX_TRAIL; ++i)
            {
                trail.trail[i * 3] = p.x;
                trail.trail[i * 3 + 1] = p.y;
                trail.trail[i * 3 + 2] = p.z;
            }

            trail.htrail = 0;
        }
    }

    private DtCrowdAgentParams GetAgentParams()
    {
        var settings = _sample.GetSettings();

        DtCrowdAgentParams ap = new DtCrowdAgentParams();
        ap.radius = settings.agentRadius;
        ap.height = settings.agentHeight;
        ap.maxAcceleration = settings.agentMaxAcceleration;
        ap.maxSpeed = settings.agentMaxSpeed;
        ap.collisionQueryRange = ap.radius * 12.0f;
        ap.pathOptimizationRange = ap.radius * 30.0f;
        ap.updateFlags = _option.GetUpdateFlags();
        ap.obstacleAvoidanceType = _option.obstacleAvoidanceType;
        ap.separationWeight = _option.separationWeight;
        return ap;
    }

    private DtCrowdAgent HitTestAgents(RcVec3f s, RcVec3f p)
    {
        DtCrowdAgent isel = null;
        float tsel = float.MaxValue;

        foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
        {
            RcVec3f bmin = new RcVec3f();
            RcVec3f bmax = new RcVec3f();
            GetAgentBounds(ag, ref bmin, ref bmax);
            if (Intersections.IsectSegAABB(s, p, bmin, bmax, out var tmin, out var tmax))
            {
                if (tmin > 0 && tmin < tsel)
                {
                    isel = ag;
                    tsel = tmin;
                }
            }
        }

        return isel;
    }

    private void GetAgentBounds(DtCrowdAgent ag, ref RcVec3f bmin, ref RcVec3f bmax)
    {
        RcVec3f p = ag.npos;
        float r = ag.option.radius;
        float h = ag.option.height;
        bmin.x = p.x - r;
        bmin.y = p.y;
        bmin.z = p.z - r;
        bmax.x = p.x + r;
        bmax.y = p.y + h;
        bmax.z = p.z + r;
    }

    private void SetMoveTarget(RcVec3f p, bool adjust)
    {
        if (crowd == null)
            return;

        // Find nearest point on navmesh and set move request to that location.
        DtNavMeshQuery navquery = _sample.GetNavMeshQuery();
        IDtQueryFilter filter = crowd.GetFilter(0);
        RcVec3f halfExtents = crowd.GetQueryExtents();

        if (adjust)
        {
            // Request velocity
            if (_agentDebug.agent != null)
            {
                RcVec3f vel = CalcVel(_agentDebug.agent.npos, p, _agentDebug.agent.option.maxSpeed);
                crowd.RequestMoveVelocity(_agentDebug.agent, vel);
            }
            else
            {
                foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
                {
                    RcVec3f vel = CalcVel(ag.npos, p, ag.option.maxSpeed);
                    crowd.RequestMoveVelocity(ag, vel);
                }
            }
        }
        else
        {
            navquery.FindNearestPoly(p, halfExtents, filter, out m_targetRef, out m_targetPos, out var _);
            if (_agentDebug.agent != null)
            {
                crowd.RequestMoveTarget(_agentDebug.agent, m_targetRef, m_targetPos);
            }
            else
            {
                foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
                {
                    crowd.RequestMoveTarget(ag, m_targetRef, m_targetPos);
                }
            }
        }
    }

    private RcVec3f CalcVel(RcVec3f pos, RcVec3f tgt, float speed)
    {
        RcVec3f vel = tgt.Subtract(pos);
        vel.y = 0.0f;
        vel.Normalize();
        return vel.Scale(speed);
    }


    public void HandleUpdate(float dt)
    {
        if (crowd == null)
            return;

        DtNavMesh nav = _sample.GetNavMesh();
        if (nav == null)
            return;

        long startTime = RcFrequency.Ticks;
        crowd.Update(dt, _agentDebug);
        long endTime = RcFrequency.Ticks;

        // Update agent trails
        foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
        {
            CrowdAgentTrail trail = m_trails[ag.idx];
            // Update agent movement trail.
            trail.htrail = (trail.htrail + 1) % CrowdAgentTrail.AGENT_MAX_TRAIL;
            trail.trail[trail.htrail * 3] = ag.npos.x;
            trail.trail[trail.htrail * 3 + 1] = ag.npos.y;
            trail.trail[trail.htrail * 3 + 2] = ag.npos.z;
        }

        _agentDebug.vod.NormalizeSamples();

        // m_crowdSampleCount.addSample((float) crowd.GetVelocitySampleCount());
        crowdUpdateTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
    }

    private void HighlightAgent(DtCrowdAgent agent)
    {
        _agentDebug.agent = agent;
    }


    private void UpdateAgentParams()
    {
        if (crowd == null)
        {
            return;
        }

        foreach (DtCrowdAgent ag in crowd.GetActiveAgents())
        {
            DtCrowdAgentParams option = new DtCrowdAgentParams();
            option.radius = ag.option.radius;
            option.height = ag.option.height;
            option.maxAcceleration = ag.option.maxAcceleration;
            option.maxSpeed = ag.option.maxSpeed;
            option.collisionQueryRange = ag.option.collisionQueryRange;
            option.pathOptimizationRange = ag.option.pathOptimizationRange;
            option.obstacleAvoidanceType = ag.option.obstacleAvoidanceType;
            option.queryFilterType = ag.option.queryFilterType;
            option.userData = ag.option.userData;
            option.updateFlags = _option.GetUpdateFlags();
            option.obstacleAvoidanceType = _option.obstacleAvoidanceType;
            option.separationWeight = _option.separationWeight;
            crowd.UpdateAgentParameters(ag, option);
        }
    }


    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}