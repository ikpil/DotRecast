using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Numerics
{
    public static class RcVecUtils
    {
        public const float EPSILON = 1e-6f;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(float[] @out, int n, float[] @in, int m)
        {
            @out[n + 0] = @in[m + 0];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] +
                   v1[1] * v2[1] +
                   v1[2] * v2[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float[] v1, RcVec3f vector2)
        {
            return v1[0] * vector2.X +
                   v1[1] * vector2.Y +
                   v1[2] * vector2.Z;
        }

        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(RcVec3f v1, float[] v2, int i)
        {
            float dx = v2[i] - v1.X;
            float dy = v2[i + 1] - v1.Y;
            float dz = v2[i + 2] - v1.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// Normalizes the vector if the length is greater than zero.
        /// If the magnitude is zero, the vector is unchanged.
        /// @param[in,out]	v	The vector to normalize. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f SafeNormalize(RcVec3f v)
        {
            float sqMag = RcMath.Sqr(v.X) + RcMath.Sqr(v.Y) + RcMath.Sqr(v.Z);
            if (sqMag > EPSILON)
            {
                float inverseMag = 1.0f / MathF.Sqrt(sqMag);
                return new RcVec3f(
                    v.X *= inverseMag,
                    v.Y *= inverseMag,
                    v.Z *= inverseMag
                );
            }

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Min(RcVec3f v, float[] @in, int i)
        {
            return new RcVec3f(
                (v.X < @in[i + 0]) ? v.X : @in[i + 0],
                (v.Y < @in[i + 1]) ? v.Y : @in[i + 1],
                (v.Z < @in[i + 2]) ? v.Z : @in[i + 2]
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Max(RcVec3f v, float[] @in, int i)
        {
            return new RcVec3f(
                (v.X > @in[i + 0]) ? v.X : @in[i + 0],
                (v.Y > @in[i + 1]) ? v.Y : @in[i + 1],
                (v.Z > @in[i + 2]) ? v.Z : @in[i + 2]
            );
        }
    }
}