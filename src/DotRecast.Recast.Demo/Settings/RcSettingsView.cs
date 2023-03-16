/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

using System.Numerics;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.UI;
using ImGuiNET;
using Silk.NET.Windowing;

namespace DotRecast.Recast.Demo.Settings;

public class RcSettingsView : IRcView
{
    private float cellSize = 0.3f;
    private float cellHeight = 0.2f;

    private float agentHeight = 2.0f;
    private float agentRadius = 0.6f;
    private float agentMaxClimb = 0.9f;
    private float agentMaxSlope = 45f;

    private int minRegionSize = 8;
    private int mergedRegionSize = 20;

    private PartitionType partitioning = PartitionType.WATERSHED;

    private bool filterLowHangingObstacles = true;
    private bool filterLedgeSpans = true;
    private bool filterWalkableLowHeightSpans = true;

    private readonly float[] edgeMaxLen = new[] { 12f };
    private readonly float[] edgeMaxError = new[] { 1.3f };
    private readonly int[] vertsPerPoly = new[] { 6 };

    private readonly float[] detailSampleDist = new[] { 6f };
    private readonly float[] detailSampleMaxError = new[] { 1f };

    private bool tiled = false;
    private readonly int[] tileSize = new[] { 32 };

    // public readonly NkColor white = NkColor.create();
    // public readonly NkColor background = NkColor.create();
    // public readonly NkColor transparent = NkColor.create();
    private bool buildTriggered;
    private long buildTime;
    private readonly int[] voxels = new int[2];
    private readonly int[] tiles = new int[2];
    private int maxTiles;
    private int maxPolys;

    private DrawMode drawMode = DrawMode.DRAWMODE_NAVMESH;
    private bool meshInputTrigerred;
    private bool navMeshInputTrigerred;

    public bool render(IWindow i, int x, int y, int width, int height, int mouseX, int mouseY)
    {
        ImGui.Begin("Properties");
        renderInternal(i, x, y, width, height, mouseX, mouseY);
        ImGui.End();

        return true;
    }

    public bool renderInternal(IWindow i, int x, int y, int width, int height, int mouseX, int mouseY)
    {
        bool mouseInside = false;
        ImGui.Text("Input Mesh");
        ImGui.Separator();
        ImGui.Button("Load Source Geom...");
        ImGui.Text($"Verts: {voxels[0]} Tris: {voxels[1]}");
        ImGui.NewLine();

        ImGui.Text("Rasterization");
        ImGui.Separator();

        ImGui.SliderFloat("Cell Size", ref cellSize, 0.01f, 1f, $"{cellSize}");
        ImGui.SliderFloat("Cell Height", ref cellHeight, 0.01f, 1f, $"{cellHeight}");
        ImGui.Text($"Voxels {voxels[0]} x {voxels[1]}");
        ImGui.NewLine();

        ImGui.Text("Agent");
        ImGui.Separator();
        ImGui.SliderFloat("Height", ref agentHeight, 5f, 0.1f, $"{agentHeight}");
        ImGui.SliderFloat("Radius", ref agentRadius, 5f, 0.1f, $"{agentRadius}");
        ImGui.SliderFloat("Max Climb", ref agentMaxClimb, 5f, 0.1f, $"{agentMaxClimb}");
        ImGui.SliderFloat("Max Slope", ref agentMaxSlope, 90f, 1f, $"{agentMaxSlope}");
        ImGui.NewLine();

        ImGui.Text("Region");
        ImGui.Separator();
        ImGui.SliderInt("Min Region Size", ref minRegionSize, 1, 150);
        ImGui.SliderInt("Merged Region Size", ref mergedRegionSize, 1, 150);
        //
        //         nk_layout_row_dynamic(ctx, 3, 1);
        //         nk_spacing(ctx, 1);
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, "Partitioning", NK_TEXT_ALIGN_LEFT);
        //         partitioning = NuklearUIHelper.nk_radio(ctx, PartitionType.values(), partitioning,
        //                 p => p.name().substring(0, 1) + p.name().substring(1).toLowerCase());
        //
        //         nk_layout_row_dynamic(ctx, 3, 1);
        //         nk_spacing(ctx, 1);
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, "Filtering", NK_TEXT_ALIGN_LEFT);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         filterLowHangingObstacles = nk_option_text(ctx, "Low Hanging Obstacles", filterLowHangingObstacles);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         filterLedgeSpans = nk_option_text(ctx, "Ledge Spans", filterLedgeSpans);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         filterWalkableLowHeightSpans = nk_option_text(ctx, "Walkable Low Height Spans",
        //                 filterWalkableLowHeightSpans);
        //
        //         nk_layout_row_dynamic(ctx, 3, 1);
        //         nk_spacing(ctx, 1);
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, "Polygonization", NK_TEXT_ALIGN_LEFT);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         nk_property_float(ctx, "Max Edge Length", 0f, edgeMaxLen, 50f, 0.1f, 0.1f);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         nk_property_float(ctx, "Max Edge Error", 0.1f, edgeMaxError, 3f, 0.1f, 0.1f);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         nk_property_int(ctx, "Vert Per Poly", 3, vertsPerPoly, 12, 1, 1);
        //
        //         nk_layout_row_dynamic(ctx, 3, 1);
        //         nk_spacing(ctx, 1);
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, "Detail Mesh", NK_TEXT_ALIGN_LEFT);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         nk_property_float(ctx, "Sample Distance", 0f, detailSampleDist, 16f, 0.1f, 0.1f);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         nk_property_float(ctx, "Max Sample Error", 0f, detailSampleMaxError, 16f, 0.1f, 0.1f);
        //
        //         nk_layout_row_dynamic(ctx, 3, 1);
        //         nk_spacing(ctx, 1);
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, "Tiling", NK_TEXT_ALIGN_LEFT);
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         tiled = nk_check_text(ctx, "Enable", tiled);
        //         if (tiled) {
        //             nk_layout_row_dynamic(ctx, 20, 1);
        //             nk_property_int(ctx, "Tile Size", 16, tileSize, 1024, 16, 16);
        //             nk_layout_row_dynamic(ctx, 18, 1);
        //             nk_label(ctx, string.format("Tiles %d x %d", tiles[0], tiles[1]), NK_TEXT_ALIGN_RIGHT);
        //             nk_layout_row_dynamic(ctx, 18, 1);
        //             nk_label(ctx, string.format("Max Tiles %d", maxTiles), NK_TEXT_ALIGN_RIGHT);
        //             nk_layout_row_dynamic(ctx, 18, 1);
        //             nk_label(ctx, string.format("Max Polys %d", maxPolys), NK_TEXT_ALIGN_RIGHT);
        //         }
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, string.format("Build Time: %d ms", buildTime), NK_TEXT_ALIGN_LEFT);
        //
        //         nk_layout_row_dynamic(ctx, 20, 1);
        //         buildTriggered = nk_button_text(ctx, "Build");
        //         nk_layout_row_dynamic(ctx, 3, 1);
        //         nk_spacing(ctx, 1);
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         navMeshInputTrigerred = nk_button_text(ctx, "Load Nav Mesh...");
        //
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, "Draw", NK_TEXT_ALIGN_LEFT);
        //         drawMode = NuklearUIHelper.nk_radio(ctx, DrawMode.values(), drawMode, dm => dm.toString());
        //
        //         nk_window_get_bounds(ctx, rect);
        //         if (mouseX >= rect.x() && mouseX <= rect.x() + rect.w() && mouseY >= rect.y()
        //                 && mouseY <= rect.y() + rect.h()) {
        //             mouseInside = true;
        //         }
        //     }
        //     nk_end(ctx);
        // }
        return mouseInside;
    }

    public float getCellSize()
    {
        return cellSize;
    }

    public float getCellHeight()
    {
        return cellHeight;
    }

    public float getAgentHeight()
    {
        return agentHeight;
    }

    public float getAgentRadius()
    {
        return agentRadius;
    }

    public float getAgentMaxClimb()
    {
        return agentMaxClimb;
    }

    public float getAgentMaxSlope()
    {
        return agentMaxSlope;
    }

    public int getMinRegionSize()
    {
        return minRegionSize;
    }

    public int getMergedRegionSize()
    {
        return mergedRegionSize;
    }

    public PartitionType getPartitioning()
    {
        return partitioning;
    }

    public bool isBuildTriggered()
    {
        return buildTriggered;
    }

    public bool isFilterLowHangingObstacles()
    {
        return filterLowHangingObstacles;
    }

    public bool isFilterLedgeSpans()
    {
        return filterLedgeSpans;
    }

    public bool isFilterWalkableLowHeightSpans()
    {
        return filterWalkableLowHeightSpans;
    }

    public void setBuildTime(long buildTime)
    {
        this.buildTime = buildTime;
    }

    public DrawMode getDrawMode()
    {
        return drawMode;
    }

    public float getEdgeMaxLen()
    {
        return edgeMaxLen[0];
    }

    public float getEdgeMaxError()
    {
        return edgeMaxError[0];
    }

    public int getVertsPerPoly()
    {
        return vertsPerPoly[0];
    }

    public float getDetailSampleDist()
    {
        return detailSampleDist[0];
    }

    public float getDetailSampleMaxError()
    {
        return detailSampleMaxError[0];
    }

    public void setVoxels(int[] voxels)
    {
        this.voxels[0] = voxels[0];
        this.voxels[1] = voxels[1];
    }

    public bool isTiled()
    {
        return tiled;
    }

    public int getTileSize()
    {
        return tileSize[0];
    }

    public void setTiles(int[] tiles)
    {
        this.tiles[0] = tiles[0];
        this.tiles[1] = tiles[1];
    }

    public void setMaxTiles(int maxTiles)
    {
        this.maxTiles = maxTiles;
    }

    public void setMaxPolys(int maxPolys)
    {
        this.maxPolys = maxPolys;
    }

    public bool isMeshInputTrigerred()
    {
        return meshInputTrigerred;
    }

    public bool isNavMeshInputTrigerred()
    {
        return navMeshInputTrigerred;
    }
}