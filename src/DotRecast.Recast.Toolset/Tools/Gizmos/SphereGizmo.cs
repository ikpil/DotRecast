using DotRecast.Core;
using static DotRecast.Recast.Toolset.Tools.Gizmos.GizmoHelper;


namespace DotRecast.Recast.Toolset.Tools.Gizmos
{
    public class SphereGizmo : IRcGizmoMeshFilter
    {
        public readonly float[] vertices;
        public readonly int[] triangles;
        public readonly float radius;
        public readonly RcVec3f center;

        public SphereGizmo(RcVec3f center, float radius)
        {
            this.center = center;
            this.radius = radius;
            vertices = GenerateSphericalVertices();
            triangles = GenerateSphericalTriangles();
        }
    }
}