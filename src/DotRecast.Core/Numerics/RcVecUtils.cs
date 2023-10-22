using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Numerics
{
    public static class RcVecUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Get(this RcVec2f v, int i)
        {
            switch (i)
            {
                case 0: return v.X;
                case 1: return v.Y;
                default: throw new IndexOutOfRangeException("vector2f index out of range");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Get(this RcVec3f v, int i)
        {
            switch (i)
            {
                case 0: return v.X;
                case 1: return v.Y;
                case 2: return v.Z;
                default: throw new IndexOutOfRangeException("vector3f index out of range");
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Scale(this RcVec3f v, float scale)
        {
            return v * scale;
        }

        /// Derives the dot product of two vectors on the xz-plane. (@p u . @p v)
        /// @param[in] u A vector [(x, y, z)]
        /// @param[in] v A vector [(x, y, z)]
        /// @return The dot product on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot2D(this RcVec3f @this, RcVec3f v)
        {
            return @this.X * v.X +
                   @this.Z * v.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot2D(this RcVec3f @this, float[] v, int vi)
        {
            return @this.X * v[vi] +
                   @this.Z * v[vi + 2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Add(RcVec3f a, float[] verts, int i)
        {
            return new RcVec3f(
                a.X + verts[i],
                a.Y + verts[i + 1],
                a.Z + verts[i + 2]
            );
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Subtract(float[] verts, int i, int j)
        {
            return new RcVec3f(
                verts[i] - verts[j],
                verts[i + 1] - verts[j + 1],
                verts[i + 2] - verts[j + 2]
            );
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Subtract(RcVec3f i, float[] verts, int j)
        {
            return new RcVec3f(
                i.X - verts[j],
                i.Y - verts[j + 1],
                i.Z - verts[j + 2]
            );
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cross(float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
        }
    }
}