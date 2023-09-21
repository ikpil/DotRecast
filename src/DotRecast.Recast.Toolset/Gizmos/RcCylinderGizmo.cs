using DotRecast.Core;
using static DotRecast.Core.RcMath;
using static DotRecast.Recast.Toolset.Gizmos.RcGizmoHelper;


namespace DotRecast.Recast.Toolset.Gizmos
{
    public class RcCylinderGizmo : IRcGizmoMeshFilter
    {
        public readonly float[] vertices;
        public readonly int[] triangles;
        public readonly RcVec3f center;
        public readonly float[] gradient;

        public RcCylinderGizmo(RcVec3f start, RcVec3f end, float radius)
        {
            center = RcVec3f.Of(
                0.5f * (start.x + end.x), 0.5f * (start.y + end.y),
                0.5f * (start.z + end.z)
            );
            RcVec3f axis = RcVec3f.Of(end.x - start.x, end.y - start.y, end.z - start.z);
            RcVec3f[] normals = new RcVec3f[3];
            normals[1] = RcVec3f.Of(end.x - start.x, end.y - start.y, end.z - start.z);
            RcVec3f.Normalize(ref normals[1]);
            normals[0] = GetSideVector(axis);
            normals[2] = RcVec3f.Zero;
            RcVec3f.Cross(ref normals[2], normals[0], normals[1]);
            RcVec3f.Normalize(ref normals[2]);
            triangles = GenerateCylindricalTriangles();
            RcVec3f trX = RcVec3f.Of(normals[0].x, normals[1].x, normals[2].x);
            RcVec3f trY = RcVec3f.Of(normals[0].y, normals[1].y, normals[2].y);
            RcVec3f trZ = RcVec3f.Of(normals[0].z, normals[1].z, normals[2].z);
            vertices = GenerateCylindricalVertices();
            float halfLength = 0.5f * axis.Length();
            gradient = new float[vertices.Length / 3];
            RcVec3f v = new RcVec3f();
            for (int i = 0; i < vertices.Length; i += 3)
            {
                float offset = (i >= vertices.Length / 2) ? -halfLength : halfLength;
                float x = radius * vertices[i];
                float y = vertices[i + 1] + offset;
                float z = radius * vertices[i + 2];
                vertices[i] = x * trX.x + y * trX.y + z * trX.z + center.x;
                vertices[i + 1] = x * trY.x + y * trY.y + z * trY.z + center.y;
                vertices[i + 2] = x * trZ.x + y * trZ.y + z * trZ.z + center.z;
                if (i < vertices.Length / 4 || i >= 3 * vertices.Length / 4)
                {
                    gradient[i / 3] = 1;
                }
                else
                {
                    v.x = vertices[i] - center.x;
                    v.y = vertices[i + 1] - center.y;
                    v.z = vertices[i + 2] - center.z;
                    RcVec3f.Normalize(ref v);
                    gradient[i / 3] = Clamp(0.57735026f * (v.x + v.y + v.z), -1, 1);
                }
            }
        }

        private RcVec3f GetSideVector(RcVec3f axis)
        {
            RcVec3f side = RcVec3f.Of(1, 0, 0);
            if (axis.x > 0.8)
            {
                side = RcVec3f.Of(0, 0, 1);
            }

            RcVec3f forward = new RcVec3f();
            RcVec3f.Cross(ref forward, side, axis);
            RcVec3f.Cross(ref side, axis, forward);
            RcVec3f.Normalize(ref side);
            return side;
        }
    }
}