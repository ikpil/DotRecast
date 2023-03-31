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
    private readonly float[] center;
    private readonly float[] gradient;

    public CylinderGizmo(Vector3f start, Vector3f end, float radius)
    {
        center = new float[]
        {
            0.5f * (start[0] + end[0]), 0.5f * (start[1] + end[1]),
            0.5f * (start[2] + end[2])
        };
        Vector3f axis = Vector3f.Of(end[0] - start[0], end[1] - start[1], end[2] - start[2]);
        Vector3f[] normals = new Vector3f[3];
        normals[1] = Vector3f.Of(end[0] - start[0], end[1] - start[1], end[2] - start[2]);
        normalize(ref normals[1]);
        normals[0] = getSideVector(axis);
        normals[2] = Vector3f.Zero;
        cross(ref normals[2], normals[0], normals[1]);
        normalize(ref normals[2]);
        triangles = generateCylindricalTriangles();
        Vector3f trX = Vector3f.Of(normals[0][0], normals[1][0], normals[2][0]);
        Vector3f trY = Vector3f.Of(normals[0][1], normals[1][1], normals[2][1]);
        Vector3f trZ = Vector3f.Of(normals[0][2], normals[1][2], normals[2][2]);
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
            vertices[i] = x * trX[0] + y * trX[1] + z * trX[2] + center[0];
            vertices[i + 1] = x * trY[0] + y * trY[1] + z * trY[2] + center[1];
            vertices[i + 2] = x * trZ[0] + y * trZ[1] + z * trZ[2] + center[2];
            if (i < vertices.Length / 4 || i >= 3 * vertices.Length / 4)
            {
                gradient[i / 3] = 1;
            }
            else
            {
                v[0] = vertices[i] - center[0];
                v[1] = vertices[i + 1] - center[1];
                v[2] = vertices[i + 2] - center[2];
                normalize(ref v);
                gradient[i / 3] = clamp(0.57735026f * (v[0] + v[1] + v[2]), -1, 1);
            }
        }
    }

    private Vector3f getSideVector(Vector3f axis)
    {
        Vector3f side = Vector3f.Of(1, 0, 0);
        if (axis[0] > 0.8)
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