using System;
using DotRecast.Core;

using static DotRecast.Recast.Toolset.Gizmos.RcGizmoHelper;

namespace DotRecast.Recast.Toolset.Gizmos
{
    public class RcCapsuleGizmo : IRcGizmoMeshFilter
    {
        public readonly float[] vertices;
        public readonly int[] triangles;
        public readonly float[] center;
        public readonly float[] gradient;

        public RcCapsuleGizmo(RcVec3f start, RcVec3f end, float radius)
        {
            center = new float[]
            {
                0.5f * (start.X + end.X), 0.5f * (start.Y + end.Y),
                0.5f * (start.Z + end.Z)
            };
            RcVec3f axis = RcVec3f.Of(end.X - start.X, end.Y - start.Y, end.Z - start.Z);
            RcVec3f[] normals = new RcVec3f[3];
            normals[1] = RcVec3f.Of(end.X - start.X, end.Y - start.Y, end.Z - start.Z);
            RcVec3f.Normalize(ref normals[1]);
            normals[0] = GetSideVector(axis);
            normals[2] = RcVec3f.Zero;
            RcVec3f.Cross(ref normals[2], normals[0], normals[1]);
            RcVec3f.Normalize(ref normals[2]);
            triangles = GenerateSphericalTriangles();
            var trX = RcVec3f.Of(normals[0].X, normals[1].X, normals[2].X);
            var trY = RcVec3f.Of(normals[0].Y, normals[1].Y, normals[2].Y);
            var trZ = RcVec3f.Of(normals[0].Z, normals[1].Z, normals[2].Z);
            float[] spVertices = GenerateSphericalVertices();
            float halfLength = 0.5f * axis.Length();
            vertices = new float[spVertices.Length];
            gradient = new float[spVertices.Length / 3];
            RcVec3f v = new RcVec3f();
            for (int i = 0; i < spVertices.Length; i += 3)
            {
                float offset = (i >= spVertices.Length / 2) ? -halfLength : halfLength;
                float x = radius * spVertices[i];
                float y = radius * spVertices[i + 1] + offset;
                float z = radius * spVertices[i + 2];
                vertices[i] = x * trX.X + y * trX.Y + z * trX.Z + center[0];
                vertices[i + 1] = x * trY.X + y * trY.Y + z * trY.Z + center[1];
                vertices[i + 2] = x * trZ.X + y * trZ.Y + z * trZ.Z + center[2];
                v.X = vertices[i] - center[0];
                v.Y = vertices[i + 1] - center[1];
                v.Z = vertices[i + 2] - center[2];
                RcVec3f.Normalize(ref v);
                gradient[i / 3] = Math.Clamp(0.57735026f * (v.X + v.Y + v.Z), -1, 1);
            }
        }

        private RcVec3f GetSideVector(RcVec3f axis)
        {
            RcVec3f side = RcVec3f.Of(1, 0, 0);
            if (axis.X > 0.8)
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