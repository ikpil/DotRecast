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
using System.Diagnostics;
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
using Silk.NET.Windowing;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;
using static DotRecast.Detour.DetourCommon;

namespace DotRecast.Recast.Demo.Tools;

public class DynamicUpdateTool : Tool
{
    private enum ToolMode
    {
        BUILD,
        COLLIDERS,
        RAYCAST
    }

    private enum ColliderShape
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

    private Sample sample;
    private ToolMode mode = ToolMode.BUILD;
    private float cellSize = 0.3f;
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
    private float[] spos;
    private float[] epos;
    private bool raycastHit;
    private float[] raycastHitPos;

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

    public override void handleClick(float[] s, float[] p, bool shift)
    {
        if (mode == ToolMode.COLLIDERS)
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

        if (mode == ToolMode.RAYCAST)
        {
            if (shift)
            {
                sposSet = true;
                spos = ArrayUtils.CopyOf(p, p.Length);
            }
            else
            {
                eposSet = true;
                epos = ArrayUtils.CopyOf(p, p.Length);
            }

            if (sposSet && eposSet && dynaMesh != null)
            {
                float[] sp = { spos[0], spos[1] + 1.3f, spos[2] };
                float[] ep = { epos[0], epos[1] + 1.3f, epos[2] };
                long t1 = Stopwatch.GetTimestamp();
                float? hitPos = dynaMesh.voxelQuery().raycast(sp, ep);
                long t2 = Stopwatch.GetTimestamp();
                raycastTime = (t2 - t1) / 1_000_000L;
                raycastHit = hitPos.HasValue;
                raycastHitPos = hitPos.HasValue
                    ? new float[] { sp[0] + hitPos.Value * (ep[0] - sp[0]), sp[1] + hitPos.Value * (ep[1] - sp[1]), sp[2] + hitPos.Value * (ep[2] - sp[2]) }
                    : ep;
            }
        }
    }

    private Tuple<Collider, ColliderGizmo> sphereCollider(float[] p)
    {
        float radius = 1 + (float)random.NextDouble() * 10;
        return Tuple.Create<Collider, ColliderGizmo>(
            new SphereCollider(p, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, dynaMesh.config.walkableClimb),
            GizmoFactory.sphere(p, radius));
    }

    private Tuple<Collider, ColliderGizmo> capsuleCollider(float[] p)
    {
        float radius = 0.4f + (float)random.NextDouble() * 4f;
        float[] a = new float[] { (1f - 2 * (float)random.NextDouble()), 0.01f + (float)random.NextDouble(), (1f - 2 * (float)random.NextDouble()) };
        vNormalize(a);
        float len = 1f + (float)random.NextDouble() * 20f;
        a[0] *= len;
        a[1] *= len;
        a[2] *= len;
        float[] start = new float[] { p[0], p[1], p[2] };
        float[] end = new float[] { p[0] + a[0], p[1] + a[1], p[2] + a[2] };
        return Tuple.Create<Collider, ColliderGizmo>(new CapsuleCollider(start, end, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER,
            dynaMesh.config.walkableClimb), GizmoFactory.capsule(start, end, radius));
    }

    private Tuple<Collider, ColliderGizmo> boxCollider(float[] p)
    {
        float[] extent = new float[]
        {
            0.5f + (float)random.NextDouble() * 6f, 0.5f + (float)random.NextDouble() * 6f,
            0.5f + (float)random.NextDouble() * 6f
        };
        float[] forward = new float[] { (1f - 2 * (float)random.NextDouble()), 0, (1f - 2 * (float)random.NextDouble()) };
        float[] up = new float[] { (1f - 2 * (float)random.NextDouble()), 0.01f + (float)random.NextDouble(), (1f - 2 * (float)random.NextDouble()) };
        float[][] halfEdges = BoxCollider.getHalfEdges(up, forward, extent);
        return Tuple.Create<Collider, ColliderGizmo>(
            new BoxCollider(p, halfEdges, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, dynaMesh.config.walkableClimb),
            GizmoFactory.box(p, halfEdges));
    }

    private Tuple<Collider, ColliderGizmo> cylinderCollider(float[] p)
    {
        float radius = 0.7f + (float)random.NextDouble() * 4f;
        float[] a = new float[] { (1f - 2 * (float)random.NextDouble()), 0.01f + (float)random.NextDouble(), (1f - 2 * (float)random.NextDouble()) };
        vNormalize(a);
        float len = 2f + (float)random.NextDouble() * 20f;
        a[0] *= len;
        a[1] *= len;
        a[2] *= len;
        float[] start = new float[] { p[0], p[1], p[2] };
        float[] end = new float[] { p[0] + a[0], p[1] + a[1], p[2] + a[2] };
        return Tuple.Create<Collider, ColliderGizmo>(new CylinderCollider(start, end, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER,
            dynaMesh.config.walkableClimb), GizmoFactory.cylinder(start, end, radius));
    }

    private Tuple<Collider, ColliderGizmo> compositeCollider(float[] p)
    {
        float[] baseExtent = new float[] { 5, 3, 8 };
        float[] baseCenter = new float[] { p[0], p[1] + 3, p[2] };
        float[] baseUp = new float[] { 0, 1, 0 };
        float[] forward = new float[] { (1f - 2 * (float)random.NextDouble()), 0, (1f - 2 * (float)random.NextDouble()) };
        vNormalize(forward);
        float[] side = DemoMath.vCross(forward, baseUp);
        BoxCollider @base = new BoxCollider(baseCenter, BoxCollider.getHalfEdges(baseUp, forward, baseExtent),
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb);
        float[] roofExtent = new float[] { 4.5f, 4.5f, 8f };
        float[] rx = GLU.build_4x4_rotation_matrix(45, forward[0], forward[1], forward[2]);
        float[] roofUp = mulMatrixVector(new float[3], rx, baseUp);
        float[] roofCenter = new float[] { p[0], p[1] + 6, p[2] };
        BoxCollider roof = new BoxCollider(roofCenter, BoxCollider.getHalfEdges(roofUp, forward, roofExtent),
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb);
        float[] trunkStart = new float[]
        {
            baseCenter[0] - forward[0] * 15 + side[0] * 6, p[1],
            baseCenter[2] - forward[2] * 15 + side[2] * 6
        };
        float[] trunkEnd = new float[] { trunkStart[0], trunkStart[1] + 10, trunkStart[2] };
        CapsuleCollider trunk = new CapsuleCollider(trunkStart, trunkEnd, 0.5f, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD,
            dynaMesh.config.walkableClimb);
        float[] crownCenter = new float[]
        {
            baseCenter[0] - forward[0] * 15 + side[0] * 6, p[1] + 10,
            baseCenter[2] - forward[2] * 15 + side[2] * 6
        };
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

    private Tuple<Collider, ColliderGizmo> trimeshBridge(float[] p)
    {
        return trimeshCollider(p, bridgeGeom);
    }

    private Tuple<Collider, ColliderGizmo> trimeshHouse(float[] p)
    {
        return trimeshCollider(p, houseGeom);
    }

    private Tuple<Collider, ColliderGizmo> convexTrimesh(float[] p)
    {
        float[] verts = transformVertices(p, convexGeom, 360);
        ConvexTrimeshCollider collider = new ConvexTrimeshCollider(verts, convexGeom.faces,
            SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, dynaMesh.config.walkableClimb * 10);
        return Tuple.Create<Collider, ColliderGizmo>(collider, GizmoFactory.trimesh(verts, convexGeom.faces));
    }

    private Tuple<Collider, ColliderGizmo> trimeshCollider(float[] p, DemoInputGeomProvider geom)
    {
        float[] verts = transformVertices(p, geom, 0);
        TrimeshCollider collider = new TrimeshCollider(verts, geom.faces, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD,
            dynaMesh.config.walkableClimb * 10);
        return Tuple.Create<Collider, ColliderGizmo>(collider, GizmoFactory.trimesh(verts, geom.faces));
    }

    private float[] transformVertices(float[] p, DemoInputGeomProvider geom, float ax)
    {
        float[] rx = GLU.build_4x4_rotation_matrix((float)random.NextDouble() * ax, 1, 0, 0);
        float[] ry = GLU.build_4x4_rotation_matrix((float)random.NextDouble() * 360, 0, 1, 0);
        float[] m = GLU.mul(rx, ry);
        float[] verts = new float[geom.vertices.Length];
        float[] v = new float[3];
        float[] vr = new float[3];
        for (int i = 0; i < geom.vertices.Length; i += 3)
        {
            v[0] = geom.vertices[i];
            v[1] = geom.vertices[i + 1];
            v[2] = geom.vertices[i + 2];
            mulMatrixVector(vr, m, v);
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

    public override void handleClickRay(float[] start, float[] dir, bool shift)
    {
        if (mode == ToolMode.COLLIDERS)
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

    private bool hit(float[] point, float[] dir, float[] bounds)
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
        if (mode == ToolMode.COLLIDERS)
        {
            if (showColliders)
            {
                colliderGizmos.Values.forEach(g => g.render(renderer.getDebugDraw()));
            }
        }

        if (mode == ToolMode.RAYCAST)
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
            if (raycastHitPos != null)
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

    private void drawAgent(RecastDebugDraw dd, float[] pos, int col)
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
        long t = Stopwatch.GetTimestamp();
        try
        {
            bool updated = dynaMesh.update(executor).Result;
            if (updated)
            {
                buildTime = (Stopwatch.GetTimestamp() - t) / 1_000_000;
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
        // nk_layout_row_dynamic(ctx, 18, 1);
        // if (nk_option_label(ctx, "Build", mode == ToolMode.BUILD)) {
        //     mode = ToolMode.BUILD;
        // }
        // nk_layout_row_dynamic(ctx, 18, 1);
        // if (nk_option_label(ctx, "Colliders", mode == ToolMode.COLLIDERS)) {
        //     mode = ToolMode.COLLIDERS;
        // }
        // nk_layout_row_dynamic(ctx, 18, 1);
        // if (nk_option_label(ctx, "Raycast", mode == ToolMode.RAYCAST)) {
        //     mode = ToolMode.RAYCAST;
        // }
        //
        // nk_layout_row_dynamic(ctx, 1, 1);
        // nk_spacing(ctx, 1);
        // if (mode == ToolMode.BUILD) {
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     if (nk_button_text(ctx, "Load Voxels...")) {
        //         load();
        //     }
        //     if (dynaMesh != null) {
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         compression = nk_check_text(ctx, "Compression", compression);
        //         if (nk_button_text(ctx, "Save Voxels...")) {
        //             save();
        //         }
        //     }
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Rasterization");
        //     nk_layout_row_dynamic(ctx, 18, 2);
        ImGui.Text("Cell Size");
        //     nk_label(ctx, string.format("%.2f", cellSize[0]), NK_TEXT_ALIGN_RIGHT);
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Agent");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Height", ref walkableHeight, 0f, 5f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Radius", ref walkableRadius, 0f, 10f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Max Climb", ref walkableClimb, 0f, 10f, "%.2f");
        //     nk_layout_row_dynamic(ctx, 18, 2);
        ImGui.Text("Max Slope");
        //     nk_label(ctx, string.format("%.0f", walkableSlopeAngle[0]), NK_TEXT_ALIGN_RIGHT);
        //
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Partitioning");
        //     partitioning = NuklearUIHelper.nk_radio(ctx, PartitionType.values(), partitioning,
        //             p => p.name().substring(0, 1) + p.name().substring(1).toLowerCase());
        //
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Filtering");
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     filterLowHangingObstacles = nk_option_text(ctx, "Low Hanging Obstacles", filterLowHangingObstacles);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     filterLedgeSpans = nk_option_text(ctx, "Ledge Spans", filterLedgeSpans);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     filterWalkableLowHeightSpans = nk_option_text(ctx, "Walkable Low Height Spans", filterWalkableLowHeightSpans);
        //
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Region");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Min Region Size", ref minRegionArea, 0, 150, "%.1f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Merged Region Size", ref regionMergeSize, 0, 400, "%.1f");
        //
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Polygonization");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Max Edge Length", ref maxEdgeLen, 0f, 50f, "%.1f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Max Edge Error", ref maxSimplificationError, 0.1f, 10f, "%.1f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderInt("Verts Per Poly", ref vertsPerPoly, 3, 12);
        //
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Detail Mesh");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     buildDetailMesh = nk_check_text(ctx, "Enable", buildDetailMesh);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Sample Distance", ref detailSampleDist, 0f, 16f, "%.1f");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        ImGui.SliderFloat("Max Sample Error", ref detailSampleMaxError, 0f, 16f, "%.1f");
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     if (nk_button_text(ctx, "Build")) {
        //         if (dynaMesh != null) {
        //             buildDynaMesh();
        //             sample.setChanged(false);
        //         }
        //     }
        // }
        // if (mode == ToolMode.COLLIDERS) {
        //     nk_layout_row_dynamic(ctx, 1, 1);
        //     nk_spacing(ctx, 1);
        //     nk_layout_row_dynamic(ctx, 18, 1);
        ImGui.Text("Colliders");
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     showColliders = nk_check_text(ctx, "Show", showColliders);
        //     nk_layout_row_dynamic(ctx, 20, 1);
        //     if (nk_option_label(ctx, "Sphere", colliderShape == ColliderShape.SPHERE)) {
        //         colliderShape = ColliderShape.SPHERE;
        //     }
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     if (nk_option_label(ctx, "Capsule", colliderShape == ColliderShape.CAPSULE)) {
        //         colliderShape = ColliderShape.CAPSULE;
        //     }
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     if (nk_option_label(ctx, "Box", colliderShape == ColliderShape.BOX)) {
        //         colliderShape = ColliderShape.BOX;
        //     }
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     if (nk_option_label(ctx, "Cylinder", colliderShape == ColliderShape.CYLINDER)) {
        //         colliderShape = ColliderShape.CYLINDER;
        //     }
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     if (nk_option_label(ctx, "Composite", colliderShape == ColliderShape.COMPOSITE)) {
        //         colliderShape = ColliderShape.COMPOSITE;
        //     }
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     if (nk_option_label(ctx, "Convex Trimesh", colliderShape == ColliderShape.CONVEX)) {
        //         colliderShape = ColliderShape.CONVEX;
        //     }
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     if (nk_option_label(ctx, "Trimesh Bridge", colliderShape == ColliderShape.TRIMESH_BRIDGE)) {
        //         colliderShape = ColliderShape.TRIMESH_BRIDGE;
        //     }
        //     nk_layout_row_dynamic(ctx, 18, 1);
        //     if (nk_option_label(ctx, "Trimesh House", colliderShape == ColliderShape.TRIMESH_HOUSE)) {
        //         colliderShape = ColliderShape.TRIMESH_HOUSE;
        //     }
        // }
        // nk_layout_row_dynamic(ctx, 2, 1);
        // nk_spacing(ctx, 1);
        // nk_layout_row_dynamic(ctx, 18, 1);
        // if (mode == ToolMode.RAYCAST) {
        //     nk_label(ctx, string.format("Raycast Time: %d ms", raycastTime), NK_TEXT_ALIGN_LEFT);
        //     if (sposSet) {
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, string.format("Start: %.3f, %.3f, %.3f", spos[0], spos[1] + 1.3f, spos[2]), NK_TEXT_ALIGN_LEFT);
        //     }
        //     if (eposSet) {
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, string.format("End: %.3f, %.3f, %.3f", epos[0], epos[1] + 1.3f, epos[2]), NK_TEXT_ALIGN_LEFT);
        //     }
        //     if (raycastHit) {
        //         nk_layout_row_dynamic(ctx, 18, 1);
        //         nk_label(ctx, string.format("Hit: %.3f, %.3f, %.3f", raycastHitPos[0], raycastHitPos[1], raycastHitPos[2]),
        //                 NK_TEXT_ALIGN_LEFT);
        //     }
        // } else {
        //     nk_label(ctx, string.format("Build Time: %d ms", buildTime), NK_TEXT_ALIGN_LEFT);
        // }
    }

    private void load()
    {
        // try (MemoryStack stack = stackPush()) {
        //     PointerBuffer aFilterPatterns = stack.mallocPointer(1);
        //     aFilterPatterns.put(stack.UTF8("*.voxels"));
        //     aFilterPatterns.flip();
        //     string filename = TinyFileDialogs.tinyfd_openFileDialog("Open Voxel File", "", aFilterPatterns, "Voxel File", false);
        //     if (filename != null) {
        //         load(filename);
        //     }
        // }
    }

    private void load(string filename)
    {
        // File file = new File(filename);
        // if (file.exists()) {
        //     VoxelFileReader reader = new VoxelFileReader();
        //     try (FileInputStream fis = new FileInputStream(file)) {
        //         VoxelFile voxelFile = reader.read(fis);
        //         dynaMesh = new DynamicNavMesh(voxelFile);
        //         dynaMesh.config.keepIntermediateResults = true;
        //         updateUI();
        //         buildDynaMesh();
        //         colliders.clear();
        //     } catch (Exception e) {
        //         Console.WriteLine(e);
        //         dynaMesh = null;
        //     }
        // }
    }

    private void save()
    {
        // try (MemoryStack stack = stackPush()) {
        //     PointerBuffer aFilterPatterns = stack.mallocPointer(1);
        //     aFilterPatterns.put(stack.UTF8("*.voxels"));
        //     aFilterPatterns.flip();
        //     string filename = TinyFileDialogs.tinyfd_saveFileDialog("Save Voxel File", "", aFilterPatterns, "Voxel File");
        //     if (filename != null) {
        //         save(filename);
        //     }
        // }
    }

    private void save(string filename)
    {
        // File file = new File(filename);
        // try (FileOutputStream fos = new FileOutputStream(file)) {
        //     VoxelFile voxelFile = VoxelFile.from(dynaMesh);
        //     VoxelFileWriter writer = new VoxelFileWriter();
        //     writer.write(fos, voxelFile, compression);
        // } catch (Exception e) {
        //     Console.WriteLine(e);
        // }
    }

    private void buildDynaMesh()
    {
        configDynaMesh();
        long t = Stopwatch.GetTimestamp();
        try
        {
            var _ = dynaMesh.build(executor).Result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        buildTime = (Stopwatch.GetTimestamp() - t) / 1_000_000;
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