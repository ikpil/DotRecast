/*
recast4j copyright (c) 2020-2021 Piotr Piastucki piotr@jtilia.org

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

using System.Linq;
using System.Numerics;
using DotRecast.Detour.Extras.Jumplink;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Tools;
using ImGuiNET;
using Serilog;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class JumpLinkBuilderSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<JumpLinkBuilderSampleTool>();
    private DemoSample _sample;

    public const int DRAW_WALKABLE_SURFACE = 1 << 0;
    public const int DRAW_WALKABLE_BORDER = 1 << 1;
    public const int DRAW_SELECTED_EDGE = 1 << 2;
    public const int DRAW_ANIM_TRAJECTORY = 1 << 3;
    public const int DRAW_LAND_SAMPLES = 1 << 4;
    public const int DRAW_COLLISION_SLICES = 1 << 5;
    public const int DRAW_ANNOTATIONS = 1 << 6;

    private int _drawFlags = DRAW_WALKABLE_SURFACE | DRAW_WALKABLE_BORDER | DRAW_SELECTED_EDGE | DRAW_ANIM_TRAJECTORY | DRAW_LAND_SAMPLES | DRAW_ANNOTATIONS;

    private readonly RcJumpLinkBuilderTool _tool;
    private readonly RcJumpLinkBuilderToolConfig _cfg;

    public JumpLinkBuilderSampleTool()
    {
        _tool = new();
        _cfg = new();
    }

    public void Layout()
    {
        ImGui.Text("Options");
        ImGui.Separator();
        ImGui.SliderFloat("Ground Tolerance", ref _cfg.groundTolerance, 0f, 2f, "%.2f");
        ImGui.NewLine();

        ImGui.Text("Climb Down");
        ImGui.Separator();
        ImGui.SliderFloat("Distance", ref _cfg.climbDownDistance, 0f, 5f, "%.2f");
        ImGui.SliderFloat("Min Cliff Height", ref _cfg.climbDownMinHeight, 0f, 10f, "%.2f");
        ImGui.SliderFloat("Max Cliff Height", ref _cfg.climbDownMaxHeight, 0f, 10f, "%.2f");
        ImGui.NewLine();

        ImGui.Text("Jump Down");
        ImGui.Separator();
        ImGui.SliderFloat("Max Distance", ref _cfg.edgeJumpEndDistance, 0f, 10f, "%.2f");
        ImGui.SliderFloat("Jump Height", ref _cfg.edgeJumpHeight, 0f, 10f, "%.2f");
        ImGui.SliderFloat("Max Jump Down", ref _cfg.edgeJumpDownMaxHeight, 0f, 10f, "%.2f");
        ImGui.SliderFloat("Max Jump Up", ref _cfg.edgeJumpUpMaxHeight, 0f, 10f, "%.2f");
        ImGui.NewLine();

        ImGui.Text("Mode");
        ImGui.Separator();
        //int buildTypes = 0;
        ImGui.CheckboxFlags("Climb Down", ref _cfg.buildTypes, JumpLinkType.EDGE_CLIMB_DOWN.Bit);
        ImGui.CheckboxFlags("Edge Jump", ref _cfg.buildTypes, JumpLinkType.EDGE_JUMP.Bit);
        ImGui.NewLine();
        //option.buildTypes = buildTypes;

        bool build = false;
        _cfg.buildOffMeshConnections = false;
        if (ImGui.Button("Build Jump Link"))
        {
            build = true;
        }

        if (ImGui.Button("Build Off-Mesh Links"))
        {
            _cfg.buildOffMeshConnections = true;
        }

        if (build || _cfg.buildOffMeshConnections)
        {
            do
            {
                if (0 >= _sample.GetRecastResults().Count)
                {
                    Logger.Error("build navmesh");
                    break;
                }

                if (_sample.GetRecastResults().Any(x => null == x.SolidHeightfiled))
                {
                    Logger.Error("Tick 'Keep Itermediate Results' option");
                    break;
                }

                var geom = _sample.GetInputGeom();
                var settings = _sample.GetSettings();

                _tool.Build(geom, settings, _sample.GetRecastResults(), _cfg);
            } while (false);
        }

        ImGui.NewLine();

        ImGui.Text("Debug Draw Options");
        ImGui.Separator();
        //int newFlags = 0;
        ImGui.CheckboxFlags("Walkable Border", ref _drawFlags, DRAW_WALKABLE_BORDER);
        ImGui.CheckboxFlags("Selected Edge", ref _drawFlags, DRAW_SELECTED_EDGE);
        ImGui.CheckboxFlags("Anim Trajectory", ref _drawFlags, DRAW_ANIM_TRAJECTORY);
        ImGui.CheckboxFlags("Land Samples", ref _drawFlags, DRAW_LAND_SAMPLES);
        ImGui.CheckboxFlags("All Annotations", ref _drawFlags, DRAW_ANNOTATIONS);
        //option.flags = newFlags;
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        int col0 = DuLerpCol(DuRGBA(32, 255, 96, 255), DuRGBA(255, 255, 255, 255), 200);
        int col1 = DuRGBA(32, 255, 96, 255);
        RecastDebugDraw dd = renderer.GetDebugDraw();
        dd.DepthMask(false);

        var annotationBuilder = _tool.GetAnnotationBuilder();

        if ((_drawFlags & DRAW_WALKABLE_BORDER) != 0)
        {
            if (annotationBuilder != null)
            {
                var selEdge = _tool.GetSelEdge();

                foreach (JumpEdge[] edges in annotationBuilder.GetEdges())
                {
                    dd.Begin(LINES, 3.0f);
                    for (int i = 0; i < edges.Length; ++i)
                    {
                        int col = DuRGBA(0, 96, 128, 255);
                        if (i == selEdge)
                            continue;
                        dd.Vertex(edges[i].sp, col);
                        dd.Vertex(edges[i].sq, col);
                    }

                    dd.End();

                    dd.Begin(POINTS, 8.0f);
                    for (int i = 0; i < edges.Length; ++i)
                    {
                        int col = DuRGBA(0, 96, 128, 255);
                        if (i == selEdge)
                            continue;
                        dd.Vertex(edges[i].sp, col);
                        dd.Vertex(edges[i].sq, col);
                    }

                    dd.End();

                    if (selEdge >= 0 && selEdge < edges.Length)
                    {
                        int col = DuRGBA(48, 16, 16, 255); // DuRGBA(255,192,0,255);
                        dd.Begin(LINES, 3.0f);
                        dd.Vertex(edges[selEdge].sp, col);
                        dd.Vertex(edges[selEdge].sq, col);
                        dd.End();
                        dd.Begin(POINTS, 8.0f);
                        dd.Vertex(edges[selEdge].sp, col);
                        dd.Vertex(edges[selEdge].sq, col);
                        dd.End();
                    }

                    dd.Begin(POINTS, 4.0f);
                    for (int i = 0; i < edges.Length; ++i)
                    {
                        int col = DuRGBA(190, 190, 190, 255);
                        dd.Vertex(edges[i].sp, col);
                        dd.Vertex(edges[i].sq, col);
                    }

                    dd.End();
                }
            }
        }

        if ((_drawFlags & DRAW_ANNOTATIONS) != 0)
        {
            dd.Begin(QUADS);
            foreach (JumpLink link in _tool.GetLinks())
            {
                for (int j = 0; j < link.nspine - 1; ++j)
                {
                    int u = (j * 255) / link.nspine;
                    int col = DuTransCol(DuLerpCol(col0, col1, u), 128);
                    dd.Vertex(link.spine1[j * 3], link.spine1[j * 3 + 1], link.spine1[j * 3 + 2], col);
                    dd.Vertex(link.spine1[(j + 1) * 3], link.spine1[(j + 1) * 3 + 1], link.spine1[(j + 1) * 3 + 2], col);
                    dd.Vertex(link.spine0[(j + 1) * 3], link.spine0[(j + 1) * 3 + 1], link.spine0[(j + 1) * 3 + 2], col);
                    dd.Vertex(link.spine0[j * 3], link.spine0[j * 3 + 1], link.spine0[j * 3 + 2], col);
                }
            }

            dd.End();
            dd.Begin(LINES, 3.0f);
            foreach (JumpLink link in _tool.GetLinks())
            {
                for (int j = 0; j < link.nspine - 1; ++j)
                {
                    // int u = (j*255)/link.nspine;
                    int col = DuTransCol(DuDarkenCol(col1) /*DuDarkenCol(DuLerpCol(col0,col1,u))*/, 128);

                    dd.Vertex(link.spine0[j * 3], link.spine0[j * 3 + 1], link.spine0[j * 3 + 2], col);
                    dd.Vertex(link.spine0[(j + 1) * 3], link.spine0[(j + 1) * 3 + 1], link.spine0[(j + 1) * 3 + 2], col);
                    dd.Vertex(link.spine1[j * 3], link.spine1[j * 3 + 1], link.spine1[j * 3 + 2], col);
                    dd.Vertex(link.spine1[(j + 1) * 3], link.spine1[(j + 1) * 3 + 1], link.spine1[(j + 1) * 3 + 2], col);
                }

                dd.Vertex(link.spine0[0], link.spine0[1], link.spine0[2], DuDarkenCol(col1));
                dd.Vertex(link.spine1[0], link.spine1[1], link.spine1[2], DuDarkenCol(col1));

                dd.Vertex(link.spine0[(link.nspine - 1) * 3], link.spine0[(link.nspine - 1) * 3 + 1], link.spine0[(link.nspine - 1) * 3 + 2], DuDarkenCol(col1));
                dd.Vertex(link.spine1[(link.nspine - 1) * 3], link.spine1[(link.nspine - 1) * 3 + 1], link.spine1[(link.nspine - 1) * 3 + 2], DuDarkenCol(col1));
            }

            dd.End();
        }

        if (annotationBuilder != null)
        {
            foreach (JumpLink link in _tool.GetLinks())
            {
                if ((_drawFlags & DRAW_ANIM_TRAJECTORY) != 0)
                {
                    float r = link.start.height;

                    int col = DuLerpCol(DuRGBA(255, 192, 0, 255), DuRGBA(255, 255, 255, 255), 64);
                    int cola = DuTransCol(col, 192);
                    int colb = DuRGBA(255, 255, 255, 255);

                    // Start segment.
                    dd.Begin(LINES, 3.0f);
                    dd.Vertex(link.start.p, col);
                    dd.Vertex(link.start.q, col);
                    dd.End();

                    dd.Begin(LINES, 1.0f);
                    dd.Vertex(link.start.p.X, link.start.p.Y, link.start.p.Z, colb);
                    dd.Vertex(link.start.p.X, link.start.p.Y + r, link.start.p.Z, colb);
                    dd.Vertex(link.start.p.X, link.start.p.Y + r, link.start.p.Z, colb);
                    dd.Vertex(link.start.q.X, link.start.q.Y + r, link.start.q.Z, colb);
                    dd.Vertex(link.start.q.X, link.start.q.Y + r, link.start.q.Z, colb);
                    dd.Vertex(link.start.q.X, link.start.q.Y, link.start.q.Z, colb);
                    dd.Vertex(link.start.q.X, link.start.q.Y, link.start.q.Z, colb);
                    dd.Vertex(link.start.p.X, link.start.p.Y, link.start.p.Z, colb);
                    dd.End();

                    GroundSegment end = link.end;
                    r = end.height;
                    // End segment.
                    dd.Begin(LINES, 3.0f);
                    dd.Vertex(end.p, col);
                    dd.Vertex(end.q, col);
                    dd.End();

                    dd.Begin(LINES, 1.0f);
                    dd.Vertex(end.p.X, end.p.Y, end.p.Z, colb);
                    dd.Vertex(end.p.X, end.p.Y + r, end.p.Z, colb);
                    dd.Vertex(end.p.X, end.p.Y + r, end.p.Z, colb);
                    dd.Vertex(end.q.X, end.q.Y + r, end.q.Z, colb);
                    dd.Vertex(end.q.X, end.q.Y + r, end.q.Z, colb);
                    dd.Vertex(end.q.X, end.q.Y, end.q.Z, colb);
                    dd.Vertex(end.q.X, end.q.Y, end.q.Z, colb);
                    dd.Vertex(end.p.X, end.p.Y, end.p.Z, colb);
                    dd.End();

                    dd.Begin(LINES, 4.0f);
                    DrawTrajectory(dd, link, link.start.p, end.p, link.trajectory, cola);
                    DrawTrajectory(dd, link, link.start.q, end.q, link.trajectory, cola);
                    dd.End();

                    dd.Begin(LINES, 8.0f);
                    dd.Vertex(link.start.p, DuDarkenCol(col));
                    dd.Vertex(link.start.q, DuDarkenCol(col));
                    dd.Vertex(end.p, DuDarkenCol(col));
                    dd.Vertex(end.q, DuDarkenCol(col));
                    dd.End();

                    int colm = DuRGBA(255, 255, 255, 255);
                    dd.Begin(LINES, 3.0f);
                    dd.Vertex(link.start.p, colm);
                    dd.Vertex(link.start.q, colm);
                    dd.Vertex(end.p, colm);
                    dd.Vertex(end.q, colm);
                    dd.End();
                }

                if ((_drawFlags & DRAW_LAND_SAMPLES) != 0)
                {
                    dd.Begin(POINTS, 8.0f);
                    for (int i = 0; i < link.start.gsamples.Length; ++i)
                    {
                        GroundSample s = link.start.gsamples[i];
                        float u = i / (float)(link.start.gsamples.Length - 1);
                        Vector3 spt = Vector3.Lerp(link.start.p, link.start.q, u);
                        int col = DuRGBA(48, 16, 16, 255); // DuRGBA(255,(s->flags & 4)?255:0,0,255);
                        float off = 0.1f;
                        if (!s.validHeight)
                        {
                            off = 0;
                            col = DuRGBA(220, 32, 32, 255);
                        }

                        spt.Y = s.p.Y + off;
                        dd.Vertex(spt, col);
                    }

                    dd.End();

                    dd.Begin(POINTS, 4.0f);
                    for (int i = 0; i < link.start.gsamples.Length; ++i)
                    {
                        GroundSample s = link.start.gsamples[i];
                        float u = i / (float)(link.start.gsamples.Length - 1);
                        Vector3 spt = Vector3.Lerp(link.start.p, link.start.q, u);
                        int col = DuRGBA(255, 255, 255, 255);
                        float off = 0;
                        if (s.validHeight)
                        {
                            off = 0.1f;
                        }

                        spt.Y = s.p.Y + off;
                        dd.Vertex(spt, col);
                    }

                    dd.End();
                    {
                        GroundSegment end = link.end;
                        dd.Begin(POINTS, 8.0f);
                        for (int i = 0; i < end.gsamples.Length; ++i)
                        {
                            GroundSample s = end.gsamples[i];
                            float u = i / (float)(end.gsamples.Length - 1);
                            Vector3 spt = Vector3.Lerp(end.p, end.q, u);
                            int col = DuRGBA(48, 16, 16, 255); // DuRGBA(255,(s->flags & 4)?255:0,0,255);
                            float off = 0.1f;
                            if (!s.validHeight)
                            {
                                off = 0;
                                col = DuRGBA(220, 32, 32, 255);
                            }

                            spt.Y = s.p.Y + off;
                            dd.Vertex(spt, col);
                        }

                        dd.End();
                        dd.Begin(POINTS, 4.0f);
                        for (int i = 0; i < end.gsamples.Length; ++i)
                        {
                            GroundSample s = end.gsamples[i];
                            float u = i / (float)(end.gsamples.Length - 1);
                            Vector3 spt = Vector3.Lerp(end.p, end.q, u);
                            int col = DuRGBA(255, 255, 255, 255);
                            float off = 0;
                            if (s.validHeight)
                            {
                                off = 0.1f;
                            }

                            spt.Y = s.p.Y + off;
                            dd.Vertex(spt, col);
                        }

                        dd.End();
                    }
                }
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
        _tool.Clear();
    }


    public void HandleClick(Vector3 s, Vector3 p, bool shift)
    {
    }


    private void DrawTrajectory(RecastDebugDraw dd, JumpLink link, Vector3 pa, Vector3 pb, ITrajectory tra, int cola)
    {
    }

    public void HandleUpdate(float dt)
    {
    }


    public void HandleClickRay(Vector3 start, Vector3 direction, bool shift)
    {
    }
}