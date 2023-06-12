/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

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
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Detour.Dynamic;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Recast.DemoTool.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.DemoTool.Geom;
using DotRecast.Recast.Demo.Tools.Gizmos;
using DotRecast.Recast.Demo.UI;
using DotRecast.Recast.DemoTool;
using DotRecast.Recast.DemoTool.Tools;
using ImGuiNET;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class DynamicUpdateTool : IRcTool
{
    private Sample sample;
    private int toolModeIdx = DynamicUpdateToolMode.BUILD.Idx;
    private DynamicUpdateToolMode mode = DynamicUpdateToolMode.BUILD;
    private float cellSize = 0.3f;

    private int partitioningIdx = PartitionType.WATERSHED.Idx;
    private PartitionType partitioning = PartitionType.WATERSHED;
    private bool filterLowHangingObstacles = true;
    private bool filterLedgeSpans = true;
    private bool filterWalkableLowHeightSpans = true;
    private float walkableHeight = 2f;
    private float walkableRadius = 0.6f;
    private float walkableClimb = 0.9f;
    private float walkableSlopeAngle = 45f;
    private float minRegionArea = 6f;
    private float regionMergeSize = 36f;
    private float maxEdgeLen = 12f;
    private float maxSimplificationError = 1.3f;
    private int vertsPerPoly = 6;
    private bool buildDetailMesh = true;
    private bool compression = true;
    private float detailSampleDist = 6f;
    private float detailSampleMaxError = 1f;
    private bool showColliders = false;
    private long buildTime;
    private long raycastTime;

    private int colliderShapeIdx = (int)DynamicColliderShape.SPHERE;
    private DynamicColliderShape colliderShape = DynamicColliderShape.SPHERE;

    private DynamicNavMesh dynaMesh;
    private readonly TaskFactory executor;
    private readonly Dictionary<long, ICollider> colliders = new();
    private readonly Dictionary<long, IColliderGizmo> colliderGizmos = new();
    private readonly Random random = Random.Shared;
    private readonly DemoInputGeomProvider bridgeGeom;
    private readonly DemoInputGeomProvider houseGeom;
    private readonly DemoInputGeomProvider convexGeom;
    private bool sposSet;
    private bool eposSet;
    private RcVec3f spos;
    private RcVec3f epos;
    private bool raycastHit;
    private RcVec3f raycastHitPos;

    public DynamicUpdateTool()
    {
        executor = Task.Factory;
        bridgeGeom = DemoObjImporter.Load(Loader.ToBytes("bridge.obj"));
        houseGeom = DemoObjImporter.Load(Loader.ToBytes("house.obj"));
        convexGeom = DemoObjImporter.Load(Loader.ToBytes("convex.obj"));
    }

    public void SetSample(Sample sample)
    {
        this.sample = sample;
    }

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        if (mode == DynamicUpdateToolMode.COLLIDERS)
        {
            if (!shift)
            {
                Tuple<ICollider, IColliderGizmo> colliderWithGizmo = null;
                if (dynaMesh != null)
                {
                    if (colliderShape == DynamicColliderShape.SPHERE)
                    {
                        colliderWithGizmo = SphereCollider(p);
                    }
                    else if (colliderShape == DynamicColliderShape.CAPSULE)
                    {
                        colliderWithGizmo = CapsuleCollider(p);
                    }
                    else if (colliderShape == DynamicColliderShape.BOX)
                    {
                        colliderWithGizmo = BoxCollider(p);
                    }
                    else if (colliderShape == DynamicColliderShape.CYLINDER)
                    {
                        colliderWithGizmo = CylinderCollider(p);
                    }
                    else if (colliderShape == DynamicColliderShape.COMPOSITE)
                    {
                        colliderWithGizmo = CompositeCollider(p);
                    }
                    else if (colliderShape == DynamicColliderShape.TRIMESH_BRIDGE)
                    {
                        colliderWithGizmo = TrimeshBridge(p);
                    }
                    else if (colliderShape == DynamicColliderShape.TRIMESH_HOUSE)
                    {
                        colliderWithGizmo = TrimeshHouse(p);
                    }
                    else if (colliderShape == DynamicColliderShape.CONVEX)
                    {
                        colliderWithGizmo = ConvexTrimesh(p);
                    }
                }

                if (colliderWithGizmo != null)
                {
                    long id = dynaMesh.AddCollider(colliderWithGizmo.Item1);
                    colliders.Add(id, colliderWithGizmo.Item1);
                    colliderGizmos.Add(id, colliderWithGizmo.Item2);
                }
            }
        }

        if (mode == DynamicUpdateToolMode.RAYCAST)
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

            if (sposSet && eposSet && dynaMesh != null)
            {
                RcVec3f sp = RcVec3f.Of(spos.x, spos.y + 1.3f, spos.z);
                RcVec3f ep = RcVec3f.Of(epos.x, epos.y + 1.3f, epos.z);
                long t1 = RcFrequency.Ticks;
                float? hitPos = dynaMesh.VoxelQuery().Raycast(sp, ep);
                long t2 = RcFrequency.Ticks;
                raycastTime = (t2 - t1) / TimeSpan.TicksPerMillisecond;
                raycastHit = hitPos.HasValue;
                raycastHitPos = hitPos.HasValue
                    ? RcVec3f.Of(sp.x + hitPos.Value * (ep.x - sp.x), sp.y + hitPos.Value * (ep.y - sp.y), sp.z + hitPos.Value * (ep.z - sp.z))
                    : ep;
            }
        }
    }

    private Tuple<ICollider, IColliderGizmo> SphereCollider(RcVec3f p)
    {
        float radius = 1 + (float)random.NextDouble() * 10;
        return Tuple.Create<ICollider, IColliderGizmo>(
            new SphereCollider(p, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, dynaMesh.config.walkableClimb),
            GizmoFactory.Sphere(p, radius));
    }

    private Tuple<ICollider, IColliderGizmo> CapsuleCollider(RcVec3f p)
    {
        float radius = 0.4f + (float)random.NextDouble() * 4f;
        RcVec3f a = RcVec3f.Of(
            (1f - 2 * (float)random.NextDouble()),
            0.01f + (float)random.NextDouble(),
            (1f - 2 * (float)random.NextDouble())
        );
        a.Normalize();
        float len = 1f + (float)random.NextDouble() * 20f;
        a.x *= len;
        a.y *= len;
        a.z *= len;
        RcVec3f start = RcVec3f.Of(p.x, p.y, p.z);
        RcVec3f end = RcVec3f.Of(p.x + a.x, p.y + a.y, p.z + a.z);
        return Tuple.Create<ICollider, IColliderGizmo>(new CapsuleCollider(
            start, end, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, dynaMesh.config.walkableClimb), GizmoFactory.Capsule(start, end, radius));
    }

    private Tuple<ICollider, IColliderGizmo> BoxCollider(RcVec3f p)
    {
        RcVec3f extent = RcVec3f.Of(
            0.5f + (float)random.NextDouble() * 6f,
            0.5f + (float)random.NextDouble() * 6f,
            0.5f + (float)random.NextDouble() * 6f
        );
        RcVec3f forward = RcVec3f.Of((1f - 2 * (float)random.NextDouble()), 0, (1f - 2 * (float)random.NextDouble()));
        RcVec3f up = RcVec3f.Of((1f - 2 * (float)random.NextDouble()), 0.01f + (float)random.NextDouble(), (1f - 2 * (float)random.NextDouble()));
        RcVec3f[] halfEdges = Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(up, forward, extent);
        return Tuple.Create<ICollider, IColliderGizmo>(
            new BoxCollider(p, halfEdges, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, dynaMesh.config.walkableClimb), GizmoFactory.Box(p, halfEdges));
    }

    private Tuple<ICollider, IColliderGizmo> CylinderCollider(RcVec3f p)
    {
        float radius = 0.7f + (float)random.NextDouble() * 4f;
        RcVec3f a = RcVec3f.Of(1f - 2 * (float)random.NextDouble(), 0.01f + (float)random.NextDouble(), 1f - 2 * (float)random.NextDouble());
        a.Normalize();
        float len = 2f + (float)random.NextDouble() * 20f;
        a[0] *= len;
        a[1] *= len;
        a[2] *= len;
        RcVec3f start = RcVec3f.Of(p.x, p.y, p.z);
        RcVec3f end = RcVec3f.Of(p.x + a.x, p.y + a.y, p.z + a.z);
        return Tuple.Create<ICollider, IColliderGizmo>(new CylinderCollider(start, end, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER,
            dynaMesh.config.walkableClimb), GizmoFactory.Cylinder(start, end, radius));
    }

    private Tuple<ICollider, IColliderGizmo> CompositeCollider(RcVec3f p)
    {
        RcVec3f baseExtent = RcVec3f.Of(5, 3, 8);
        RcVec3f baseCenter = RcVec3f.Of(p.x, p.y + 3, p.z);
        RcVec3f baseUp = RcVec3f.Of(0, 1, 0);
        RcVec3f forward = RcVec3f.Of((1f - 2 * (float)random.NextDouble()), 0, (1f - 2 * (float)random.NextDouble()));
        forward.Normalize();
        RcVec3f side = RcVec3f.Cross(forward, baseUp);
        BoxCollider @base = new BoxCollider(baseCenter, Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(baseUp, forward, baseExtent),
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb);
        var roofUp = RcVec3f.Zero;
        RcVec3f roofExtent = RcVec3f.Of(4.5f, 4.5f, 8f);
        float[] rx = GLU.Build_4x4_rotation_matrix(45, forward.x, forward.y, forward.z);
        roofUp = MulMatrixVector(ref roofUp, rx, baseUp);
        RcVec3f roofCenter = RcVec3f.Of(p.x, p.y + 6, p.z);
        BoxCollider roof = new BoxCollider(roofCenter, Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(roofUp, forward, roofExtent),
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb);
        RcVec3f trunkStart = RcVec3f.Of(
            baseCenter.x - forward.x * 15 + side.x * 6,
            p.y,
            baseCenter.z - forward.z * 15 + side.z * 6
        );
        RcVec3f trunkEnd = RcVec3f.Of(trunkStart.x, trunkStart.y + 10, trunkStart.z);
        CapsuleCollider trunk = new CapsuleCollider(trunkStart, trunkEnd, 0.5f, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD,
            dynaMesh.config.walkableClimb);
        RcVec3f crownCenter = RcVec3f.Of(
            baseCenter.x - forward.x * 15 + side.x * 6, p.y + 10,
            baseCenter.z - forward.z * 15 + side.z * 6
        );
        SphereCollider crown = new SphereCollider(crownCenter, 4f, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GRASS,
            dynaMesh.config.walkableClimb);
        CompositeCollider collider = new CompositeCollider(@base, roof, trunk, crown);
        IColliderGizmo baseGizmo = GizmoFactory.Box(baseCenter, Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(baseUp, forward, baseExtent));
        IColliderGizmo roofGizmo = GizmoFactory.Box(roofCenter, Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(roofUp, forward, roofExtent));
        IColliderGizmo trunkGizmo = GizmoFactory.Capsule(trunkStart, trunkEnd, 0.5f);
        IColliderGizmo crownGizmo = GizmoFactory.Sphere(crownCenter, 4f);
        IColliderGizmo gizmo = GizmoFactory.Composite(baseGizmo, roofGizmo, trunkGizmo, crownGizmo);
        return Tuple.Create<ICollider, IColliderGizmo>(collider, gizmo);
    }

    private Tuple<ICollider, IColliderGizmo> TrimeshBridge(RcVec3f p)
    {
        return TrimeshCollider(p, bridgeGeom);
    }

    private Tuple<ICollider, IColliderGizmo> TrimeshHouse(RcVec3f p)
    {
        return TrimeshCollider(p, houseGeom);
    }

    private Tuple<ICollider, IColliderGizmo> ConvexTrimesh(RcVec3f p)
    {
        float[] verts = TransformVertices(p, convexGeom, 360);
        ConvexTrimeshCollider collider = new ConvexTrimeshCollider(verts, convexGeom.faces,
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb * 10);
        return Tuple.Create<ICollider, IColliderGizmo>(collider, GizmoFactory.Trimesh(verts, convexGeom.faces));
    }

    private Tuple<ICollider, IColliderGizmo> TrimeshCollider(RcVec3f p, DemoInputGeomProvider geom)
    {
        float[] verts = TransformVertices(p, geom, 0);
        TrimeshCollider collider = new TrimeshCollider(verts, geom.faces, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD,
            dynaMesh.config.walkableClimb * 10);
        return Tuple.Create<ICollider, IColliderGizmo>(collider, GizmoFactory.Trimesh(verts, geom.faces));
    }

    private float[] TransformVertices(RcVec3f p, DemoInputGeomProvider geom, float ax)
    {
        float[] rx = GLU.Build_4x4_rotation_matrix((float)random.NextDouble() * ax, 1, 0, 0);
        float[] ry = GLU.Build_4x4_rotation_matrix((float)random.NextDouble() * 360, 0, 1, 0);
        float[] m = GLU.Mul(rx, ry);
        float[] verts = new float[geom.vertices.Length];
        RcVec3f v = new RcVec3f();
        RcVec3f vr = new RcVec3f();
        for (int i = 0; i < geom.vertices.Length; i += 3)
        {
            v.x = geom.vertices[i];
            v.y = geom.vertices[i + 1];
            v.z = geom.vertices[i + 2];
            MulMatrixVector(ref vr, m, v);
            vr.x += p.x;
            vr.y += p.y - 0.1f;
            vr.z += p.z;
            verts[i] = vr.x;
            verts[i + 1] = vr.y;
            verts[i + 2] = vr.z;
        }

        return verts;
    }

    private float[] MulMatrixVector(float[] resultvector, float[] matrix, float[] pvector)
    {
        resultvector[0] = matrix[0] * pvector[0] + matrix[4] * pvector[1] + matrix[8] * pvector[2];
        resultvector[1] = matrix[1] * pvector[0] + matrix[5] * pvector[1] + matrix[9] * pvector[2];
        resultvector[2] = matrix[2] * pvector[0] + matrix[6] * pvector[1] + matrix[10] * pvector[2];
        return resultvector;
    }

    private RcVec3f MulMatrixVector(ref RcVec3f resultvector, float[] matrix, RcVec3f pvector)
    {
        resultvector.x = matrix[0] * pvector.x + matrix[4] * pvector.y + matrix[8] * pvector.z;
        resultvector.y = matrix[1] * pvector.x + matrix[5] * pvector.y + matrix[9] * pvector.z;
        resultvector.z = matrix[2] * pvector.x + matrix[6] * pvector.y + matrix[10] * pvector.z;
        return resultvector;
    }


    public void HandleClickRay(RcVec3f start, RcVec3f dir, bool shift)
    {
        if (mode == DynamicUpdateToolMode.COLLIDERS)
        {
            if (shift)
            {
                foreach (var e in colliders)
                {
                    if (Hit(start, dir, e.Value.Bounds()))
                    {
                        dynaMesh.RemoveCollider(e.Key);
                        colliders.Remove(e.Key);
                        colliderGizmos.Remove(e.Key);
                        break;
                    }
                }
            }
        }
    }

    private bool Hit(RcVec3f point, RcVec3f dir, float[] bounds)
    {
        float cx = 0.5f * (bounds[0] + bounds[3]);
        float cy = 0.5f * (bounds[1] + bounds[4]);
        float cz = 0.5f * (bounds[2] + bounds[5]);
        float dx = 0.5f * (bounds[3] - bounds[0]);
        float dy = 0.5f * (bounds[4] - bounds[1]);
        float dz = 0.5f * (bounds[5] - bounds[2]);
        float rSqr = dx * dx + dy * dy + dz * dz;
        float mx = point.x - cx;
        float my = point.y - cy;
        float mz = point.z - cz;
        float c = mx * mx + my * my + mz * mz - rSqr;
        if (c <= 0.0f)
        {
            return true;
        }

        float b = mx * dir.x + my * dir.y + mz * dir.z;
        if (b > 0.0f)
        {
            return false;
        }

        float disc = b * b - c;
        return disc >= 0.0f;
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        if (mode == DynamicUpdateToolMode.COLLIDERS)
        {
            if (showColliders)
            {
                colliderGizmos.Values.ForEach(g => g.Render(renderer.GetDebugDraw()));
            }
        }

        if (mode == DynamicUpdateToolMode.RAYCAST)
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
                dd.Vertex(spos.x, spos.y + 1.3f, spos.z, spathCol);
                dd.Vertex(raycastHitPos.x, raycastHitPos.y, raycastHitPos.z, spathCol);
                dd.End();
            }

            dd.DepthMask(true);
        }
    }

    private void DrawAgent(RecastDebugDraw dd, RcVec3f pos, int col)
    {
        float r = sample.GetSettings().agentRadius;
        float h = sample.GetSettings().agentHeight;
        float c = sample.GetSettings().agentMaxClimb;
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
        if (dynaMesh != null)
        {
            UpdateDynaMesh();
        }
    }

    private void UpdateDynaMesh()
    {
        long t = RcFrequency.Ticks;
        try
        {
            bool updated = dynaMesh.Update(executor).Result;
            if (updated)
            {
                buildTime = (RcFrequency.Ticks - t) / TimeSpan.TicksPerMillisecond;
                sample.Update(null, dynaMesh.RecastResults(), dynaMesh.NavMesh());
                sample.SetChanged(false);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Layout()
    {
        ImGui.Text($"Dynamic Update Tool Modes");
        ImGui.Separator();

        var prevMode = mode;
        ImGui.RadioButton(DynamicUpdateToolMode.BUILD.Label, ref toolModeIdx, DynamicUpdateToolMode.BUILD.Idx);
        ImGui.RadioButton(DynamicUpdateToolMode.COLLIDERS.Label, ref toolModeIdx, DynamicUpdateToolMode.COLLIDERS.Idx);
        ImGui.RadioButton(DynamicUpdateToolMode.RAYCAST.Label, ref toolModeIdx, DynamicUpdateToolMode.RAYCAST.Idx);
        ImGui.NewLine();

        if (prevMode.Idx != toolModeIdx)
        {
            mode = DynamicUpdateToolMode.Values[toolModeIdx];
        }

        ImGui.Text($"Selected mode - {mode.Label}");
        ImGui.Separator();

        if (mode == DynamicUpdateToolMode.BUILD)
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
                    sample.SetChanged(false);
                }
            }
        }

        if (mode == DynamicUpdateToolMode.COLLIDERS)
        {
            ImGui.Text("Colliders");
            ImGui.Separator();
            var prev = colliderShape;
            ImGui.Checkbox("Show", ref showColliders);
            ImGui.RadioButton("Sphere", ref colliderShapeIdx, (int)DynamicColliderShape.SPHERE);
            ImGui.RadioButton("Capsule", ref colliderShapeIdx, (int)DynamicColliderShape.CAPSULE);
            ImGui.RadioButton("Box", ref colliderShapeIdx, (int)DynamicColliderShape.BOX);
            ImGui.RadioButton("Cylinder", ref colliderShapeIdx, (int)DynamicColliderShape.CYLINDER);
            ImGui.RadioButton("Composite", ref colliderShapeIdx, (int)DynamicColliderShape.COMPOSITE);
            ImGui.RadioButton("Convex Trimesh", ref colliderShapeIdx, (int)DynamicColliderShape.CONVEX);
            ImGui.RadioButton("Trimesh Bridge", ref colliderShapeIdx, (int)DynamicColliderShape.TRIMESH_BRIDGE);
            ImGui.RadioButton("Trimesh House", ref colliderShapeIdx, (int)DynamicColliderShape.TRIMESH_HOUSE);
            ImGui.NewLine();

            if ((int)prev != colliderShapeIdx)
            {
                colliderShape = (DynamicColliderShape)colliderShapeIdx;
            }
        }

        if (mode == DynamicUpdateToolMode.RAYCAST)
        {
            ImGui.Text($"Raycast Time: {raycastTime} ms");
            ImGui.Separator();
            if (sposSet)
            {
                ImGui.Text($"Start: {spos.x}, {spos.y + 1.3f}, {spos.z}");
            }

            if (eposSet)
            {
                ImGui.Text($"End: {epos.x}, {epos.y + 1.3f}, {epos.z}");
            }

            if (raycastHit)
            {
                ImGui.Text($"Hit: {raycastHitPos.x}, {raycastHitPos.y}, {raycastHitPos.z}");
            }

            ImGui.NewLine();
        }
        else
        {
            ImGui.Text($"Build Time: {buildTime} ms");
        }
    }


    private void Load(string filename)
    {
        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            VoxelFileReader reader = new VoxelFileReader();
            VoxelFile voxelFile = reader.Read(br);
            dynaMesh = new DynamicNavMesh(voxelFile);
            dynaMesh.config.keepIntermediateResults = true;
            UpdateUI();
            BuildDynaMesh();

            colliders.Clear();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            dynaMesh = null;
        }
    }


    private void Save(string filename)
    {
        using var fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);
        using var bw = new BinaryWriter(fs);
        VoxelFile voxelFile = VoxelFile.From(dynaMesh);
        VoxelFileWriter writer = new VoxelFileWriter();
        writer.Write(bw, voxelFile, compression);
    }

    private void BuildDynaMesh()
    {
        ConfigDynaMesh();
        long t = RcFrequency.Ticks;
        try
        {
            var _ = dynaMesh.Build(executor).Result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        buildTime = (RcFrequency.Ticks - t) / TimeSpan.TicksPerMillisecond;
        sample.Update(null, dynaMesh.RecastResults(), dynaMesh.NavMesh());
    }

    private void ConfigDynaMesh()
    {
        dynaMesh.config.partitionType = partitioning;
        dynaMesh.config.walkableHeight = walkableHeight;
        dynaMesh.config.walkableSlopeAngle = walkableSlopeAngle;
        dynaMesh.config.walkableRadius = walkableRadius;
        dynaMesh.config.walkableClimb = walkableClimb;
        dynaMesh.config.filterLowHangingObstacles = filterLowHangingObstacles;
        dynaMesh.config.filterLedgeSpans = filterLedgeSpans;
        dynaMesh.config.filterWalkableLowHeightSpans = filterWalkableLowHeightSpans;
        dynaMesh.config.minRegionArea = minRegionArea;
        dynaMesh.config.regionMergeArea = regionMergeSize;
        dynaMesh.config.maxEdgeLen = maxEdgeLen;
        dynaMesh.config.maxSimplificationError = maxSimplificationError;
        dynaMesh.config.vertsPerPoly = vertsPerPoly;
        dynaMesh.config.buildDetailMesh = buildDetailMesh;
        dynaMesh.config.detailSampleDistance = detailSampleDist;
        dynaMesh.config.detailSampleMaxError = detailSampleMaxError;
    }

    private void UpdateUI()
    {
        cellSize = dynaMesh.config.cellSize;
        partitioning = dynaMesh.config.partitionType;
        walkableHeight = dynaMesh.config.walkableHeight;
        walkableSlopeAngle = dynaMesh.config.walkableSlopeAngle;
        walkableRadius = dynaMesh.config.walkableRadius;
        walkableClimb = dynaMesh.config.walkableClimb;
        minRegionArea = dynaMesh.config.minRegionArea;
        regionMergeSize = dynaMesh.config.regionMergeArea;
        maxEdgeLen = dynaMesh.config.maxEdgeLen;
        maxSimplificationError = dynaMesh.config.maxSimplificationError;
        vertsPerPoly = dynaMesh.config.vertsPerPoly;
        buildDetailMesh = dynaMesh.config.buildDetailMesh;
        detailSampleDist = dynaMesh.config.detailSampleDistance;
        detailSampleMaxError = dynaMesh.config.detailSampleMaxError;
        filterLowHangingObstacles = dynaMesh.config.filterLowHangingObstacles;
        filterLedgeSpans = dynaMesh.config.filterLedgeSpans;
        filterWalkableLowHeightSpans = dynaMesh.config.filterWalkableLowHeightSpans;
    }

    public string GetName()
    {
        return "Dynamic Updates";
    }
}