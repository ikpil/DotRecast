using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Recast.Toolset.Gizmos;

namespace DotRecast.Recast.Toolset.Gizmos
{
    public class RcGizmo
    {
        public readonly IRcGizmoMeshFilter Gizmo;
        public readonly ICollider Collider;

        public RcGizmo(ICollider collider, IRcGizmoMeshFilter gizmo)
        {
            Collider = collider;
            Gizmo = gizmo;
        }
    }
}