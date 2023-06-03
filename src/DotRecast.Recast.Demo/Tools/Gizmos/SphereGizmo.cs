using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;
using static DotRecast.Core.RcMath;
using static DotRecast.Recast.Demo.Tools.Gizmos.GizmoHelper;


namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class SphereGizmo : IColliderGizmo
{
    private readonly float[] vertices;
    private readonly int[] triangles;
    private readonly float radius;
    private readonly RcVec3f center;

    public SphereGizmo(RcVec3f center, float radius)
    {
        this.center = center;
        this.radius = radius;
        vertices = GenerateSphericalVertices();
        triangles = GenerateSphericalTriangles();
    }

    public void Render(RecastDebugDraw debugDraw)
    {
        debugDraw.Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int v = triangles[i + j] * 3;
                float c = Clamp(0.57735026f * (vertices[v] + vertices[v + 1] + vertices[v + 2]), -1, 1);
                int col = DebugDraw.DuLerpCol(DebugDraw.DuRGBA(32, 32, 0, 160), DebugDraw.DuRGBA(220, 220, 0, 160),
                    (int)(127 * (1 + c)));
                debugDraw.Vertex(radius * vertices[v] + center.x, radius * vertices[v + 1] + center.y,
                    radius * vertices[v + 2] + center.z, col);
            }
        }

        debugDraw.End();
    }
}