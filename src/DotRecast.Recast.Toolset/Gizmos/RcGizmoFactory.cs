using System.Numerics;

namespace DotRecast.Recast.Toolset.Gizmos
{
    public static class RcGizmoFactory
    {
        public static RcBoxGizmo Box(Vector3 center, Vector3[] halfEdges)
        {
            return new RcBoxGizmo(center, halfEdges);
        }

        public static RcSphereGizmo Sphere(Vector3 center, float radius)
        {
            return new RcSphereGizmo(center, radius);
        }

        public static RcCapsuleGizmo Capsule(Vector3 start, Vector3 end, float radius)
        {
            return new RcCapsuleGizmo(start, end, radius);
        }

        public static RcCylinderGizmo Cylinder(Vector3 start, Vector3 end, float radius)
        {
            return new RcCylinderGizmo(start, end, radius);
        }

        public static RcTrimeshGizmo Trimesh(float[] verts, int[] faces)
        {
            return new RcTrimeshGizmo(verts, faces);
        }

        public static RcCompositeGizmo Composite(params IRcGizmoMeshFilter[] gizmos)
        {
            return new RcCompositeGizmo(gizmos);
        }
    }
}