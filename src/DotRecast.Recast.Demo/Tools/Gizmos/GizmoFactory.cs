using DotRecast.Core;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public static class GizmoFactory
{
    public static ColliderGizmo Box(Vector3f center, Vector3f[] halfEdges)
    {
        return new BoxGizmo(center, halfEdges);
    }

    public static ColliderGizmo Sphere(Vector3f center, float radius)
    {
        return new SphereGizmo(center, radius);
    }

    public static ColliderGizmo Capsule(Vector3f start, Vector3f end, float radius)
    {
        return new CapsuleGizmo(start, end, radius);
    }

    public static ColliderGizmo Cylinder(Vector3f start, Vector3f end, float radius)
    {
        return new CylinderGizmo(start, end, radius);
    }

    public static ColliderGizmo Trimesh(float[] verts, int[] faces)
    {
        return new TrimeshGizmo(verts, faces);
    }

    public static ColliderGizmo Composite(params ColliderGizmo[] gizmos)
    {
        return new CompositeGizmo(gizmos);
    }
}