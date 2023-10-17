/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Numerics
{
    public struct RcVec3f
    {
        public float X;
        public float Y;
        public float Z;

        public static RcVec3f Zero { get; } = new RcVec3f(0.0f, 0.0f, 0.0f);
        public static RcVec3f One { get; } = new RcVec3f(1.0f);
        public static RcVec3f UnitX { get; } = new RcVec3f(1.0f, 0.0f, 0.0f);
        public static RcVec3f UnitY { get; } = new RcVec3f(0.0f, 1.0f, 0.0f);
        public static RcVec3f UnitZ { get; } = new RcVec3f(0.0f, 0.0f, 1.0f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RcVec3f(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RcVec3f(float f)
        {
            X = f;
            Y = f;
            Z = f;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RcVec3f(ReadOnlySpan<float> values)
        {
            X = values[0];
            Y = values[1];
            Z = values[2];
        }

        public float this[int index]
        {
            get => GetElement(index);
            set => SetElement(index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetElement(int index)
        {
            switch (index)
            {
                case 0: return X;
                case 1: return Y;
                case 2: return Z;
                default: throw new IndexOutOfRangeException($"{index}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetElement(int index, float value)
        {
            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;

                default: throw new IndexOutOfRangeException($"{index}-{value}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float a, float b, float c)
        {
            X = a;
            Y = b;
            Z = c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float[] @in)
        {
            Set(@in, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float[] @in, int i)
        {
            X = @in[i];
            Y = @in[i + 1];
            Z = @in[i + 2];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Subtract(RcVec3f left, RcVec3f right)
        {
            return left - right;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RcVec3f Add(RcVec3f v2)
        {
            return new RcVec3f(
                X + v2.X,
                Y + v2.Y,
                Z + v2.Z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RcVec3f Scale(float scale)
        {
            return new RcVec3f(
                X * scale,
                Y * scale,
                Z * scale
            );
        }


        /// Derives the dot product of two vectors on the xz-plane. (@p u . @p v)
        /// @param[in] u A vector [(x, y, z)]
        /// @param[in] v A vector [(x, y, z)]
        /// @return The dot product on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot2D(RcVec3f v)
        {
            return X * v.X + Z * v.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot2D(float[] v, int vi)
        {
            return X * v[vi] + Z * v[vi + 2];
        }


        public override bool Equals(object obj)
        {
            if (!(obj is RcVec3f))
                return false;

            return Equals((RcVec3f)obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RcVec3f other)
        {
            return X.Equals(other.X) &&
                   Y.Equals(other.Y) &&
                   Z.Equals(other.Z);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            int hash = X.GetHashCode();
            hash = RcHashCodes.CombineHashCodes(hash, Y.GetHashCode());
            hash = RcHashCodes.CombineHashCodes(hash, Z.GetHashCode());
            return hash;
        }

        /// Normalizes the vector.
        /// @param[in,out] v The vector to normalize. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float d = (float)(1.0f / Math.Sqrt(RcMath.Sqr(X) + RcMath.Sqr(Y) + RcMath.Sqr(Z)));
            if (d != 0)
            {
                X *= d;
                Y *= d;
                Z *= d;
            }
        }

        public const float EPSILON = 1e-6f;

        /// Normalizes the vector if the length is greater than zero.
        /// If the magnitude is zero, the vector is unchanged.
        /// @param[in,out]	v	The vector to normalize. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SafeNormalize()
        {
            float sqMag = RcMath.Sqr(X) + RcMath.Sqr(Y) + RcMath.Sqr(Z);
            if (sqMag > EPSILON)
            {
                float inverseMag = 1.0f / (float)Math.Sqrt(sqMag);
                X *= inverseMag;
                Y *= inverseMag;
                Z *= inverseMag;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Min(float[] @in, int i)
        {
            X = Math.Min(X, @in[i]);
            Y = Math.Min(Y, @in[i + 1]);
            Z = Math.Min(Z, @in[i + 2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Min(RcVec3f b)
        {
            X = Math.Min(X, b.X);
            Y = Math.Min(Y, b.Y);
            Z = Math.Min(Z, b.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Max(RcVec3f b)
        {
            X = Math.Max(X, b.X);
            Y = Math.Max(Y, b.Y);
            Z = Math.Max(Z, b.Z);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Max(float[] @in, int i)
        {
            X = Math.Max(X, @in[i]);
            Y = Math.Max(Y, @in[i + 1]);
            Z = Math.Max(Z, @in[i + 2]);
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RcVec3f left, RcVec3f right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RcVec3f left, RcVec3f right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator -(RcVec3f left, RcVec3f right)
        {
            return new RcVec3f(
                left.X - right.X,
                left.Y - right.Y,
                left.Z - right.Z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator +(RcVec3f left, RcVec3f right)
        {
            return left.Add(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator *(RcVec3f left, RcVec3f right)
        {
            return new RcVec3f(
                left.X * right.X,
                left.Y * right.Y,
                left.Z * right.Z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator *(RcVec3f left, float right)
        {
            return left * new RcVec3f(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator *(float left, RcVec3f right)
        {
            return right * left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Cross(RcVec3f v1, RcVec3f v2)
        {
            return new RcVec3f(
                (v1.Y * v2.Z) - (v1.Z * v2.Y),
                (v1.Z * v2.X) - (v1.X * v2.Z),
                (v1.X * v2.Y) - (v1.Y * v2.X)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Lerp(RcVec3f v1, RcVec3f v2, float t)
        {
            return new RcVec3f(
                v1.X + (v2.X - v1.X) * t,
                v1.Y + (v2.Y - v1.Y) * t,
                v1.Z + (v2.Z - v1.Z) * t
            );
        }

        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(RcVec3f v1, RcVec3f v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y)
                                 + (v1.Z * v2.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float[] v1, RcVec3f v2)
        {
            return v1[0] * v2.X + v1[1] * v2.Y + v1[2] * v2.Z;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PerpXZ(RcVec3f a, RcVec3f b)
        {
            return (a.X * b.Z) - (a.Z * b.X);
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

        /// Performs a linear interpolation between two vectors. (@p v1 toward @p
        /// v2)
        /// @param[out] dest The result vector. [(x, y, x)]
        /// @param[in] v1 The starting vector.
        /// @param[in] v2 The destination vector.
        /// @param[in] t The interpolation factor. [Limits: 0 <= value <= 1.0]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Lerp(float[] verts, int v1, int v2, float t)
        {
            return new RcVec3f(
                verts[v1 + 0] + (verts[v2 + 0] - verts[v1 + 0]) * t,
                verts[v1 + 1] + (verts[v2 + 1] - verts[v1 + 1]) * t,
                verts[v1 + 2] + (verts[v2 + 2] - verts[v1 + 2]) * t
            );
        }


        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(RcVec3f v1, float[] v2, int i)
        {
            float dx = v2[i] - v1.X;
            float dy = v2[i + 1] - v1.Y;
            float dz = v2[i + 2] - v1.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(float[] v, int i, int j)
        {
            float dx = v[i] - v[j];
            float dy = v[i + 1] - v[j + 1];
            float dz = v[i + 2] - v[j + 2];
            return dx * dx + dy * dy + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }

        /// Derives the distance between the specified points on the xz-plane.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the point on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2D(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2D(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.X - v1.X;
            float dz = v2.Z - v1.Z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2DSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return dx * dx + dz * dz;
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

        /// Derives the square of the scalar length of the vector. (len * len)
        /// @param[in] v The vector. [(x, y, z)]
        /// @return The square of the scalar length of the vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LenSqr(RcVec3f v)
        {
            return v.X * v.X + v.Y * v.Y + v.Z * v.Z;
        }


        /// Checks that the specified vector's components are all finite.
        /// @param[in] v A point. [(x, y, z)]
        /// @return True if all of the point's components are finite, i.e. not NaN
        /// or any of the infinities.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(RcVec3f v)
        {
            return float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);
        }

        /// Checks that the specified vector's 2D components are finite.
        /// @param[in] v A point. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite2D(RcVec3f v)
        {
            return float.IsFinite(v.X) && float.IsFinite(v.Z);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(ref RcVec3f @out, float[] @in, int i)
        {
            Copy(ref @out, 0, @in, i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(float[] @out, int n, float[] @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(float[] @out, int n, RcVec3f @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(ref RcVec3f @out, int n, float[] @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref RcVec3f e0, RcVec3f a, float[] verts, int i)
        {
            e0.X = a.X + verts[i];
            e0.Y = a.Y + verts[i + 1];
            e0.Z = a.Z + verts[i + 2];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(ref RcVec3f e0, float[] verts, int i, int j)
        {
            e0.X = verts[i] - verts[j];
            e0.Y = verts[i + 1] - verts[j + 1];
            e0.Z = verts[i + 2] - verts[j + 2];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sub(ref RcVec3f e0, RcVec3f i, float[] verts, int j)
        {
            e0.X = i.X - verts[j];
            e0.Y = i.Y - verts[j + 1];
            e0.Z = i.Z - verts[j + 2];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cross(float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cross(float[] dest, RcVec3f v1, RcVec3f v2)
        {
            dest[0] = v1.Y * v2.Z - v1.Z * v2.Y;
            dest[1] = v1.Z * v2.X - v1.X * v2.Z;
            dest[2] = v1.X * v2.Y - v1.Y * v2.X;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cross(ref RcVec3f dest, RcVec3f v1, RcVec3f v2)
        {
            dest.X = v1.Y * v2.Z - v1.Z * v2.Y;
            dest.Y = v1.Z * v2.X - v1.X * v2.Z;
            dest.Z = v1.X * v2.Y - v1.Y * v2.X;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(ref RcVec3f v)
        {
            float d = (float)(1.0f / Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z));
            v.X *= d;
            v.Y *= d;
            v.Z *= d;
        }
    }
}