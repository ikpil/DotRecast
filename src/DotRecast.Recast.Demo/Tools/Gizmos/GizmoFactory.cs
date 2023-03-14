namespace DotRecast.Recast.Demo.Tools.Gizmos;

public static class GizmoFactory
{
    public static ColliderGizmo box(float[] center, float[][] halfEdges) {
        return new BoxGizmo(center, halfEdges);
    }

    public static ColliderGizmo sphere(float[] center, float radius) {
        return new SphereGizmo(center, radius);
    }

    public static ColliderGizmo capsule(float[] start, float[] end, float radius) {
        return new CapsuleGizmo(start, end, radius);
    }

    public static ColliderGizmo cylinder(float[] start, float[] end, float radius) {
        return new CylinderGizmo(start, end, radius);
    }

    public static ColliderGizmo trimesh(float[] verts, int[] faces) {
        return new TrimeshGizmo(verts, faces);
    }

    public static ColliderGizmo composite(params ColliderGizmo[] gizmos) {
        return new CompositeGizmo(gizmos);
    }
}