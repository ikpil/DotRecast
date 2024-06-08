using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Numerics
{
    public static class RcVecUtils
    {
        public const float EPSILON = 1e-6f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Create(Span<float> values, int n)
        {
            return new RcVec3f(values[n + 0], values[n + 1], values[n + 2]);
        }

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
        public static float Dot2D(this RcVec3f @this, Span<float> v, int vi)
        {
            return @this.X * v[vi] +
                   @this.Z * v[vi + 2];
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

        /// Derives the distance between the specified points on the xz-plane.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the point on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2D(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.X - v1.X;
            float dz = v2.Z - v1.Z;
            return (float)MathF.Sqrt(dx * dx + dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2DSqr(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.X - v1.X;
            float dz = v2.Z - v1.Z;
            return dx * dx + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2DSqr(RcVec3f p, float[] verts, int i)
        {
            float dx = verts[i] - p.X;
            float dz = verts[i + 2] - p.Z;
            return dx * dx + dz * dz;
        }

        /// Derives the xz-plane 2D perp product of the two vectors. (uz*vx - ux*vz)
        /// @param[in] u The LHV vector [(x, y, z)]
        /// @param[in] v The RHV vector [(x, y, z)]
        /// @return The dot product on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Perp2D(RcVec3f u, RcVec3f v)
        {
            return u.Z * v.X - u.X * v.Z;
        }

        /// Checks that the specified vector's components are all finite.
        /// @param[in] v A point. [(x, y, z)]
        /// @return True if all of the point's components are finite, i.e. not NaN
        /// or any of the infinities.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(this RcVec3f v)
        {
            return float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);
        }

        /// Checks that the specified vector's 2D components are finite.
        /// @param[in] v A point. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite2D(this RcVec3f v)
        {
            return float.IsFinite(v.X) && float.IsFinite(v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PerpXZ(RcVec3f a, RcVec3f b)
        {
            return (a.X * b.Z) - (a.Z * b.X);
        }

        /// Performs a linear interpolation between two vectors. (@p v1 toward @p
        /// v2)
        /// @param[out] dest The result vector. [(x, y, x)]
        /// @param[in] v1 The starting vector.
        /// @param[in] v2 The destination vector.
        /// @param[in] t The interpolation factor. [Limits: 0 <= value <= 1.0]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Lerp(Span<float> verts, int v1, int v2, float t)
        {
            return new RcVec3f(
                verts[v1 + 0] + (verts[v2 + 0] - verts[v1 + 0]) * t,
                verts[v1 + 1] + (verts[v2 + 1] - verts[v1 + 1]) * t,
                verts[v1 + 2] + (verts[v2 + 2] - verts[v1 + 2]) * t
            );
        }

        /// Performs a scaled vector addition. (@p v1 + (@p v2 * @p s))
        /// @param[out] dest The result vector. [(x, y, z)]
        /// @param[in] v1 The base vector. [(x, y, z)]
        /// @param[in] v2 The vector to scale and add to @p v1. [(x, y, z)]
        /// @param[in] s The amount to scale @p v2 by before adding to @p v1.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Mad(RcVec3f v1, RcVec3f v2, float s)
        {
            return new RcVec3f()
            {
                X = v1.X + (v2.X * s),
                Y = v1.Y + (v2.Y * s),
                Z = v1.Z + (v2.Z * s),
            };
        }
    }
}