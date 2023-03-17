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
using System.Diagnostics;
using Silk.NET.Windowing;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using DotRecast.Detour.Crowd.Tracking;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;
using ImGuiNET;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class CrowdTool : Tool
{
    private enum ToolMode
    {
        CREATE,
        MOVE_TARGET,
        SELECT,
        TOGGLE_POLYS,
        PROFILING
    }

    private readonly CrowdToolParams toolParams = new CrowdToolParams();
    private Sample sample;
    private NavMesh m_nav;
    private Crowd crowd;
    private readonly CrowdProfilingTool profilingTool;
    private readonly CrowdAgentDebugInfo m_agentDebug = new CrowdAgentDebugInfo();

    private static readonly int AGENT_MAX_TRAIL = 64;

    private class AgentTrail
    {
        public float[] trail = new float[AGENT_MAX_TRAIL * 3];
        public int htrail;
    };

    private readonly Dictionary<long, AgentTrail> m_trails = new();
    private float[] m_targetPos;
    private long m_targetRef;
    private ToolMode m_mode = ToolMode.CREATE;
    private long crowdUpdateTime;

    public CrowdTool()
    {
        m_agentDebug.vod = new ObstacleAvoidanceDebugData(2048);
        profilingTool = new CrowdProfilingTool(getAgentParams);
    }

    public override void setSample(Sample psample)
    {
        if (sample != psample)
        {
            sample = psample;
        }

        NavMesh nav = sample.getNavMesh();

        if (nav != null && m_nav != nav)
        {
            m_nav = nav;

            CrowdConfig config = new CrowdConfig(sample.getSettingsUI().getAgentRadius());

            crowd = new Crowd(config, nav, __ => new DefaultQueryFilter(SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
                SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED, new float[] { 1f, 10f, 1f, 1f, 2f, 1.5f }));

            // Setup local avoidance option to different qualities.
            // Use mostly default settings, copy from dtCrowd.
            ObstacleAvoidanceQuery.ObstacleAvoidanceParams option = new ObstacleAvoidanceQuery.ObstacleAvoidanceParams(crowd.getObstacleAvoidanceParams(0));

            // Low (11)
            option.velBias = 0.5f;
            option.adaptiveDivs = 5;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 1;
            crowd.setObstacleAvoidanceParams(0, option);

            // Medium (22)
            option.velBias = 0.5f;
            option.adaptiveDivs = 5;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 2;
            crowd.setObstacleAvoidanceParams(1, option);

            // Good (45)
            option.velBias = 0.5f;
            option.adaptiveDivs = 7;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 3;
            crowd.setObstacleAvoidanceParams(2, option);

            // High (66)
            option.velBias = 0.5f;
            option.adaptiveDivs = 7;
            option.adaptiveRings = 3;
            option.adaptiveDepth = 3;

            crowd.setObstacleAvoidanceParams(3, option);

            profilingTool.setup(sample.getSettingsUI().getAgentRadius(), m_nav);
        }
    }

    public override void handleClick(float[] s, float[] p, bool shift)
    {
        if (m_mode == ToolMode.PROFILING)
        {
            return;
        }

        if (crowd == null)
        {
            return;
        }

        if (m_mode == ToolMode.CREATE)
        {
            if (shift)
            {
                // Delete
                CrowdAgent ahit = hitTestAgents(s, p);
                if (ahit != null)
                {
                    removeAgent(ahit);
                }
            }
            else
            {
                // Add
                addAgent(p);
            }
        }
        else if (m_mode == ToolMode.MOVE_TARGET)
        {
            setMoveTarget(p, shift);
        }
        else if (m_mode == ToolMode.SELECT)
        {
            // Highlight
            CrowdAgent ahit = hitTestAgents(s, p);
            hilightAgent(ahit);
        }
        else if (m_mode == ToolMode.TOGGLE_POLYS)
        {
            NavMesh nav = sample.getNavMesh();
            NavMeshQuery navquery = sample.getNavMeshQuery();
            if (nav != null && navquery != null)
            {
                QueryFilter filter = new DefaultQueryFilter();
                float[] halfExtents = crowd.getQueryExtents();
                Result<FindNearestPolyResult> result = navquery.findNearestPoly(p, halfExtents, filter);
                long refs = result.result.getNearestRef();
                if (refs != 0)
                {
                    Result<int> flags = nav.getPolyFlags(refs);
                    if (flags.succeeded())
                    {
                        nav.setPolyFlags(refs, flags.result ^ SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED);
                    }
                }
            }
        }
    }

    private void removeAgent(CrowdAgent agent)
    {
        crowd.removeAgent(agent);
        if (agent == m_agentDebug.agent)
        {
            m_agentDebug.agent = null;
        }
    }

    private void addAgent(float[] p)
    {
        CrowdAgentParams ap = getAgentParams();
        CrowdAgent ag = crowd.addAgent(p, ap);
        if (ag != null)
        {
            if (m_targetRef != 0)
                crowd.requestMoveTarget(ag, m_targetRef, m_targetPos);

            // Init trail
            if (!m_trails.TryGetValue(ag.idx, out var trail))
            {
                trail = new AgentTrail();
                m_trails.Add(ag.idx, trail);
            }

            for (int i = 0; i < AGENT_MAX_TRAIL; ++i)
            {
                trail.trail[i * 3] = p[0];
                trail.trail[i * 3 + 1] = p[1];
                trail.trail[i * 3 + 2] = p[2];
            }

            trail.htrail = 0;
        }
    }

    private CrowdAgentParams getAgentParams()
    {
        CrowdAgentParams ap = new CrowdAgentParams();
        ap.radius = sample.getSettingsUI().getAgentRadius();
        ap.height = sample.getSettingsUI().getAgentHeight();
        ap.maxAcceleration = 8.0f;
        ap.maxSpeed = 3.5f;
        ap.collisionQueryRange = ap.radius * 12.0f;
        ap.pathOptimizationRange = ap.radius * 30.0f;
        ap.updateFlags = getUpdateFlags();
        ap.obstacleAvoidanceType = toolParams.m_obstacleAvoidanceType;
        ap.separationWeight = toolParams.m_separationWeight;
        return ap;
    }

    private CrowdAgent hitTestAgents(float[] s, float[] p)
    {
        CrowdAgent isel = null;
        float tsel = float.MaxValue;

        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            float[] bmin = new float[3], bmax = new float[3];
            getAgentBounds(ag, bmin, bmax);
            float[] isect = Intersections.intersectSegmentAABB(s, p, bmin, bmax);
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

    private void getAgentBounds(CrowdAgent ag, float[] bmin, float[] bmax)
    {
        float[] p = ag.npos;
        float r = ag.option.radius;
        float h = ag.option.height;
        bmin[0] = p[0] - r;
        bmin[1] = p[1];
        bmin[2] = p[2] - r;
        bmax[0] = p[0] + r;
        bmax[1] = p[1] + h;
        bmax[2] = p[2] + r;
    }

    private void setMoveTarget(float[] p, bool adjust)
    {
        if (sample == null || crowd == null)
            return;

        // Find nearest point on navmesh and set move request to that location.
        NavMeshQuery navquery = sample.getNavMeshQuery();
        QueryFilter filter = crowd.getFilter(0);
        float[] halfExtents = crowd.getQueryExtents();

        if (adjust)
        {
            // Request velocity
            if (m_agentDebug.agent != null)
            {
                float[] vel = calcVel(m_agentDebug.agent.npos, p, m_agentDebug.agent.option.maxSpeed);
                crowd.requestMoveVelocity(m_agentDebug.agent, vel);
            }
            else
            {
                foreach (CrowdAgent ag in crowd.getActiveAgents())
                {
                    float[] vel = calcVel(ag.npos, p, ag.option.maxSpeed);
                    crowd.requestMoveVelocity(ag, vel);
                }
            }
        }
        else
        {
            Result<FindNearestPolyResult> result = navquery.findNearestPoly(p, halfExtents, filter);
            m_targetRef = result.result.getNearestRef();
            m_targetPos = result.result.getNearestPos();
            if (m_agentDebug.agent != null)
            {
                crowd.requestMoveTarget(m_agentDebug.agent, m_targetRef, m_targetPos);
            }
            else
            {
                foreach (CrowdAgent ag in crowd.getActiveAgents())
                {
                    crowd.requestMoveTarget(ag, m_targetRef, m_targetPos);
                }
            }
        }
    }

    private float[] calcVel(float[] pos, float[] tgt, float speed)
    {
        float[] vel = DetourCommon.vSub(tgt, pos);
        vel[1] = 0.0f;
        DetourCommon.vNormalize(vel);
        return DetourCommon.vScale(vel, speed);
    }

    public override void handleRender(NavMeshRenderer renderer)
    {
        if (m_mode == ToolMode.PROFILING)
        {
            profilingTool.handleRender(renderer);
            return;
        }

        RecastDebugDraw dd = renderer.getDebugDraw();
        float rad = sample.getSettingsUI().getAgentRadius();
        NavMesh nav = sample.getNavMesh();
        if (nav == null || crowd == null)
            return;

        if (toolParams.m_showNodes && crowd.getPathQueue() != null)
        {
//            NavMeshQuery navquery = crowd.getPathQueue().getNavQuery();
//            if (navquery != null) {
//                dd.debugDrawNavMeshNodes(navquery);
//            }
        }

        dd.depthMask(false);

        // Draw paths
        if (toolParams.m_showPath)
        {
            foreach (CrowdAgent ag in crowd.getActiveAgents())
            {
                if (!toolParams.m_showDetailAll && ag != m_agentDebug.agent)
                    continue;
                List<long> path = ag.corridor.getPath();
                int npath = ag.corridor.getPathCount();
                for (int j = 0; j < npath; ++j)
                {
                    dd.debugDrawNavMeshPoly(nav, path[j], duRGBA(255, 255, 255, 24));
                }
            }
        }

        if (m_targetRef != 0)
            dd.debugDrawCross(m_targetPos[0], m_targetPos[1] + 0.1f, m_targetPos[2], rad, duRGBA(255, 255, 255, 192), 2.0f);

        // Occupancy grid.
        if (toolParams.m_showGrid)
        {
            float gridy = -float.MaxValue;
            foreach (CrowdAgent ag in crowd.getActiveAgents())
            {
                float[] pos = ag.corridor.getPos();
                gridy = Math.Max(gridy, pos[1]);
            }

            gridy += 1.0f;

            dd.begin(QUADS);
            ProximityGrid grid = crowd.getGrid();
            float cs = grid.getCellSize();
            foreach (int[] ic in grid.getItemCounts())
            {
                int x = ic[0];
                int y = ic[1];
                int count = ic[2];
                if (count != 0)
                {
                    int col = duRGBA(128, 0, 0, Math.Min(count * 40, 255));
                    dd.vertex(x * cs, gridy, y * cs, col);
                    dd.vertex(x * cs, gridy, y * cs + cs, col);
                    dd.vertex(x * cs + cs, gridy, y * cs + cs, col);
                    dd.vertex(x * cs + cs, gridy, y * cs, col);
                }
            }

            dd.end();
        }

        // Trail
        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            AgentTrail trail = m_trails[ag.idx];
            float[] pos = ag.npos;

            dd.begin(LINES, 3.0f);
            float[] prev = new float[3];
            float preva = 1;
            DetourCommon.vCopy(prev, pos);
            for (int j = 0; j < AGENT_MAX_TRAIL - 1; ++j)
            {
                int idx = (trail.htrail + AGENT_MAX_TRAIL - j) % AGENT_MAX_TRAIL;
                int v = idx * 3;
                float a = 1 - j / (float)AGENT_MAX_TRAIL;
                dd.vertex(prev[0], prev[1] + 0.1f, prev[2], duRGBA(0, 0, 0, (int)(128 * preva)));
                dd.vertex(trail.trail[v], trail.trail[v + 1] + 0.1f, trail.trail[v + 2], duRGBA(0, 0, 0, (int)(128 * a)));
                preva = a;
                DetourCommon.vCopy(prev, trail.trail, v);
            }

            dd.end();
        }

        // Corners & co
        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            if (toolParams.m_showDetailAll == false && ag != m_agentDebug.agent)
                continue;

            float radius = ag.option.radius;
            float[] pos = ag.npos;

            if (toolParams.m_showCorners)
            {
                if (0 < ag.corners.Count)
                {
                    dd.begin(LINES, 2.0f);
                    for (int j = 0; j < ag.corners.Count; ++j)
                    {
                        float[] va = j == 0 ? pos : ag.corners[j - 1].getPos();
                        float[] vb = ag.corners[j].getPos();
                        dd.vertex(va[0], va[1] + radius, va[2], duRGBA(128, 0, 0, 192));
                        dd.vertex(vb[0], vb[1] + radius, vb[2], duRGBA(128, 0, 0, 192));
                    }

                    if ((ag.corners[ag.corners.Count - 1].getFlags()
                         & NavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        float[] v = ag.corners[ag.corners.Count - 1].getPos();
                        dd.vertex(v[0], v[1], v[2], duRGBA(192, 0, 0, 192));
                        dd.vertex(v[0], v[1] + radius * 2, v[2], duRGBA(192, 0, 0, 192));
                    }

                    dd.end();

                    if (toolParams.m_anticipateTurns)
                    {
                        /*                  float dvel[3], pos[3];
                         calcSmoothSteerDirection(ag.pos, ag.cornerVerts, ag.ncorners, dvel);
                         pos[0] = ag.pos[0] + dvel[0];
                         pos[1] = ag.pos[1] + dvel[1];
                         pos[2] = ag.pos[2] + dvel[2];

                         float off = ag.radius+0.1f;
                         float[] tgt = &ag.cornerVerts[0];
                         float y = ag.pos[1]+off;

                         dd.begin(DU_DRAW_LINES, 2.0f);

                         dd.vertex(ag.pos[0],y,ag.pos[2], duRGBA(255,0,0,192));
                         dd.vertex(pos[0],y,pos[2], duRGBA(255,0,0,192));

                         dd.vertex(pos[0],y,pos[2], duRGBA(255,0,0,192));
                         dd.vertex(tgt[0],y,tgt[2], duRGBA(255,0,0,192));

                         dd.end();*/
                    }
                }
            }

            if (toolParams.m_showCollisionSegments)
            {
                float[] center = ag.boundary.getCenter();
                dd.debugDrawCross(center[0], center[1] + radius, center[2], 0.2f, duRGBA(192, 0, 128, 255), 2.0f);
                dd.debugDrawCircle(center[0], center[1] + radius, center[2], ag.option.collisionQueryRange,
                    duRGBA(192, 0, 128, 128), 2.0f);

                dd.begin(LINES, 3.0f);
                for (int j = 0; j < ag.boundary.getSegmentCount(); ++j)
                {
                    int col = duRGBA(192, 0, 128, 192);
                    float[] s = ag.boundary.getSegment(j);
                    float[] s0 = new float[] { s[0], s[1], s[2] };
                    float[] s3 = new float[] { s[3], s[4], s[5] };
                    if (DetourCommon.triArea2D(pos, s0, s3) < 0.0f)
                        col = duDarkenCol(col);

                    dd.appendArrow(s[0], s[1] + 0.2f, s[2], s[3], s[4] + 0.2f, s[5], 0.0f, 0.3f, col);
                }

                dd.end();
            }

            if (toolParams.m_showNeis)
            {
                dd.debugDrawCircle(pos[0], pos[1] + radius, pos[2], ag.option.collisionQueryRange, duRGBA(0, 192, 128, 128),
                    2.0f);

                dd.begin(LINES, 2.0f);
                for (int j = 0; j < ag.neis.Count; ++j)
                {
                    CrowdAgent nei = ag.neis[j].agent;
                    if (nei != null)
                    {
                        dd.vertex(pos[0], pos[1] + radius, pos[2], duRGBA(0, 192, 128, 128));
                        dd.vertex(nei.npos[0], nei.npos[1] + radius, nei.npos[2], duRGBA(0, 192, 128, 128));
                    }
                }

                dd.end();
            }

            if (toolParams.m_showOpt)
            {
                dd.begin(LINES, 2.0f);
                dd.vertex(m_agentDebug.optStart[0], m_agentDebug.optStart[1] + 0.3f, m_agentDebug.optStart[2],
                    duRGBA(0, 128, 0, 192));
                dd.vertex(m_agentDebug.optEnd[0], m_agentDebug.optEnd[1] + 0.3f, m_agentDebug.optEnd[2], duRGBA(0, 128, 0, 192));
                dd.end();
            }
        }

        // Agent cylinders.
        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            float radius = ag.option.radius;
            float[] pos = ag.npos;

            int col = duRGBA(0, 0, 0, 32);
            if (m_agentDebug.agent == ag)
                col = duRGBA(255, 0, 0, 128);

            dd.debugDrawCircle(pos[0], pos[1], pos[2], radius, col, 2.0f);
        }

        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            float height = ag.option.height;
            float radius = ag.option.radius;
            float[] pos = ag.npos;

            int col = duRGBA(220, 220, 220, 128);
            if (ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                || ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                col = duLerpCol(col, duRGBA(128, 0, 255, 128), 32);
            else if (ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                col = duLerpCol(col, duRGBA(128, 0, 255, 128), 128);
            else if (ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                col = duRGBA(255, 32, 16, 128);
            else if (ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                col = duLerpCol(col, duRGBA(64, 255, 0, 128), 128);

            dd.debugDrawCylinder(pos[0] - radius, pos[1] + radius * 0.1f, pos[2] - radius, pos[0] + radius, pos[1] + height,
                pos[2] + radius, col);
        }

        if (toolParams.m_showVO)
        {
            foreach (CrowdAgent ag in crowd.getActiveAgents())
            {
                if (toolParams.m_showDetailAll == false && ag != m_agentDebug.agent)
                    continue;

                // Draw detail about agent sela
                ObstacleAvoidanceDebugData vod = m_agentDebug.vod;

                float dx = ag.npos[0];
                float dy = ag.npos[1] + ag.option.height;
                float dz = ag.npos[2];

                dd.debugDrawCircle(dx, dy, dz, ag.option.maxSpeed, duRGBA(255, 255, 255, 64), 2.0f);

                dd.begin(QUADS);
                for (int j = 0; j < vod.getSampleCount(); ++j)
                {
                    float[] p = vod.getSampleVelocity(j);
                    float sr = vod.getSampleSize(j);
                    float pen = vod.getSamplePenalty(j);
                    float pen2 = vod.getSamplePreferredSidePenalty(j);
                    int col = duLerpCol(duRGBA(255, 255, 255, 220), duRGBA(128, 96, 0, 220), (int)(pen * 255));
                    col = duLerpCol(col, duRGBA(128, 0, 0, 220), (int)(pen2 * 128));
                    dd.vertex(dx + p[0] - sr, dy, dz + p[2] - sr, col);
                    dd.vertex(dx + p[0] - sr, dy, dz + p[2] + sr, col);
                    dd.vertex(dx + p[0] + sr, dy, dz + p[2] + sr, col);
                    dd.vertex(dx + p[0] + sr, dy, dz + p[2] - sr, col);
                }

                dd.end();
            }
        }

        // Velocity stuff.
        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            float radius = ag.option.radius;
            float height = ag.option.height;
            float[] pos = ag.npos;
            float[] vel = ag.vel;
            float[] dvel = ag.dvel;

            int col = duRGBA(220, 220, 220, 192);
            if (ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING
                || ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                col = duLerpCol(col, duRGBA(128, 0, 255, 192), 48);
            else if (ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                col = duLerpCol(col, duRGBA(128, 0, 255, 192), 128);
            else if (ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                col = duRGBA(255, 32, 16, 192);
            else if (ag.targetState == CrowdAgent.MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                col = duLerpCol(col, duRGBA(64, 255, 0, 192), 128);

            dd.debugDrawCircle(pos[0], pos[1] + height, pos[2], radius, col, 2.0f);

            dd.debugDrawArrow(pos[0], pos[1] + height, pos[2], pos[0] + dvel[0], pos[1] + height + dvel[1], pos[2] + dvel[2],
                0.0f, 0.4f, duRGBA(0, 192, 255, 192), m_agentDebug.agent == ag ? 2.0f : 1.0f);

            dd.debugDrawArrow(pos[0], pos[1] + height, pos[2], pos[0] + vel[0], pos[1] + height + vel[1], pos[2] + vel[2], 0.0f,
                0.4f, duRGBA(0, 0, 0, 160), 2.0f);
        }

        dd.depthMask(true);
    }

    public override void handleUpdate(float dt)
    {
        updateTick(dt);
    }

    private void updateTick(float dt)
    {
        if (m_mode == ToolMode.PROFILING)
        {
            profilingTool.update(dt);
            return;
        }

        if (crowd == null)
            return;
        NavMesh nav = sample.getNavMesh();
        if (nav == null)
            return;

        long startTime = Stopwatch.GetTimestamp();
        crowd.update(dt, m_agentDebug);
        long endTime = Stopwatch.GetTimestamp();

        // Update agent trails
        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            AgentTrail trail = m_trails[ag.idx];
            // Update agent movement trail.
            trail.htrail = (trail.htrail + 1) % AGENT_MAX_TRAIL;
            trail.trail[trail.htrail * 3] = ag.npos[0];
            trail.trail[trail.htrail * 3 + 1] = ag.npos[1];
            trail.trail[trail.htrail * 3 + 2] = ag.npos[2];
        }

        m_agentDebug.vod.normalizeSamples();

        // m_crowdSampleCount.addSample((float) crowd.getVelocitySampleCount());
        crowdUpdateTime = (endTime - startTime) / 1_000_000;
    }

    private void hilightAgent(CrowdAgent agent)
    {
        m_agentDebug.agent = agent;
    }

    public override void layout(IWindow ctx)
    {
        // ToolMode previousToolMode = m_mode;
        // nk_layout_row_dynamic(ctx, 20, 1);
        // if (nk_option_label(ctx, "Create Agents", m_mode == ToolMode.CREATE)) {
        //     m_mode = ToolMode.CREATE;
        // }
        // nk_layout_row_dynamic(ctx, 20, 1);
        // if (nk_option_label(ctx, "Move Target", m_mode == ToolMode.MOVE_TARGET)) {
        //     m_mode = ToolMode.MOVE_TARGET;
        // }
        // nk_layout_row_dynamic(ctx, 20, 1);
        // if (nk_option_label(ctx, "Select Agent", m_mode == ToolMode.SELECT)) {
        //     m_mode = ToolMode.SELECT;
        // }
        // nk_layout_row_dynamic(ctx, 20, 1);
        // if (nk_option_label(ctx, "Toggle Polys", m_mode == ToolMode.TOGGLE_POLYS)) {
        //     m_mode = ToolMode.TOGGLE_POLYS;
        // }
        // nk_layout_row_dynamic(ctx, 20, 1);
        // if (nk_option_label(ctx, "Profiling", m_mode == ToolMode.PROFILING)) {
        //     m_mode = ToolMode.PROFILING;
        // }
        // nk_layout_row_dynamic(ctx, 1, 1);
        // nk_spacing(ctx, 1);
        // if (nk_tree_state_push(ctx, 0, "Options", toolParams.m_expandOptions)) {
        //     bool m_optimizeVis = toolParams.m_optimizeVis;
        //     bool m_optimizeTopo = toolParams.m_optimizeTopo;
        //     bool m_anticipateTurns = toolParams.m_anticipateTurns;
        //     bool m_obstacleAvoidance = toolParams.m_obstacleAvoidance;
        //     bool m_separation = toolParams.m_separation;
        //     int m_obstacleAvoidanceType = toolParams.m_obstacleAvoidanceType[0];
        //     float m_separationWeight = toolParams.m_separationWeight[0];
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     toolParams.m_optimizeVis = nk_option_text(ctx, "Optimize Visibility", toolParams.m_optimizeVis);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     toolParams.m_optimizeTopo = nk_option_text(ctx, "Optimize Topology", toolParams.m_optimizeTopo);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     toolParams.m_anticipateTurns = nk_option_text(ctx, "Anticipate Turns", toolParams.m_anticipateTurns);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     toolParams.m_obstacleAvoidance = nk_option_text(ctx, "Obstacle Avoidance", toolParams.m_obstacleAvoidance);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderInt("Avoidance Quality", ref toolParams.m_obstacleAvoidanceType, 0, 3);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     toolParams.m_separation = nk_option_text(ctx, "Separation", toolParams.m_separation);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Separation Weight", ref toolParams.m_separationWeight, 0f, 20f, "%.2f");
        //     if (m_optimizeVis != toolParams.m_optimizeVis || m_optimizeTopo != toolParams.m_optimizeTopo
        //             || m_anticipateTurns != toolParams.m_anticipateTurns || m_obstacleAvoidance != toolParams.m_obstacleAvoidance
        //             || m_separation != toolParams.m_separation
        //             || m_obstacleAvoidanceType != toolParams.m_obstacleAvoidanceType[0]
        //             || m_separationWeight != toolParams.m_separationWeight[0]) {
        //         updateAgentParams();
        //     }
        //     nk_tree_state_pop(ctx);
        // }
        // if (m_mode == ToolMode.PROFILING) {
        //     profilingTool.layout(ctx);
        // }
        // if (m_mode != ToolMode.PROFILING) {
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     if (nk_tree_state_push(ctx, 0, "Selected Debug Draw", toolParams.m_expandSelectedDebugDraw)) {
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         toolParams.m_showCorners = nk_option_text(ctx, "Show Corners", toolParams.m_showCorners);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         toolParams.m_showCollisionSegments = nk_option_text(ctx, "Show Collision Segs", toolParams.m_showCollisionSegments);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         toolParams.m_showPath = nk_option_text(ctx, "Show Path", toolParams.m_showPath);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         toolParams.m_showVO = nk_option_text(ctx, "Show VO", toolParams.m_showVO);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         toolParams.m_showOpt = nk_option_text(ctx, "Show Path Optimization", toolParams.m_showOpt);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         toolParams.m_showNeis = nk_option_text(ctx, "Show Neighbours", toolParams.m_showNeis);
        //         nk_tree_state_pop(ctx);
        //     }
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     if (nk_tree_state_push(ctx, 0, "Debug Draw", toolParams.m_expandDebugDraw)) {
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         toolParams.m_showGrid = nk_option_text(ctx, "Show Prox Grid", toolParams.m_showGrid);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         toolParams.m_showNodes = nk_option_text(ctx, "Show Nodes", toolParams.m_showNodes);
        //         nk_tree_state_pop(ctx);
        //     }
        //     nk_layout_row_dynamic(ctx, 2, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     nk_label(ctx, string.format("Update Time: %d ms", crowdUpdateTime), NK_TEXT_ALIGN_LEFT);
        // }
    }

    private void updateAgentParams()
    {
        if (crowd == null)
        {
            return;
        }

        int updateFlags = getUpdateFlags();
        profilingTool.updateAgentParams(updateFlags, toolParams.m_obstacleAvoidanceType, toolParams.m_separationWeight);
        foreach (CrowdAgent ag in crowd.getActiveAgents())
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
            crowd.updateAgentParameters(ag, option);
        }
    }

    private int getUpdateFlags()
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

    public override string getName()
    {
        return "Crowd";
    }
}