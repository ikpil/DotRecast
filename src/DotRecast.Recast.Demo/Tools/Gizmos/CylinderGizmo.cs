using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;
using static DotRecast.Recast.RecastVectors;
using static DotRecast.Core.RecastMath;
using static DotRecast.Recast.Demo.Tools.Gizmos.GizmoHelper;


namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class CylinderGizmo : ColliderGizmo
{
    private readonly float[] vertices;
    private readonly int[] triangles;
    private readonly Vector3f center;
    private readonly float[] gradient;

    public CylinderGizmo(Vector3f start, Vector3f end, float radius)
    {
        center = Vector3f.Of(
            0.5f * (start.x + end.x), 0.5f * (start.y + end.y),
            0.5f * (start.z + end.z)
        );
        Vector3f axis = Vector3f.Of(end.x - start.x, end.y - start.y, end.z - start.z);
        Vector3f[] normals = new Vector3f[3];
        normals[1] = Vector3f.Of(end.x - start.x, end.y - start.y, end.z - start.z);
        normalize(ref normals[1]);
        normals[0] = getSideVector(axis);
        normals[2] = Vector3f.Zero;
        cross(ref normals[2], normals[0], normals[1]);
        normalize(ref normals[2]);
        triangles = generateCylindricalTriangles();
        Vector3f trX = Vector3f.Of(normals[0].x, normals[1].x, normals[2].x);
        Vector3f trY = Vector3f.Of(normals[0].y, normals[1].y, normals[2].y);
        Vector3f trZ = Vector3f.Of(normals[0].z, normals[1].z, normals[2].z);
        vertices = generateCylindricalVertices();
        float halfLength = 0.5f * vLen(axis);
        gradient = new float[vertices.Length / 3];
        Vector3f v = new Vector3f();
        for (int i = 0; i < vertices.Length; i += 3)
        {
            float offset = (i >= vertices.Length / 2) ? -halfLength : halfLength;
            float x = radius * vertices[i];
            float y = vertices[i + 1] + offset;
            float z = radius * vertices[i + 2];
            vertices[i] = x * trX.x + y * trX.y + z * trX.z + center.x;
            vertices[i + 1] = x * trY.x + y * trY.y + z * trY.z + center.y;
            vertices[i + 2] = x * trZ.x + y * trZ.y + z * trZ.z + center.z;
            if (i < vertices.Length / 4 || i >= 3 * vertices.Length / 4)
            {
                gradient[i / 3] = 1;
            }
            else
            {
                v.x = vertices[i] - center.x;
                v.y = vertices[i + 1] - center.y;
                v.z = vertices[i + 2] - center.z;
                normalize(ref v);
                gradient[i / 3] = clamp(0.57735026f * (v.x + v.y + v.z), -1, 1);
            }
        }
    }

    private Vector3f getSideVector(Vector3f axis)
    {
        Vector3f side = Vector3f.Of(1, 0, 0);
        if (axis.x > 0.8)
        {
            side = Vector3f.Of(0, 0, 1);
        }

        Vector3f forward = new Vector3f();
        cross(ref forward, side, axis);
        cross(ref side, axis, forward);
        normalize(ref side);
        return side;
    }

    public void render(RecastDebugDraw debugDraw)
    {
        debugDraw.begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int v = triangles[i + j] * 3;
                float c = gradient[triangles[i + j]];
                int col = DebugDraw.duLerpCol(DebugDraw.duRGBA(32, 32, 0, 160), DebugDraw.duRGBA(220, 220, 0, 160),
                    (int)(127 * (1 + c)));
                debugDraw.vertex(vertices[v], vertices[v + 1], vertices[v + 2], col);
            }
        }

        debugDraw.end();
    }
}
