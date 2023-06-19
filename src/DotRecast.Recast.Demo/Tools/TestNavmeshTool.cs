using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.QueryResults;
using DotRecast.Recast.DemoTool.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.DemoTool;
using DotRecast.Recast.DemoTool.Tools;
using ImGuiNET;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class TestNavmeshTool : IRcTool
{
    private const int MAX_POLYS = 256;
    private const int MAX_SMOOTH = 2048;

    private readonly TestNavmeshToolImpl _impl;

    private int m_toolModeIdx = TestNavmeshToolMode.PATHFIND_FOLLOW.Idx;
    private TestNavmeshToolMode m_toolMode => TestNavmeshToolMode.Values[m_toolModeIdx];
    private bool m_sposSet;
    private bool m_eposSet;
    private RcVec3f m_spos;
    private RcVec3f m_epos;
    private readonly DtQueryDefaultFilter m_filter;
    private readonly RcVec3f m_polyPickExt = RcVec3f.Of(2, 4, 2);
    private long m_startRef;
    private long m_endRef;
    private RcVec3f m_hitPos;
    private float m_distanceToWall;
    private RcVec3f m_hitNormal;
    private List<StraightPathItem> m_straightPath;
    private int m_straightPathOptions;
    private List<long> m_polys;
    private bool m_hitResult;
    private List<long> m_parent;
    private float m_neighbourhoodRadius;
    private readonly float[] m_queryPoly = new float[12];
    private List<RcVec3f> m_smoothPath;
    private DtStatus m_pathFindStatus = DtStatus.DT_FAILURE;
    private bool enableRaycast = true;
    private readonly List<RcVec3f> randomPoints = new();
    private bool constrainByCircle;

    private int includeFlags = SampleAreaModifications.SAMPLE_POLYFLAGS_ALL;
    private int excludeFlags = 0;

    public TestNavmeshTool()
    {
        _impl = new();
        m_filter = new DtQueryDefaultFilter(
            SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
            SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED,
            new float[] { 1f, 1f, 1f, 1f, 2f, 1.5f }
        );
    }

    public ISampleTool GetTool()
    {
        return _impl;
    }

    public void OnSampleChanged()
    {
        // ..
    }


    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        if (shift)
        {
            m_sposSet = true;
            m_spos = p;
        }
        else
        {
            m_eposSet = true;
            m_epos = p;
        }

        Recalc();
    }

    public void Layout()
    {
        var previousToolMode = m_toolMode;
        int previousStraightPathOptions = m_straightPathOptions;
        int previousIncludeFlags = m_filter.GetIncludeFlags();
        int previousExcludeFlags = m_filter.GetExcludeFlags();
        bool previousConstrainByCircle = constrainByCircle;

        ImGui.Text("Mode");
        ImGui.Separator();
        ImGui.RadioButton(TestNavmeshToolMode.PATHFIND_FOLLOW.Label, ref m_toolModeIdx, TestNavmeshToolMode.PATHFIND_FOLLOW.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.PATHFIND_STRAIGHT.Label, ref m_toolModeIdx, TestNavmeshToolMode.PATHFIND_STRAIGHT.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.PATHFIND_SLICED.Label, ref m_toolModeIdx, TestNavmeshToolMode.PATHFIND_SLICED.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.DISTANCE_TO_WALL.Label, ref m_toolModeIdx, TestNavmeshToolMode.DISTANCE_TO_WALL.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.RAYCAST.Label, ref m_toolModeIdx, TestNavmeshToolMode.RAYCAST.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.FIND_POLYS_IN_CIRCLE.Label, ref m_toolModeIdx, TestNavmeshToolMode.FIND_POLYS_IN_CIRCLE.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.FIND_POLYS_IN_SHAPE.Label, ref m_toolModeIdx, TestNavmeshToolMode.FIND_POLYS_IN_SHAPE.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD.Label, ref m_toolModeIdx, TestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE.Label, ref m_toolModeIdx, TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE.Idx);
        ImGui.NewLine();

        // selecting mode
        ImGui.Text(m_toolMode.Label);
        ImGui.Separator();
        ImGui.NewLine();

        if (m_toolMode == TestNavmeshToolMode.PATHFIND_FOLLOW)
        {
        }

        if (m_toolMode == TestNavmeshToolMode.PATHFIND_STRAIGHT)
        {
            ImGui.Text("Vertices at crossings");
            ImGui.Separator();
            ImGui.RadioButton("None", ref m_straightPathOptions, 0);
            ImGui.RadioButton("Area", ref m_straightPathOptions, DtNavMeshQuery.DT_STRAIGHTPATH_AREA_CROSSINGS);
            ImGui.RadioButton("All", ref m_straightPathOptions, DtNavMeshQuery.DT_STRAIGHTPATH_ALL_CROSSINGS);
        }

        if (m_toolMode == TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            ImGui.Checkbox("Constrained", ref constrainByCircle);
        }

        ImGui.Text("Common");
        ImGui.Separator();

        ImGui.Text("Include Flags");
        ImGui.Separator();
        ImGui.CheckboxFlags("Walk", ref includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_WALK);
        ImGui.CheckboxFlags("Swim", ref includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM);
        ImGui.CheckboxFlags("Door", ref includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR);
        ImGui.CheckboxFlags("Jump", ref includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP);
        ImGui.NewLine();

        m_filter.SetIncludeFlags(includeFlags);

        ImGui.Text("Exclude Flags");
        ImGui.Separator();
        ImGui.CheckboxFlags("Walk", ref excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_WALK);
        ImGui.CheckboxFlags("Swim", ref excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM);
        ImGui.CheckboxFlags("Door", ref excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR);
        ImGui.CheckboxFlags("Jump", ref excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP);
        ImGui.NewLine();

        m_filter.SetExcludeFlags(excludeFlags);

        bool previousEnableRaycast = enableRaycast;
        ImGui.Checkbox("Raycast shortcuts", ref enableRaycast);

        if (previousToolMode != m_toolMode || m_straightPathOptions != previousStraightPathOptions
                                           || previousIncludeFlags != includeFlags || previousExcludeFlags != excludeFlags
                                           || previousEnableRaycast != enableRaycast || previousConstrainByCircle != constrainByCircle)
        {
            Recalc();
        }
    }


    private void Recalc()
    {
        if (_impl.GetSample().GetNavMesh() == null)
        {
            return;
        }

        DtNavMeshQuery m_navQuery = _impl.GetSample().GetNavMeshQuery();
        if (m_sposSet)
        {
            m_navQuery.FindNearestPoly(m_spos, m_polyPickExt, m_filter, out m_startRef, out var _, out var _);
        }
        else
        {
            m_startRef = 0;
        }

        if (m_eposSet)
        {
            m_navQuery.FindNearestPoly(m_epos, m_polyPickExt, m_filter, out m_endRef, out var _, out var _);
        }
        else
        {
            m_endRef = 0;
        }

        DtNavMesh m_navMesh = _impl.GetSample().GetNavMesh();
        if (m_toolMode == TestNavmeshToolMode.PATHFIND_FOLLOW)
        {
            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                m_polys = m_navQuery.FindPath(m_startRef, m_endRef, m_spos, m_epos, m_filter,
                    enableRaycast ? DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE : 0, float.MaxValue).result;
                if (0 < m_polys.Count)
                {
                    List<long> polys = new(m_polys);
                    // Iterate over the path to find smooth path on the detail mesh surface.
                    m_navQuery.ClosestPointOnPoly(m_startRef, m_spos, out var iterPos, out var _);
                    m_navQuery.ClosestPointOnPoly(polys[polys.Count - 1], m_epos, out var targetPos, out var _);

                    float STEP_SIZE = 0.5f;
                    float SLOP = 0.01f;

                    m_smoothPath = new();
                    m_smoothPath.Add(iterPos);

                    // Move towards target a small advancement at a time until target reached or
                    // when ran out of memory to store the path.
                    while (0 < polys.Count && m_smoothPath.Count < MAX_SMOOTH)
                    {
                        // Find location to steer towards.
                        SteerTarget steerTarget = PathUtils.GetSteerTarget(m_navQuery, iterPos, targetPos,
                            SLOP, polys);
                        if (null == steerTarget)
                        {
                            break;
                        }

                        bool endOfPath = (steerTarget.steerPosFlag & DtNavMeshQuery.DT_STRAIGHTPATH_END) != 0
                            ? true
                            : false;
                        bool offMeshConnection = (steerTarget.steerPosFlag
                                                  & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0
                            ? true
                            : false;

                        // Find movement delta.
                        RcVec3f delta = steerTarget.steerPos.Subtract(iterPos);
                        float len = (float)Math.Sqrt(RcVec3f.Dot(delta, delta));
                        // If the steer target is end of path or off-mesh link, do not move past the location.
                        if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                        {
                            len = 1;
                        }
                        else
                        {
                            len = STEP_SIZE / len;
                        }

                        RcVec3f moveTgt = RcVec3f.Mad(iterPos, delta, len);

                        // Move
                        m_navQuery.MoveAlongSurface(polys[0], iterPos, moveTgt, m_filter, out var result, out var visited);

                        iterPos = result;

                        polys = PathUtils.FixupCorridor(polys, visited);
                        polys = PathUtils.FixupShortcuts(polys, m_navQuery);

                        var status = m_navQuery.GetPolyHeight(polys[0], result, out var h);
                        if (status.Succeeded())
                        {
                            iterPos.y = h;
                        }

                        // Handle end of path and off-mesh links when close enough.
                        if (endOfPath && PathUtils.InRange(iterPos, steerTarget.steerPos, SLOP, 1.0f))
                        {
                            // Reached end of path.
                            iterPos = targetPos;
                            if (m_smoothPath.Count < MAX_SMOOTH)
                            {
                                m_smoothPath.Add(iterPos);
                            }

                            break;
                        }
                        else if (offMeshConnection && PathUtils.InRange(iterPos, steerTarget.steerPos, SLOP, 1.0f))
                        {
                            // Reached off-mesh connection.
                            RcVec3f startPos = RcVec3f.Zero;
                            RcVec3f endPos = RcVec3f.Zero;

                            // Advance the path up to and over the off-mesh connection.
                            long prevRef = 0;
                            long polyRef = polys[0];
                            int npos = 0;
                            while (npos < polys.Count && polyRef != steerTarget.steerPosRef)
                            {
                                prevRef = polyRef;
                                polyRef = polys[npos];
                                npos++;
                            }

                            polys = polys.GetRange(npos, polys.Count - npos);

                            // Handle the connection.
                            var status2 = m_navMesh.GetOffMeshConnectionPolyEndPoints(prevRef, polyRef, ref startPos, ref endPos);
                            if (status2.Succeeded())
                            {
                                if (m_smoothPath.Count < MAX_SMOOTH)
                                {
                                    m_smoothPath.Add(startPos);
                                    // Hack to make the dotted path not visible during off-mesh connection.
                                    if ((m_smoothPath.Count & 1) != 0)
                                    {
                                        m_smoothPath.Add(startPos);
                                    }
                                }

                                // Move position at the other side of the off-mesh link.
                                iterPos = endPos;
                                m_navQuery.GetPolyHeight(polys[0], iterPos, out var eh);
                                iterPos.y = eh;
                            }
                        }

                        // Store results.
                        if (m_smoothPath.Count < MAX_SMOOTH)
                        {
                            m_smoothPath.Add(iterPos);
                        }
                    }
                }
            }
            else
            {
                m_polys = null;
                m_smoothPath = null;
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.PATHFIND_STRAIGHT)
        {
            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                m_polys = m_navQuery.FindPath(m_startRef, m_endRef, m_spos, m_epos, m_filter,
                    enableRaycast ? DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE : 0, float.MaxValue).result;
                if (0 < m_polys.Count)
                {
                    // In case of partial path, make sure the end point is clamped to the last polygon.
                    var epos = RcVec3f.Of(m_epos.x, m_epos.y, m_epos.z);
                    if (m_polys[m_polys.Count - 1] != m_endRef)
                    {
                        var result = m_navQuery.ClosestPointOnPoly(m_polys[m_polys.Count - 1], m_epos, out var closest, out var _);
                        if (result.Succeeded())
                        {
                            epos = closest;
                        }
                    }

                    m_navQuery.FindStraightPath(m_spos, epos, m_polys, ref m_straightPath, MAX_POLYS, m_straightPathOptions);
                }
            }
            else
            {
                m_straightPath = null;
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.PATHFIND_SLICED)
        {
            m_polys = null;
            m_straightPath = null;
            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                m_pathFindStatus = m_navQuery.InitSlicedFindPath(m_startRef, m_endRef, m_spos, m_epos, m_filter,
                    enableRaycast ? DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE : 0, float.MaxValue);
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.RAYCAST)
        {
            m_straightPath = null;
            if (m_sposSet && m_eposSet && m_startRef != 0)
            {
                {
                    var status = m_navQuery.Raycast(m_startRef, m_spos, m_epos, m_filter, 0, 0, out var rayHit);
                    if (status.Succeeded())
                    {
                        m_polys = rayHit.path;
                        if (rayHit.t > 1)
                        {
                            // No hit
                            m_hitPos = m_epos;
                            m_hitResult = false;
                        }
                        else
                        {
                            // Hit
                            m_hitPos = RcVec3f.Lerp(m_spos, m_epos, rayHit.t);
                            m_hitNormal = rayHit.hitNormal;
                            m_hitResult = true;
                        }

                        // Adjust height.
                        if (rayHit.path.Count > 0)
                        {
                            var result = m_navQuery.GetPolyHeight(rayHit.path[rayHit.path.Count - 1], m_hitPos, out var h);
                            if (result.Succeeded())
                            {
                                m_hitPos.y = h;
                            }
                        }
                    }

                    m_straightPath = new();
                    m_straightPath.Add(new StraightPathItem(m_spos, 0, 0));
                    m_straightPath.Add(new StraightPathItem(m_hitPos, 0, 0));
                }
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.DISTANCE_TO_WALL)
        {
            m_distanceToWall = 0;
            if (m_sposSet && m_startRef != 0)
            {
                m_distanceToWall = 0.0f;
                var result = m_navQuery.FindDistanceToWall(m_startRef, m_spos, 100.0f, m_filter, out var hitDist, out var hitPos, out var hitNormal);
                if (result.Succeeded())
                {
                    m_distanceToWall = hitDist;
                    m_hitPos = hitPos;
                    m_hitNormal = hitNormal;
                }
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.FIND_POLYS_IN_CIRCLE)
        {
            if (m_sposSet && m_startRef != 0 && m_eposSet)
            {
                float dx = m_epos.x - m_spos.x;
                float dz = m_epos.z - m_spos.z;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                var status = m_navQuery.FindPolysAroundCircle(m_startRef, m_spos, dist, m_filter, out var refs, out var parentRefs, out var costs);
                if (status.Succeeded())
                {
                    m_polys = refs;
                    m_parent = parentRefs;
                }
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.FIND_POLYS_IN_SHAPE)
        {
            if (m_sposSet && m_startRef != 0 && m_eposSet)
            {
                float nx = (m_epos.z - m_spos.z) * 0.25f;
                float nz = -(m_epos.x - m_spos.x) * 0.25f;
                float agentHeight = _impl.GetSample() != null ? _impl.GetSample().GetSettings().agentHeight : 0;

                m_queryPoly[0] = m_spos.x + nx * 1.2f;
                m_queryPoly[1] = m_spos.y + agentHeight / 2;
                m_queryPoly[2] = m_spos.z + nz * 1.2f;

                m_queryPoly[3] = m_spos.x - nx * 1.3f;
                m_queryPoly[4] = m_spos.y + agentHeight / 2;
                m_queryPoly[5] = m_spos.z - nz * 1.3f;

                m_queryPoly[6] = m_epos.x - nx * 0.8f;
                m_queryPoly[7] = m_epos.y + agentHeight / 2;
                m_queryPoly[8] = m_epos.z - nz * 0.8f;

                m_queryPoly[9] = m_epos.x + nx;
                m_queryPoly[10] = m_epos.y + agentHeight / 2;
                m_queryPoly[11] = m_epos.z + nz;

                var status = m_navQuery.FindPolysAroundShape(m_startRef, m_queryPoly, m_filter, out var refs, out var parentRefs, out var costs);
                if (status.Succeeded())
                {
                    m_polys = refs;
                    m_parent = parentRefs;
                }
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD)
        {
            if (m_sposSet && m_startRef != 0)
            {
                m_neighbourhoodRadius = _impl.GetSample().GetSettings().agentRadius * 20.0f;
                m_navQuery.FindLocalNeighbourhood(m_startRef, m_spos, m_neighbourhoodRadius, m_filter, ref m_polys, ref m_parent);
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            randomPoints.Clear();
            if (m_sposSet && m_startRef != 0 && m_eposSet)
            {
                float dx = m_epos.x - m_spos.x;
                float dz = m_epos.z - m_spos.z;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                IPolygonByCircleConstraint constraint = constrainByCircle
                    ? StrictPolygonByCircleConstraint.Strict
                    : NoOpPolygonByCircleConstraint.Noop;

                for (int i = 0; i < 200; i++)
                {
                    var status = m_navQuery.FindRandomPointAroundCircle(m_startRef, m_spos, dist, m_filter, new FRand(), constraint,
                        out var randomRef, out var randomPt);
                    if (status.Succeeded())
                    {
                        randomPoints.Add(randomPt);
                    }
                }
            }
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        if (_impl.GetSample() == null)
        {
            return;
        }

        RecastDebugDraw dd = renderer.GetDebugDraw();
        int startCol = DuRGBA(128, 25, 0, 192);
        int endCol = DuRGBA(51, 102, 0, 129);
        int pathCol = DuRGBA(0, 0, 0, 64);

        float agentRadius = _impl.GetSample().GetSettings().agentRadius;
        float agentHeight = _impl.GetSample().GetSettings().agentHeight;
        float agentClimb = _impl.GetSample().GetSettings().agentMaxClimb;

        if (m_sposSet)
        {
            DrawAgent(dd, m_spos, startCol);
        }

        if (m_eposSet)
        {
            DrawAgent(dd, m_epos, endCol);
        }

        dd.DepthMask(true);

        DtNavMesh m_navMesh = _impl.GetSample().GetNavMesh();
        if (m_navMesh == null)
        {
            return;
        }

        if (m_toolMode == TestNavmeshToolMode.PATHFIND_FOLLOW)
        {
            dd.DebugDrawNavMeshPoly(m_navMesh, m_startRef, startCol);
            dd.DebugDrawNavMeshPoly(m_navMesh, m_endRef, endCol);

            if (m_polys != null)
            {
                foreach (long poly in m_polys)
                {
                    if (poly == m_startRef || poly == m_endRef)
                    {
                        continue;
                    }

                    dd.DebugDrawNavMeshPoly(m_navMesh, poly, pathCol);
                }
            }

            if (m_smoothPath != null)
            {
                dd.DepthMask(false);
                int spathCol = DuRGBA(0, 0, 0, 220);
                dd.Begin(LINES, 3.0f);
                for (int i = 0; i < m_smoothPath.Count; ++i)
                {
                    dd.Vertex(m_smoothPath[i].x, m_smoothPath[i].y + 0.1f, m_smoothPath[i].z, spathCol);
                }

                dd.End();
                dd.DepthMask(true);
            }
            /*
            if (m_pathIterNum)
            {
                DuDebugDrawNavMeshPoly(&dd, *m_navMesh, m_pathIterPolys.x, DebugDraw.DuRGBA(255,255,255,128));

                dd.DepthMask(false);
                dd.Begin(DebugDrawPrimitives.LINES, 1.0f);

                int prevCol = DebugDraw.DuRGBA(255,192,0,220);
                int curCol = DebugDraw.DuRGBA(255,255,255,220);
                int steerCol = DebugDraw.DuRGBA(0,192,255,220);

                dd.Vertex(m_prevIterPos.x,m_prevIterPos.y-0.3f,m_prevIterPos.z, prevCol);
                dd.Vertex(m_prevIterPos.x,m_prevIterPos.y+0.3f,m_prevIterPos.z, prevCol);

                dd.Vertex(m_iterPos.x,m_iterPos.y-0.3f,m_iterPos.z, curCol);
                dd.Vertex(m_iterPos.x,m_iterPos.y+0.3f,m_iterPos.z, curCol);

                dd.Vertex(m_prevIterPos.x,m_prevIterPos.y+0.3f,m_prevIterPos.z, prevCol);
                dd.Vertex(m_iterPos.x,m_iterPos.y+0.3f,m_iterPos.z, prevCol);

                dd.Vertex(m_prevIterPos.x,m_prevIterPos.y+0.3f,m_prevIterPos.z, steerCol);
                dd.Vertex(m_steerPos.x,m_steerPos.y+0.3f,m_steerPos.z, steerCol);

                for (int i = 0; i < m_steerPointCount-1; ++i)
                {
                    dd.Vertex(m_steerPoints[i*3+0],m_steerPoints[i*3+1]+0.2f,m_steerPoints[i*3+2], DuDarkenCol(steerCol));
                    dd.Vertex(m_steerPoints[(i+1)*3+0],m_steerPoints[(i+1)*3+1]+0.2f,m_steerPoints[(i+1)*3+2], DuDarkenCol(steerCol));
                }

                dd.End();
                dd.DepthMask(true);
            }
            */
        }
        else if (m_toolMode == TestNavmeshToolMode.PATHFIND_STRAIGHT || m_toolMode == TestNavmeshToolMode.PATHFIND_SLICED)
        {
            dd.DebugDrawNavMeshPoly(m_navMesh, m_startRef, startCol);
            dd.DebugDrawNavMeshPoly(m_navMesh, m_endRef, endCol);

            if (m_polys != null)
            {
                foreach (long poly in m_polys)
                {
                    dd.DebugDrawNavMeshPoly(m_navMesh, poly, pathCol);
                }
            }

            if (m_straightPath != null)
            {
                dd.DepthMask(false);
                int spathCol = DuRGBA(64, 16, 0, 220);
                int offMeshCol = DuRGBA(128, 96, 0, 220);
                dd.Begin(LINES, 2.0f);
                for (int i = 0; i < m_straightPath.Count - 1; ++i)
                {
                    StraightPathItem straightPathItem = m_straightPath[i];
                    StraightPathItem straightPathItem2 = m_straightPath[i + 1];
                    int col;
                    if ((straightPathItem.GetFlags() & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        col = offMeshCol;
                    }
                    else
                    {
                        col = spathCol;
                    }

                    dd.Vertex(straightPathItem.GetPos().x, straightPathItem.GetPos().y + 0.4f,
                        straightPathItem.GetPos().z, col);
                    dd.Vertex(straightPathItem2.GetPos().x, straightPathItem2.GetPos().y + 0.4f,
                        straightPathItem2.GetPos().z, col);
                }

                dd.End();
                dd.Begin(POINTS, 6.0f);
                for (int i = 0; i < m_straightPath.Count; ++i)
                {
                    StraightPathItem straightPathItem = m_straightPath[i];
                    int col;
                    if ((straightPathItem.GetFlags() & DtNavMeshQuery.DT_STRAIGHTPATH_START) != 0)
                    {
                        col = startCol;
                    }
                    else if ((straightPathItem.GetFlags() & DtNavMeshQuery.DT_STRAIGHTPATH_END) != 0)
                    {
                        col = endCol;
                    }
                    else if ((straightPathItem.GetFlags() & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        col = offMeshCol;
                    }
                    else
                    {
                        col = spathCol;
                    }

                    dd.Vertex(straightPathItem.GetPos().x, straightPathItem.GetPos().y + 0.4f,
                        straightPathItem.GetPos().z, col);
                }

                dd.End();
                dd.DepthMask(true);
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.RAYCAST)
        {
            dd.DebugDrawNavMeshPoly(m_navMesh, m_startRef, startCol);

            if (m_straightPath != null)
            {
                if (m_polys != null)
                {
                    foreach (long poly in m_polys)
                    {
                        dd.DebugDrawNavMeshPoly(m_navMesh, poly, pathCol);
                    }
                }

                dd.DepthMask(false);
                int spathCol = m_hitResult ? DuRGBA(64, 16, 0, 220) : DuRGBA(240, 240, 240, 220);
                dd.Begin(LINES, 2.0f);
                for (int i = 0; i < m_straightPath.Count - 1; ++i)
                {
                    StraightPathItem straightPathItem = m_straightPath[i];
                    StraightPathItem straightPathItem2 = m_straightPath[i + 1];
                    dd.Vertex(straightPathItem.GetPos().x, straightPathItem.GetPos().y + 0.4f,
                        straightPathItem.GetPos().z, spathCol);
                    dd.Vertex(straightPathItem2.GetPos().x, straightPathItem2.GetPos().y + 0.4f,
                        straightPathItem2.GetPos().z, spathCol);
                }

                dd.End();
                dd.Begin(POINTS, 4.0f);
                for (int i = 0; i < m_straightPath.Count; ++i)
                {
                    StraightPathItem straightPathItem = m_straightPath[i];
                    dd.Vertex(straightPathItem.GetPos().x, straightPathItem.GetPos().y + 0.4f,
                        straightPathItem.GetPos().z, spathCol);
                }

                dd.End();

                if (m_hitResult)
                {
                    int hitCol = DuRGBA(0, 0, 0, 128);
                    dd.Begin(LINES, 2.0f);
                    dd.Vertex(m_hitPos.x, m_hitPos.y + 0.4f, m_hitPos.z, hitCol);
                    dd.Vertex(m_hitPos.x + m_hitNormal.x * agentRadius,
                        m_hitPos.y + 0.4f + m_hitNormal.y * agentRadius,
                        m_hitPos.z + m_hitNormal.z * agentRadius, hitCol);
                    dd.End();
                }

                dd.DepthMask(true);
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.DISTANCE_TO_WALL)
        {
            dd.DebugDrawNavMeshPoly(m_navMesh, m_startRef, startCol);
            dd.DepthMask(false);
            if (m_spos != RcVec3f.Zero)
            {
                dd.DebugDrawCircle(m_spos.x, m_spos.y + agentHeight / 2, m_spos.z, m_distanceToWall,
                    DuRGBA(64, 16, 0, 220), 2.0f);
            }

            if (m_hitPos != RcVec3f.Zero)
            {
                dd.Begin(LINES, 3.0f);
                dd.Vertex(m_hitPos.x, m_hitPos.y + 0.02f, m_hitPos.z, DuRGBA(0, 0, 0, 192));
                dd.Vertex(m_hitPos.x, m_hitPos.y + agentHeight, m_hitPos.z, DuRGBA(0, 0, 0, 192));
                dd.End();
            }

            dd.DepthMask(true);
        }
        else if (m_toolMode == TestNavmeshToolMode.FIND_POLYS_IN_CIRCLE)
        {
            if (m_polys != null)
            {
                for (int i = 0; i < m_polys.Count; i++)
                {
                    dd.DebugDrawNavMeshPoly(m_navMesh, m_polys[i], pathCol);
                    dd.DepthMask(false);
                    if (m_parent[i] != 0)
                    {
                        dd.DepthMask(false);
                        RcVec3f p0 = GetPolyCenter(m_navMesh, m_parent[i]);
                        RcVec3f p1 = GetPolyCenter(m_navMesh, m_polys[i]);
                        dd.DebugDrawArc(p0.x, p0.y, p0.z, p1.x, p1.y, p1.z, 0.25f, 0.0f, 0.4f,
                            DuRGBA(0, 0, 0, 128), 2.0f);
                        dd.DepthMask(true);
                    }

                    dd.DepthMask(true);
                }
            }

            if (m_sposSet && m_eposSet)
            {
                dd.DepthMask(false);
                float dx = m_epos.x - m_spos.x;
                float dz = m_epos.z - m_spos.z;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                dd.DebugDrawCircle(m_spos.x, m_spos.y + agentHeight / 2, m_spos.z, dist, DuRGBA(64, 16, 0, 220),
                    2.0f);
                dd.DepthMask(true);
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.FIND_POLYS_IN_SHAPE)
        {
            if (m_polys != null)
            {
                for (int i = 0; i < m_polys.Count; i++)
                {
                    dd.DebugDrawNavMeshPoly(m_navMesh, m_polys[i], pathCol);
                    dd.DepthMask(false);
                    if (m_parent[i] != 0)
                    {
                        dd.DepthMask(false);
                        RcVec3f p0 = GetPolyCenter(m_navMesh, m_parent[i]);
                        RcVec3f p1 = GetPolyCenter(m_navMesh, m_polys[i]);
                        dd.DebugDrawArc(p0.x, p0.y, p0.z, p1.x, p1.y, p1.z, 0.25f, 0.0f, 0.4f,
                            DuRGBA(0, 0, 0, 128), 2.0f);
                        dd.DepthMask(true);
                    }

                    dd.DepthMask(true);
                }
            }

            if (m_sposSet && m_eposSet)
            {
                dd.DepthMask(false);
                int col = DuRGBA(64, 16, 0, 220);
                dd.Begin(LINES, 2.0f);
                for (int i = 0, j = 3; i < 4; j = i++)
                {
                    dd.Vertex(m_queryPoly[j * 3], m_queryPoly[j * 3 + 1], m_queryPoly[j * 3 + 2], col);
                    dd.Vertex(m_queryPoly[i * 3], m_queryPoly[i * 3 + 1], m_queryPoly[i * 3 + 2], col);
                }

                dd.End();
                dd.DepthMask(true);
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD)
        {
            if (m_polys != null)
            {
                var segmentVerts = new List<SegmentVert>();
                var segmentRefs = new List<long>();

                for (int i = 0; i < m_polys.Count; i++)
                {
                    dd.DebugDrawNavMeshPoly(m_navMesh, m_polys[i], pathCol);
                    dd.DepthMask(false);
                    if (m_parent[i] != 0)
                    {
                        dd.DepthMask(false);
                        RcVec3f p0 = GetPolyCenter(m_navMesh, m_parent[i]);
                        RcVec3f p1 = GetPolyCenter(m_navMesh, m_polys[i]);
                        dd.DebugDrawArc(p0.x, p0.y, p0.z, p1.x, p1.y, p1.z, 0.25f, 0.0f, 0.4f,
                            DuRGBA(0, 0, 0, 128), 2.0f);
                        dd.DepthMask(true);
                    }

                    dd.DepthMask(true);
                    if (_impl.GetSample().GetNavMeshQuery() != null)
                    {
                        var result = _impl.GetSample()
                            .GetNavMeshQuery()
                            .GetPolyWallSegments(m_polys[i], false, m_filter, ref segmentVerts, ref segmentRefs);

                        if (result.Succeeded())
                        {
                            dd.Begin(LINES, 2.0f);
                            for (int j = 0; j < segmentVerts.Count; ++j)
                            {
                                SegmentVert s = segmentVerts[j];
                                var v0 = RcVec3f.Of(s[0], s[1], s[2]);
                                var s3 = RcVec3f.Of(s[3], s[4], s[5]);
                                // Skip too distant segments.
                                var distSqr = DetourCommon.DistancePtSegSqr2D(m_spos, v0, s3, out var tseg);
                                if (distSqr > RcMath.Sqr(m_neighbourhoodRadius))
                                {
                                    continue;
                                }

                                RcVec3f delta = s3.Subtract(s.vmin);
                                RcVec3f p0 = RcVec3f.Mad(s.vmin, delta, 0.5f);
                                RcVec3f norm = RcVec3f.Of(delta.z, 0, -delta.x);
                                norm.Normalize();
                                RcVec3f p1 = RcVec3f.Mad(p0, norm, agentRadius * 0.5f);
                                // Skip backfacing segments.
                                if (segmentRefs[j] != 0)
                                {
                                    int col = DuRGBA(255, 255, 255, 32);
                                    dd.Vertex(s[0], s[1] + agentClimb, s[2], col);
                                    dd.Vertex(s[3], s[4] + agentClimb, s[5], col);
                                }
                                else
                                {
                                    int col = DuRGBA(192, 32, 16, 192);
                                    if (DetourCommon.TriArea2D(m_spos, s.vmin, s3) < 0.0f)
                                    {
                                        col = DuRGBA(96, 32, 16, 192);
                                    }

                                    dd.Vertex(p0.x, p0.y + agentClimb, p0.z, col);
                                    dd.Vertex(p1.x, p1.y + agentClimb, p1.z, col);

                                    dd.Vertex(s[0], s[1] + agentClimb, s[2], col);
                                    dd.Vertex(s[3], s[4] + agentClimb, s[5], col);
                                }
                            }

                            dd.End();
                        }
                    }

                    dd.DepthMask(true);
                }

                if (m_sposSet)
                {
                    dd.DepthMask(false);
                    dd.DebugDrawCircle(m_spos.x, m_spos.y + agentHeight / 2, m_spos.z, m_neighbourhoodRadius,
                        DuRGBA(64, 16, 0, 220), 2.0f);
                    dd.DepthMask(true);
                }
            }
        }
        else if (m_toolMode == TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            dd.DepthMask(false);
            dd.Begin(POINTS, 4.0f);
            int col = DuRGBA(64, 16, 0, 220);
            foreach (RcVec3f point in randomPoints)
            {
                dd.Vertex(point.x, point.y + 0.1f, point.z, col);
            }

            dd.End();
            if (m_sposSet && m_eposSet)
            {
                dd.DepthMask(false);
                float dx = m_epos.x - m_spos.x;
                float dz = m_epos.z - m_spos.z;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                dd.DebugDrawCircle(m_spos.x, m_spos.y + agentHeight / 2, m_spos.z, dist, DuRGBA(64, 16, 0, 220),
                    2.0f);
                dd.DepthMask(true);
            }

            dd.DepthMask(true);
        }
    }

    private void DrawAgent(RecastDebugDraw dd, RcVec3f pos, int col)
    {
        float r = _impl.GetSample().GetSettings().agentRadius;
        float h = _impl.GetSample().GetSettings().agentHeight;
        float c = _impl.GetSample().GetSettings().agentMaxClimb;
        dd.DepthMask(false);
        // Agent dimensions.
        dd.DebugDrawCylinderWire(pos.x - r, pos.y + 0.02f, pos.z - r, pos.x + r, pos.y + h, pos.z + r, col, 2.0f);
        dd.DebugDrawCircle(pos.x, pos.y + c, pos.z, r, DuRGBA(0, 0, 0, 64), 1.0f);
        int colb = DuRGBA(0, 0, 0, 196);
        dd.Begin(LINES);
        dd.Vertex(pos.x, pos.y - c, pos.z, colb);
        dd.Vertex(pos.x, pos.y + c, pos.z, colb);
        dd.Vertex(pos.x - r / 2, pos.y + 0.02f, pos.z, colb);
        dd.Vertex(pos.x + r / 2, pos.y + 0.02f, pos.z, colb);
        dd.Vertex(pos.x, pos.y + 0.02f, pos.z - r / 2, colb);
        dd.Vertex(pos.x, pos.y + 0.02f, pos.z + r / 2, colb);
        dd.End();
        dd.DepthMask(true);
    }

    private RcVec3f GetPolyCenter(DtNavMesh navMesh, long refs)
    {
        RcVec3f center = RcVec3f.Zero;

        var status = navMesh.GetTileAndPolyByRef(refs, out var tile, out var poly);
        if (status.Succeeded())
        {
            for (int i = 0; i < poly.vertCount; ++i)
            {
                int v = poly.verts[i] * 3;
                center.x += tile.data.verts[v];
                center.y += tile.data.verts[v + 1];
                center.z += tile.data.verts[v + 2];
            }

            float s = 1.0f / poly.vertCount;
            center.x *= s;
            center.y *= s;
            center.z *= s;
        }

        return center;
    }

    public void HandleUpdate(float dt)
    {
        // TODO Auto-generated method stub
        if (m_toolMode == TestNavmeshToolMode.PATHFIND_SLICED)
        {
            DtNavMeshQuery m_navQuery = _impl.GetSample().GetNavMeshQuery();
            if (m_pathFindStatus.InProgress())
            {
                m_pathFindStatus = m_navQuery.UpdateSlicedFindPath(1).status;
            }

            if (m_pathFindStatus.Succeeded())
            {
                m_polys = m_navQuery.FinalizeSlicedFindPath().result;
                m_straightPath = null;
                if (m_polys != null)
                {
                    // In case of partial path, make sure the end point is clamped to the last polygon.
                    RcVec3f epos = new RcVec3f();
                    epos = m_epos;
                    if (m_polys[m_polys.Count - 1] != m_endRef)
                    {
                        var result = m_navQuery.ClosestPointOnPoly(m_polys[m_polys.Count - 1], m_epos, out var closest, out var _);
                        if (result.Succeeded())
                        {
                            epos = closest;
                        }
                    }

                    m_navQuery.FindStraightPath(m_spos, epos, m_polys, ref m_straightPath, MAX_POLYS, DtNavMeshQuery.DT_STRAIGHTPATH_ALL_CROSSINGS);
                }

                m_pathFindStatus = DtStatus.DT_FAILURE;
            }
        }
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}