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
        public Vector3f Subtract(Vector3f right)
        {
            return new Vector3f(
                x - right.x,
                y - right.y,
                z - right.z
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
        public float Dot2D(Vector3f v)
        {
            return x * v.x + z * v.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot2D(float[] v, int vi)
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

        public static bool operator ==(Vector3f left, Vector3f right)
        {
            return left.Equals(right);
        }

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
    }
}