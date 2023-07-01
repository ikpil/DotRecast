using DotRecast.Detour.Dynamic.Colliders;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class ColliderWithGizmo
{
    public readonly ICollider Collider;
    public readonly IColliderGizmo Gizmo;

    public ColliderWithGizmo(ICollider collider, IColliderGizmo gizmo)
    {
        Collider = collider;
        Gizmo = gizmo;
    }
}