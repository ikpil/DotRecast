using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset.Gizmos;


namespace DotRecast.Recast.Demo.Tools;

public static class GizmoRenderer
{
    public static void Render(RecastDebugDraw dd, IRcGizmoMeshFilter gizmo)
    {
        if (gizmo is RcBoxGizmo box)
        {
            RenderBox(dd, box);
        }
        else if (gizmo is RcCapsuleGizmo capsule)
        {
            RenderCapsule(dd, capsule);
        }
        else if (gizmo is RcTrimeshGizmo trimesh)
        {
            RenderTrimesh(dd, trimesh);
        }
        else if (gizmo is RcCylinderGizmo cylinder)
        {
            RenderCylinder(dd, cylinder);
        }
        else if (gizmo is RcSphereGizmo sphere)
        {
            RenderSphere(dd, sphere);
        }
        else if (gizmo is RcCompositeGizmo composite)
        {
            RenderComposite(dd, composite);
        }
    }

    public static int GetColorByNormal(float[] vertices, int v0, int v1, int v2)
    {
        RcVec3f e0 = new RcVec3f();
        RcVec3f e1 = new RcVec3f();
        RcVec3f normal = new RcVec3f();
        for (int j = 0; j < 3; ++j)
        {
            e0[j] = vertices[v1 + j] - vertices[v0 + j];
            e1[j] = vertices[v2 + j] - vertices[v0 + j];
        }

        normal.x = e0.y * e1.z - e0.z * e1.y;
        normal.y = e0.z * e1.x - e0.x * e1.z;
        normal.z = e0.x * e1.y - e0.y * e1.x;
        RcVec3f.Normalize(ref normal);
        float c = RcMath.Clamp(0.57735026f * (normal.x + normal.y + normal.z), -1, 1);
        int col = DebugDraw.DuLerpCol(
            DebugDraw.DuRGBA(32, 32, 0, 160),
            DebugDraw.DuRGBA(220, 220, 0, 160),
            (int)(127 * (1 + c))
        );
        return col;
    }

    public static void RenderBox(RecastDebugDraw debugDraw, RcBoxGizmo box)
    {
        var trX = RcVec3f.Of(box.halfEdges[0].x, box.halfEdges[1].x, box.halfEdges[2].x);
        var trY = RcVec3f.Of(box.halfEdges[0].y, box.halfEdges[1].y, box.halfEdges[2].y);
        var trZ = RcVec3f.Of(box.halfEdges[0].z, box.halfEdges[1].z, box.halfEdges[2].z);
        float[] vertices = new float[8 * 3];
        for (int i = 0; i < 8; i++)
        {
            vertices[i * 3 + 0] = RcVec3f.Dot(RcBoxGizmo.VERTS[i], trX) + box.center.x;
            vertices[i * 3 + 1] = RcVec3f.Dot(RcBoxGizmo.VERTS[i], trY) + box.center.y;
            vertices[i * 3 + 2] = RcVec3f.Dot(RcBoxGizmo.VERTS[i], trZ) + box.center.z;
        }

        debugDraw.Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < 12; i++)
        {
            int col = DebugDraw.DuRGBA(200, 200, 50, 160);
            if (i == 4 || i == 5 || i == 8 || i == 9)
            {
                col = DebugDraw.DuRGBA(160, 160, 40, 160);
            }
            else if (i > 4)
            {
                col = DebugDraw.DuRGBA(120, 120, 30, 160);
            }

            for (int j = 0; j < 3; j++)
            {
                int v = RcBoxGizmo.TRIANLGES[i * 3 + j] * 3;
                debugDraw.Vertex(vertices[v], vertices[v + 1], vertices[v + 2], col);
            }
        }

        debugDraw.End();
    }

    public static void RenderCapsule(RecastDebugDraw debugDraw, RcCapsuleGizmo capsule)
    {
        debugDraw.Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < capsule.triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int v = capsule.triangles[i + j] * 3;
                float c = capsule.gradient[capsule.triangles[i + j]];
                int col = DebugDraw.DuLerpCol(DebugDraw.DuRGBA(32, 32, 0, 160), DebugDraw.DuRGBA(220, 220, 0, 160),
                    (int)(127 * (1 + c)));
                debugDraw.Vertex(capsule.vertices[v], capsule.vertices[v + 1], capsule.vertices[v + 2], col);
            }
        }

        debugDraw.End();
    }

    public static void RenderCylinder(RecastDebugDraw debugDraw, RcCylinderGizmo cylinder)
    {
        debugDraw.Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < cylinder.triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int v = cylinder.triangles[i + j] * 3;
                float c = cylinder.gradient[cylinder.triangles[i + j]];
                int col = DebugDraw.DuLerpCol(DebugDraw.DuRGBA(32, 32, 0, 160), DebugDraw.DuRGBA(220, 220, 0, 160),
                    (int)(127 * (1 + c)));
                debugDraw.Vertex(cylinder.vertices[v], cylinder.vertices[v + 1], cylinder.vertices[v + 2], col);
            }
        }

        debugDraw.End();
    }

    public static void RenderSphere(RecastDebugDraw debugDraw, RcSphereGizmo sphere)
    {
        debugDraw.Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < sphere.triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int v = sphere.triangles[i + j] * 3;
                float c = RcMath.Clamp(0.57735026f * (sphere.vertices[v] + sphere.vertices[v + 1] + sphere.vertices[v + 2]), -1, 1);
                int col = DebugDraw.DuLerpCol(DebugDraw.DuRGBA(32, 32, 0, 160), DebugDraw.DuRGBA(220, 220, 0, 160), (int)(127 * (1 + c)));

                debugDraw.Vertex(
                    sphere.radius * sphere.vertices[v] + sphere.center.x,
                    sphere.radius * sphere.vertices[v + 1] + sphere.center.y,
                    sphere.radius * sphere.vertices[v + 2] + sphere.center.z,
                    col
                );
            }
        }

        debugDraw.End();
    }

    public static void RenderTrimesh(RecastDebugDraw debugDraw, RcTrimeshGizmo trimesh)
    {
        debugDraw.Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < trimesh.triangles.Length; i += 3)
        {
            int v0 = 3 * trimesh.triangles[i];
            int v1 = 3 * trimesh.triangles[i + 1];
            int v2 = 3 * trimesh.triangles[i + 2];
            int col = GetColorByNormal(trimesh.vertices, v0, v1, v2);
            debugDraw.Vertex(trimesh.vertices[v0], trimesh.vertices[v0 + 1], trimesh.vertices[v0 + 2], col);
            debugDraw.Vertex(trimesh.vertices[v1], trimesh.vertices[v1 + 1], trimesh.vertices[v1 + 2], col);
            debugDraw.Vertex(trimesh.vertices[v2], trimesh.vertices[v2 + 1], trimesh.vertices[v2 + 2], col);
        }

        debugDraw.End();
    }

    public static void RenderComposite(RecastDebugDraw debugDraw, RcCompositeGizmo composite)
    {
        composite.gizmoMeshes.ForEach(g => Render(debugDraw, g));
    }
}