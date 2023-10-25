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
        public readonly float Length()
        {
            float lengthSquared = LengthSquared();
            return MathF.Sqrt(lengthSquared);
        }

        /// Derives the square of the scalar length of the vector. (len * len)
        /// @param[in] v The vector. [(x, y, z)]
        /// @return The square of the scalar length of the vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float LengthSquared()
        {
            return Dot(this, this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Subtract(RcVec3f left, RcVec3f right)
        {
            return left - right;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Add(RcVec3f left, RcVec3f right)
        {
            return left + right;
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
            return HashCode.Combine(X, Y, Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Min(RcVec3f value1, RcVec3f value2)
        {
            return new RcVec3f(
                (value1.X < value2.X) ? value1.X : value2.X,
                (value1.Y < value2.Y) ? value1.Y : value2.Y,
                (value1.Z < value2.Z) ? value1.Z : value2.Z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Max(RcVec3f value1, RcVec3f value2)
        {
            return new RcVec3f(
                (value1.X > value2.X) ? value1.X : value2.X,
                (value1.Y > value2.Y) ? value1.Y : value2.Y,
                (value1.Z > value2.Z) ? value1.Z : value2.Z
            );
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
            return new RcVec3f(
                left.X + right.X,
                left.Y + right.Y,
                left.Z + right.Z
            );
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
        public static float Distance(RcVec3f value1, RcVec3f value2)
        {
            float distanceSquared = DistanceSquared(value1, value2);
            return MathF.Sqrt(distanceSquared);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(RcVec3f value1, RcVec3f value2)
        {
            var difference = value1 - value2;
            return Dot(difference, difference);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(RcVec3f vector1, RcVec3f vector2)
        {
            return (vector1.X * vector2.X) +
                   (vector1.Y * vector2.Y) +
                   (vector1.Z * vector2.Z);
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
        public readonly void CopyTo(float[] array)
        {
            CopyTo(array, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(float[] array, int n)
        {
            array[n + 0] = X;
            array[n + 1] = Y;
            array[n + 2] = Z;
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

        /// Normalizes the vector.
        /// @param[in,out] v The vector to normalize. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Normalize(RcVec3f v)
        {
            float d = 1.0f / MathF.Sqrt(RcMath.Sqr(v.X) + RcMath.Sqr(v.Y) + RcMath.Sqr(v.Z));

            return new RcVec3f(
                v.X *= d,
                v.Y *= d,
                v.Z *= d
            );
        }
    }
}