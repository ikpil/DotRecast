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
using static DotRecast.Core.RecastMath;

namespace DotRecast.Recast.Demo.Tools;

public class CrowdToolMode
{
    public static readonly CrowdToolMode CREATE = new(0, "Create Agents");
    public static readonly CrowdToolMode MOVE_TARGET = new(1, "Move Target");
    public static readonly CrowdToolMode SELECT = new(2, "Select Agent");
    public static readonly CrowdToolMode TOGGLE_POLYS = new(3, "Toggle Polys");
    public static readonly CrowdToolMode PROFILING = new(4, "Profiling");

    public static readonly ImmutableArray<CrowdToolMode> Values = ImmutableArray.Create(
        CREATE,
        MOVE_TARGET,
        SELECT,
        TOGGLE_POLYS,
        PROFILING
    );

    public int Idx { get; }
    public string Label { get; }

    private CrowdToolMode(int idx, string label)
    {
        Idx = idx;
        Label = label;
    }
}

public class CrowdTool : Tool
{
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
    private Vector3f m_targetPos;
    private long m_targetRef;
    private CrowdToolMode m_mode = CrowdToolMode.CREATE;
    private int m_modeIdx = CrowdToolMode.CREATE.Idx;
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

    public override void handleClick(float[] s, Vector3f p, bool shift)
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
        else if (m_mode == CrowdToolMode.MOVE_TARGET)
        {
            setMoveTarget(p, shift);
        }
        else if (m_mode == CrowdToolMode.SELECT)
        {
            // Highlight
            CrowdAgent ahit = hitTestAgents(s, p);
            hilightAgent(ahit);
        }
        else if (m_mode == CrowdToolMode.TOGGLE_POLYS)
        {
            NavMesh nav = sample.getNavMesh();
            NavMeshQuery navquery = sample.getNavMeshQuery();
            if (nav != null && navquery != null)
            {
                QueryFilter filter = new DefaultQueryFilter();
                Vector3f halfExtents = crowd.getQueryExtents();
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

    private void addAgent(Vector3f p)
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

    private CrowdAgent hitTestAgents(float[] s, Vector3f p)
    {
        CrowdAgent isel = null;
        float tsel = float.MaxValue;

        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            Vector3f bmin = new Vector3f();
            Vector3f bmax = new Vector3f();
            getAgentBounds(ag, ref bmin, ref bmax);
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

    private void getAgentBounds(CrowdAgent ag, ref Vector3f bmin, ref Vector3f bmax)
    {
        Vector3f p = ag.npos;
        float r = ag.option.radius;
        float h = ag.option.height;
        bmin[0] = p[0] - r;
        bmin[1] = p[1];
        bmin[2] = p[2] - r;
        bmax[0] = p[0] + r;
        bmax[1] = p[1] + h;
        bmax[2] = p[2] + r;
    }

    private void setMoveTarget(Vector3f p, bool adjust)
    {
        if (sample == null || crowd == null)
            return;

        // Find nearest point on navmesh and set move request to that location.
        NavMeshQuery navquery = sample.getNavMeshQuery();
        QueryFilter filter = crowd.getFilter(0);
        Vector3f halfExtents = crowd.getQueryExtents();

        if (adjust)
        {
            // Request velocity
            if (m_agentDebug.agent != null)
            {
                float[] vel = calcVel(ref m_agentDebug.agent.npos, p, m_agentDebug.agent.option.maxSpeed);
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

    private Vector3f calcVel(Vector3f pos, Vector3f tgt, float speed)
    {
        Vector3f vel = vSub(tgt, pos);
        vel[1] = 0.0f;
        vNormalize(ref vel);
        return vScale(vel, speed);
    }

    public override void handleRender(NavMeshRenderer renderer)
    {
        if (m_mode == CrowdToolMode.PROFILING)
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
            Vector3f prev = new Vector3f();
            float preva = 1;
            vCopy(prev, pos);
            for (int j = 0; j < AGENT_MAX_TRAIL - 1; ++j)
            {
                int idx = (trail.htrail + AGENT_MAX_TRAIL - j) % AGENT_MAX_TRAIL;
                int v = idx * 3;
                float a = 1 - j / (float)AGENT_MAX_TRAIL;
                dd.vertex(prev[0], prev[1] + 0.1f, prev[2], duRGBA(0, 0, 0, (int)(128 * preva)));
                dd.vertex(trail.trail[v], trail.trail[v + 1] + 0.1f, trail.trail[v + 2], duRGBA(0, 0, 0, (int)(128 * a)));
                preva = a;
                vCopy(prev, trail.trail, v);
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
                    Vector3f[] s = ag.boundary.getSegment(j);
                    Vector3f s0 = s[0];
                    Vector3f s3 = s[1];
                    if (triArea2D(pos, s0, s3) < 0.0f)
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
            Vector3f pos = ag.npos;

            int col = duRGBA(0, 0, 0, 32);
            if (m_agentDebug.agent == ag)
                col = duRGBA(255, 0, 0, 128);

            dd.debugDrawCircle(pos[0], pos[1], pos[2], radius, col, 2.0f);
        }

        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            float height = ag.option.height;
            float radius = ag.option.radius;
            Vector3f pos = ag.npos;

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
                    Vector3f p = vod.getSampleVelocity(j);
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
            Vector3f pos = ag.npos;
            Vector3f vel = ag.vel;
            Vector3f dvel = ag.dvel;

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
        if (m_mode == CrowdToolMode.PROFILING)
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
        crowdUpdateTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
    }

    private void hilightAgent(CrowdAgent agent)
    {
        m_agentDebug.agent = agent;
    }

    public override void layout()
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
            updateAgentParams();
        }


        if (m_mode == CrowdToolMode.PROFILING)
        {
            profilingTool.layout();
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