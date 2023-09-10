namespace DotRecast.Recast.Toolset.Tools.Gizmos
{
    public class CompositeGizmo : IRcGizmoMeshFilter
    {
        public readonly IRcGizmoMeshFilter[] gizmoMeshes;

        public CompositeGizmo(params IRcGizmoMeshFilter[] gizmoMeshes)
        {
            this.gizmoMeshes = gizmoMeshes;
        }
    }
}