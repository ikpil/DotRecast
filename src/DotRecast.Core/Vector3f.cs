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
using System.Security.Permissions;

namespace DotRecast.Core
{
    public struct Vector3f
    {
        public float x;
        public float y;
        public float z;

        public static Vector3f Zero { get; } = new Vector3f(0, 0, 0);
        public static Vector3f Up { get; } = new Vector3f(0, 1, 0);

        public static Vector3f Of(float[] f)
        {
            return Of(f, 0);
        }

        public static Vector3f Of(float[] f, int idx)
        {
            return Of(f[idx + 0], f[idx + 1], f[idx + 2]);
        }

        public static Vector3f Of(float x, float y, float z)
        {
            return new Vector3f(x, y, z);
        }

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3f(float f)
        {
            x = f;
            y = f;
            z = f;
        }


        public Vector3f(float[] f)
        {
            x = f[0];
            y = f[1];
            z = f[2];
        }

        public float this[int index]
        {
            get => GetElement(index);
            set => SetElement(index, value);
        }

        public float GetElement(int index)
        {
            switch (index)
            {
                case 0: return x;
                case 1: return y;
                case 2: return z;
                default: throw new IndexOutOfRangeException($"{index}");
            }
        }

        public void SetElement(int index, float value)
        {
            switch (index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;

                default: throw new IndexOutOfRangeException($"{index}-{value}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float a, float b, float c)
        {
            x = a;
            y = b;
            z = c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float[] @in)
        {
            Set(@in, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float[] @in, int i)
        {
            x = @in[i];
            y = @in[i + 1];
            z = @in[i + 2];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Length()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3f Subtract(Vector3f right)
        {
            return new Vector3f(
                x - right.x,
                y - right.y,
                z - right.z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3f Add(Vector3f v2)
        {
            return new Vector3f(
                x + v2.x,
                y + v2.y,
                z + v2.z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3f Scale(float scale)
        {
            return new Vector3f(
                x * scale,
                y * scale,
                z * scale
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
        public readonly float Dot2D(Vector3f v)
        {
            return x * v.x + z * v.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot2D(float[] v, int vi)
        {
            return x * v[vi] + z * v[vi + 2];
        }


        public override bool Equals(object obj)
        {
            if (!(obj is Vector3f))
                return false;

            return Equals((Vector3f)obj);
        }

        public bool Equals(Vector3f other)
        {
            return x.Equals(other.x) &&
                   y.Equals(other.y) &&
                   z.Equals(other.z);
        }


        public override int GetHashCode()
        {
            int hash = x.GetHashCode();
            hash = RcHashCodes.CombineHashCodes(hash, y.GetHashCode());
            hash = RcHashCodes.CombineHashCodes(hash, z.GetHashCode());
            return hash;
        }

        /// Normalizes the vector.
        /// @param[in,out] v The vector to normalize. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float d = (float)(1.0f / Math.Sqrt(RcMath.Sqr(x) + RcMath.Sqr(y) + RcMath.Sqr(z)));
            if (d != 0)
            {
                x *= d;
                y *= d;
                z *= d;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Min(float[] @in, int i)
        {
            x = Math.Min(x, @in[i]);
            y = Math.Min(y, @in[i + 1]);
            z = Math.Min(z, @in[i + 2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Max(float[] @in, int i)
        {
            x = Math.Max(x, @in[i]);
            y = Math.Max(y, @in[i + 1]);
            z = Math.Max(z, @in[i + 2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3f left, Vector3f right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3f left, Vector3f right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f operator -(Vector3f left, Vector3f right)
        {
            return left.Subtract(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f operator +(Vector3f left, Vector3f right)
        {
            return left.Add(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f operator *(Vector3f left, Vector3f right)
        {
            return new Vector3f(
                left.x * right.x,
                left.y * right.y,
                left.z * right.z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f operator *(Vector3f left, float right)
        {
            return left * new Vector3f(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f operator *(float left, Vector3f right)
        {
            return right * left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f Cross(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(
                (v1.y * v2.z) - (v1.z * v2.y),
                (v1.z * v2.x) - (v1.x * v2.z),
                (v1.x * v2.y) - (v1.y * v2.x)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f Lerp(Vector3f v1, Vector3f v2, float t)
        {
            return new Vector3f(
                v1.x + (v2.x - v1.x) * t,
                v1.y + (v2.y - v1.y) * t,
                v1.z + (v2.z - v1.z) * t
            );
        }

        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3f v1, Vector3f v2)
        {
            float dx = v2.x - v1.x;
            float dy = v2.y - v1.y;
            float dz = v2.z - v1.z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector3f v1, Vector3f v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y)
                                 + (v1.z * v2.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float[] v1, Vector3f v2)
        {
            return v1[0] * v2.x + v1[1] * v2.y + v1[2] * v2.z;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PerpXZ(Vector3f a, Vector3f b)
        {
            return (a.x * b.z) - (a.z * b.x);
        }

        /// Performs a scaled vector addition. (@p v1 + (@p v2 * @p s))
        /// @param[out] dest The result vector. [(x, y, z)]
        /// @param[in] v1 The base vector. [(x, y, z)]
        /// @param[in] v2 The vector to scale and add to @p v1. [(x, y, z)]
        /// @param[in] s The amount to scale @p v2 by before adding to @p v1.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f Mad(Vector3f v1, Vector3f v2, float s)
        {
            return new Vector3f()
            {
                x = v1.x + (v2.x * s),
                y = v1.y + (v2.y * s),
                z = v1.z + (v2.z * s),
            };
        }

        /// Performs a linear interpolation between two vectors. (@p v1 toward @p
        /// v2)
        /// @param[out] dest The result vector. [(x, y, x)]
        /// @param[in] v1 The starting vector.
        /// @param[in] v2 The destination vector.
        /// @param[in] t The interpolation factor. [Limits: 0 <= value <= 1.0]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3f Lerp(float[] verts, int v1, int v2, float t)
        {
            return new Vector3f(
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
        public static float DistSqr(Vector3f v1, float[] v2, int i)
        {
            float dx = v2[i] - v1.x;
            float dy = v2[i + 1] - v1.y;
            float dz = v2[i + 2] - v1.z;
            return dx * dx + dy * dy + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(Vector3f v1, Vector3f v2)
        {
            float dx = v2.x - v1.x;
            float dy = v2.y - v1.y;
            float dz = v2.z - v1.z;
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
        public static float Dist2D(Vector3f v1, Vector3f v2)
        {
            float dx = v2.x - v1.x;
            float dz = v2.z - v1.z;
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
        public static float Dist2DSqr(Vector3f v1, Vector3f v2)
        {
            float dx = v2.x - v1.x;
            float dz = v2.z - v1.z;
            return dx * dx + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2DSqr(Vector3f p, float[] verts, int i)
        {
            float dx = verts[i] - p.x;
            float dz = verts[i + 2] - p.z;
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
        public static float Perp2D(Vector3f u, Vector3f v)
        {
            return u.z * v.x - u.x * v.z;
        }

        /// Derives the square of the scalar length of the vector. (len * len)
        /// @param[in] v The vector. [(x, y, z)]
        /// @return The square of the scalar length of the vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LenSqr(Vector3f v)
        {
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }


        /// Checks that the specified vector's components are all finite.
        /// @param[in] v A point. [(x, y, z)]
        /// @return True if all of the point's components are finite, i.e. not NaN
        /// or any of the infinities.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(Vector3f v)
        {
            return float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
        }

        /// Checks that the specified vector's 2D components are finite.
        /// @param[in] v A point. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite2D(Vector3f v)
        {
            return float.IsFinite(v.x) && float.IsFinite(v.z);
        }

        public static void Min(ref Vector3f a, float[] b, int i)
        {
            a.x = Math.Min(a.x, b[i + 0]);
            a.y = Math.Min(a.y, b[i + 1]);
            a.z = Math.Min(a.z, b[i + 2]);
        }

        public static void Min(ref Vector3f a, Vector3f b)
        {
            a.x = Math.Min(a.x, b.x);
            a.y = Math.Min(a.y, b.y);
            a.z = Math.Min(a.z, b.z);
        }

        public static void Max(ref Vector3f a, float[] b, int i)
        {
            a.x = Math.Max(a.x, b[i + 0]);
            a.y = Math.Max(a.y, b[i + 1]);
            a.z = Math.Max(a.z, b[i + 2]);
        }

        public static void Max(ref Vector3f a, Vector3f b)
        {
            a.x = Math.Max(a.x, b.x);
            a.y = Math.Max(a.y, b.y);
            a.z = Math.Max(a.z, b.z);
        }

        public static void Copy(ref Vector3f @out, float[] @in, int i)
        {
            Copy(ref @out, 0, @in, i);
        }

        public static void Copy(float[] @out, int n, float[] @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        public static void Copy(float[] @out, int n, Vector3f @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        public static void Copy(ref Vector3f @out, int n, float[] @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        public static void Add(ref Vector3f e0, Vector3f a, float[] verts, int i)
        {
            e0.x = a.x + verts[i];
            e0.y = a.y + verts[i + 1];
            e0.z = a.z + verts[i + 2];
        }


        public static void Sub(ref Vector3f e0, float[] verts, int i, int j)
        {
            e0.x = verts[i] - verts[j];
            e0.y = verts[i + 1] - verts[j + 1];
            e0.z = verts[i + 2] - verts[j + 2];
        }


        public static void Sub(ref Vector3f e0, Vector3f i, float[] verts, int j)
        {
            e0.x = i.x - verts[j];
            e0.y = i.y - verts[j + 1];
            e0.z = i.z - verts[j + 2];
        }


        public static void Cross(float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
        }

        public static void Cross(float[] dest, Vector3f v1, Vector3f v2)
        {
            dest[0] = v1.y * v2.z - v1.z * v2.y;
            dest[1] = v1.z * v2.x - v1.x * v2.z;
            dest[2] = v1.x * v2.y - v1.y * v2.x;
        }

        public static void Cross(ref Vector3f dest, Vector3f v1, Vector3f v2)
        {
            dest.x = v1.y * v2.z - v1.z * v2.y;
            dest.y = v1.z * v2.x - v1.x * v2.z;
            dest.z = v1.x * v2.y - v1.y * v2.x;
        }


        public static void Normalize(float[] v)
        {
            float d = (float)(1.0f / Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]));
            v[0] *= d;
            v[1] *= d;
            v[2] *= d;
        }

        public static void Normalize(ref Vector3f v)
        {
            float d = (float)(1.0f / Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
            v.x *= d;
            v.y *= d;
            v.z *= d;
        }
    }
}