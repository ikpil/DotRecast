/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour.Dynamic;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Tools;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.UI;
using DotRecast.Recast.Toolset.Geom;
using ImGuiNET;
using Serilog;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class DynamicUpdateSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<DynamicUpdateSampleTool>();

    private DemoSample _sample;
    private readonly RcDynamicUpdateTool _tool;

    private RcDynamicUpdateToolMode mode = RcDynamicUpdateToolMode.BUILD;
    private float cellSize = 0.3f;

    // build config
    private int partitioning = RcPartitionType.WATERSHED.Value;
    private float walkableSlopeAngle = 45f;
    private float walkableHeight = 2f;
    private float walkableRadius = 0.6f;
    private float walkableClimb = 0.9f;

    private float minRegionArea = 6f;
    private float regionMergeSize = 36f;

    private float maxEdgeLen = 12f;
    private float maxSimplificationError = 1.3f;
    private int vertsPerPoly = 6;

    private float detailSampleDist = 6f;
    private float detailSampleMaxError = 1f;

    private bool filterLowHangingObstacles = true;
    private bool filterLedgeSpans = true;
    private bool filterWalkableLowHeightSpans = true;

    private bool buildDetailMesh = true;

    private bool compression = true;
    private bool showColliders = false;
    private long buildTime;
    private long raycastTime;

    private RcDynamicColliderShape colliderShape = RcDynamicColliderShape.SPHERE;

    private readonly TaskFactory executor;

    private bool sposSet;
    private bool eposSet;
    private RcVec3f spos;
    private RcVec3f epos;
    private bool raycastHit;
    private RcVec3f raycastHitPos;

    public DynamicUpdateSampleTool()
    {
        var bridgeGeom = DemoInputGeomProvider.LoadFile("bridge.obj");
        var houseGeom = DemoInputGeomProvider.LoadFile("house.obj");
        var convexGeom = DemoInputGeomProvider.LoadFile("convex.obj");
        _tool = new(Random.Shared, bridgeGeom, houseGeom, convexGeom);
        executor = Task.Factory;
    }

    public void Layout()
    {
        var prevModeIdx = mode.Idx;

        ImGui.Text($"Dynamic Update Tool Modes");
        ImGui.Separator();
        ImGui.RadioButton(RcDynamicUpdateToolMode.BUILD.Label, ref prevModeIdx, RcDynamicUpdateToolMode.BUILD.Idx);
        ImGui.RadioButton(RcDynamicUpdateToolMode.COLLIDERS.Label, ref prevModeIdx, RcDynamicUpdateToolMode.COLLIDERS.Idx);
        ImGui.RadioButton(RcDynamicUpdateToolMode.RAYCAST.Label, ref prevModeIdx, RcDynamicUpdateToolMode.RAYCAST.Idx);
        ImGui.NewLine();

        if (prevModeIdx != mode.Idx)
        {
            mode = RcDynamicUpdateToolMode.Values[prevModeIdx];
        }

        ImGui.Text($"Selected mode - {mode.Label}");
        ImGui.Separator();

        if (mode == RcDynamicUpdateToolMode.BUILD)
        {
            var loadVoxelPopupStrId = "Load Voxels Popup";
            bool isLoadVoxelPopup = true;
            if (ImGui.Button("Load Voxels..."))
            {
                ImGui.OpenPopup(loadVoxelPopupStrId);
            }

            if (ImGui.BeginPopupModal(loadVoxelPopupStrId, ref isLoadVoxelPopup, ImGuiWindowFlags.NoTitleBar))
            {
                var picker = ImFilePicker.GetFilePicker(loadVoxelPopupStrId, Path.Combine(Environment.CurrentDirectory), ".voxels");
                if (picker.Draw())
                {
                    Load(picker.SelectedFile);
                    ImFilePicker.RemoveFilePicker(loadVoxelPopupStrId);
                }

                ImGui.EndPopup();
            }

            var saveVoxelPopupStrId = "Save Voxels Popup";
            bool isSaveVoxelPopup = true;

            var dynaMesh = _tool.GetDynamicNavMesh();
            if (dynaMesh != null)
            {
                ImGui.Checkbox("Compression", ref compression);
                if (ImGui.Button("Save Voxels..."))
                {
                    ImGui.BeginPopup(saveVoxelPopupStrId);
                }

                if (ImGui.BeginPopupModal(saveVoxelPopupStrId, ref isSaveVoxelPopup, ImGuiWindowFlags.NoTitleBar))
                {
                    var picker = ImFilePicker.GetFilePicker(saveVoxelPopupStrId, Path.Combine(Environment.CurrentDirectory), ".voxels");
                    if (picker.Draw())
                    {
                        if (string.IsNullOrEmpty(picker.SelectedFile))
                            Save(picker.SelectedFile);

                        ImFilePicker.RemoveFilePicker(saveVoxelPopupStrId);
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.NewLine();

            ImGui.Text("Rasterization");
            ImGui.Separator();
            ImGui.Text($"Cell Size - {cellSize}");
            ImGui.NewLine();

            ImGui.Text("Agent");
            ImGui.Separator();
            ImGui.SliderFloat("Height", ref walkableHeight, 0f, 5f, "%.2f");
            ImGui.SliderFloat("Radius", ref walkableRadius, 0f, 10f, "%.2f");
            ImGui.SliderFloat("Max Climb", ref walkableClimb, 0f, 10f, "%.2f");
            ImGui.Text($"Max Slope : {walkableSlopeAngle}");
            ImGui.NewLine();

            ImGui.Text("Partitioning");
            ImGui.Separator();
            RcPartitionType.Values.ForEach(partition =>
            {
                var label = partition.Name.Substring(0, 1).ToUpper()
                            + partition.Name.Substring(1).ToLower();
                ImGui.RadioButton(label, ref partitioning, partition.Value);
            });
            ImGui.NewLine();

            ImGui.Text("Filtering");
            ImGui.Separator();
            ImGui.Checkbox("Low Hanging Obstacles", ref filterLowHangingObstacles);
            ImGui.Checkbox("Ledge Spans", ref filterLedgeSpans);
            ImGui.Checkbox("Walkable Low Height Spans", ref filterWalkableLowHeightSpans);
            ImGui.NewLine();

            ImGui.Text("Region");
            ImGui.Separator();
            ImGui.SliderFloat("Min Region Size", ref minRegionArea, 0, 150, "%.1f");
            ImGui.SliderFloat("Merged Region Size", ref regionMergeSize, 0, 400, "%.1f");
            ImGui.NewLine();

            ImGui.Text("Polygonization");
            ImGui.Separator();
            ImGui.SliderFloat("Max Edge Length", ref maxEdgeLen, 0f, 50f, "%.1f");
            ImGui.SliderFloat("Max Edge Error", ref maxSimplificationError, 0.1f, 10f, "%.1f");
            ImGui.SliderInt("Verts Per Poly", ref vertsPerPoly, 3, 12);
            ImGui.NewLine();

            ImGui.Text("Detail Mesh");
            ImGui.Separator();
            ImGui.Checkbox("Enable", ref buildDetailMesh);
            ImGui.SliderFloat("Sample Distance", ref detailSampleDist, 0f, 16f, "%.1f");
            ImGui.SliderFloat("Max Sample Error", ref detailSampleMaxError, 0f, 16f, "%.1f");
            ImGui.NewLine();

            if (ImGui.Button("Build Dynamic mesh"))
            {
                if (dynaMesh != null)
                {
                    BuildDynaMesh();
                    _sample.SetChanged(false);
                }
            }
        }

        if (mode == RcDynamicUpdateToolMode.COLLIDERS)
        {
            var prevColliderShape = (int)colliderShape;

            ImGui.Text("Colliders");
            ImGui.Separator();
            ImGui.Checkbox("Show", ref showColliders);
            ImGui.RadioButton("Sphere", ref prevColliderShape, (int)RcDynamicColliderShape.SPHERE);
            ImGui.RadioButton("Capsule", ref prevColliderShape, (int)RcDynamicColliderShape.CAPSULE);
            ImGui.RadioButton("Box", ref prevColliderShape, (int)RcDynamicColliderShape.BOX);
            ImGui.RadioButton("Cylinder", ref prevColliderShape, (int)RcDynamicColliderShape.CYLINDER);
            ImGui.RadioButton("Composite", ref prevColliderShape, (int)RcDynamicColliderShape.COMPOSITE);
            ImGui.RadioButton("Convex Trimesh", ref prevColliderShape, (int)RcDynamicColliderShape.CONVEX);
            ImGui.RadioButton("Trimesh Bridge", ref prevColliderShape, (int)RcDynamicColliderShape.TRIMESH_BRIDGE);
            ImGui.RadioButton("Trimesh House", ref prevColliderShape, (int)RcDynamicColliderShape.TRIMESH_HOUSE);
            ImGui.NewLine();

            if (prevColliderShape != (int)colliderShape)
            {
                colliderShape = (RcDynamicColliderShape)prevColliderShape;
            }
        }

        if (mode == RcDynamicUpdateToolMode.RAYCAST)
        {
            ImGui.Text($"Raycast Time: {raycastTime} ms");
            ImGui.Separator();
            if (sposSet)
            {
                ImGui.Text($"Start: {spos.X}, {spos.Y + 1.3f}, {spos.Z}");
            }

            if (eposSet)
            {
                ImGui.Text($"End: {epos.X}, {epos.Y + 1.3f}, {epos.Z}");
            }

            if (raycastHit)
            {
                ImGui.Text($"Hit: {raycastHitPos.X}, {raycastHitPos.Y}, {raycastHitPos.Z}");
            }

            ImGui.NewLine();
        }
        else
        {
            ImGui.Text($"Build Time: {buildTime} ms");
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        if (mode == RcDynamicUpdateToolMode.COLLIDERS)
        {
            if (showColliders)
            {
                foreach (var gizmo in _tool.GetGizmos())
                {
                    GizmoRenderer.Render(renderer.GetDebugDraw(), gizmo.Gizmo);
                }
            }
        }

        if (mode == RcDynamicUpdateToolMode.RAYCAST)
        {
            RecastDebugDraw dd = renderer.GetDebugDraw();
            int startCol = DuRGBA(128, 25, 0, 192);
            int endCol = DuRGBA(51, 102, 0, 129);
            if (sposSet)
            {
                DrawAgent(dd, spos, startCol);
            }

            if (eposSet)
            {
                DrawAgent(dd, epos, endCol);
            }

            dd.DepthMask(false);
            if (raycastHitPos != RcVec3f.Zero)
            {
                int spathCol = raycastHit ? DuRGBA(128, 32, 16, 220) : DuRGBA(64, 128, 240, 220);
                dd.Begin(LINES, 2.0f);
                dd.Vertex(spos.X, spos.Y + 1.3f, spos.Z, spathCol);
                dd.Vertex(raycastHitPos.X, raycastHitPos.Y, raycastHitPos.Z, spathCol);
                dd.End();
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
        if (mode == RcDynamicUpdateToolMode.COLLIDERS)
        {
            if (!shift)
            {
                _tool.AddShape(colliderShape, p);
            }
        }

        if (mode == RcDynamicUpdateToolMode.RAYCAST)
        {
            if (shift)
            {
                sposSet = true;
                spos = p;
            }
            else
            {
                eposSet = true;
                epos = p;
            }

            var dynaMesh = _tool.GetDynamicNavMesh();
            if (sposSet && eposSet && dynaMesh != null)
            {
                long t1 = RcFrequency.Ticks;
                bool hasHit = _tool.Raycast(spos, epos, out var hitPos, out raycastHitPos);
                long t2 = RcFrequency.Ticks;
                raycastTime = (t2 - t1) / TimeSpan.TicksPerMillisecond;
                raycastHit = hasHit;
            }
        }
    }


    public void HandleClickRay(RcVec3f start, RcVec3f dir, bool shift)
    {
        if (mode == RcDynamicUpdateToolMode.COLLIDERS)
        {
            if (shift)
            {
                _tool.RemoveShape(start, dir);
            }
        }
    }

    public void HandleUpdate(float dt)
    {
        long t = RcFrequency.Ticks;
        try
        {
            bool updated = _tool.UpdateDynaMesh(executor);
            if (updated)
            {
                buildTime = (RcFrequency.Ticks - t) / TimeSpan.TicksPerMillisecond;
                var dynaMesh = _tool.GetDynamicNavMesh();
                _sample.Update(null, dynaMesh.RecastResults(), dynaMesh.NavMesh());
                _sample.SetChanged(false);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "");
        }
    }


    private void Load(string filename)
    {
        try
        {
            var dynaMesh = _tool.Load(filename, DtVoxelTileLZ4DemoCompressor.Shared);

            UpdateFrom(dynaMesh.config);
            BuildDynaMesh();
        }
        catch (Exception e)
        {
            Logger.Error(e, "");
        }
    }


    private void Save(string filename)
    {
        _tool.Save(filename, compression, DtVoxelTileLZ4DemoCompressor.Shared);
    }

    private void BuildDynaMesh()
    {
        var dynaMesh = _tool.GetDynamicNavMesh();
        UpdateTo(dynaMesh.config);
        long t = RcFrequency.Ticks;
        try
        {
            var _ = dynaMesh.Build(executor).Result;
        }
        catch (Exception e)
        {
            Logger.Error(e, "");
        }

        buildTime = (RcFrequency.Ticks - t) / TimeSpan.TicksPerMillisecond;
        _sample.Update(null, dynaMesh.RecastResults(), dynaMesh.NavMesh());
    }

    private void UpdateTo(DtDynamicNavMeshConfig config)
    {
        config.partition = partitioning;
        config.walkableHeight = walkableHeight;
        config.walkableSlopeAngle = walkableSlopeAngle;
        config.walkableRadius = walkableRadius;
        config.walkableClimb = walkableClimb;
        config.filterLowHangingObstacles = filterLowHangingObstacles;
        config.filterLedgeSpans = filterLedgeSpans;
        config.filterWalkableLowHeightSpans = filterWalkableLowHeightSpans;
        config.minRegionArea = minRegionArea;
        config.regionMergeArea = regionMergeSize;
        config.maxEdgeLen = maxEdgeLen;
        config.maxSimplificationError = maxSimplificationError;
        config.vertsPerPoly = vertsPerPoly;
        config.buildDetailMesh = buildDetailMesh;
        config.detailSampleDistance = detailSampleDist;
        config.detailSampleMaxError = detailSampleMaxError;
    }

    private void UpdateFrom(DtDynamicNavMeshConfig config)
    {
        cellSize = config.cellSize;
        partitioning = config.partition;
        walkableHeight = config.walkableHeight;
        walkableSlopeAngle = config.walkableSlopeAngle;
        walkableRadius = config.walkableRadius;
        walkableClimb = config.walkableClimb;
        minRegionArea = config.minRegionArea;
        regionMergeSize = config.regionMergeArea;
        maxEdgeLen = config.maxEdgeLen;
        maxSimplificationError = config.maxSimplificationError;
        vertsPerPoly = config.vertsPerPoly;
        buildDetailMesh = config.buildDetailMesh;
        detailSampleDist = config.detailSampleDistance;
        detailSampleMaxError = config.detailSampleMaxError;
        filterLowHangingObstacles = config.filterLowHangingObstacles;
        filterLedgeSpans = config.filterLedgeSpans;
        filterWalkableLowHeightSpans = config.filterWalkableLowHeightSpans;
    }
}