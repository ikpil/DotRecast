using DotRecast.Recast.Demo.Draw;

using static DotRecast.Detour.DetourCommon;
using static DotRecast.Recast.Demo.Tools.Gizmos.GizmoHelper;


namespace DotRecast.Recast.Demo.Tools.Gizmos;


public class SphereGizmo : ColliderGizmo {
    private readonly float[] vertices;
    private readonly int[] triangles;
    private readonly float radius;
    private readonly float[] center;

    public SphereGizmo(float[] center, float radius) {
        this.center = center;
        this.radius = radius;
        vertices = generateSphericalVertices();
        triangles = generateSphericalTriangles();
    }

    public void render(RecastDebugDraw debugDraw) {
        debugDraw.begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < triangles.Length; i += 3) {
            for (int j = 0; j < 3; j++) {
                int v = triangles[i + j] * 3;
                float c = clamp(0.57735026f * (vertices[v] + vertices[v + 1] + vertices[v + 2]), -1, 1);
                int col = DebugDraw.duLerpCol(DebugDraw.duRGBA(32, 32, 0, 160), DebugDraw.duRGBA(220, 220, 0, 160),
                    (int) (127 * (1 + c)));
                debugDraw.vertex(radius * vertices[v] + center[0], radius * vertices[v + 1] + center[1],
                    radius * vertices[v + 2] + center[2], col);
            }
        }
        debugDraw.end();
    }

}