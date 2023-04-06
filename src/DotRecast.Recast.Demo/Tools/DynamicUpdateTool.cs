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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Detour.Dynamic;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;
using DotRecast.Recast.Demo.Tools.Gizmos;
using DotRecast.Recast.Demo.UI;
using ImGuiNET;
using Silk.NET.OpenAL;
using Silk.NET.Windowing;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;
using static DotRecast.Core.RecastMath;

namespace DotRecast.Recast.Demo.Tools;

public class DynamicUpdateToolMode
{
    public static readonly DynamicUpdateToolMode BUILD = new(0, "Build");
    public static readonly DynamicUpdateToolMode COLLIDERS = new(1, "Colliders");
    public static readonly DynamicUpdateToolMode RAYCAST = new(2, "Raycast");

    public static readonly ImmutableArray<DynamicUpdateToolMode> Values = ImmutableArray.Create(
        BUILD, COLLIDERS, RAYCAST
    );

    public int Idx { get; }
    public string Label { get; }

    private DynamicUpdateToolMode(int idx, string label)
    {
        Idx = idx;
        Label = label;
    }
}

public enum ColliderShape
{
    SPHERE,
    CAPSULE,
    BOX,
    CYLINDER,
    COMPOSITE,
    CONVEX,
    TRIMESH_BRIDGE,
    TRIMESH_HOUSE
}

public class DynamicUpdateTool : Tool
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

    private int colliderShapeIdx = (int)ColliderShape.SPHERE;
    private ColliderShape colliderShape = ColliderShape.SPHERE;

    private DynamicNavMesh dynaMesh;
    private readonly TaskFactory executor;
    private readonly Dictionary<long, Collider> colliders = new();
    private readonly Dictionary<long, ColliderGizmo> colliderGizmos = new();
    private readonly Random random = Random.Shared;
    private readonly DemoInputGeomProvider bridgeGeom;
    private readonly DemoInputGeomProvider houseGeom;
    private readonly DemoInputGeomProvider convexGeom;
    private bool sposSet;
    private bool eposSet;
    private Vector3f spos;
    private Vector3f epos;
    private bool raycastHit;
    private Vector3f raycastHitPos;

    public DynamicUpdateTool()
    {
        executor = Task.Factory;
        bridgeGeom = DemoObjImporter.load(Loader.ToBytes("bridge.obj"));
        houseGeom = DemoObjImporter.load(Loader.ToBytes("house.obj"));
        convexGeom = DemoObjImporter.load(Loader.ToBytes("convex.obj"));
    }

    public override void setSample(Sample sample)
    {
        this.sample = sample;
    }

    public override void handleClick(Vector3f s, Vector3f p, bool shift)
    {
        if (mode == DynamicUpdateToolMode.COLLIDERS)
        {
            if (!shift)
            {
                Tuple<Collider, ColliderGizmo> colliderWithGizmo = null;
                if (dynaMesh != null)
                {
                    if (colliderShape == ColliderShape.SPHERE)
                    {
                        colliderWithGizmo = sphereCollider(p);
                    }
                    else if (colliderShape == ColliderShape.CAPSULE)
                    {
                        colliderWithGizmo = capsuleCollider(p);
                    }
                    else if (colliderShape == ColliderShape.BOX)
                    {
                        colliderWithGizmo = boxCollider(p);
                    }
                    else if (colliderShape == ColliderShape.CYLINDER)
                    {
                        colliderWithGizmo = cylinderCollider(p);
                    }
                    else if (colliderShape == ColliderShape.COMPOSITE)
                    {
                        colliderWithGizmo = compositeCollider(p);
                    }
                    else if (colliderShape == ColliderShape.TRIMESH_BRIDGE)
                    {
                        colliderWithGizmo = trimeshBridge(p);
                    }
                    else if (colliderShape == ColliderShape.TRIMESH_HOUSE)
                    {
                        colliderWithGizmo = trimeshHouse(p);
                    }
                    else if (colliderShape == ColliderShape.CONVEX)
                    {
                        colliderWithGizmo = convexTrimesh(p);
                    }
                }

                if (colliderWithGizmo != null)
                {
                    long id = dynaMesh.addCollider(colliderWithGizmo.Item1);
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
                Vector3f sp = Vector3f.Of(spos[0], spos[1] + 1.3f, spos[2]);
                Vector3f ep = Vector3f.Of(epos[0], epos[1] + 1.3f, epos[2]);
                long t1 = TickWatch.Ticks;
                float? hitPos = dynaMesh.voxelQuery().raycast(sp, ep);
                long t2 = TickWatch.Ticks;
                raycastTime = (t2 - t1) / TimeSpan.TicksPerMillisecond;
                raycastHit = hitPos.HasValue;
                raycastHitPos = hitPos.HasValue
                    ? Vector3f.Of(sp[0] + hitPos.Value * (ep[0] - sp[0]), sp[1] + hitPos.Value * (ep[1] - sp[1]), sp[2] + hitPos.Value * (ep[2] - sp[2]))
                    : ep;
            }
        }
    }

    private Tuple<Collider, ColliderGizmo> sphereCollider(Vector3f p)
    {
        float radius = 1 + (float)random.NextDouble() * 10;
        return Tuple.Create<Collider, ColliderGizmo>(
            new SphereCollider(p, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, dynaMesh.config.walkableClimb),
            GizmoFactory.sphere(p, radius));
    }

    private Tuple<Collider, ColliderGizmo> capsuleCollider(Vector3f p)
    {
        float radius = 0.4f + (float)random.NextDouble() * 4f;
        Vector3f a = Vector3f.Of(
            (1f - 2 * (float)random.NextDouble()),
            0.01f + (float)random.NextDouble(),
            (1f - 2 * (float)random.NextDouble())
        );
        vNormalize(ref a);
        float len = 1f + (float)random.NextDouble() * 20f;
        a[0] *= len;
        a[1] *= len;
        a[2] *= len;
        Vector3f start = Vector3f.Of(p[0], p[1], p[2]);
        Vector3f end = Vector3f.Of(p[0] + a[0], p[1] + a[1], p[2] + a[2]);
        return Tuple.Create<Collider, ColliderGizmo>(new CapsuleCollider(
            start, end, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, dynaMesh.config.walkableClimb), GizmoFactory.capsule(start, end, radius));
    }

    private Tuple<Collider, ColliderGizmo> boxCollider(Vector3f p)
    {
        Vector3f extent = Vector3f.Of(
            0.5f + (float)random.NextDouble() * 6f, 
            0.5f + (float)random.NextDouble() * 6f,
            0.5f + (float)random.NextDouble() * 6f
        );
        Vector3f forward = Vector3f.Of((1f - 2 * (float)random.NextDouble()), 0, (1f - 2 * (float)random.NextDouble()));
        Vector3f up = Vector3f.Of((1f - 2 * (float)random.NextDouble()), 0.01f + (float)random.NextDouble(), (1f - 2 * (float)random.NextDouble()));
        Vector3f[] halfEdges = BoxCollider.getHalfEdges(up, forward, extent);
        return Tuple.Create<Collider, ColliderGizmo>(
            new BoxCollider(p, halfEdges, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, dynaMesh.config.walkableClimb), GizmoFactory.box(p, halfEdges));
    }

    private Tuple<Collider, ColliderGizmo> cylinderCollider(Vector3f p)
    {
        float radius = 0.7f + (float)random.NextDouble() * 4f;
        float[] a = new float[] { (1f - 2 * (float)random.NextDouble()), 0.01f + (float)random.NextDouble(), (1f - 2 * (float)random.NextDouble()) };
        vNormalize(a);
        float len = 2f + (float)random.NextDouble() * 20f;
        a[0] *= len;
        a[1] *= len;
        a[2] *= len;
        Vector3f start = Vector3f.Of(p[0], p[1], p[2]);
        Vector3f end = Vector3f.Of(p[0] + a[0], p[1] + a[1], p[2] + a[2]);
        return Tuple.Create<Collider, ColliderGizmo>(new CylinderCollider(start, end, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER,
            dynaMesh.config.walkableClimb), GizmoFactory.cylinder(start, end, radius));
    }

    private Tuple<Collider, ColliderGizmo> compositeCollider(Vector3f p)
    {
        Vector3f baseExtent = Vector3f.Of(5, 3, 8);
        Vector3f baseCenter = Vector3f.Of(p[0], p[1] + 3, p[2]);
        Vector3f baseUp = Vector3f.Of(0, 1, 0);
        Vector3f forward = Vector3f.Of((1f - 2 * (float)random.NextDouble()), 0, (1f - 2 * (float)random.NextDouble()));
        vNormalize(ref forward);
        Vector3f side = vCross(forward, baseUp);
        BoxCollider @base = new BoxCollider(baseCenter, BoxCollider.getHalfEdges(baseUp, forward, baseExtent),
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb);
        var roofUp = Vector3f.Zero;
        Vector3f roofExtent = Vector3f.Of(4.5f, 4.5f, 8f);
        float[] rx = GLU.build_4x4_rotation_matrix(45, forward[0], forward[1], forward[2]);
        roofUp = mulMatrixVector(ref roofUp, rx, baseUp);
        Vector3f roofCenter = Vector3f.Of(p[0], p[1] + 6, p[2]);
        BoxCollider roof = new BoxCollider(roofCenter, BoxCollider.getHalfEdges(roofUp, forward, roofExtent),
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb);
        Vector3f trunkStart = Vector3f.Of(
            baseCenter[0] - forward[0] * 15 + side[0] * 6,
            p[1],
            baseCenter[2] - forward[2] * 15 + side[2] * 6
        );
        Vector3f trunkEnd = Vector3f.Of(trunkStart[0], trunkStart[1] + 10, trunkStart[2]);
        CapsuleCollider trunk = new CapsuleCollider(trunkStart, trunkEnd, 0.5f, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD,
            dynaMesh.config.walkableClimb);
        Vector3f crownCenter = Vector3f.Of(
            baseCenter[0] - forward[0] * 15 + side[0] * 6, p[1] + 10,
            baseCenter[2] - forward[2] * 15 + side[2] * 6
        );
        SphereCollider crown = new SphereCollider(crownCenter, 4f, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GRASS,
            dynaMesh.config.walkableClimb);
        CompositeCollider collider = new CompositeCollider(@base, roof, trunk, crown);
        ColliderGizmo baseGizmo = GizmoFactory.box(baseCenter, BoxCollider.getHalfEdges(baseUp, forward, baseExtent));
        ColliderGizmo roofGizmo = GizmoFactory.box(roofCenter, BoxCollider.getHalfEdges(roofUp, forward, roofExtent));
        ColliderGizmo trunkGizmo = GizmoFactory.capsule(trunkStart, trunkEnd, 0.5f);
        ColliderGizmo crownGizmo = GizmoFactory.sphere(crownCenter, 4f);
        ColliderGizmo gizmo = GizmoFactory.composite(baseGizmo, roofGizmo, trunkGizmo, crownGizmo);
        return Tuple.Create<Collider, ColliderGizmo>(collider, gizmo);
    }

    private Tuple<Collider, ColliderGizmo> trimeshBridge(Vector3f p)
    {
        return trimeshCollider(p, bridgeGeom);
    }

    private Tuple<Collider, ColliderGizmo> trimeshHouse(Vector3f p)
    {
        return trimeshCollider(p, houseGeom);
    }

    private Tuple<Collider, ColliderGizmo> convexTrimesh(Vector3f p)
    {
        float[] verts = transformVertices(p, convexGeom, 360);
        ConvexTrimeshCollider collider = new ConvexTrimeshCollider(verts, convexGeom.faces,
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb * 10);
        return Tuple.Create<Collider, ColliderGizmo>(collider, GizmoFactory.trimesh(verts, convexGeom.faces));
    }

    private Tuple<Collider, ColliderGizmo> trimeshCollider(Vector3f p, DemoInputGeomProvider geom)
    {
        float[] verts = transformVertices(p, geom, 0);
        TrimeshCollider collider = new TrimeshCollider(verts, geom.faces, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD,
            dynaMesh.config.walkableClimb * 10);
        return Tuple.Create<Collider, ColliderGizmo>(collider, GizmoFactory.trimesh(verts, geom.faces));
    }

    private float[] transformVertices(Vector3f p, DemoInputGeomProvider geom, float ax)
    {
        float[] rx = GLU.build_4x4_rotation_matrix((float)random.NextDouble() * ax, 1, 0, 0);
        float[] ry = GLU.build_4x4_rotation_matrix((float)random.NextDouble() * 360, 0, 1, 0);
        float[] m = GLU.mul(rx, ry);
        float[] verts = new float[geom.vertices.Length];
        Vector3f v = new Vector3f();
        Vector3f vr = new Vector3f();
        for (int i = 0; i < geom.vertices.Length; i += 3)
        {
            v[0] = geom.vertices[i];
            v[1] = geom.vertices[i + 1];
            v[2] = geom.vertices[i + 2];
            mulMatrixVector(ref vr, m, v);
            vr[0] += p[0];
            vr[1] += p[1] - 0.1f;
            vr[2] += p[2];
            verts[i] = vr[0];
            verts[i + 1] = vr[1];
            verts[i + 2] = vr[2];
        }

        return verts;
    }

    private float[] mulMatrixVector(float[] resultvector, float[] matrix, float[] pvector)
    {
        resultvector[0] = matrix[0] * pvector[0] + matrix[4] * pvector[1] + matrix[8] * pvector[2];
        resultvector[1] = matrix[1] * pvector[0] + matrix[5] * pvector[1] + matrix[9] * pvector[2];
        resultvector[2] = matrix[2] * pvector[0] + matrix[6] * pvector[1] + matrix[10] * pvector[2];
        return resultvector;
    }

    private Vector3f mulMatrixVector(ref Vector3f resultvector, float[] matrix, Vector3f pvector)
    {
        resultvector[0] = matrix[0] * pvector[0] + matrix[4] * pvector[1] + matrix[8] * pvector[2];
        resultvector[1] = matrix[1] * pvector[0] + matrix[5] * pvector[1] + matrix[9] * pvector[2];
        resultvector[2] = matrix[2] * pvector[0] + matrix[6] * pvector[1] + matrix[10] * pvector[2];
        return resultvector;
    }


    public override void handleClickRay(Vector3f start, float[] dir, bool shift)
    {
        if (mode == DynamicUpdateToolMode.COLLIDERS)
        {
            if (shift)
            {
                foreach (var e in colliders)
                {
                    if (hit(start, dir, e.Value.bounds()))
                    {
                        dynaMesh.removeCollider(e.Key);
                        colliders.Remove(e.Key);
                        colliderGizmos.Remove(e.Key);
                        break;
                    }
                }
            }
        }
    }

    private bool hit(Vector3f point, float[] dir, float[] bounds)
    {
        float cx = 0.5f * (bounds[0] + bounds[3]);
        float cy = 0.5f * (bounds[1] + bounds[4]);
        float cz = 0.5f * (bounds[2] + bounds[5]);
        float dx = 0.5f * (bounds[3] - bounds[0]);
        float dy = 0.5f * (bounds[4] - bounds[1]);
        float dz = 0.5f * (bounds[5] - bounds[2]);
        float rSqr = dx * dx + dy * dy + dz * dz;
        float mx = point[0] - cx;
        float my = point[1] - cy;
        float mz = point[2] - cz;
        float c = mx * mx + my * my + mz * mz - rSqr;
        if (c <= 0.0f)
        {
            return true;
        }

        float b = mx * dir[0] + my * dir[1] + mz * dir[2];
        if (b > 0.0f)
        {
            return false;
        }

        float disc = b * b - c;
        return disc >= 0.0f;
    }

    public override void handleRender(NavMeshRenderer renderer)
    {
        if (mode == DynamicUpdateToolMode.COLLIDERS)
        {
            if (showColliders)
            {
                colliderGizmos.Values.forEach(g => g.render(renderer.getDebugDraw()));
            }
        }

        if (mode == DynamicUpdateToolMode.RAYCAST)
        {
            RecastDebugDraw dd = renderer.getDebugDraw();
            int startCol = duRGBA(128, 25, 0, 192);
            int endCol = duRGBA(51, 102, 0, 129);
            if (sposSet)
            {
                drawAgent(dd, spos, startCol);
            }

            if (eposSet)
            {
                drawAgent(dd, epos, endCol);
            }

            dd.depthMask(false);
            if (raycastHitPos != Vector3f.Zero)
            {
                int spathCol = raycastHit ? duRGBA(128, 32, 16, 220) : duRGBA(64, 128, 240, 220);
                dd.begin(LINES, 2.0f);
                dd.vertex(spos[0], spos[1] + 1.3f, spos[2], spathCol);
                dd.vertex(raycastHitPos[0], raycastHitPos[1], raycastHitPos[2], spathCol);
                dd.end();
            }

            dd.depthMask(true);
        }
    }

    private void drawAgent(RecastDebugDraw dd, Vector3f pos, int col)
    {
        float r = sample.getSettingsUI().getAgentRadius();
        float h = sample.getSettingsUI().getAgentHeight();
        float c = sample.getSettingsUI().getAgentMaxClimb();
        dd.depthMask(false);
        // Agent dimensions.
        dd.debugDrawCylinderWire(pos[0] - r, pos[1] + 0.02f, pos[2] - r, pos[0] + r, pos[1] + h, pos[2] + r, col, 2.0f);
        dd.debugDrawCircle(pos[0], pos[1] + c, pos[2], r, duRGBA(0, 0, 0, 64), 1.0f);
        int colb = duRGBA(0, 0, 0, 196);
        dd.begin(LINES);
        dd.vertex(pos[0], pos[1] - c, pos[2], colb);
        dd.vertex(pos[0], pos[1] + c, pos[2], colb);
        dd.vertex(pos[0] - r / 2, pos[1] + 0.02f, pos[2], colb);
        dd.vertex(pos[0] + r / 2, pos[1] + 0.02f, pos[2], colb);
        dd.vertex(pos[0], pos[1] + 0.02f, pos[2] - r / 2, colb);
        dd.vertex(pos[0], pos[1] + 0.02f, pos[2] + r / 2, colb);
        dd.end();
        dd.depthMask(true);
    }

    public override void handleUpdate(float dt)
    {
        if (dynaMesh != null)
        {
            updateDynaMesh();
        }
    }

    private void updateDynaMesh()
    {
        long t = TickWatch.Ticks;
        try
        {
            bool updated = dynaMesh.update(executor).Result;
            if (updated)
            {
                buildTime = (TickWatch.Ticks - t) / TimeSpan.TicksPerMillisecond;
                sample.update(null, dynaMesh.recastResults(), dynaMesh.navMesh());
                sample.setChanged(false);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public override void layout()
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
                    load(picker.SelectedFile);
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
                            save(picker.SelectedFile);

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
            PartitionType.Values.forEach(partition =>
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

            if (ImGui.Button("Build"))
            {
                if (dynaMesh != null)
                {
                    buildDynaMesh();
                    sample.setChanged(false);
                }
            }
        }

        if (mode == DynamicUpdateToolMode.COLLIDERS)
        {
            ImGui.Text("Colliders");
            ImGui.Separator();
            var prev = colliderShape;
            ImGui.Checkbox("Show", ref showColliders);
            ImGui.RadioButton("Sphere", ref colliderShapeIdx, (int)ColliderShape.SPHERE);
            ImGui.RadioButton("Capsule", ref colliderShapeIdx, (int)ColliderShape.CAPSULE);
            ImGui.RadioButton("Box", ref colliderShapeIdx, (int)ColliderShape.BOX);
            ImGui.RadioButton("Cylinder", ref colliderShapeIdx, (int)ColliderShape.CYLINDER);
            ImGui.RadioButton("Composite", ref colliderShapeIdx, (int)ColliderShape.COMPOSITE);
            ImGui.RadioButton("Convex Trimesh", ref colliderShapeIdx, (int)ColliderShape.CONVEX);
            ImGui.RadioButton("Trimesh Bridge", ref colliderShapeIdx, (int)ColliderShape.TRIMESH_BRIDGE);
            ImGui.RadioButton("Trimesh House", ref colliderShapeIdx, (int)ColliderShape.TRIMESH_HOUSE);
            ImGui.NewLine();

            if ((int)prev != colliderShapeIdx)
            {
                colliderShape = (ColliderShape)colliderShapeIdx;
            }
        }

        if (mode == DynamicUpdateToolMode.RAYCAST)
        {
            ImGui.Text($"Raycast Time: {raycastTime} ms");
            ImGui.Separator();
            if (sposSet)
            {
                ImGui.Text($"Start: {spos[0]}, {spos[1] + 1.3f}, {spos[2]}");
            }

            if (eposSet)
            {
                ImGui.Text($"End: {epos[0]}, {epos[1] + 1.3f}, {epos[2]}");
            }

            if (raycastHit)
            {
                ImGui.Text($"Hit: {raycastHitPos[0]}, {raycastHitPos[1]}, {raycastHitPos[2]}");
            }

            ImGui.NewLine();
        }
        else
        {
            ImGui.Text($"Build Time: {buildTime} ms");
        }
    }


    private void load(string filename)
    {
        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            VoxelFileReader reader = new VoxelFileReader();
            VoxelFile voxelFile = reader.read(br);
            dynaMesh = new DynamicNavMesh(voxelFile);
            dynaMesh.config.keepIntermediateResults = true;
            updateUI();
            buildDynaMesh();

            colliders.Clear();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            dynaMesh = null;
        }
    }


    private void save(string filename)
    {
        using var fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);
        using var bw = new BinaryWriter(fs);
        VoxelFile voxelFile = VoxelFile.from(dynaMesh);
        VoxelFileWriter writer = new VoxelFileWriter();
        writer.write(bw, voxelFile, compression);
    }

    private void buildDynaMesh()
    {
        configDynaMesh();
        long t = TickWatch.Ticks;
        try
        {
            var _ = dynaMesh.build(executor).Result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        buildTime = (TickWatch.Ticks - t) / TimeSpan.TicksPerMillisecond;
        sample.update(null, dynaMesh.recastResults(), dynaMesh.navMesh());
    }

    private void configDynaMesh()
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

    private void updateUI()
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

    public override string getName()
    {
        return "Dynamic Updates";
    }
}