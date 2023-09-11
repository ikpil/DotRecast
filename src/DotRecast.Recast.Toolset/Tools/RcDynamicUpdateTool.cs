using System;
using System.IO;

using DotRecast.Core;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Toolset.Geom;
using DotRecast.Recast.Toolset.Gizmos;

namespace DotRecast.Recast.Toolset.Tools
{
    public class RcDynamicUpdateTool : IRcToolable
    {
        private readonly Random random;
        private readonly DemoInputGeomProvider bridgeGeom;
        private readonly DemoInputGeomProvider houseGeom;
        private readonly DemoInputGeomProvider convexGeom;

        public RcDynamicUpdateTool(Random rand, DemoInputGeomProvider bridgeGeom, DemoInputGeomProvider houseGeom, DemoInputGeomProvider convexGeom)
        {
            this.random = rand;
            this.bridgeGeom = bridgeGeom;
            this.houseGeom = houseGeom;
            this.convexGeom = convexGeom;
        }

        public string GetName()
        {
            return "Dynamic Updates";
        }

        public VoxelFile Load(string filename, IRcCompressor compressor)
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            VoxelFileReader reader = new VoxelFileReader(compressor);
            VoxelFile voxelFile = reader.Read(br);

            return voxelFile;
        }

        public void Save(string filename, VoxelFile voxelFile, bool compression, IRcCompressor compressor)
        {
            using var fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);
            using var bw = new BinaryWriter(fs);
            VoxelFileWriter writer = new VoxelFileWriter(compressor);
            writer.Write(bw, voxelFile, compression);
        }


        public RcGizmo SphereCollider(RcVec3f p, float walkableClimb)
        {
            float radius = 1 + (float)random.NextDouble() * 10;
            var collider = new SphereCollider(p, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, walkableClimb);
            var gizmo = GizmoFactory.Sphere(p, radius);

            return new RcGizmo(collider, gizmo);
        }

        public RcGizmo CapsuleCollider(RcVec3f p, float walkableClimb)
        {
            float radius = 0.4f + (float)random.NextDouble() * 4f;
            RcVec3f a = RcVec3f.Of(
                (1f - 2 * (float)random.NextDouble()),
                0.01f + (float)random.NextDouble(),
                (1f - 2 * (float)random.NextDouble())
            );
            a.Normalize();
            float len = 1f + (float)random.NextDouble() * 20f;
            a.x *= len;
            a.y *= len;
            a.z *= len;
            RcVec3f start = RcVec3f.Of(p.x, p.y, p.z);
            RcVec3f end = RcVec3f.Of(p.x + a.x, p.y + a.y, p.z + a.z);
            var collider = new CapsuleCollider(start, end, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, walkableClimb);
            var gizmo = GizmoFactory.Capsule(start, end, radius);
            return new RcGizmo(collider, gizmo);
        }

        public RcGizmo BoxCollider(RcVec3f p, float walkableClimb)
        {
            RcVec3f extent = RcVec3f.Of(
                0.5f + (float)random.NextDouble() * 6f,
                0.5f + (float)random.NextDouble() * 6f,
                0.5f + (float)random.NextDouble() * 6f
            );
            RcVec3f forward = RcVec3f.Of((1f - 2 * (float)random.NextDouble()), 0, (1f - 2 * (float)random.NextDouble()));
            RcVec3f up = RcVec3f.Of((1f - 2 * (float)random.NextDouble()), 0.01f + (float)random.NextDouble(), (1f - 2 * (float)random.NextDouble()));
            RcVec3f[] halfEdges = Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(up, forward, extent);
            var collider = new BoxCollider(p, halfEdges, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, walkableClimb);
            var gizmo = GizmoFactory.Box(p, halfEdges);
            return new RcGizmo(collider, gizmo);
        }

        public RcGizmo CylinderCollider(RcVec3f p, float walkableClimb)
        {
            float radius = 0.7f + (float)random.NextDouble() * 4f;
            RcVec3f a = RcVec3f.Of(1f - 2 * (float)random.NextDouble(), 0.01f + (float)random.NextDouble(), 1f - 2 * (float)random.NextDouble());
            a.Normalize();
            float len = 2f + (float)random.NextDouble() * 20f;
            a[0] *= len;
            a[1] *= len;
            a[2] *= len;
            RcVec3f start = RcVec3f.Of(p.x, p.y, p.z);
            RcVec3f end = RcVec3f.Of(p.x + a.x, p.y + a.y, p.z + a.z);
            var collider = new CylinderCollider(start, end, radius, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER, walkableClimb);
            var gizmo = GizmoFactory.Cylinder(start, end, radius);

            return new RcGizmo(collider, gizmo);
        }

        public RcGizmo CompositeCollider(RcVec3f p, float walkableClimb)
        {
            RcVec3f baseExtent = RcVec3f.Of(5, 3, 8);
            RcVec3f baseCenter = RcVec3f.Of(p.x, p.y + 3, p.z);
            RcVec3f baseUp = RcVec3f.Of(0, 1, 0);
            RcVec3f forward = RcVec3f.Of((1f - 2 * (float)random.NextDouble()), 0, (1f - 2 * (float)random.NextDouble()));
            forward.Normalize();
            RcVec3f side = RcVec3f.Cross(forward, baseUp);
            BoxCollider @base = new BoxCollider(baseCenter, Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(baseUp, forward, baseExtent),
                SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, walkableClimb);
            var roofUp = RcVec3f.Zero;
            RcVec3f roofExtent = RcVec3f.Of(4.5f, 4.5f, 8f);
            var rx = Matrix(45, forward.x, forward.y, forward.z);
            roofUp = MulMatrixVector(ref roofUp, rx, baseUp);
            RcVec3f roofCenter = RcVec3f.Of(p.x, p.y + 6, p.z);
            BoxCollider roof = new BoxCollider(roofCenter, Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(roofUp, forward, roofExtent),
                SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, walkableClimb);
            RcVec3f trunkStart = RcVec3f.Of(
                baseCenter.x - forward.x * 15 + side.x * 6,
                p.y,
                baseCenter.z - forward.z * 15 + side.z * 6
            );
            RcVec3f trunkEnd = RcVec3f.Of(trunkStart.x, trunkStart.y + 10, trunkStart.z);
            CapsuleCollider trunk = new CapsuleCollider(trunkStart, trunkEnd, 0.5f, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD,
                walkableClimb);
            RcVec3f crownCenter = RcVec3f.Of(
                baseCenter.x - forward.x * 15 + side.x * 6, p.y + 10,
                baseCenter.z - forward.z * 15 + side.z * 6
            );
            SphereCollider crown = new SphereCollider(crownCenter, 4f, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GRASS,
                walkableClimb);
            CompositeCollider collider = new CompositeCollider(@base, roof, trunk, crown);
            IRcGizmoMeshFilter baseGizmo = GizmoFactory.Box(baseCenter, Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(baseUp, forward, baseExtent));
            IRcGizmoMeshFilter roofGizmo = GizmoFactory.Box(roofCenter, Detour.Dynamic.Colliders.BoxCollider.GetHalfEdges(roofUp, forward, roofExtent));
            IRcGizmoMeshFilter trunkGizmo = GizmoFactory.Capsule(trunkStart, trunkEnd, 0.5f);
            IRcGizmoMeshFilter crownGizmo = GizmoFactory.Sphere(crownCenter, 4f);
            IRcGizmoMeshFilter gizmo = GizmoFactory.Composite(baseGizmo, roofGizmo, trunkGizmo, crownGizmo);
            return new RcGizmo(collider, gizmo);
        }

        public RcGizmo TrimeshBridge(RcVec3f p, float walkableClimb)
        {
            return TrimeshCollider(p, bridgeGeom, walkableClimb);
        }

        public RcGizmo TrimeshHouse(RcVec3f p, float walkableClimb)
        {
            return TrimeshCollider(p, houseGeom, walkableClimb);
        }

        public RcGizmo ConvexTrimesh(RcVec3f p, float walkableClimb)
        {
            float[] verts = TransformVertices(p, convexGeom, 360);
            var collider = new ConvexTrimeshCollider(verts, convexGeom.faces,
                SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD, walkableClimb * 10);
            var gizmo = GizmoFactory.Trimesh(verts, convexGeom.faces);
            return new RcGizmo(collider, gizmo);
        }

        private RcGizmo TrimeshCollider(RcVec3f p, DemoInputGeomProvider geom, float walkableClimb)
        {
            float[] verts = TransformVertices(p, geom, 0);
            var collider = new TrimeshCollider(verts, geom.faces, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD,
                walkableClimb * 10);
            var gizmo = GizmoFactory.Trimesh(verts, geom.faces);

            return new RcGizmo(collider, gizmo);
        }

        private float[] TransformVertices(RcVec3f p, DemoInputGeomProvider geom, float ax)
        {
            var rx = Matrix((float)random.NextDouble() * ax, 1, 0, 0);
            var ry = Matrix((float)random.NextDouble() * 360, 0, 1, 0);
            var m = RcMatrix4x4f.Mul(rx, ry);
            float[] verts = new float[geom.vertices.Length];
            RcVec3f v = new RcVec3f();
            RcVec3f vr = new RcVec3f();
            for (int i = 0; i < geom.vertices.Length; i += 3)
            {
                v.x = geom.vertices[i];
                v.y = geom.vertices[i + 1];
                v.z = geom.vertices[i + 2];
                MulMatrixVector(ref vr, m, v);
                vr.x += p.x;
                vr.y += p.y - 0.1f;
                vr.z += p.z;
                verts[i] = vr.x;
                verts[i + 1] = vr.y;
                verts[i + 2] = vr.z;
            }

            return verts;
        }

        private static float[] MulMatrixVector(float[] resultvector, float[] matrix, float[] pvector)
        {
            resultvector[0] = matrix[0] * pvector[0] + matrix[4] * pvector[1] + matrix[8] * pvector[2];
            resultvector[1] = matrix[1] * pvector[0] + matrix[5] * pvector[1] + matrix[9] * pvector[2];
            resultvector[2] = matrix[2] * pvector[0] + matrix[6] * pvector[1] + matrix[10] * pvector[2];
            return resultvector;
        }

        private static RcVec3f MulMatrixVector(ref RcVec3f resultvector, RcMatrix4x4f matrix, RcVec3f pvector)
        {
            resultvector.x = matrix.M11 * pvector.x + matrix.M21 * pvector.y + matrix.M31 * pvector.z;
            resultvector.y = matrix.M12 * pvector.x + matrix.M22 * pvector.y + matrix.M32 * pvector.z;
            resultvector.z = matrix.M13 * pvector.x + matrix.M23 * pvector.y + matrix.M33 * pvector.z;
            return resultvector;
        }

        public static RcMatrix4x4f Matrix(float a, float x, float y, float z)
        {
            var matrix = new RcMatrix4x4f();
            a = (float)(a * Math.PI / 180.0); // convert to radians
            float s = (float)Math.Sin(a);
            float c = (float)Math.Cos(a);
            float t = 1.0f - c;

            float tx = t * x;
            float ty = t * y;
            float tz = t * z;

            float sz = s * z;
            float sy = s * y;
            float sx = s * x;

            matrix.M11 = tx * x + c;
            matrix.M12 = tx * y + sz;
            matrix.M13 = tx * z - sy;
            matrix.M14 = 0;

            matrix.M21 = tx * y - sz;
            matrix.M22 = ty * y + c;
            matrix.M23 = ty * z + sx;
            matrix.M24 = 0;

            matrix.M31 = tx * z + sy;
            matrix.M32 = ty * z - sx;
            matrix.M33 = tz * z + c;
            matrix.M34 = 0;

            matrix.M41 = 0;
            matrix.M42 = 0;
            matrix.M43 = 0;
            matrix.M44 = 1;

            return matrix;
        }
    }
}