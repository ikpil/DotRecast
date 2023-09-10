namespace DotRecast.Recast.Toolset.Tools.Gizmos
{
    public class TrimeshGizmo : IRcGizmoMeshFilter
    {
        public readonly float[] vertices;
        public readonly int[] triangles;

        public TrimeshGizmo(float[] vertices, int[] triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }
    }
}