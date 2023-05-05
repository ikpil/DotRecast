using DotRecast.Core;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public static class GizmoFactory
{
    public static IColliderGizmo Box(Vector3f center, Vector3f[] halfEdges)
    {
        return new BoxGizmo(center, halfEdges);
    }

    public static IColliderGizmo Sphere(Vector3f center, float radius)
    {
        return new SphereGizmo(center, radius);
    }

    public static IColliderGizmo Capsule(Vector3f start, Vector3f end, float radius)
    {
        return new CapsuleGizmo(start, end, radius);
    }

    public static IColliderGizmo Cylinder(Vector3f start, Vector3f end, float radius)
    {
        return new CylinderGizmo(start, end, radius);
    }

    public static IColliderGizmo Trimesh(float[] verts, int[] faces)
    {
        return new TrimeshGizmo(verts, faces);
    }

    public static IColliderGizmo Composite(params IColliderGizmo[] gizmos)
    {
        return new CompositeGizmo(gizmos);
    }
}