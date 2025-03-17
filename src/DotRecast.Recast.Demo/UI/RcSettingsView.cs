/*
recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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
using System.IO;
using System.Numerics;
using DotRecast.Core.Collections;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Messages;
using ImGuiNET;
using Serilog;

namespace DotRecast.Recast.Demo.UI;

public class RcSettingsView : IRcView
{
    private static readonly ILogger Logger = Log.ForContext<RcSettingsView>();

    private readonly IRecastDemoChannel _channel;
    private long buildTime;

    private readonly int[] voxels = new int[2];
    private readonly int[] tiles = new int[2];
    private int maxTiles;
    private int maxPolys;

    private int drawMode = DrawMode.DRAWMODE_NAVMESH.Idx;

    public bool RenderAsLeftHanded => _renderAsLeftHanded;
    private bool _renderAsLeftHanded = false;

    private DemoSample _sample;
    private RcCanvas _canvas;

    public RcSettingsView(IRecastDemoChannel channel)
    {
        _channel = channel;
    }

    public void SetSample(DemoSample sample)
    {
        _sample = sample;
    }

    public void Bind(RcCanvas canvas)
    {
        _canvas = canvas;
    }

    public void Update(double dt)
    {
    }



    public void Draw(double dt)
    {
        var settings = _sample.GetSettings();

        ImGui.SetNextWindowPos(new Vector2(_canvas.Size.X - _canvas.Layout.PropertiesMenuWidth - _canvas.Layout.WidthPadding, _canvas.Layout.TopPadding));
        ImGui.SetNextWindowSize(new Vector2(_canvas.Layout.PropertiesMenuWidth, _canvas.Size.Y - _canvas.Layout.BottomPadding));
        
        ImGui.Begin("Properties", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

        ImGui.Checkbox("Render As Left-Handed", ref _renderAsLeftHanded);

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
                _channel.SendMessage(new GeomLoadBeganEvent()
                {
                    FilePath = picker.SelectedFile,
                });
                ImFilePicker.RemoveFilePicker(strLoadSourceGeom);
            }

            ImGui.EndPopup();
        }

        ImGui.Text($"Verts: {voxels[0]} Tris: {voxels[1]}");
        ImGui.NewLine();

        ImGui.Text("Rasterization");
        ImGui.Separator();

        ImGui.SliderFloat("Cell Size", ref settings.cellSize, 0.01f, 1f, "%.2f");
        ImGui.SliderFloat("Cell Height", ref settings.cellHeight, 0.01f, 1f, "%.2f");
        ImGui.Text($"Voxels {voxels[0]} x {voxels[1]}");
        ImGui.NewLine();

        ImGui.Text("Agent");
        ImGui.Separator();
        ImGui.SliderFloat("Height", ref settings.agentHeight, 0.1f, 5f, "%.1f");
        ImGui.SliderFloat("Radius", ref settings.agentRadius, 0.0f, 5f, "%.1f");
        ImGui.SliderFloat("Max Climb", ref settings.agentMaxClimb, 0.1f, 5f, "%.1f");
        ImGui.SliderFloat("Max Slope", ref settings.agentMaxSlope, 1f, 90f, "%.0f");
        ImGui.SliderFloat("Max Acceleration", ref settings.agentMaxAcceleration, 8f, 999f, "%.1f");
        ImGui.SliderFloat("Max Speed", ref settings.agentMaxSpeed, 1f, 10f, "%.1f");

        ImGui.NewLine();

        ImGui.Text("Region");
        ImGui.Separator();
        ImGui.SliderInt("Min Region Size", ref settings.minRegionSize, 1, 150);
        ImGui.SliderInt("Merged Region Size", ref settings.mergedRegionSize, 1, 150);
        ImGui.NewLine();

        ImGui.Text("Partitioning");
        ImGui.Separator();
        RcPartitionType.Values.ForEach(partition =>
        {
            var label = partition.Name.Substring(0, 1).ToUpper() + partition.Name.Substring(1).ToLower();
            ImGui.RadioButton(label, ref settings.partitioning, partition.Value);
        });
        ImGui.NewLine();

        ImGui.Text("Filtering");
        ImGui.Separator();
        ImGui.Checkbox("Low Hanging Obstacles", ref settings.filterLowHangingObstacles);
        ImGui.Checkbox("Ledge Spans", ref settings.filterLedgeSpans);
        ImGui.Checkbox("Walkable Low Height Spans", ref settings.filterWalkableLowHeightSpans);
        ImGui.NewLine();

        ImGui.Text("Polygonization");
        ImGui.Separator();
        ImGui.SliderFloat("Max Edge Length", ref settings.edgeMaxLen, 0f, 50f, "%.1f");
        ImGui.SliderFloat("Max Edge Error", ref settings.edgeMaxError, 0.1f, 3f, "%.1f");
        ImGui.SliderInt("Vert Per Poly", ref settings.vertsPerPoly, 3, 12);
        ImGui.NewLine();

        ImGui.Text("Detail Mesh");
        ImGui.Separator();
        ImGui.SliderFloat("Sample Distance", ref settings.detailSampleDist, 0f, 16f, "%.1f");
        ImGui.SliderFloat("Max Sample Error", ref settings.detailSampleMaxError, 0f, 16f, "%.1f");
        ImGui.NewLine();

        ImGui.Checkbox("Keep Itermediate Results", ref settings.keepInterResults);
        ImGui.Checkbox("Build All Tiles", ref settings.buildAll);
        ImGui.NewLine();

        ImGui.Text("Tiling");
        ImGui.Separator();
        ImGui.Checkbox("Enable", ref settings.tiled);
        if (settings.tiled)
        {
            if (0 < (settings.tileSize % 16))
                settings.tileSize = settings.tileSize + (16 - (settings.tileSize % 16));
            ImGui.SliderInt("Tile Size", ref settings.tileSize, 16, 1024);

            ImGui.Text($"Tiles {tiles[0]} x {tiles[1]}");
            ImGui.Text($"Max Tiles {maxTiles}");
            ImGui.Text($"Max Polys {maxPolys}");
        }

        ImGui.NewLine();

        ImGui.Text($"Build Time: {buildTime} ms");

        ImGui.Separator();
        if (ImGui.Button("Build NavMesh"))
        {
            _channel.SendMessage(new NavMeshBuildBeganEvent());
        }

        {
            const string strLoadNavMesh = "Load NavMesh";
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
                    _channel.SendMessage(new NavMeshLoadBeganEvent()
                    {
                        FilePath = picker.SelectedFile,
                    });
                    ImFilePicker.RemoveFilePicker(strLoadNavMesh);
                }

                ImGui.EndPopup();
            }
        }

        if (ImGui.Button("Save NavMesh"))
        {
            _channel.SendMessage(new NavMeshSaveBeganEvent());
        }


        ImGui.NewLine();

        ImGui.Text("Draw");
        ImGui.Separator();

        DrawMode.Values.ForEach(dm => { ImGui.RadioButton(dm.Text, ref drawMode, dm.Idx); });
        ImGui.NewLine();

        ImGui.Separator();
        ImGui.Text("Tick 'Keep Itermediate Results'");
        ImGui.Text("rebuild some tiles to see");
        ImGui.Text("more debug mode options.");
        ImGui.NewLine();

        ImGui.End();
    }

    public void SetBuildTime(long buildTime)
    {
        this.buildTime = buildTime;
    }

    public DrawMode GetDrawMode()
    {
        return DrawMode.Values[drawMode];
    }

    public void SetVoxels(int gw, int gh)
    {
        voxels[0] = gw;
        voxels[1] = gh;
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
}