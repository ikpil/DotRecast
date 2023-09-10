using DotRecast.Core;

namespace DotRecast.Recast.Toolset.Tools.Gizmos
{
    public static class GizmoFactory
    {
        public static BoxGizmo Box(RcVec3f center, RcVec3f[] halfEdges)
        {
            return new BoxGizmo(center, halfEdges);
        }

        public static SphereGizmo Sphere(RcVec3f center, float radius)
        {
            return new SphereGizmo(center, radius);
        }

        public static CapsuleGizmo Capsule(RcVec3f start, RcVec3f end, float radius)
        {
            return new CapsuleGizmo(start, end, radius);
        }

        public static CylinderGizmo Cylinder(RcVec3f start, RcVec3f end, float radius)
        {
            return new CylinderGizmo(start, end, radius);
        }

        public static TrimeshGizmo Trimesh(float[] verts, int[] faces)
        {
            return new TrimeshGizmo(verts, faces);
        }

        public static CompositeGizmo Composite(params IRcGizmoMeshFilter[] gizmos)
        {
            return new CompositeGizmo(gizmos);
        }
    }
}