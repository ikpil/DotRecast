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
using System.Linq;
using DotRecast.Core;
using Silk.NET.Windowing;
using DotRecast.Detour.Extras.Jumplink;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;
using ImGuiNET;
using static DotRecast.Core.RcMath;
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

    public override void SetSample(Sample sample)
    {
        this.sample = sample;
        annotationBuilder = null;
    }

    public override void HandleClick(Vector3f s, Vector3f p, bool shift)
    {
    }

    public override void HandleRender(NavMeshRenderer renderer)
    {
        int col0 = DuLerpCol(DuRGBA(32, 255, 96, 255), DuRGBA(255, 255, 255, 255), 200);
        int col1 = DuRGBA(32, 255, 96, 255);
        RecastDebugDraw dd = renderer.GetDebugDraw();
        dd.DepthMask(false);

        if ((option.flags & JumpLinkBuilderToolParams.DRAW_WALKABLE_BORDER) != 0)
        {
            if (annotationBuilder != null)
            {
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

        if ((option.flags & JumpLinkBuilderToolParams.DRAW_ANNOTATIONS) != 0)
        {
            dd.Begin(QUADS);
            foreach (JumpLink link in links)
            {
                for (int j = 0; j < link.nspine - 1; ++j)
                {
                    int u = (j * 255) / link.nspine;
                    int col = DuTransCol(DuLerpCol(col0, col1, u), 128);
                    dd.Vertex(link.spine1[j * 3], link.spine1[j * 3 + 1], link.spine1[j * 3 + 2], col);
                    dd.Vertex(link.spine1[(j + 1) * 3], link.spine1[(j + 1) * 3 + 1], link.spine1[(j + 1) * 3 + 2],
                        col);
                    dd.Vertex(link.spine0[(j + 1) * 3], link.spine0[(j + 1) * 3 + 1], link.spine0[(j + 1) * 3 + 2],
                        col);
                    dd.Vertex(link.spine0[j * 3], link.spine0[j * 3 + 1], link.spine0[j * 3 + 2], col);
                }
            }

            dd.End();
            dd.Begin(LINES, 3.0f);
            foreach (JumpLink link in links)
            {
                for (int j = 0; j < link.nspine - 1; ++j)
                {
                    // int u = (j*255)/link.nspine;
                    int col = DuTransCol(DuDarkenCol(col1) /*DuDarkenCol(DuLerpCol(col0,col1,u))*/, 128);

                    dd.Vertex(link.spine0[j * 3], link.spine0[j * 3 + 1], link.spine0[j * 3 + 2], col);
                    dd.Vertex(link.spine0[(j + 1) * 3], link.spine0[(j + 1) * 3 + 1], link.spine0[(j + 1) * 3 + 2],
                        col);
                    dd.Vertex(link.spine1[j * 3], link.spine1[j * 3 + 1], link.spine1[j * 3 + 2], col);
                    dd.Vertex(link.spine1[(j + 1) * 3], link.spine1[(j + 1) * 3 + 1], link.spine1[(j + 1) * 3 + 2],
                        col);
                }

                dd.Vertex(link.spine0[0], link.spine0[1], link.spine0[2], DuDarkenCol(col1));
                dd.Vertex(link.spine1[0], link.spine1[1], link.spine1[2], DuDarkenCol(col1));

                dd.Vertex(link.spine0[(link.nspine - 1) * 3], link.spine0[(link.nspine - 1) * 3 + 1],
                    link.spine0[(link.nspine - 1) * 3 + 2], DuDarkenCol(col1));
                dd.Vertex(link.spine1[(link.nspine - 1) * 3], link.spine1[(link.nspine - 1) * 3 + 1],
                    link.spine1[(link.nspine - 1) * 3 + 2], DuDarkenCol(col1));
            }

            dd.End();
        }

        if (annotationBuilder != null)
        {
            foreach (JumpLink link in links)
            {
                if ((option.flags & JumpLinkBuilderToolParams.DRAW_ANIM_TRAJECTORY) != 0)
                {
                    float r = link.start.height;

                    int col = DuLerpCol(DuRGBA(255, 192, 0, 255),
                        DuRGBA(255, 255, 255, 255), 64);
                    int cola = DuTransCol(col, 192);
                    int colb = DuRGBA(255, 255, 255, 255);

                    // Start segment.
                    dd.Begin(LINES, 3.0f);
                    dd.Vertex(link.start.p, col);
                    dd.Vertex(link.start.q, col);
                    dd.End();

                    dd.Begin(LINES, 1.0f);
                    dd.Vertex(link.start.p.x, link.start.p.y, link.start.p.z, colb);
                    dd.Vertex(link.start.p.x, link.start.p.y + r, link.start.p.z, colb);
                    dd.Vertex(link.start.p.x, link.start.p.y + r, link.start.p.z, colb);
                    dd.Vertex(link.start.q.x, link.start.q.y + r, link.start.q.z, colb);
                    dd.Vertex(link.start.q.x, link.start.q.y + r, link.start.q.z, colb);
                    dd.Vertex(link.start.q.x, link.start.q.y, link.start.q.z, colb);
                    dd.Vertex(link.start.q.x, link.start.q.y, link.start.q.z, colb);
                    dd.Vertex(link.start.p.x, link.start.p.y, link.start.p.z, colb);
                    dd.End();

                    GroundSegment end = link.end;
                    r = end.height;
                    // End segment.
                    dd.Begin(LINES, 3.0f);
                    dd.Vertex(end.p, col);
                    dd.Vertex(end.q, col);
                    dd.End();

                    dd.Begin(LINES, 1.0f);
                    dd.Vertex(end.p.x, end.p.y, end.p.z, colb);
                    dd.Vertex(end.p.x, end.p.y + r, end.p.z, colb);
                    dd.Vertex(end.p.x, end.p.y + r, end.p.z, colb);
                    dd.Vertex(end.q.x, end.q.y + r, end.q.z, colb);
                    dd.Vertex(end.q.x, end.q.y + r, end.q.z, colb);
                    dd.Vertex(end.q.x, end.q.y, end.q.z, colb);
                    dd.Vertex(end.q.x, end.q.y, end.q.z, colb);
                    dd.Vertex(end.p.x, end.p.y, end.p.z, colb);
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

                if ((option.flags & JumpLinkBuilderToolParams.DRAW_LAND_SAMPLES) != 0)
                {
                    dd.Begin(POINTS, 8.0f);
                    for (int i = 0; i < link.start.gsamples.Length; ++i)
                    {
                        GroundSample s = link.start.gsamples[i];
                        float u = i / (float)(link.start.gsamples.Length - 1);
                        Vector3f spt = Vector3f.Lerp(link.start.p, link.start.q, u);
                        int col = DuRGBA(48, 16, 16, 255); // DuRGBA(255,(s->flags & 4)?255:0,0,255);
                        float off = 0.1f;
                        if (!s.validHeight)
                        {
                            off = 0;
                            col = DuRGBA(220, 32, 32, 255);
                        }

                        spt.y = s.p.y + off;
                        dd.Vertex(spt, col);
                    }

                    dd.End();

                    dd.Begin(POINTS, 4.0f);
                    for (int i = 0; i < link.start.gsamples.Length; ++i)
                    {
                        GroundSample s = link.start.gsamples[i];
                        float u = i / (float)(link.start.gsamples.Length - 1);
                        Vector3f spt = Vector3f.Lerp(link.start.p, link.start.q, u);
                        int col = DuRGBA(255, 255, 255, 255);
                        float off = 0;
                        if (s.validHeight)
                        {
                            off = 0.1f;
                        }

                        spt.y = s.p.y + off;
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
                            Vector3f spt = Vector3f.Lerp(end.p, end.q, u);
                            int col = DuRGBA(48, 16, 16, 255); // DuRGBA(255,(s->flags & 4)?255:0,0,255);
                            float off = 0.1f;
                            if (!s.validHeight)
                            {
                                off = 0;
                                col = DuRGBA(220, 32, 32, 255);
                            }

                            spt.y = s.p.y + off;
                            dd.Vertex(spt, col);
                        }

                        dd.End();
                        dd.Begin(POINTS, 4.0f);
                        for (int i = 0; i < end.gsamples.Length; ++i)
                        {
                            GroundSample s = end.gsamples[i];
                            float u = i / (float)(end.gsamples.Length - 1);
                            Vector3f spt = Vector3f.Lerp(end.p, end.q, u);
                            int col = DuRGBA(255, 255, 255, 255);
                            float off = 0;
                            if (s.validHeight)
                            {
                                off = 0.1f;
                            }

                            spt.y = s.p.y + off;
                            dd.Vertex(spt, col);
                        }

                        dd.End();
                    }
                }
            }
        }

        dd.DepthMask(true);
    }

    private void DrawTrajectory(RecastDebugDraw dd, JumpLink link, Vector3f pa, Vector3f pb, Trajectory tra, int cola)
    {
    }

    public override void HandleUpdate(float dt)
    {
    }

    public override void Layout()
    {
        if (0 >= sample.GetRecastResults().Count)
            return;

        ImGui.Text("Options");
        ImGui.Separator();
        ImGui.SliderFloat("Ground Tolerance", ref option.groundTolerance, 0f, 2f, "%.2f");
        ImGui.NewLine();

        ImGui.Text("Climb Down");
        ImGui.Separator();
        ImGui.SliderFloat("Distance", ref option.climbDownDistance, 0f, 5f, "%.2f");
        ImGui.SliderFloat("Min Cliff Height", ref option.climbDownMinHeight, 0f, 10f, "%.2f");
        ImGui.SliderFloat("Max Cliff Height", ref option.climbDownMaxHeight, 0f, 10f, "%.2f");
        ImGui.NewLine();

        ImGui.Text("Jump Down");
        ImGui.Separator();
        ImGui.SliderFloat("Max Distance", ref option.edgeJumpEndDistance, 0f, 10f, "%.2f");
        ImGui.SliderFloat("Jump Height", ref option.edgeJumpHeight, 0f, 10f, "%.2f");
        ImGui.SliderFloat("Max Jump Down", ref option.edgeJumpDownMaxHeight, 0f, 10f, "%.2f");
        ImGui.SliderFloat("Max Jump Up", ref option.edgeJumpUpMaxHeight, 0f, 10f, "%.2f");
        ImGui.NewLine();

        ImGui.Text("Mode");
        ImGui.Separator();
        //int buildTypes = 0;
        ImGui.CheckboxFlags("Climb Down", ref option.buildTypes, JumpLinkType.EDGE_CLIMB_DOWN.Bit);
        ImGui.CheckboxFlags("Edge Jump", ref option.buildTypes, JumpLinkType.EDGE_JUMP.Bit);
        //option.buildTypes = buildTypes;
        bool build = false;
        bool buildOffMeshConnections = false;
        if (ImGui.Button("Build"))
        {
            build = true;
        }

        if (ImGui.Button("Build Off-Mesh Links"))
        {
            buildOffMeshConnections = true;
        }

        if (build || buildOffMeshConnections)
        {
            if (annotationBuilder == null)
            {
                if (sample != null && 0 < sample.GetRecastResults().Count)
                {
                    annotationBuilder = new JumpLinkBuilder(sample.GetRecastResults());
                }
            }

            links.Clear();
            if (annotationBuilder != null)
            {
                float cellSize = sample.GetSettingsUI().GetCellSize();
                float agentHeight = sample.GetSettingsUI().GetAgentHeight();
                float agentRadius = sample.GetSettingsUI().GetAgentRadius();
                float agentClimb = sample.GetSettingsUI().GetAgentMaxClimb();
                float cellHeight = sample.GetSettingsUI().GetCellHeight();
                if ((option.buildTypes & JumpLinkType.EDGE_CLIMB_DOWN.Bit) != 0)
                {
                    JumpLinkBuilderConfig config = new JumpLinkBuilderConfig(cellSize, cellHeight, agentRadius,
                        agentHeight, agentClimb, option.groundTolerance, -agentRadius * 0.2f,
                        cellSize + 2 * agentRadius + option.climbDownDistance,
                        -option.climbDownMaxHeight, -option.climbDownMinHeight, 0);
                    links.AddRange(annotationBuilder.Build(config, JumpLinkType.EDGE_CLIMB_DOWN));
                }

                if ((option.buildTypes & JumpLinkType.EDGE_JUMP.Bit) != 0)
                {
                    JumpLinkBuilderConfig config = new JumpLinkBuilderConfig(cellSize, cellHeight, agentRadius,
                        agentHeight, agentClimb, option.groundTolerance, -agentRadius * 0.2f,
                        option.edgeJumpEndDistance, -option.edgeJumpDownMaxHeight,
                        option.edgeJumpUpMaxHeight, option.edgeJumpHeight);
                    links.AddRange(annotationBuilder.Build(config, JumpLinkType.EDGE_JUMP));
                }

                if (buildOffMeshConnections)
                {
                    DemoInputGeomProvider geom = sample.GetInputGeom();
                    if (geom != null)
                    {
                        int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP_AUTO;
                        geom.RemoveOffMeshConnections(c => c.area == area);
                        links.ForEach(l => AddOffMeshLink(l, geom, agentRadius));
                    }
                }
            }
        }

        ImGui.Text("Debug Draw Options");
        ImGui.Separator();
        //int newFlags = 0;
        ImGui.CheckboxFlags("Walkable Border", ref option.flags, JumpLinkBuilderToolParams.DRAW_WALKABLE_BORDER);
        ImGui.CheckboxFlags("Selected Edge", ref option.flags, JumpLinkBuilderToolParams.DRAW_SELECTED_EDGE);
        ImGui.CheckboxFlags("Anim Trajectory", ref option.flags, JumpLinkBuilderToolParams.DRAW_ANIM_TRAJECTORY);
        ImGui.CheckboxFlags("Land Samples", ref option.flags, JumpLinkBuilderToolParams.DRAW_LAND_SAMPLES);
        ImGui.CheckboxFlags("All Annotations", ref option.flags, JumpLinkBuilderToolParams.DRAW_ANNOTATIONS);
        //option.flags = newFlags;
    }

    private void AddOffMeshLink(JumpLink link, DemoInputGeomProvider geom, float agentRadius)
    {
        int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP_AUTO;
        int flags = SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP;
        Vector3f prev = new Vector3f();
        for (int i = 0; i < link.startSamples.Length; i++)
        {
            Vector3f p = link.startSamples[i].p;
            Vector3f q = link.endSamples[i].p;
            if (i == 0 || Vector3f.Dist2D(prev, p) > agentRadius)
            {
                geom.AddOffMeshConnection(p, q, agentRadius, false, area, flags);
                prev = p;
            }
        }
    }

    public override string GetName()
    {
        return "Annotation Builder";
    }
}
