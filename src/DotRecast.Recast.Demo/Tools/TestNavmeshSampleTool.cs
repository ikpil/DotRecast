using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Core.Numerics;
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
    private readonly RcVec3f m_polyPickExt = new RcVec3f(2, 4, 2);

    // for hit
    private RcVec3f m_hitPos;
    private RcVec3f m_hitNormal;
    private bool m_hitResult;

    private float m_distanceToWall;
    private List<DtStraightPath> m_straightPath;
    private List<long> m_polys;
    private List<long> m_parent;
    private float m_neighbourhoodRadius;
    private RcVec3f[] m_queryPoly = new RcVec3f[4];
    private List<RcVec3f> m_smoothPath;
    private DtStatus m_pathFindStatus = DtStatus.DT_FAILURE;

    // for mode RANDOM_POINTS_IN_CIRCLE
    private List<RcVec3f> _randomPoints = new();

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
                    dd.Vertex(m_smoothPath[i].X, m_smoothPath[i].Y + 0.1f, m_smoothPath[i].Z, spathCol);
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
                    DtStraightPath straightPathItem = m_straightPath[i];
                    DtStraightPath straightPathItem2 = m_straightPath[i + 1];
                    int col;
                    if ((straightPathItem.flags & DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        col = offMeshCol;
                    }
                    else
                    {
                        col = spathCol;
                    }

                    dd.Vertex(straightPathItem.pos.X, straightPathItem.pos.Y + 0.4f, straightPathItem.pos.Z, col);
                    dd.Vertex(straightPathItem2.pos.X, straightPathItem2.pos.Y + 0.4f, straightPathItem2.pos.Z, col);
                }

                dd.End();
                dd.Begin(POINTS, 6.0f);
                for (int i = 0; i < m_straightPath.Count; ++i)
                {
                    DtStraightPath straightPathItem = m_straightPath[i];
                    int col;
                    if ((straightPathItem.flags & DtStraightPathFlags.DT_STRAIGHTPATH_START) != 0)
                    {
                        col = startCol;
                    }
                    else if ((straightPathItem.flags & DtStraightPathFlags.DT_STRAIGHTPATH_END) != 0)
                    {
                        col = endCol;
                    }
                    else if ((straightPathItem.flags & DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        col = offMeshCol;
                    }
                    else
                    {
                        col = spathCol;
                    }

                    dd.Vertex(straightPathItem.pos.X, straightPathItem.pos.Y + 0.4f, straightPathItem.pos.Z, col);
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
                    DtStraightPath straightPathItem = m_straightPath[i];
                    DtStraightPath straightPathItem2 = m_straightPath[i + 1];
                    dd.Vertex(straightPathItem.pos.X, straightPathItem.pos.Y + 0.4f, straightPathItem.pos.Z, spathCol);
                    dd.Vertex(straightPathItem2.pos.X, straightPathItem2.pos.Y + 0.4f, straightPathItem2.pos.Z, spathCol);
                }

                dd.End();
                dd.Begin(POINTS, 4.0f);
                for (int i = 0; i < m_straightPath.Count; ++i)
                {
                    DtStraightPath straightPathItem = m_straightPath[i];
                    dd.Vertex(straightPathItem.pos.X, straightPathItem.pos.Y + 0.4f, straightPathItem.pos.Z, spathCol);
                }

                dd.End();

                if (m_hitResult)
                {
                    int hitCol = DuRGBA(0, 0, 0, 128);
                    dd.Begin(LINES, 2.0f);
                    dd.Vertex(m_hitPos.X, m_hitPos.Y + 0.4f, m_hitPos.Z, hitCol);
                    dd.Vertex(m_hitPos.X + m_hitNormal.X * agentRadius, m_hitPos.Y + 0.4f + m_hitNormal.Y * agentRadius, m_hitPos.Z + m_hitNormal.Z * agentRadius, hitCol);
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
                dd.DebugDrawCircle(m_spos.X, m_spos.Y + agentHeight / 2, m_spos.Z, m_distanceToWall, DuRGBA(64, 16, 0, 220), 2.0f);
            }

            if (m_hitPos != RcVec3f.Zero)
            {
                dd.Begin(LINES, 3.0f);
                dd.Vertex(m_hitPos.X, m_hitPos.Y + 0.02f, m_hitPos.Z, DuRGBA(0, 0, 0, 192));
                dd.Vertex(m_hitPos.X, m_hitPos.Y + agentHeight, m_hitPos.Z, DuRGBA(0, 0, 0, 192));
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
                        dd.DebugDrawArc(p0.X, p0.Y, p0.Z, p1.X, p1.Y, p1.Z, 0.25f, 0.0f, 0.4f, DuRGBA(0, 0, 0, 128), 2.0f);
                        dd.DepthMask(true);
                    }

                    dd.DepthMask(true);
                }
            }

            if (m_sposSet && m_eposSet)
            {
                dd.DepthMask(false);
                float dx = m_epos.X - m_spos.X;
                float dz = m_epos.Z - m_spos.Z;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                dd.DebugDrawCircle(m_spos.X, m_spos.Y + agentHeight / 2, m_spos.Z, dist, DuRGBA(64, 16, 0, 220), 2.0f);
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
                        dd.DebugDrawArc(p0.X, p0.Y, p0.Z, p1.X, p1.Y, p1.Z, 0.25f, 0.0f, 0.4f, DuRGBA(0, 0, 0, 128), 2.0f);
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
                    dd.Vertex(m_queryPoly[j].X, m_queryPoly[j].Y, m_queryPoly[j].Z, col);
                    dd.Vertex(m_queryPoly[i].X, m_queryPoly[i].Y, m_queryPoly[i].Z, col);
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
                        dd.DebugDrawArc(p0.X, p0.Y, p0.Z, p1.X, p1.Y, p1.Z, 0.25f, 0.0f, 0.4f, DuRGBA(0, 0, 0, 128), 2.0f);
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
                                RcVec3f norm = new RcVec3f(delta.Z, 0, -delta.X);
                                norm.Normalize();
                                RcVec3f p1 = RcVec3f.Mad(p0, norm, agentRadius * 0.5f);
                                // Skip backfacing segments.
                                if (segmentRefs[j] != 0)
                                {
                                    int col = DuRGBA(255, 255, 255, 32);
                                    dd.Vertex(s.vmin.X, s.vmin.Y + agentClimb, s.vmin.Z, col);
                                    dd.Vertex(s.vmax.X, s.vmax.Y + agentClimb, s.vmax.Z, col);
                                }
                                else
                                {
                                    int col = DuRGBA(192, 32, 16, 192);
                                    if (DtUtils.TriArea2D(m_spos, s.vmin, s3) < 0.0f)
                                    {
                                        col = DuRGBA(96, 32, 16, 192);
                                    }

                                    dd.Vertex(p0.X, p0.Y + agentClimb, p0.Z, col);
                                    dd.Vertex(p1.X, p1.Y + agentClimb, p1.Z, col);

                                    dd.Vertex(s.vmin.X, s.vmin.Y + agentClimb, s.vmin.Z, col);
                                    dd.Vertex(s.vmax.X, s.vmax.Y + agentClimb, s.vmax.Z, col);
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
                    dd.DebugDrawCircle(m_spos.X, m_spos.Y + agentHeight / 2, m_spos.Z, m_neighbourhoodRadius, DuRGBA(64, 16, 0, 220), 2.0f);
                    dd.DepthMask(true);
                }
            }
        }
        else if (_mode == RcTestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            dd.DepthMask(false);
            dd.Begin(POINTS, 4.0f);
            int col = DuRGBA(64, 16, 0, 220);
            foreach (RcVec3f point in _randomPoints)
            {
                dd.Vertex(point.X, point.Y + 0.1f, point.Z, col);
            }

            dd.End();
            if (m_sposSet && m_eposSet)
            {
                dd.DepthMask(false);
                float dx = m_epos.X - m_spos.X;
                float dz = m_epos.Z - m_spos.Z;
                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                dd.DebugDrawCircle(m_spos.X, m_spos.Y + agentHeight / 2, m_spos.Z, dist, DuRGBA(64, 16, 0, 220), 2.0f);
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
        dd.DebugDrawCylinderWire(pos.X - r, pos.Y + 0.02f, pos.Z - r, pos.X + r, pos.Y + h, pos.Z + r, col, 2.0f);
        dd.DebugDrawCircle(pos.X, pos.Y + c, pos.Z, r, DuRGBA(0, 0, 0, 64), 1.0f);
        int colb = DuRGBA(0, 0, 0, 196);
        dd.Begin(LINES);
        dd.Vertex(pos.X, pos.Y - c, pos.Z, colb);
        dd.Vertex(pos.X, pos.Y + c, pos.Z, colb);
        dd.Vertex(pos.X - r / 2, pos.Y + 0.02f, pos.Z, colb);
        dd.Vertex(pos.X + r / 2, pos.Y + 0.02f, pos.Z, colb);
        dd.Vertex(pos.X, pos.Y + 0.02f, pos.Z - r / 2, colb);
        dd.Vertex(pos.X, pos.Y + 0.02f, pos.Z + r / 2, colb);
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
            _tool.FindFollowPath(navMesh, navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter, _enableRaycast,
                ref m_polys, ref m_smoothPath);
        }
        else if (_mode == RcTestNavmeshToolMode.PATHFIND_STRAIGHT)
        {
            _tool.FindStraightPath(navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter, _enableRaycast,
                ref m_polys, ref m_straightPath, _straightPathOption);
        }
        else if (_mode == RcTestNavmeshToolMode.PATHFIND_SLICED)
        {
            m_polys?.Clear();
            m_straightPath?.Clear();
            m_pathFindStatus = _tool.InitSlicedFindPath(navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter, _enableRaycast);
        }
        else if (_mode == RcTestNavmeshToolMode.RAYCAST)
        {
            _tool.Raycast(navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter,
                ref m_polys, ref m_straightPath, ref m_hitPos, ref m_hitNormal, ref m_hitResult);
        }
        else if (_mode == RcTestNavmeshToolMode.DISTANCE_TO_WALL)
        {
            _tool.FindDistanceToWall(navQuery, m_startRef, m_spos, 100.0f, m_filter, ref m_distanceToWall, ref m_hitPos, ref m_hitNormal);
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
            m_neighbourhoodRadius = settings.agentRadius * 20.0f;
            _tool.FindLocalNeighbourhood(navQuery, m_startRef, m_spos, m_neighbourhoodRadius, m_filter, ref m_polys, ref m_parent);
        }
        else if (_mode == RcTestNavmeshToolMode.RANDOM_POINTS_IN_CIRCLE)
        {
            _tool.FindRandomPointAroundCircle(navQuery, m_startRef, m_endRef, m_spos, m_epos, m_filter, _constrainByCircle, _randomPointCount, ref _randomPoints);
        }
    }


    public void HandleUpdate(float dt)
    {
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