using DotRecast.Recast.Demo.Draw;

using static DotRecast.Recast.RecastVectors;
using static DotRecast.Detour.DetourCommon;
using static DotRecast.Recast.Demo.Tools.Gizmos.GizmoHelper;

namespace DotRecast.Recast.Demo.Tools.Gizmos;


public class CapsuleGizmo : ColliderGizmo {
    private readonly float[] vertices;
    private readonly int[] triangles;
    private readonly float[] center;
    private readonly float[] gradient;

    public CapsuleGizmo(float[] start, float[] end, float radius) {
        center = new float[] { 0.5f * (start[0] + end[0]), 0.5f * (start[1] + end[1]),
                0.5f * (start[2] + end[2]) };
        float[] axis = new float[] { end[0] - start[0], end[1] - start[1], end[2] - start[2] };
        float[][] normals = new float[3][];
        normals[1] = new float[] { end[0] - start[0], end[1] - start[1], end[2] - start[2] };
        normalize(normals[1]);
        normals[0] = getSideVector(axis);
        normals[2] = new float[3];
        cross(normals[2], normals[0], normals[1]);
        normalize(normals[2]);
        triangles = generateSphericalTriangles();
        float[] trX = new float[] { normals[0][0], normals[1][0], normals[2][0] };
        float[] trY = new float[] { normals[0][1], normals[1][1], normals[2][1] };
        float[] trZ = new float[] { normals[0][2], normals[1][2], normals[2][2] };
        float[] spVertices = generateSphericalVertices();
        float halfLength = 0.5f * vLen(axis);
        vertices = new float[spVertices.Length];
        gradient = new float[spVertices.Length / 3];
        float[] v = new float[3];
        for (int i = 0; i < spVertices.Length; i += 3) {
            float offset = (i >= spVertices.Length / 2) ? -halfLength : halfLength;
            float x = radius * spVertices[i];
            float y = radius * spVertices[i + 1] + offset;
            float z = radius * spVertices[i + 2];
            vertices[i] = x * trX[0] + y * trX[1] + z * trX[2] + center[0];
            vertices[i + 1] = x * trY[0] + y * trY[1] + z * trY[2] + center[1];
            vertices[i + 2] = x * trZ[0] + y * trZ[1] + z * trZ[2] + center[2];
            v[0] = vertices[i] - center[0];
            v[1] = vertices[i + 1] - center[1];
            v[2] = vertices[i + 2] - center[2];
            normalize(v);
            gradient[i / 3] = clamp(0.57735026f * (v[0] + v[1] + v[2]), -1, 1);
        }

    }

    private float[] getSideVector(float[] axis) {
        float[] side = { 1, 0, 0 };
        if (axis[0] > 0.8) {
            side = new float[] { 0, 0, 1 };
        }
        float[] forward = new float[3];
        cross(forward, side, axis);
        cross(side, axis, forward);
        normalize(side);
        return side;
    }

    public void render(RecastDebugDraw debugDraw) {

        debugDraw.begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < triangles.Length; i += 3) {
            for (int j = 0; j < 3; j++) {
                int v = triangles[i + j] * 3;
                float c = gradient[triangles[i + j]];
                int col = DebugDraw.duLerpCol(DebugDraw.duRGBA(32, 32, 0, 160), DebugDraw.duRGBA(220, 220, 0, 160),
                        (int) (127 * (1 + c)));
                debugDraw.vertex(vertices[v], vertices[v + 1], vertices[v + 2], col);
            }
        }
        debugDraw.end();
    }

}