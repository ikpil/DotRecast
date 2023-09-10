using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Tools;
using ImGuiNET;
using Serilog;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class TestNavmeshSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<TestNavmeshSampleTool>();

    private const int MAX_POLYS = 256;

    private DemoSample _sample;
    private readonly RcTestNavMeshTool _tool;

    // mode select
    private RcTestNavmeshToolMode _mode = RcTestNavmeshToolMode.Values[RcTestNavmeshToolMode.PATHFIND_FOLLOW.Idx];

    // flags
    private int _includeFlags = SampleAreaModifications.SAMPLE_POLYFLAGS_ALL;
    private int _excludeFlags = 0;

    private bool _enableRaycast = true;

    // for pathfind straight mode
    private int _straightPathOption;

    // for random point in circle mode
    private int _randomPointCount = 300;
    private bool _constrainByCircle;

    // 
    private bool m_sposSet;
    private long m_startRef;
    private RcVec3f m_spos;

    private bool m_eposSet;
    private long m_endRef;
    private RcVec3f m_epos;

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

    public TestNavmeshSampleTool()
    {
        _tool = new();

        m_filter = new DtQueryDefaultFilter(
            SampleAreaModifications.SAMPLE_POLYFLAGS_ALL,
            SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED,
            new float[] { 1f, 1f, 1f, 1f, 2f, 1.5f }
        );
    }

    public void Layout()
    {
        var prevMode = _mode;
        int prevModeIdx = _mode.Idx;

        int prevIncludeFlags = m_filter.GetIncludeFlags();
        int prevExcludeFlags = m_filter.GetExcludeFlags();

        bool prevEnableRaycast = _enableRaycast;

        int prevStraightPathOption = _straightPathOption;
        bool prevConstrainByCircle = _constrainByCircle;

        ImGui.Text("Mode");
        ImGui.Separator();
        ImGui.RadioButton(RcTestNavmeshToolMode.PATHFIND_FOLLOW.Label, ref prevModeIdx, RcTestNavmeshToolMode.PATHFIND_FOLLOW.Idx);
        ImGui.RadioButton(RcTestNavmeshToolMode.PATHFIND_STRAIGHT.Label, ref prevModeIdx, RcTestNavmeshToolMode.PATHFIND_STRAIGHT.Idx);
        ImGui.RadioButton(RcTestNavmeshToolMode.PATHFIND_SLICED.Label, ref prevModeIdx, RcTestNavmeshToolMode.PATHFIND_SLICED.Idx);
        ImGui.RadioButton(RcTestNavmeshToolMode.DISTANCE_TO_WALL.Label, ref prevModeIdx, RcTestNavmeshToolMode.DISTANCE_TO_WALL.Idx);
        ImGui.RadioButton(RcTestNavmeshToolMode.RAYCAST.Label, ref prevModeIdx, RcTestNavmeshToolMode.RAYCAST.Idx);
        ImGui.RadioButton(RcTestNavmeshToolMode.FIND_POLYS_IN_CIRCLE.Label, ref prevModeIdx, RcTestNavmeshToolMode.FIND_POLYS_IN_CIRCLE.Idx);
        ImGui.RadioButton(RcTestNavmeshToolMode.FIND_POLYS_IN_SHAPE.Label, ref prevModeIdx, RcTestNavmeshToolMode.FIND_POLYS_IN_SHAPE.Idx);
        ImGui.RadioButton(RcTestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD.Label, ref prevModeIdx, RcTestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD.Idx);
        ImGui.RadioButton(RcTestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE.Label, ref prevModeIdx, RcTestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE.Idx);
        ImGui.NewLine();

        if (prevModeIdx != _mode.Idx)
        {
            _mode = RcTestNavmeshToolMode.Values[prevModeIdx];
        }

        // selecting mode
        ImGui.Text(_mode.Label);
        ImGui.Separator();
        ImGui.NewLine();

        if (_mode == RcTestNavmeshToolMode.PATHFIND_FOLLOW)
        {
        }

        if (_mode == RcTestNavmeshToolMode.PATHFIND_STRAIGHT)
        {
            ImGui.Text("Vertices at crossings");
            ImGui.Separator();
            ImGui.RadioButton("None", ref _straightPathOption, DtStraightPathOption.None.Value);
            ImGui.RadioButton("Area", ref _straightPathOption, DtStraightPathOption.AreaCrossings.Value);
            ImGui.RadioButton("All", ref _straightPathOption, DtStraightPathOption.AllCrossings.Value);
        }

        if (_mode == RcTestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            ImGui.SliderInt("Random point count", ref _randomPointCount, 0, 10000);
            ImGui.Checkbox("Constrained", ref _constrainByCircle);
        }

        ImGui.Text("Common");
        ImGui.Separator();

        ImGui.Text("Include Flags");
        ImGui.Separator();
        ImGui.CheckboxFlags("Walk", ref _includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_WALK);
        ImGui.CheckboxFlags("Swim", ref _includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM);
        ImGui.CheckboxFlags("Door", ref _includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR);
        ImGui.CheckboxFlags("Jump", ref _includeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP);
        ImGui.NewLine();

        m_filter.SetIncludeFlags(_includeFlags);

        ImGui.Text("Exclude Flags");
        ImGui.Separator();
        ImGui.CheckboxFlags("Walk", ref _excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_WALK);
        ImGui.CheckboxFlags("Swim", ref _excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM);
        ImGui.CheckboxFlags("Door", ref _excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR);
        ImGui.CheckboxFlags("Jump", ref _excludeFlags, SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP);
        ImGui.NewLine();

        m_filter.SetExcludeFlags(_excludeFlags);

        ImGui.Checkbox("Raycast shortcuts", ref _enableRaycast);

        if (prevMode != _mode || prevIncludeFlags != _includeFlags
                              || prevExcludeFlags != _excludeFlags
                              || prevEnableRaycast != _enableRaycast
                              || prevStraightPathOption != _straightPathOption
                              || prevConstrainByCircle != _constrainByCircle)
        {
            Recalc();
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();
        int startCol = DuRGBA(128, 25, 0, 192);
        int endCol = DuRGBA(51, 102, 0, 129);
        int pathCol = DuRGBA(0, 0, 0, 64);

        var settings = _sample.GetSettings();
        float agentRadius = settings.agentRadius;
        float agentHeight = settings.agentHeight;
        float agentClimb = settings.agentMaxClimb;

        if (m_sposSet)
        {
            DrawAgent(dd, m_spos, startCol);
        }

        if (m_eposSet)
        {
            DrawAgent(dd, m_epos, endCol);
        }

        dd.DepthMask(true);

        DtNavMesh m_navMesh = _sample.GetNavMesh();
        if (m_navMesh == null)
        {
            return;
        }

        if (_mode == RcTestNavmeshToolMode.PATHFIND_FOLLOW)
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
        else if (_mode == RcTestNavmeshToolMode.PATHFIND_STRAIGHT || _mode == RcTestNavmeshToolMode.PATHFIND_SLICED)
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
                    if ((straightPathItem.flags & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        col = offMeshCol;
                    }
                    else
                    {
                        col = spathCol;
                    }

                    dd.Vertex(straightPathItem.pos.x, straightPathItem.pos.y + 0.4f, straightPathItem.pos.z, col);
                    dd.Vertex(straightPathItem2.pos.x, straightPathItem2.pos.y + 0.4f, straightPathItem2.pos.z, col);
                }

                dd.End();
                dd.Begin(POINTS, 6.0f);
                for (int i = 0; i < m_straightPath.Count; ++i)
                {
                    StraightPathItem straightPathItem = m_straightPath[i];
                    int col;
                    if ((straightPathItem.flags & DtNavMeshQuery.DT_STRAIGHTPATH_START) != 0)
                    {
                        col = startCol;
                    }
                    else if ((straightPathItem.flags & DtNavMeshQuery.DT_STRAIGHTPATH_END) != 0)
                    {
                        col = endCol;
                    }
                    else if ((straightPathItem.flags & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        col = offMeshCol;
                    }
                    else
                    {
                        col = spathCol;
                    }

                    dd.Vertex(straightPathItem.pos.x, straightPathItem.pos.y + 0.4f, straightPathItem.pos.z, col);
                }

                dd.End();
                dd.DepthMask(true);
            }
        }
        else if (_mode == RcTestNavmeshToolMode.RAYCAST)
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
                    dd.Vertex(straightPathItem.pos.x, straightPathItem.pos.y + 0.4f, straightPathItem.pos.z, spathCol);
                    dd.Vertex(straightPathItem2.pos.x, straightPathItem2.pos.y + 0.4f, straightPathItem2.pos.z, spathCol);
                }

                dd.End();
                dd.Begin(POINTS, 4.0f);
                for (int i = 0; i < m_straightPath.Count; ++i)
                {
                    StraightPathItem straightPathItem = m_straightPath[i];
                    dd.Vertex(straightPathItem.pos.x, straightPathItem.pos.y + 0.4f, straightPathItem.pos.z, spathCol);
                }

                dd.End();

                if (m_hitResult)
                {
                    int hitCol = DuRGBA(0, 0, 0, 128);
                    dd.Begin(LINES, 2.0f);
                    dd.Vertex(m_hitPos.x, m_hitPos.y + 0.4f, m_hitPos.z, hitCol);
                    dd.Vertex(m_hitPos.x + m_hitNormal.x * agentRadius, m_hitPos.y + 0.4f + m_hitNormal.y * agentRadius, m_hitPos.z + m_hitNormal.z * agentRadius, hitCol);
                    dd.End();
                }

                dd.DepthMask(true);
            }
        }
        else if (_mode == RcTestNavmeshToolMode.DISTANCE_TO_WALL)
        {
            dd.DebugDrawNavMeshPoly(m_navMesh, m_startRef, startCol);
            dd.DepthMask(false);
            if (m_spos != RcVec3f.Zero)
            {
                dd.DebugDrawCircle(m_spos.x, m_spos.y + agentHeight / 2, m_spos.z, m_distanceToWall, DuRGBA(64, 16, 0, 220), 2.0f);
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
        else if (_mode == RcTestNavmeshToolMode.FIND_POLYS_IN_CIRCLE)
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
                        dd.DebugDrawArc(p0.x, p0.y, p0.z, p1.x, p1.y, p1.z, 0.25f, 0.0f, 0.4f, DuRGBA(0, 0, 0, 128), 2.0f);
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
                dd.DebugDrawCircle(m_spos.x, m_spos.y + agentHeight / 2, m_spos.z, dist, DuRGBA(64, 16, 0, 220), 2.0f);
                dd.DepthMask(true);
            }
        }
        else if (_mode == RcTestNavmeshToolMode.FIND_POLYS_IN_SHAPE)
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
                        dd.DebugDrawArc(p0.x, p0.y, p0.z, p1.x, p1.y, p1.z, 0.25f, 0.0f, 0.4f, DuRGBA(0, 0, 0, 128), 2.0f);
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
        else if (_mode == RcTestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD)
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
                        dd.DebugDrawArc(p0.x, p0.y, p0.z, p1.x, p1.y, p1.z, 0.25f, 0.0f, 0.4f, DuRGBA(0, 0, 0, 128), 2.0f);
                        dd.DepthMask(true);
                    }

                    dd.DepthMask(true);
                    if (_sample.GetNavMeshQuery() != null)
                    {
                        var result = _sample
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
                                var distSqr = DtUtils.DistancePtSegSqr2D(m_spos, v0, s3, out var tseg);
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
                                    if (DtUtils.TriArea2D(m_spos, s.vmin, s3) < 0.0f)
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
                    dd.DebugDrawCircle(m_spos.x, m_spos.y + agentHeight / 2, m_spos.z, m_neighbourhoodRadius, DuRGBA(64, 16, 0, 220), 2.0f);
                    dd.DepthMask(true);
                }
            }
        }
        else if (_mode == RcTestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
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
                dd.DebugDrawCircle(m_spos.x, m_spos.y + agentHeight / 2, m_spos.z, dist, DuRGBA(64, 16, 0, 220), 2.0f);
                dd.DepthMask(true);
            }

            dd.DepthMask(true);
        }
    }

    private void DrawAgent(RecastDebugDraw dd, RcVec3f pos, int col)
    {
        var settings = _sample.GetSettings();
        float r = settings.agentRadius;
        float h = settings.agentHeight;
        float c = settings.agentMaxClimb;
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


    private void Recalc()
    {
        var geom = _sample.GetInputGeom();
        var settings = _sample.GetSettings();
        var navMesh = _sample.GetNavMesh();
        var navQuery = _sample.GetNavMeshQuery();

        if (null == geom || null == navQuery)
            return;

        if (m_sposSet)
        {
            navQuery.FindNearestPoly(m_spos, m_polyPickExt, m_filter, out m_startRef, out var _, out var _);
        }
        else
        {
            m_startRef = 0;
        }

        if (m_eposSet)
        {
            navQuery.FindNearestPoly(m_epos, m_polyPickExt, m_filter, out m_endRef, out var _, out var _);
        }
        else
        {
            m_endRef = 0;
        }

        if (_mode == RcTestNavmeshToolMode.PATHFIND_FOLLOW)
        {
            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                var polys = new List<long>();
                var smoothPath = new List<RcVec3f>();

                var status = _tool.FindFollowPath(navMesh, navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter, _enableRaycast,
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
        else if (_mode == RcTestNavmeshToolMode.PATHFIND_STRAIGHT)
        {
            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                var polys = new List<long>();
                var straightPath = new List<StraightPathItem>();
                var status = _tool.FindStraightPath(navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter, _enableRaycast,
                    ref polys, ref straightPath, _straightPathOption);

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
        else if (_mode == RcTestNavmeshToolMode.PATHFIND_SLICED)
        {
            m_polys = null;
            m_straightPath = null;

            if (m_sposSet && m_eposSet && m_startRef != 0 && m_endRef != 0)
            {
                m_pathFindStatus = _tool.InitSlicedFindPath(navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter, _enableRaycast);
            }
        }
        else if (_mode == RcTestNavmeshToolMode.RAYCAST)
        {
            m_straightPath = null;
            if (m_sposSet && m_eposSet && m_startRef != 0)
            {
                var polys = new List<long>();
                var straightPath = new List<StraightPathItem>();
                var status = _tool.Raycast(navQuery, m_startRef, m_spos, m_epos, m_filter,
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
        else if (_mode == RcTestNavmeshToolMode.DISTANCE_TO_WALL)
        {
            m_distanceToWall = 0;
            if (m_sposSet && m_startRef != 0)
            {
                var result = navQuery.FindDistanceToWall(m_startRef, m_spos, 100.0f, m_filter, out var hitDist, out var hitPos, out var hitNormal);
                if (result.Succeeded())
                {
                    m_distanceToWall = hitDist;
                    m_hitPos = hitPos;
                    m_hitNormal = hitNormal;
                }
            }
        }
        else if (_mode == RcTestNavmeshToolMode.FIND_POLYS_IN_CIRCLE)
        {
            _tool.FindPolysAroundCircle(navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter, ref m_polys, ref m_parent);
        }
        else if (_mode == RcTestNavmeshToolMode.FIND_POLYS_IN_SHAPE)
        {
            _tool.FindPolysAroundShape(navQuery, settings.agentHeight, m_startRef, m_endRef, m_spos, m_epos, m_filter, ref m_polys, ref m_parent, ref m_queryPoly);
        }
        else if (_mode == RcTestNavmeshToolMode.FIND_LOCAL_NEIGHBOURHOOD)
        {
            if (m_sposSet && m_startRef != 0)
            {
                m_neighbourhoodRadius = settings.agentRadius * 20.0f;
                List<long> resultRef = new();
                List<long> resultParent = new();
                var status = navQuery.FindLocalNeighbourhood(m_startRef, m_spos, m_neighbourhoodRadius, m_filter, ref resultRef, ref resultParent);
                if (status.Succeeded())
                {
                    m_polys = resultRef;
                    m_parent = resultParent;
                }
            }
        }
        else if (_mode == RcTestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            randomPoints.Clear();
            if (m_sposSet && m_startRef != 0 && m_eposSet)
            {
                var points = new List<RcVec3f>();
                _tool.FindRandomPointAroundCircle(navQuery, m_startRef, m_spos, m_epos, m_filter, _constrainByCircle, _randomPointCount, ref points);
                randomPoints.AddRange(points);
            }
        }
    }


    public void HandleUpdate(float dt)
    {
        // TODO Auto-generated method stub
        if (_mode == RcTestNavmeshToolMode.PATHFIND_SLICED)
        {
            DtNavMeshQuery navQuery = _sample.GetNavMeshQuery();

            if (m_pathFindStatus.InProgress())
            {
                m_pathFindStatus = _tool.UpdateSlicedFindPath(navQuery, 1, m_endRef, m_spos, m_epos, ref m_polys, ref m_straightPath);
            }
        }
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}