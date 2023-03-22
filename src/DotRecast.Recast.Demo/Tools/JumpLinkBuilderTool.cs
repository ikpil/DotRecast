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

using System.Collections.Generic;
using Silk.NET.Windowing;
using DotRecast.Detour.Extras.Jumplink;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;
using ImGuiNET;
using static DotRecast.Detour.DetourCommon;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class JumpLinkBuilderTool : Tool
{
    private readonly List<JumpLink> links = new();
    private Sample sample;
    private JumpLinkBuilder annotationBuilder;
    private readonly int selEdge = -1;
    private readonly JumpLinkBuilderToolParams option = new JumpLinkBuilderToolParams();

    public override void setSample(Sample sample)
    {
        this.sample = sample;
        annotationBuilder = null;
    }

    public override void handleClick(float[] s, float[] p, bool shift)
    {
    }

    public override void handleRender(NavMeshRenderer renderer)
    {
        int col0 = duLerpCol(duRGBA(32, 255, 96, 255), duRGBA(255, 255, 255, 255), 200);
        int col1 = duRGBA(32, 255, 96, 255);
        RecastDebugDraw dd = renderer.getDebugDraw();
        dd.depthMask(false);

        if ((option.flags & JumpLinkBuilderToolParams.DRAW_WALKABLE_BORDER) != 0)
        {
            if (annotationBuilder != null)
            {
                foreach (Edge[] edges in annotationBuilder.getEdges())
                {
                    dd.begin(LINES, 3.0f);
                    for (int i = 0; i < edges.Length; ++i)
                    {
                        int col = duRGBA(0, 96, 128, 255);
                        if (i == selEdge)
                            continue;
                        dd.vertex(edges[i].sp, col);
                        dd.vertex(edges[i].sq, col);
                    }

                    dd.end();

                    dd.begin(POINTS, 8.0f);
                    for (int i = 0; i < edges.Length; ++i)
                    {
                        int col = duRGBA(0, 96, 128, 255);
                        if (i == selEdge)
                            continue;
                        dd.vertex(edges[i].sp, col);
                        dd.vertex(edges[i].sq, col);
                    }

                    dd.end();

                    if (selEdge >= 0 && selEdge < edges.Length)
                    {
                        int col = duRGBA(48, 16, 16, 255); // duRGBA(255,192,0,255);
                        dd.begin(LINES, 3.0f);
                        dd.vertex(edges[selEdge].sp, col);
                        dd.vertex(edges[selEdge].sq, col);
                        dd.end();
                        dd.begin(POINTS, 8.0f);
                        dd.vertex(edges[selEdge].sp, col);
                        dd.vertex(edges[selEdge].sq, col);
                        dd.end();
                    }

                    dd.begin(POINTS, 4.0f);
                    for (int i = 0; i < edges.Length; ++i)
                    {
                        int col = duRGBA(190, 190, 190, 255);
                        dd.vertex(edges[i].sp, col);
                        dd.vertex(edges[i].sq, col);
                    }

                    dd.end();
                }
            }
        }

        if ((option.flags & JumpLinkBuilderToolParams.DRAW_ANNOTATIONS) != 0)
        {
            dd.begin(QUADS);
            foreach (JumpLink link in links)
            {
                for (int j = 0; j < link.nspine - 1; ++j)
                {
                    int u = (j * 255) / link.nspine;
                    int col = duTransCol(duLerpCol(col0, col1, u), 128);
                    dd.vertex(link.spine1[j * 3], link.spine1[j * 3 + 1], link.spine1[j * 3 + 2], col);
                    dd.vertex(link.spine1[(j + 1) * 3], link.spine1[(j + 1) * 3 + 1], link.spine1[(j + 1) * 3 + 2],
                        col);
                    dd.vertex(link.spine0[(j + 1) * 3], link.spine0[(j + 1) * 3 + 1], link.spine0[(j + 1) * 3 + 2],
                        col);
                    dd.vertex(link.spine0[j * 3], link.spine0[j * 3 + 1], link.spine0[j * 3 + 2], col);
                }
            }

            dd.end();
            dd.begin(LINES, 3.0f);
            foreach (JumpLink link in links)
            {
                for (int j = 0; j < link.nspine - 1; ++j)
                {
                    // int u = (j*255)/link.nspine;
                    int col = duTransCol(duDarkenCol(col1) /*duDarkenCol(duLerpCol(col0,col1,u))*/, 128);

                    dd.vertex(link.spine0[j * 3], link.spine0[j * 3 + 1], link.spine0[j * 3 + 2], col);
                    dd.vertex(link.spine0[(j + 1) * 3], link.spine0[(j + 1) * 3 + 1], link.spine0[(j + 1) * 3 + 2],
                        col);
                    dd.vertex(link.spine1[j * 3], link.spine1[j * 3 + 1], link.spine1[j * 3 + 2], col);
                    dd.vertex(link.spine1[(j + 1) * 3], link.spine1[(j + 1) * 3 + 1], link.spine1[(j + 1) * 3 + 2],
                        col);
                }

                dd.vertex(link.spine0[0], link.spine0[1], link.spine0[2], duDarkenCol(col1));
                dd.vertex(link.spine1[0], link.spine1[1], link.spine1[2], duDarkenCol(col1));

                dd.vertex(link.spine0[(link.nspine - 1) * 3], link.spine0[(link.nspine - 1) * 3 + 1],
                    link.spine0[(link.nspine - 1) * 3 + 2], duDarkenCol(col1));
                dd.vertex(link.spine1[(link.nspine - 1) * 3], link.spine1[(link.nspine - 1) * 3 + 1],
                    link.spine1[(link.nspine - 1) * 3 + 2], duDarkenCol(col1));
            }

            dd.end();
        }

        if (annotationBuilder != null)
        {
            foreach (JumpLink link in links)
            {
                if ((option.flags & JumpLinkBuilderToolParams.DRAW_ANIM_TRAJECTORY) != 0)
                {
                    float r = link.start.height;

                    int col = duLerpCol(duRGBA(255, 192, 0, 255),
                        duRGBA(255, 255, 255, 255), 64);
                    int cola = duTransCol(col, 192);
                    int colb = duRGBA(255, 255, 255, 255);

                    // Start segment.
                    dd.begin(LINES, 3.0f);
                    dd.vertex(link.start.p, col);
                    dd.vertex(link.start.q, col);
                    dd.end();

                    dd.begin(LINES, 1.0f);
                    dd.vertex(link.start.p[0], link.start.p[1], link.start.p[2], colb);
                    dd.vertex(link.start.p[0], link.start.p[1] + r, link.start.p[2], colb);
                    dd.vertex(link.start.p[0], link.start.p[1] + r, link.start.p[2], colb);
                    dd.vertex(link.start.q[0], link.start.q[1] + r, link.start.q[2], colb);
                    dd.vertex(link.start.q[0], link.start.q[1] + r, link.start.q[2], colb);
                    dd.vertex(link.start.q[0], link.start.q[1], link.start.q[2], colb);
                    dd.vertex(link.start.q[0], link.start.q[1], link.start.q[2], colb);
                    dd.vertex(link.start.p[0], link.start.p[1], link.start.p[2], colb);
                    dd.end();

                    GroundSegment end = link.end;
                    r = end.height;
                    // End segment.
                    dd.begin(LINES, 3.0f);
                    dd.vertex(end.p, col);
                    dd.vertex(end.q, col);
                    dd.end();

                    dd.begin(LINES, 1.0f);
                    dd.vertex(end.p[0], end.p[1], end.p[2], colb);
                    dd.vertex(end.p[0], end.p[1] + r, end.p[2], colb);
                    dd.vertex(end.p[0], end.p[1] + r, end.p[2], colb);
                    dd.vertex(end.q[0], end.q[1] + r, end.q[2], colb);
                    dd.vertex(end.q[0], end.q[1] + r, end.q[2], colb);
                    dd.vertex(end.q[0], end.q[1], end.q[2], colb);
                    dd.vertex(end.q[0], end.q[1], end.q[2], colb);
                    dd.vertex(end.p[0], end.p[1], end.p[2], colb);
                    dd.end();

                    dd.begin(LINES, 4.0f);
                    drawTrajectory(dd, link, link.start.p, end.p, link.trajectory, cola);
                    drawTrajectory(dd, link, link.start.q, end.q, link.trajectory, cola);
                    dd.end();

                    dd.begin(LINES, 8.0f);
                    dd.vertex(link.start.p, duDarkenCol(col));
                    dd.vertex(link.start.q, duDarkenCol(col));
                    dd.vertex(end.p, duDarkenCol(col));
                    dd.vertex(end.q, duDarkenCol(col));
                    dd.end();

                    int colm = duRGBA(255, 255, 255, 255);
                    dd.begin(LINES, 3.0f);
                    dd.vertex(link.start.p, colm);
                    dd.vertex(link.start.q, colm);
                    dd.vertex(end.p, colm);
                    dd.vertex(end.q, colm);
                    dd.end();
                }

                if ((option.flags & JumpLinkBuilderToolParams.DRAW_LAND_SAMPLES) != 0)
                {
                    dd.begin(POINTS, 8.0f);
                    for (int i = 0; i < link.start.gsamples.Length; ++i)
                    {
                        GroundSample s = link.start.gsamples[i];
                        float u = i / (float)(link.start.gsamples.Length - 1);
                        float[] spt = vLerp(link.start.p, link.start.q, u);
                        int col = duRGBA(48, 16, 16, 255); // duRGBA(255,(s->flags & 4)?255:0,0,255);
                        float off = 0.1f;
                        if (!s.validHeight)
                        {
                            off = 0;
                            col = duRGBA(220, 32, 32, 255);
                        }

                        spt[1] = s.p[1] + off;
                        dd.vertex(spt, col);
                    }

                    dd.end();

                    dd.begin(POINTS, 4.0f);
                    for (int i = 0; i < link.start.gsamples.Length; ++i)
                    {
                        GroundSample s = link.start.gsamples[i];
                        float u = i / (float)(link.start.gsamples.Length - 1);
                        float[] spt = vLerp(link.start.p, link.start.q, u);
                        int col = duRGBA(255, 255, 255, 255);
                        float off = 0;
                        if (s.validHeight)
                        {
                            off = 0.1f;
                        }

                        spt[1] = s.p[1] + off;
                        dd.vertex(spt, col);
                    }

                    dd.end();
                    {
                        GroundSegment end = link.end;
                        dd.begin(POINTS, 8.0f);
                        for (int i = 0; i < end.gsamples.Length; ++i)
                        {
                            GroundSample s = end.gsamples[i];
                            float u = i / (float)(end.gsamples.Length - 1);
                            float[] spt = vLerp(end.p, end.q, u);
                            int col = duRGBA(48, 16, 16, 255); // duRGBA(255,(s->flags & 4)?255:0,0,255);
                            float off = 0.1f;
                            if (!s.validHeight)
                            {
                                off = 0;
                                col = duRGBA(220, 32, 32, 255);
                            }

                            spt[1] = s.p[1] + off;
                            dd.vertex(spt, col);
                        }

                        dd.end();
                        dd.begin(POINTS, 4.0f);
                        for (int i = 0; i < end.gsamples.Length; ++i)
                        {
                            GroundSample s = end.gsamples[i];
                            float u = i / (float)(end.gsamples.Length - 1);
                            float[] spt = vLerp(end.p, end.q, u);
                            int col = duRGBA(255, 255, 255, 255);
                            float off = 0;
                            if (s.validHeight)
                            {
                                off = 0.1f;
                            }

                            spt[1] = s.p[1] + off;
                            dd.vertex(spt, col);
                        }

                        dd.end();
                    }
                }
            }
        }

        dd.depthMask(true);
    }

    private void drawTrajectory(RecastDebugDraw dd, JumpLink link, float[] pa, float[] pb, Trajectory tra, int cola)
    {
    }

    public override void handleUpdate(float dt)
    {
    }

    public override void layout()
    {
        // if (!sample.getRecastResults().isEmpty()) {
        //
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Options");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Ground Tolerance", ref option.groundTolerance, 0f, 2f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 5, 1);
        //     nk_spacing(ctx, 1);
        //
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Climb Down");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Distance", ref option.climbDownDistance, 0f, 5f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Min Cliff Height", ref option.climbDownMinHeight, 0f, 10f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Max Cliff Height", ref option.climbDownMaxHeight, 0f, 10f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 5, 1);
        //     nk_spacing(ctx, 1);
        //
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Jump Down");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Max Distance", ref option.edgeJumpEndDistance, 0f, 10f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Jump Height", ref option.edgeJumpHeight, 0f, 10f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Max Jump Down", ref option.edgeJumpDownMaxHeight, 0f, 10f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Max Jump Up", ref option.edgeJumpUpMaxHeight, 0f, 10f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 5, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Mode");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     int buildTypes = 0;
        //     buildTypes |= nk_option_text(ctx, "Climb Down",
        //             (option.buildTypes & (1 << JumpLinkType.EDGE_CLIMB_DOWN.ordinal())) != 0)
        //                     ? (1 << JumpLinkType.EDGE_CLIMB_DOWN.ordinal())
        //                     : 0;
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     buildTypes |= nk_option_text(ctx, "Edge Jump",
        //             (option.buildTypes & (1 << JumpLinkType.EDGE_JUMP.ordinal())) != 0)
        //                     ? (1 << JumpLinkType.EDGE_JUMP.ordinal())
        //                     : 0;
        //     option.buildTypes = buildTypes;
        //     bool build = false;
        //     bool buildOffMeshConnections = false;
        //     if (nk_button_text(ctx, "Build")) {
        //         build = true;
        //     }
        //     if (nk_button_text(ctx, "Build Off-Mesh Links")) {
        //         buildOffMeshConnections = true;
        //     }
        //     if (build || buildOffMeshConnections) {
        //         if (annotationBuilder == null) {
        //             if (sample != null && !sample.getRecastResults().isEmpty()) {
        //                 annotationBuilder = new JumpLinkBuilder(sample.getRecastResults());
        //             }
        //         }
        //         links.clear();
        //         if (annotationBuilder != null) {
        //             float cellSize = sample.getSettingsUI().getCellSize();
        //             float agentHeight = sample.getSettingsUI().getAgentHeight();
        //             float agentRadius = sample.getSettingsUI().getAgentRadius();
        //             float agentClimb = sample.getSettingsUI().getAgentMaxClimb();
        //             float cellHeight = sample.getSettingsUI().getCellHeight();
        //             if ((buildTypes & (1 << JumpLinkType.EDGE_CLIMB_DOWN.ordinal())) != 0) {
        //                 JumpLinkBuilderConfig config = new JumpLinkBuilderConfig(cellSize, cellHeight, agentRadius,
        //                         agentHeight, agentClimb, option.groundTolerance[0], -agentRadius * 0.2f,
        //                         cellSize + 2 * agentRadius + option.climbDownDistance[0],
        //                         -option.climbDownMaxHeight[0], -option.climbDownMinHeight[0], 0);
        //                 links.addAll(annotationBuilder.build(config, JumpLinkType.EDGE_CLIMB_DOWN));
        //             }
        //             if ((buildTypes & (1 << JumpLinkType.EDGE_JUMP.ordinal())) != 0) {
        //                 JumpLinkBuilderConfig config = new JumpLinkBuilderConfig(cellSize, cellHeight, agentRadius,
        //                         agentHeight, agentClimb, option.groundTolerance[0], -agentRadius * 0.2f,
        //                         option.edgeJumpEndDistance[0], -option.edgeJumpDownMaxHeight[0],
        //                         option.edgeJumpUpMaxHeight[0], option.edgeJumpHeight[0]);
        //                 links.addAll(annotationBuilder.build(config, JumpLinkType.EDGE_JUMP));
        //             }
        //             if (buildOffMeshConnections) {
        //                 DemoInputGeomProvider geom = sample.getInputGeom();
        //                 if (geom != null) {
        //                     int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP_AUTO;
        //                     geom.removeOffMeshConnections(c => c.area == area);
        //                     links.forEach(l => addOffMeshLink(l, geom, agentRadius));
        //                 }
        //             }
        //         }
        //     }
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Debug Draw Options");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     int newFlags = 0;
        //     newFlags |= nk_option_text(ctx, "Walkable Border",
        //             (params.flags & JumpLinkBuilderToolParams.DRAW_WALKABLE_BORDER) != 0)
        //                     ? JumpLinkBuilderToolParams.DRAW_WALKABLE_BORDER
        //                     : 0;
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     newFlags |= nk_option_text(ctx, "Selected Edge",
        //             (params.flags & JumpLinkBuilderToolParams.DRAW_SELECTED_EDGE) != 0)
        //                     ? JumpLinkBuilderToolParams.DRAW_SELECTED_EDGE
        //                     : 0;
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     newFlags |= nk_option_text(ctx, "Anim Trajectory",
        //             (params.flags & JumpLinkBuilderToolParams.DRAW_ANIM_TRAJECTORY) != 0)
        //                     ? JumpLinkBuilderToolParams.DRAW_ANIM_TRAJECTORY
        //                     : 0;
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     newFlags |= nk_option_text(ctx, "Land Samples",
        //             (params.flags & JumpLinkBuilderToolParams.DRAW_LAND_SAMPLES) != 0)
        //                     ? JumpLinkBuilderToolParams.DRAW_LAND_SAMPLES
        //                     : 0;
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     newFlags |= nk_option_text(ctx, "All Annotations",
        //             (params.flags & JumpLinkBuilderToolParams.DRAW_ANNOTATIONS) != 0)
        //                     ? JumpLinkBuilderToolParams.DRAW_ANNOTATIONS
        //                     : 0;
        //     params.flags = newFlags;
        // }
    }

    private void addOffMeshLink(JumpLink link, DemoInputGeomProvider geom, float agentRadius)
    {
        int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP_AUTO;
        int flags = SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP;
        float[] prev = new float[3];
        for (int i = 0; i < link.startSamples.Length; i++)
        {
            float[] p = link.startSamples[i].p;
            float[] q = link.endSamples[i].p;
            if (i == 0 || vDist2D(prev, p) > agentRadius)
            {
                geom.addOffMeshConnection(p, q, agentRadius, false, area, flags);
                prev = p;
            }
        }
    }

    public override string getName()
    {
        return "Annotation Builder";
    }
}