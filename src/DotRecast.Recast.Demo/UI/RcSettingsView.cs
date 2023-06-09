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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.UI;
using ImGuiNET;
using Silk.NET.Windowing;

namespace DotRecast.Recast.Demo.UI;

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

    private int partitioningIdx = 0;
    private PartitionType partitioning = PartitionType.WATERSHED;

    private bool filterLowHangingObstacles = true;
    private bool filterLedgeSpans = true;
    private bool filterWalkableLowHeightSpans = true;

    private float edgeMaxLen = 12f;
    private float edgeMaxError = 1.3f;
    private int vertsPerPoly = 6;

    private float detailSampleDist = 6f;
    private float detailSampleMaxError = 1f;

    private bool tiled = false;
    private int tileSize = 32;

    // public readonly NkColor white = NkColor.Create();
    // public readonly NkColor background = NkColor.Create();
    // public readonly NkColor transparent = NkColor.Create();
    private bool buildTriggered;
    private long buildTime;
    private readonly int[] voxels = new int[2];
    private readonly int[] tiles = new int[2];
    private int maxTiles;
    private int maxPolys;

    private int drawMode = DrawMode.DRAWMODE_NAVMESH.Idx;

    private string meshInputFilePath;
    private bool meshInputTrigerred;
    private bool navMeshInputTrigerred;

    private bool _mouseInside;
    public bool IsMouseInside() => _mouseInside;

    private RecastDemoCanvas _canvas;

    public void Bind(RecastDemoCanvas canvas)
    {
        _canvas = canvas;
    }

    public void Update(double dt)
    {
        
    }

    public void Draw(double dt)
    {
        int width = 310;
        var posX = _canvas.Size.X - width;
        ImGui.SetNextWindowPos(new Vector2(posX, 0));
        ImGui.SetNextWindowSize(new Vector2(width, _canvas.Size.Y));
        ImGui.Begin("Properties", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        _mouseInside = ImGui.IsWindowHovered();

        ImGui.Text("Input Mesh");
        ImGui.Separator();

        const string strLoadSourceGeom = "Load Source Geom...";
        if (ImGui.Button(strLoadSourceGeom))
        {
            ImGui.OpenPopup(strLoadSourceGeom);
        }

        bool loadSourceGeomPopup = true;
        if (ImGui.BeginPopupModal(strLoadSourceGeom, ref loadSourceGeomPopup, ImGuiWindowFlags.NoTitleBar))
        {
            var picker = ImFilePicker.GetFilePicker(strLoadSourceGeom, Path.Combine(Environment.CurrentDirectory), ".obj");
            if (picker.Draw())
            {
                meshInputTrigerred = true;
                meshInputFilePath = picker.SelectedFile;
                ImFilePicker.RemoveFilePicker(strLoadSourceGeom);
            }

            ImGui.EndPopup();
        }
        else
        {
            meshInputTrigerred = false;
        }

        ImGui.Text($"Verts: {voxels[0]} Tris: {voxels[1]}");
        ImGui.NewLine();

        ImGui.Text("Rasterization");
        ImGui.Separator();

        ImGui.SliderFloat("Cell Size", ref cellSize, 0.01f, 1f, "%.2f");
        ImGui.SliderFloat("Cell Height", ref cellHeight, 0.01f, 1f, "%.2f");
        ImGui.Text($"Voxels {voxels[0]} x {voxels[1]}");
        ImGui.NewLine();

        ImGui.Text("Agent");
        ImGui.Separator();
        ImGui.SliderFloat("Height", ref agentHeight, 0.1f, 5f, "%.1f");
        ImGui.SliderFloat("Radius", ref agentRadius, 0.1f, 5f, "%.1f");
        ImGui.SliderFloat("Max Climb", ref agentMaxClimb, 0.1f, 5f, "%.1f");
        ImGui.SliderFloat("Max Slope", ref agentMaxSlope, 1f, 90f, "%.0f");
        ImGui.NewLine();

        ImGui.Text("Region");
        ImGui.Separator();
        ImGui.SliderInt("Min Region Size", ref minRegionSize, 1, 150);
        ImGui.SliderInt("Merged Region Size", ref mergedRegionSize, 1, 150);
        ImGui.NewLine();

        ImGui.Text("Partitioning");
        ImGui.Separator();
        PartitionType.Values.ForEach(partition =>
        {
            var label = partition.Name.Substring(0, 1).ToUpper()
                        + partition.Name.Substring(1).ToLower();
            ImGui.RadioButton(label, ref partitioningIdx, partition.Idx);
        });
        ImGui.NewLine();

        ImGui.Text("Filtering");
        ImGui.Separator();
        ImGui.Checkbox("Low Hanging Obstacles", ref filterLowHangingObstacles);
        ImGui.Checkbox("Ledge Spans", ref filterLedgeSpans);
        ImGui.Checkbox("Walkable Low Height Spans", ref filterWalkableLowHeightSpans);
        ImGui.NewLine();

        ImGui.Text("Polygonization");
        ImGui.Separator();
        ImGui.SliderFloat("Max Edge Length", ref edgeMaxLen, 0f, 50f, "%.1f");
        ImGui.SliderFloat("Max Edge Error", ref edgeMaxError, 0.1f, 3f, "%.1f");
        ImGui.SliderInt("Vert Per Poly", ref vertsPerPoly, 3, 12);
        ImGui.NewLine();

        ImGui.Text("Detail Mesh");
        ImGui.Separator();
        ImGui.SliderFloat("Sample Distance", ref detailSampleDist, 0f, 16f, "%.1f");
        ImGui.SliderFloat("Max Sample Error", ref detailSampleMaxError, 0f, 16f, "%.1f");
        ImGui.NewLine();

        ImGui.Text("Tiling");
        ImGui.Separator();
        ImGui.Checkbox("Enable", ref tiled);
        if (tiled)
        {
            if (0 < (tileSize % 16))
                tileSize = tileSize + (16 - (tileSize % 16));
            ImGui.SliderInt("Tile Size", ref tileSize, 16, 1024);

            ImGui.Text($"Tiles {tiles[0]} x {tiles[1]}");
            ImGui.Text($"Max Tiles {maxTiles}");
            ImGui.Text($"Max Polys {maxPolys}");
        }

        ImGui.NewLine();

        ImGui.Text($"Build Time: {buildTime} ms");

        ImGui.Separator();
        buildTriggered = ImGui.Button("Build Nav Mesh");
        const string strLoadNavMesh = "Load Nav Mesh...";
        if (ImGui.Button(strLoadNavMesh))
        {
            ImGui.OpenPopup(strLoadNavMesh);
        }

        bool isLoadNavMesh = true;
        if (ImGui.BeginPopupModal(strLoadNavMesh, ref isLoadNavMesh, ImGuiWindowFlags.NoTitleBar))
        {
            var picker = ImFilePicker.GetFilePicker(strLoadNavMesh, Path.Combine(Environment.CurrentDirectory));
            if (picker.Draw())
            {
                Console.WriteLine(picker.SelectedFile);
                ImFilePicker.RemoveFilePicker(strLoadNavMesh);
            }

            ImGui.EndPopup();
        }

        ImGui.NewLine();

        ImGui.Text("Draw");
        ImGui.Separator();

        DrawMode.Values.ForEach(dm => { ImGui.RadioButton(dm.Text, ref drawMode, dm.Idx); });
        ImGui.NewLine();

        ImGui.End();
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public float GetCellHeight()
    {
        return cellHeight;
    }

    public float GetAgentHeight()
    {
        return agentHeight;
    }

    public float GetAgentRadius()
    {
        return agentRadius;
    }

    public float GetAgentMaxClimb()
    {
        return agentMaxClimb;
    }

    public float GetAgentMaxSlope()
    {
        return agentMaxSlope;
    }

    public int GetMinRegionSize()
    {
        return minRegionSize;
    }

    public int GetMergedRegionSize()
    {
        return mergedRegionSize;
    }

    public PartitionType GetPartitioning()
    {
        return partitioning;
    }

    public bool IsBuildTriggered()
    {
        return buildTriggered;
    }

    public bool IsFilterLowHangingObstacles()
    {
        return filterLowHangingObstacles;
    }

    public bool IsFilterLedgeSpans()
    {
        return filterLedgeSpans;
    }

    public bool IsFilterWalkableLowHeightSpans()
    {
        return filterWalkableLowHeightSpans;
    }

    public void SetBuildTime(long buildTime)
    {
        this.buildTime = buildTime;
    }
    
    public DrawMode GetDrawMode()
    {
        return DrawMode.Values[drawMode];
    }

    public float GetEdgeMaxLen()
    {
        return edgeMaxLen;
    }

    public float GetEdgeMaxError()
    {
        return edgeMaxError;
    }

    public int GetVertsPerPoly()
    {
        return vertsPerPoly;
    }

    public float GetDetailSampleDist()
    {
        return detailSampleDist;
    }

    public float GetDetailSampleMaxError()
    {
        return detailSampleMaxError;
    }

    public void SetVoxels(int gw, int gh)
    {
        voxels[0] = gw;
        voxels[1] = gh;
    }

    public bool IsTiled()
    {
        return tiled;
    }

    public int GetTileSize()
    {
        return tileSize;
    }

    public void SetTiles(int[] tiles)
    {
        this.tiles[0] = tiles[0];
        this.tiles[1] = tiles[1];
    }

    public void SetMaxTiles(int maxTiles)
    {
        this.maxTiles = maxTiles;
    }

    public void SetMaxPolys(int maxPolys)
    {
        this.maxPolys = maxPolys;
    }

    public bool IsMeshInputTrigerred()
    {
        return meshInputTrigerred;
    }

    public string GetMeshInputFilePath()
    {
        return meshInputFilePath;
    }

    public bool IsNavMeshInputTrigerred()
    {
        return navMeshInputTrigerred;
    }
}
