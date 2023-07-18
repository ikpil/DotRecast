using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour;
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

    private readonly TestNavmeshToolImpl _impl;

    private bool m_sposSet;
    private bool m_eposSet;
    private RcVec3f m_spos;
    private RcVec3f m_epos;
    private long m_startRef;
    private long m_endRef;

    private readonly DtQueryDefaultFilter m_filter;
    private readonly RcVec3f m_polyPickExt = RcVec3f.Of(2, 4, 2);

    // for hit
    private RcVec3f m_hitPos;
    private RcVec3f m_hitNormal;
    private bool m_hitResult;

    private float m_distanceToWall;
    private List<StraightPathItem> m_straightPath;
    private List<long> m_polys;
    private List<long> m_parent;
    private float m_neighbourhoodRadius;
    private RcVec3f[] m_queryPoly = new RcVec3f[4];
    private List<RcVec3f> m_smoothPath;
    private DtStatus m_pathFindStatus = DtStatus.DT_FAILURE;
    private readonly List<RcVec3f> randomPoints = new();

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
        var option = _impl.GetOption();
        var previousToolMode = option.mode;
        int previousStraightPathOptions = option.straightPathOptions;
        int previousIncludeFlags = m_filter.GetIncludeFlags();
        int previousExcludeFlags = m_filter.GetExcludeFlags();
        bool previousConstrainByCircle = option.constrainByCircle;

        ImGui.Text("Mode");
        ImGui.Separator();
        ImGui.RadioButton(TestNavmeshToolMode.PATHFIND_FOLLOW.Label, ref option.modeIdx, TestNavmeshToolMode.PATHFIND_FOLLOW.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.PATHFIND_STRAIGHT.Label, ref option.modeIdx, TestNavmeshToolMode.PATHFIND_STRAIGHT.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.PATHFIND_SLICED.Label, ref option.modeIdx, TestNavmeshToolMode.PATHFIND_SLICED.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.DISTANCE_TO_WALL.Label, ref option.modeIdx, TestNavmeshToolMode.DISTANCE_TO_WALL.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.RAYCAST.Label, ref option.modeIdx, TestNavmeshToolMode.RAYCAST.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.FIND_POLYS_IN_CIRCLE.Label, ref option.modeIdx, TestNavmeshToolMode.FIND_POLYS_IN_CIRCLE.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.FIND_POLYS_IN_SHAPE.Label, ref option.modeIdx, TestNavmeshToolMode.FIND_POLYS_IN_SHAPE.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD.Label, ref option.modeIdx, TestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD.Idx);
        ImGui.RadioButton(TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE.Label, ref option.modeIdx, TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE.Idx);
        ImGui.NewLine();

        // selecting mode
        ImGui.Text(option.mode.Label);
        ImGui.Separator();
        ImGui.NewLine();

        if (option.mode == TestNavmeshToolMode.PATHFIND_FOLLOW)
        {
        }

        if (option.mode == TestNavmeshToolMode.PATHFIND_STRAIGHT)
        {
            ImGui.Text("Vertices at crossings");
            ImGui.Separator();
            ImGui.RadioButton("None", ref option.straightPathOptions, 0);
            ImGui.RadioButton("Area", ref option.straightPathOptions, DtNavMeshQuery.DT_STRAIGHTPATH_AREA_CROSSINGS);
            ImGui.RadioButton("All", ref option.straightPathOptions, DtNavMeshQuery.DT_STRAIGHTPATH_ALL_CROSSINGS);
        }

        if (option.mode == TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            ImGui.Checkbox("Constrained", ref option.constrainByCircle);
        }

        ImGui.Text("Common");
        ImGui.Separator();

        ImGui.Text("Include Flags");
        ImGui.Separator();
        ImGui.CheckboxFlags("Walk", ref option.includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_WALK);
        ImGui.CheckboxFlags("Swim", ref option.includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM);
        ImGui.CheckboxFlags("Door", ref option.includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR);
        ImGui.CheckboxFlags("Jump", ref option.includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP);
        ImGui.NewLine();

        m_filter.SetIncludeFlags(option.includeFlags);

        ImGui.Text("Exclude Flags");
        ImGui.Separator();
        ImGui.CheckboxFlags("Walk", ref option.excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_WALK);
        ImGui.CheckboxFlags("Swim", ref option.excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM);
        ImGui.CheckboxFlags("Door", ref option.excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR);
        ImGui.CheckboxFlags("Jump", ref option.excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP);
        ImGui.NewLine();

        m_filter.SetExcludeFlags(option.excludeFlags);

        bool previousEnableRaycast = option.enableRaycast;
        ImGui.Checkbox("Raycast shortcuts", ref option.enableRaycast);

        if (previousToolMode != option.mode || option.straightPathOptions != previousStraightPathOptions
                                            || previousIncludeFlags != option.includeFlags || previousExcludeFlags != option.excludeFlags
                                            || previousEnableRaycast != option.enableRaycast || previousConstrainByCircle != option.constrainByCircle)
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

        var option = _impl.GetOption();

        if (option.mode == TestNavmeshToolMode.PATHFIND_FOLLOW)
        {
            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                var polys = new List<long>();
                var smoothPath = new List<RcVec3f>();
                var status = _impl.FindFollowPath(m_startRef, m_endRef, m_spos, m_epos, m_filter, option.enableRaycast,
                    ref polys, ref smoothPath);

                if (status.Succeeded())
                {
                    m_polys = polys;
                    m_smoothPath = smoothPath;
                }
            }
            else
            {
                m_polys = null;
                m_smoothPath = null;
            }
        }
        else if (option.mode == TestNavmeshToolMode.PATHFIND_STRAIGHT)
        {
            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                var polys = new List<long>();
                var straightPath = new List<StraightPathItem>();
                var status = _impl.FindStraightPath(m_startRef, m_endRef, m_spos, m_epos, m_filter, option.enableRaycast,
                    ref polys, ref straightPath, option.straightPathOptions);

                if (status.Succeeded())
                {
                    m_polys = polys;
                    m_straightPath = straightPath;
                }
            }
            else
            {
                m_straightPath = null;
            }
        }
        else if (option.mode == TestNavmeshToolMode.PATHFIND_SLICED)
        {
            m_polys = null;
            m_straightPath = null;

            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                m_pathFindStatus = _impl.InitSlicedFindPath(m_startRef, m_endRef, m_spos, m_epos, m_filter, option.enableRaycast);
            }
        }
        else if (option.mode == TestNavmeshToolMode.RAYCAST)
        {
            m_straightPath = null;
            if (m_sposSet && m_eposSet && m_startRef != 0)
            {
                var polys = new List<long>();
                var straightPath = new List<StraightPathItem>();
                var status = _impl.Raycast(m_startRef, m_spos, m_epos, m_filter,
                    ref polys, ref straightPath, out var hitPos, out var hitNormal, out var hitResult);

                if (status.Succeeded())
                {
                    m_polys = polys;
                    m_straightPath = straightPath;
                    m_hitPos = hitPos;
                    m_hitNormal = hitNormal;
                    m_hitResult = hitResult;
                }
            }
        }
        else if (option.mode == TestNavmeshToolMode.DISTANCE_TO_WALL)
        {
            m_distanceToWall = 0;
            if (m_sposSet && m_startRef != 0)
            {
                var result = m_navQuery.FindDistanceToWall(m_startRef, m_spos, 100.0f, m_filter, out var hitDist, out var hitPos, out var hitNormal);
                if (result.Succeeded())
                {
                    m_distanceToWall = hitDist;
                    m_hitPos = hitPos;
                    m_hitNormal = hitNormal;
                }
            }
        }
        else if (option.mode == TestNavmeshToolMode.FIND_POLYS_IN_CIRCLE)
        {
            if (m_sposSet && m_startRef != 0 && m_eposSet)
            {
                float dx = m_epos.x - m_spos.x;
                float dz = m_epos.z - m_spos.z;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                
                List<long> refs = new();
                List<long> parentRefs = new();
                List<float> costs = new();

                var status = m_navQuery.FindPolysAroundCircle(m_startRef, m_spos, dist, m_filter, ref refs, ref parentRefs, ref costs);
                if (status.Succeeded())
                {
                    m_polys = refs;
                    m_parent = parentRefs;
                }
            }
        }
        else if (option.mode == TestNavmeshToolMode.FIND_POLYS_IN_SHAPE)
        {
            if (m_sposSet && m_startRef != 0 && m_eposSet)
            {
                var refs = new List<long>();
                var parentRefs = new List<long>();

                var status = _impl.FindPolysAroundShape(m_startRef, m_spos, m_epos, m_filter, ref refs, ref parentRefs, out var queryPoly);
                if (status.Succeeded())
                {
                    m_queryPoly = queryPoly;
                    m_polys = refs;
                    m_parent = parentRefs;
                }
            }
        }
        else if (option.mode == TestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD)
        {
            if (m_sposSet && m_startRef != 0)
            {
                m_neighbourhoodRadius = _impl.GetSample().GetSettings().agentRadius * 20.0f;
                List<long> resultRef = new();
                List<long> resultParent = new();
                var status = m_navQuery.FindLocalNeighbourhood(m_startRef, m_spos, m_neighbourhoodRadius, m_filter, ref resultRef, ref resultParent);
                if (status.Succeeded())
                {
                    m_polys = resultRef;
                    m_parent = resultParent;
                }
            }
        }
        else if (option.mode == TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            randomPoints.Clear();
            if (m_sposSet && m_startRef != 0 && m_eposSet)
            {
                float dx = m_epos.x - m_spos.x;
                float dz = m_epos.z - m_spos.z;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                IPolygonByCircleConstraint constraint = option.constrainByCircle
                    ? StrictPolygonByCircleConstraint.Strict
                    : NoOpPolygonByCircleConstraint.Noop;

                var frand = new FRand();
                for (int i = 0; i < 200; i++)
                {
                    var status = m_navQuery.FindRandomPointAroundCircle(m_startRef, m_spos, dist, m_filter, frand, constraint,
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

        var option = _impl.GetOption();
        if (option.mode == TestNavmeshToolMode.PATHFIND_FOLLOW)
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
        else if (option.mode == TestNavmeshToolMode.PATHFIND_STRAIGHT || option.mode == TestNavmeshToolMode.PATHFIND_SLICED)
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
        else if (option.mode == TestNavmeshToolMode.RAYCAST)
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
        else if (option.mode == TestNavmeshToolMode.DISTANCE_TO_WALL)
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
        else if (option.mode == TestNavmeshToolMode.FIND_POLYS_IN_CIRCLE)
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
                        RcVec3f p0 = m_navMesh.GetPolyCenter(m_parent[i]);
                        RcVec3f p1 = m_navMesh.GetPolyCenter(m_polys[i]);
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
        else if (option.mode == TestNavmeshToolMode.FIND_POLYS_IN_SHAPE)
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
                        RcVec3f p0 = m_navMesh.GetPolyCenter(m_parent[i]);
                        RcVec3f p1 = m_navMesh.GetPolyCenter(m_polys[i]);
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
                    dd.Vertex(m_queryPoly[j].x, m_queryPoly[j].y, m_queryPoly[j].z, col);
                    dd.Vertex(m_queryPoly[i].x, m_queryPoly[i].y, m_queryPoly[i].z, col);
                }

                dd.End();
                dd.DepthMask(true);
            }
        }
        else if (option.mode == TestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD)
        {
            if (m_polys != null)
            {
                var segmentVerts = new List<RcSegmentVert>();
                var segmentRefs = new List<long>();

                for (int i = 0; i < m_polys.Count; i++)
                {
                    dd.DebugDrawNavMeshPoly(m_navMesh, m_polys[i], pathCol);
                    dd.DepthMask(false);
                    if (m_parent[i] != 0)
                    {
                        dd.DepthMask(false);
                        RcVec3f p0 = m_navMesh.GetPolyCenter(m_parent[i]);
                        RcVec3f p1 = m_navMesh.GetPolyCenter(m_polys[i]);
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
                                RcSegmentVert s = segmentVerts[j];
                                var v0 = s.vmin;
                                var s3 = s.vmax;
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
                                    dd.Vertex(s.vmin.x, s.vmin.y + agentClimb, s.vmin.z, col);
                                    dd.Vertex(s.vmax.x, s.vmax.y + agentClimb, s.vmax.z, col);
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

                                    dd.Vertex(s.vmin.x, s.vmin.y + agentClimb, s.vmin.z, col);
                                    dd.Vertex(s.vmax.x, s.vmax.y + agentClimb, s.vmax.z, col);
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
        else if (option.mode == TestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
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



    public void HandleUpdate(float dt)
    {
        // TODO Auto-generated method stub
        var option = _impl.GetOption();
        if (option.mode == TestNavmeshToolMode.PATHFIND_SLICED)
        {
            DtNavMeshQuery m_navQuery = _impl.GetSample().GetNavMeshQuery();
            if (m_pathFindStatus.InProgress())
            {
                m_pathFindStatus = m_navQuery.UpdateSlicedFindPath(1, out var _);
            }

            if (m_pathFindStatus.Succeeded())
            {
                m_navQuery.FinalizeSlicedFindPath(ref m_polys);
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