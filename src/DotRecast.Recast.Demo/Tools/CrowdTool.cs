/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
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
using System.Collections.Immutable;
using System.Diagnostics;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using DotRecast.Detour.Crowd.Tracking;
using DotRecast.Detour.QueryResults;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;
using ImGuiNET;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;
using static DotRecast.Core.RcMath;

namespace DotRecast.Recast.Demo.Tools;

public class CrowdTool : Tool
{
    private readonly CrowdToolParams toolParams = new CrowdToolParams();
    private Sample sample;
    private NavMesh m_nav;
    private Crowd crowd;
    private readonly CrowdProfilingTool profilingTool;
    private readonly CrowdAgentDebugInfo m_agentDebug = new CrowdAgentDebugInfo();

    public static readonly int AGENT_MAX_TRAIL = 64;
    private readonly Dictionary<long, AgentTrail> m_trails = new();
    private Vector3f m_targetPos;
    private long m_targetRef;
    private CrowdToolMode m_mode = CrowdToolMode.CREATE;
    private int m_modeIdx = CrowdToolMode.CREATE.Idx;
    private long crowdUpdateTime;

    public CrowdTool()
    {
        m_agentDebug.vod = new ObstacleAvoidanceDebugData(2048);
        profilingTool = new CrowdProfilingTool(GetAgentParams);
    }

    public override void SetSample(Sample psample)
    {
        if (sample != psample)
        {
            sample = psample;
        }

        NavMesh nav = sample.GetNavMesh();

        if (nav != null && m_nav != nav)
        {
            m_nav = nav;

            CrowdConfig config = new CrowdConfig(sample.GetSettingsUI().GetAgentRadius());

            crowd = new Crowd(config, nav, __ => new DefaultQueryFilter(SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
                SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED, new float[] { 1f, 10f, 1f, 1f, 2f, 1.5f }));

            // Setup local avoidance option to different qualities.
            // Use mostly default settings, copy from dtCrowd.
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

            profilingTool.Setup(sample.GetSettingsUI().GetAgentRadius(), m_nav);
        }
    }

    public override void HandleClick(Vector3f s, Vector3f p, bool shift)
    {
        if (m_mode == CrowdToolMode.PROFILING)
        {
            return;
        }

        if (crowd == null)
        {
            return;
        }

        if (m_mode == CrowdToolMode.CREATE)
        {
            if (shift)
            {
                // Delete
                CrowdAgent ahit = HitTestAgents(s, p);
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
            CrowdAgent ahit = HitTestAgents(s, p);
            HilightAgent(ahit);
        }
        else if (m_mode == CrowdToolMode.TOGGLE_POLYS)
        {
            NavMesh nav = sample.GetNavMesh();
            NavMeshQuery navquery = sample.GetNavMeshQuery();
            if (nav != null && navquery != null)
            {
                IQueryFilter filter = new DefaultQueryFilter();
                Vector3f halfExtents = crowd.GetQueryExtents();
                Result<FindNearestPolyResult> result = navquery.FindNearestPoly(p, halfExtents, filter);
                long refs = result.result.GetNearestRef();
                if (refs != 0)
                {
                    Result<int> flags = nav.GetPolyFlags(refs);
                    if (flags.Succeeded())
                    {
                        nav.SetPolyFlags(refs, flags.result ^ SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED);
                    }
                }
            }
        }
    }

    private void RemoveAgent(CrowdAgent agent)
    {
        crowd.RemoveAgent(agent);
        if (agent == m_agentDebug.agent)
        {
            m_agentDebug.agent = null;
        }
    }

    private void AddAgent(Vector3f p)
    {
        CrowdAgentParams ap = GetAgentParams();
        CrowdAgent ag = crowd.AddAgent(p, ap);
        if (ag != null)
        {
            if (m_targetRef != 0)
                crowd.RequestMoveTarget(ag, m_targetRef, m_targetPos);

            // Init trail
            if (!m_trails.TryGetValue(ag.idx, out var trail))
            {
                trail = new AgentTrail();
                m_trails.Add(ag.idx, trail);
            }

            for (int i = 0; i < AGENT_MAX_TRAIL; ++i)
            {
                trail.trail[i * 3] = p.x;
                trail.trail[i * 3 + 1] = p.y;
                trail.trail[i * 3 + 2] = p.z;
            }

            trail.htrail = 0;
        }
    }

    private CrowdAgentParams GetAgentParams()
    {
        CrowdAgentParams ap = new CrowdAgentParams();
        ap.radius = sample.GetSettingsUI().GetAgentRadius();
        ap.height = sample.GetSettingsUI().GetAgentHeight();
        ap.maxAcceleration = 8.0f;
        ap.maxSpeed = 3.5f;
        ap.collisionQueryRange = ap.radius * 12.0f;
        ap.pathOptimizationRange = ap.radius * 30.0f;
        ap.updateFlags = GetUpdateFlags();
        ap.obstacleAvoidanceType = toolParams.m_obstacleAvoidanceType;
        ap.separationWeight = toolParams.m_separationWeight;
        return ap;
    }

    private CrowdAgent HitTestAgents(Vector3f s, Vector3f p)
    {
        CrowdAgent isel = null;
        float tsel = float.MaxValue;

        foreach (CrowdAgent ag in crowd.GetActiveAgents())
        {
            Vector3f bmin = new Vector3f();
            Vector3f bmax = new Vector3f();
            GetAgentBounds(ag, ref bmin, ref bmax);
            float[] isect = Intersections.IntersectSegmentAABB(s, p, bmin, bmax);
            if (null != isect)
            {
                float tmin = isect[0];
                if (tmin > 0 && tmin < tsel)
                {
                    isel = ag;
                    tsel = tmin;
                }
            }
        }

        return isel;
    }

    private void GetAgentBounds(CrowdAgent ag, ref Vector3f bmin, ref Vector3f bmax)
    {
        Vector3f p = ag.npos;
        float r = ag.option.radius;
        float h = ag.option.height;
        bmin.x = p.x - r;
        bmin.y = p.y;
        bmin.z = p.z - r;
        bmax.x = p.x + r;
        bmax.y = p.y + h;
        bmax.z = p.z + r;
    }

    private void SetMoveTarget(Vector3f p, bool adjust)
    {
        if (sample == null || crowd == null)
            return;

        // Find nearest point on navmesh and set move request to that location.
        NavMeshQuery navquery = sample.GetNavMeshQuery();
        IQueryFilter filter = crowd.GetFilter(0);
        Vector3f halfExtents = crowd.GetQueryExtents();

        if (adjust)
        {
            // Request velocity
            if (m_agentDebug.agent != null)
            {
                Vector3f vel = CalcVel(m_agentDebug.agent.npos, p, m_agentDebug.agent.option.maxSpeed);
                crowd.RequestMoveVelocity(m_agentDebug.agent, vel);
            }
            else
            {
                foreach (CrowdAgent ag in crowd.GetActiveAgents())
                {
                    Vector3f vel = CalcVel(ag.npos, p, ag.option.maxSpeed);
                    crowd.RequestMoveVelocity(ag, vel);
                }
            }
        }
        else
        {
            Result<FindNearestPolyResult> result = navquery.FindNearestPoly(p, halfExtents, filter);
            m_targetRef = result.result.GetNearestRef();
            m_targetPos = result.result.GetNearestPos();
            if (m_agentDebug.agent != null)
            {
                crowd.RequestMoveTarget(m_agentDebug.agent, m_targetRef, m_targetPos);
            }
            else
            {
                foreach (CrowdAgent ag in crowd.GetActiveAgents())
                {
                    crowd.RequestMoveTarget(ag, m_targetRef, m_targetPos);
                }
            }
        }
    }

    private Vector3f CalcVel(Vector3f pos, Vector3f tgt, float speed)
    {
        Vector3f vel = tgt.Subtract(pos);
        vel.y = 0.0f;
        vel.Normalize();
        return vel.Scale(speed);
    }

    public override void HandleRender(NavMeshRenderer renderer)
    {
        if (m_mode == CrowdToolMode.PROFILING)
        {
            profilingTool.HandleRender(renderer);
            return;
        }

        RecastDebugDraw dd = renderer.GetDebugDraw();
        float rad = sample.GetSettingsUI().GetAgentRadius();
        NavMesh nav = sample.GetNavMesh();
        if (nav == null || crowd == null)
            return;

        if (toolParams.m_showNodes && crowd.GetPathQueue() != null)
        {
//            NavMeshQuery navquery = crowd.GetPathQueue().GetNavQuery();
//            if (navquery != null) {
//                dd.DebugDrawNavMeshNodes(navquery);
//            }
        }

        dd.DepthMask(false);

        // Draw paths
        if (toolParams.m_showPath)
        {
            foreach (CrowdAgent ag in crowd.GetActiveAgents())
            {
                if (!toolParams.m_showDetailAll && ag != m_agentDebug.agent)
                    continue;
                List<long> path = ag.corridor.GetPath();
                int npath = ag.corridor.GetPathCount();
                for (int j = 0; j < npath; ++j)
                {
                    dd.DebugDrawNavMeshPoly(nav, path[j], DuRGBA(255, 255, 255, 24));
                }
            }
        }

        if (m_targetRef != 0)
            dd.DebugDrawCross(m_targetPos.x, m_targetPos.y + 0.1f, m_targetPos.z, rad, DuRGBA(255, 255, 255, 192), 2.0f);

        // Occupancy grid.
        if (toolParams.m_showGrid)
        {
            float gridy = -float.MaxValue;
            foreach (CrowdAgent ag in crowd.GetActiveAgents())
            {
                Vector3f pos = ag.corridor.GetPos();
                gridy = Math.Max(gridy, pos.y);
            }

            gridy += 1.0f;

            dd.Begin(QUADS);
            ProximityGrid grid = crowd.GetGrid();
            float cs = grid.GetCellSize();
            foreach (var (combinedKey, count) in grid.GetItemCounts())
            {
                ProximityGrid.DecomposeKey(combinedKey, out var x, out var y);
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

        // Trail
        foreach (CrowdAgent ag in crowd.GetActiveAgents())
        {
            AgentTrail trail = m_trails[ag.idx];
            Vector3f pos = ag.npos;

            dd.Begin(LINES, 3.0f);
            Vector3f prev = new Vector3f();
            float preva = 1;
            prev = pos;
            for (int j = 0; j < AGENT_MAX_TRAIL - 1; ++j)
            {
                int idx = (trail.htrail + AGENT_MAX_TRAIL - j) % AGENT_MAX_TRAIL;
                int v = idx * 3;
                float a = 1 - j / (float)AGENT_MAX_TRAIL;
                dd.Vertex(prev.x, prev.y + 0.1f, prev.z, DuRGBA(0, 0, 0, (int)(128 * preva)));
                dd.Vertex(trail.trail[v], trail.trail[v + 1] + 0.1f, trail.trail[v + 2], DuRGBA(0, 0, 0, (int)(128 * a)));
                preva = a;
                VCopy(ref prev, trail.trail, v);
            }

            dd.End();
        }

        // Corners & co
        foreach (CrowdAgent ag in crowd.GetActiveAgents())
        {
            if (toolParams.m_showDetailAll == false && ag != m_agentDebug.agent)
                continue;

            float radius = ag.option.radius;
            Vector3f pos = ag.npos;

            if (toolParams.m_showCorners)
            {
                if (0 < ag.corners.Count)
                {
                    dd.Begin(LINES, 2.0f);
                    for (int j = 0; j < ag.corners.Count; ++j)
                    {
                        Vector3f va = j == 0 ? pos : ag.corners[j - 1].GetPos();
                        Vector3f vb = ag.corners[j].GetPos();
                        dd.Vertex(va.x, va.y + radius, va.z, DuRGBA(128, 0, 0, 192));
                        dd.Vertex(vb.x, vb.y + radius, vb.z, DuRGBA(128, 0, 0, 192));
                    }

                    if ((ag.corners[ag.corners.Count - 1].GetFlags()
                         & NavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        Vector3f v = ag.corners[ag.corners.Count - 1].GetPos();
                        dd.Vertex(v.x, v.y, v.z, DuRGBA(192, 0, 0, 192));
                        dd.Vertex(v.x, v.y + radius * 2, v.z, DuRGBA(192, 0, 0, 192));
                    }

                    dd.End();

                    if (toolParams.m_anticipateTurns)
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

            if (toolParams.m_showCollisionSegments)
            {
                Vector3f center = ag.boundary.GetCenter();
                dd.DebugDrawCross(center.x, center.y + radius, center.z, 0.2f, DuRGBA(192, 0, 128, 255), 2.0f);
                dd.DebugDrawCircle(center.x, center.y + radius, center.z, ag.option.collisionQueryRange, DuRGBA(192, 0, 128, 128), 2.0f);

                dd.Begin(LINES, 3.0f);
                for (int j = 0; j < ag.boundary.GetSegmentCount(); ++j)
                {
                    int col = DuRGBA(192, 0, 128, 192);
                    Vector3f[] s = ag.boundary.GetSegment(j);
                    Vector3f s0 = s[0];
                    Vector3f s3 = s[1];
                    if (TriArea2D(pos, s0, s3) < 0.0f)
                        col = DuDarkenCol(col);

                    dd.AppendArrow(s[0].x, s[0].y + 0.2f, s[0].z, s[1].x, s[1].z + 0.2f, s[1].z, 0.0f, 0.3f, col);
                }

                dd.End();
            }

            if (toolParams.m_showNeis)
            {
                dd.DebugDrawCircle(pos.x, pos.y + radius, pos.z, ag.option.collisionQueryRange, DuRGBA(0, 192, 128, 128),
                    2.0f);

                dd.Begin(LINES, 2.0f);
                for (int j = 0; j < ag.neis.Count; ++j)
                {
                    CrowdAgent nei = ag.neis[j].agent;
                    if (nei != null)
                    {
                        dd.Vertex(pos.x, pos.y + radius, pos.z, DuRGBA(0, 192, 128, 128));
                        dd.Vertex(nei.npos.x, nei.npos.y + radius, nei.npos.z, DuRGBA(0, 192, 128, 128));
                    }
                }

                dd.End();
            }

            if (toolParams.m_showOpt)
            {
                dd.Begin(LINES, 2.0f);
                dd.Vertex(m_agentDebug.optStart.x, m_agentDebug.optStart.y + 0.3f, m_agentDebug.optStart.z,
                    DuRGBA(0, 128, 0, 192));
                dd.Vertex(m_agentDebug.optEnd.x, m_agentDebug.optEnd.y + 0.3f, m_agentDebug.optEnd.z, DuRGBA(0, 128, 0, 192));
                dd.End();
            }
        }

        // Agent cylinders.
        foreach (CrowdAgent ag in crowd.GetActiveAgents())
        {
            float radius = ag.option.radius;
            Vector3f pos = ag.npos;

            int col = DuRGBA(0, 0, 0, 32);
            if (m_agentDebug.agent == ag)
                col = DuRGBA(255, 0, 0, 128);

            dd.DebugDrawCircle(pos.x, pos.y, pos.z, radius, col, 2.0f);
        }

        foreach (CrowdAgent ag in crowd.GetActiveAgents())
        {
            float height = ag.option.height;
            float radius = ag.option.radius;
            Vector3f pos = ag.npos;

            int col = DuRGBA(220, 220, 220, 128);
            if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                col = DuLerpCol(col, DuRGBA(128, 0, 255, 128), 32);
            else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                col = DuLerpCol(col, DuRGBA(128, 0, 255, 128), 128);
            else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                col = DuRGBA(255, 32, 16, 128);
            else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                col = DuLerpCol(col, DuRGBA(64, 255, 0, 128), 128);

            dd.DebugDrawCylinder(pos.x - radius, pos.y + radius * 0.1f, pos.z - radius, pos.x + radius, pos.y + height,
                pos.z + radius, col);
        }

        if (toolParams.m_showVO)
        {
            foreach (CrowdAgent ag in crowd.GetActiveAgents())
            {
                if (toolParams.m_showDetailAll == false && ag != m_agentDebug.agent)
                    continue;

                // Draw detail about agent sela
                ObstacleAvoidanceDebugData vod = m_agentDebug.vod;

                float dx = ag.npos.x;
                float dy = ag.npos.y + ag.option.height;
                float dz = ag.npos.z;

                dd.DebugDrawCircle(dx, dy, dz, ag.option.maxSpeed, DuRGBA(255, 255, 255, 64), 2.0f);

                dd.Begin(QUADS);
                for (int j = 0; j < vod.GetSampleCount(); ++j)
                {
                    Vector3f p = vod.GetSampleVelocity(j);
                    float sr = vod.GetSampleSize(j);
                    float pen = vod.GetSamplePenalty(j);
                    float pen2 = vod.GetSamplePreferredSidePenalty(j);
                    int col = DuLerpCol(DuRGBA(255, 255, 255, 220), DuRGBA(128, 96, 0, 220), (int)(pen * 255));
                    col = DuLerpCol(col, DuRGBA(128, 0, 0, 220), (int)(pen2 * 128));
                    dd.Vertex(dx + p.x - sr, dy, dz + p.z - sr, col);
                    dd.Vertex(dx + p.x - sr, dy, dz + p.z + sr, col);
                    dd.Vertex(dx + p.x + sr, dy, dz + p.z + sr, col);
                    dd.Vertex(dx + p.x + sr, dy, dz + p.z - sr, col);
                }

                dd.End();
            }
        }

        // Velocity stuff.
        foreach (CrowdAgent ag in crowd.GetActiveAgents())
        {
            float radius = ag.option.radius;
            float height = ag.option.height;
            Vector3f pos = ag.npos;
            Vector3f vel = ag.vel;
            Vector3f dvel = ag.dvel;

            int col = DuRGBA(220, 220, 220, 192);
            if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                col = DuLerpCol(col, DuRGBA(128, 0, 255, 192), 48);
            else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                col = DuLerpCol(col, DuRGBA(128, 0, 255, 192), 128);
            else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                col = DuRGBA(255, 32, 16, 192);
            else if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                col = DuLerpCol(col, DuRGBA(64, 255, 0, 192), 128);

            dd.DebugDrawCircle(pos.x, pos.y + height, pos.z, radius, col, 2.0f);

            dd.DebugDrawArrow(pos.x, pos.y + height, pos.z, pos.x + dvel.x, pos.y + height + dvel.y, pos.z + dvel.z,
                0.0f, 0.4f, DuRGBA(0, 192, 255, 192), m_agentDebug.agent == ag ? 2.0f : 1.0f);

            dd.DebugDrawArrow(pos.x, pos.y + height, pos.z, pos.x + vel.x, pos.y + height + vel.y, pos.z + vel.z, 0.0f,
                0.4f, DuRGBA(0, 0, 0, 160), 2.0f);
        }

        dd.DepthMask(true);
    }

    public override void HandleUpdate(float dt)
    {
        UpdateTick(dt);
    }

    private void UpdateTick(float dt)
    {
        if (m_mode == CrowdToolMode.PROFILING)
        {
            profilingTool.Update(dt);
            return;
        }

        if (crowd == null)
            return;
        NavMesh nav = sample.GetNavMesh();
        if (nav == null)
            return;

        long startTime = RcFrequency.Ticks;
        crowd.Update(dt, m_agentDebug);
        long endTime = RcFrequency.Ticks;

        // Update agent trails
        foreach (CrowdAgent ag in crowd.GetActiveAgents())
        {
            AgentTrail trail = m_trails[ag.idx];
            // Update agent movement trail.
            trail.htrail = (trail.htrail + 1) % AGENT_MAX_TRAIL;
            trail.trail[trail.htrail * 3] = ag.npos.x;
            trail.trail[trail.htrail * 3 + 1] = ag.npos.y;
            trail.trail[trail.htrail * 3 + 2] = ag.npos.z;
        }

        m_agentDebug.vod.NormalizeSamples();

        // m_crowdSampleCount.addSample((float) crowd.GetVelocitySampleCount());
        crowdUpdateTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
    }

    private void HilightAgent(CrowdAgent agent)
    {
        m_agentDebug.agent = agent;
    }

    public override void Layout()
    {
        ImGui.Text($"Crowd Tool Mode");
        ImGui.Separator();
        CrowdToolMode previousToolMode = m_mode;
        ImGui.RadioButton(CrowdToolMode.CREATE.Label, ref m_modeIdx, CrowdToolMode.CREATE.Idx);
        ImGui.RadioButton(CrowdToolMode.MOVE_TARGET.Label, ref m_modeIdx, CrowdToolMode.MOVE_TARGET.Idx);
        ImGui.RadioButton(CrowdToolMode.SELECT.Label, ref m_modeIdx, CrowdToolMode.SELECT.Idx);
        ImGui.RadioButton(CrowdToolMode.TOGGLE_POLYS.Label, ref m_modeIdx, CrowdToolMode.TOGGLE_POLYS.Idx);
        ImGui.RadioButton(CrowdToolMode.PROFILING.Label, ref m_modeIdx, CrowdToolMode.PROFILING.Idx);
        ImGui.NewLine();

        if (previousToolMode.Idx != m_modeIdx)
        {
            m_mode = CrowdToolMode.Values[m_modeIdx];
        }

        ImGui.Text("Options");
        ImGui.Separator();
        bool m_optimizeVis = toolParams.m_optimizeVis;
        bool m_optimizeTopo = toolParams.m_optimizeTopo;
        bool m_anticipateTurns = toolParams.m_anticipateTurns;
        bool m_obstacleAvoidance = toolParams.m_obstacleAvoidance;
        bool m_separation = toolParams.m_separation;
        int m_obstacleAvoidanceType = toolParams.m_obstacleAvoidanceType;
        float m_separationWeight = toolParams.m_separationWeight;
        ImGui.Checkbox("Optimize Visibility", ref toolParams.m_optimizeVis);
        ImGui.Checkbox("Optimize Topology", ref toolParams.m_optimizeTopo);
        ImGui.Checkbox("Anticipate Turns", ref toolParams.m_anticipateTurns);
        ImGui.Checkbox("Obstacle Avoidance", ref toolParams.m_obstacleAvoidance);
        ImGui.SliderInt("Avoidance Quality", ref toolParams.m_obstacleAvoidanceType, 0, 3);
        ImGui.Checkbox("Separation", ref toolParams.m_separation);
        ImGui.SliderFloat("Separation Weight", ref toolParams.m_separationWeight, 0f, 20f, "%.2f");
        ImGui.NewLine();

        if (m_optimizeVis != toolParams.m_optimizeVis || m_optimizeTopo != toolParams.m_optimizeTopo
                                                      || m_anticipateTurns != toolParams.m_anticipateTurns || m_obstacleAvoidance != toolParams.m_obstacleAvoidance
                                                      || m_separation != toolParams.m_separation
                                                      || m_obstacleAvoidanceType != toolParams.m_obstacleAvoidanceType
                                                      || m_separationWeight != toolParams.m_separationWeight)
        {
            UpdateAgentParams();
        }


        if (m_mode == CrowdToolMode.PROFILING)
        {
            profilingTool.Layout();
        }

        if (m_mode != CrowdToolMode.PROFILING)
        {
            ImGui.Text("Selected Debug Draw");
            ImGui.Separator();
            ImGui.Checkbox("Show Corners", ref toolParams.m_showCorners);
            ImGui.Checkbox("Show Collision Segs", ref toolParams.m_showCollisionSegments);
            ImGui.Checkbox("Show Path", ref toolParams.m_showPath);
            ImGui.Checkbox("Show VO", ref toolParams.m_showVO);
            ImGui.Checkbox("Show Path Optimization", ref toolParams.m_showOpt);
            ImGui.Checkbox("Show Neighbours", ref toolParams.m_showNeis);
            ImGui.NewLine();

            ImGui.Text("Debug Draw");
            ImGui.Separator();
            ImGui.Checkbox("Show Prox Grid", ref toolParams.m_showGrid);
            ImGui.Checkbox("Show Nodes", ref toolParams.m_showNodes);
            ImGui.Text($"Update Time: {crowdUpdateTime} ms");
        }
    }

    private void UpdateAgentParams()
    {
        if (crowd == null)
        {
            return;
        }

        int updateFlags = GetUpdateFlags();
        profilingTool.UpdateAgentParams(updateFlags, toolParams.m_obstacleAvoidanceType, toolParams.m_separationWeight);
        foreach (CrowdAgent ag in crowd.GetActiveAgents())
        {
            CrowdAgentParams option = new CrowdAgentParams();
            option.radius = ag.option.radius;
            option.height = ag.option.height;
            option.maxAcceleration = ag.option.maxAcceleration;
            option.maxSpeed = ag.option.maxSpeed;
            option.collisionQueryRange = ag.option.collisionQueryRange;
            option.pathOptimizationRange = ag.option.pathOptimizationRange;
            option.obstacleAvoidanceType = ag.option.obstacleAvoidanceType;
            option.queryFilterType = ag.option.queryFilterType;
            option.userData = ag.option.userData;
            option.updateFlags = updateFlags;
            option.obstacleAvoidanceType = toolParams.m_obstacleAvoidanceType;
            option.separationWeight = toolParams.m_separationWeight;
            crowd.UpdateAgentParameters(ag, option);
        }
    }

    private int GetUpdateFlags()
    {
        int updateFlags = 0;
        if (toolParams.m_anticipateTurns)
        {
            updateFlags |= CrowdAgentParams.DT_CROWD_ANTICIPATE_TURNS;
        }

        if (toolParams.m_optimizeVis)
        {
            updateFlags |= CrowdAgentParams.DT_CROWD_OPTIMIZE_VIS;
        }

        if (toolParams.m_optimizeTopo)
        {
            updateFlags |= CrowdAgentParams.DT_CROWD_OPTIMIZE_TOPO;
        }

        if (toolParams.m_obstacleAvoidance)
        {
            updateFlags |= CrowdAgentParams.DT_CROWD_OBSTACLE_AVOIDANCE;
        }

        if (toolParams.m_separation)
        {
            updateFlags |= CrowdAgentParams.DT_CROWD_SEPARATION;
        }

        return updateFlags;
    }

    public override string GetName()
    {
        return "Crowd";
    }
}