///*
//recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
//DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

//This software is provided 'as-is', without any express or implied
//warranty.  In no event will the authors be held liable for any damages
//arising from the use of this software.
//Permission is granted to anyone to use this software for any purpose,
//including commercial applications, and to alter it and redistribute it
//freely, subject to the following restrictions:
//1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//3. This notice may not be removed or altered from any source distribution.
//*/

//using System;
//using System.Runtime.CompilerServices;

//namespace System.Numerics
//{
//    public struct Vector3
//    {
//        public float X;
//        public float Y;
//        public float Z;

//        public static readonly Vector3 Zero = new Vector3(0.0f, 0.0f, 0.0f);
//        public static readonly Vector3 One = new Vector3(1.0f);
//        public static readonly Vector3 UnitX = new Vector3(1.0f, 0.0f, 0.0f);
//        public static readonly Vector3 UnitY = new Vector3(0.0f, 1.0f, 0.0f);
//        public static readonly Vector3 UnitZ = new Vector3(0.0f, 0.0f, 1.0f);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public Vector3(float x, float y, float z)
//        {
//            X = x;
//            Y = y;
//            Z = z;
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public Vector3(float f)
//        {
//            X = f;
//            Y = f;
//            Z = f;
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public Vector3(ReadOnlySpan<float> values)
//        {
//            if (values.Length < 3)
//            {
//                RcThrowHelper.ThrowArgumentOutOfRangeException(nameof(values));
//            }

//            X = values[0];
//            Y = values[1];
//            Z = values[2];
//        }


//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public readonly float Length()
//        {
//            float lengthSquared = LengthSquared();
//            return MathF.Sqrt(lengthSquared);
//        }

//        /// Derives the square of the scalar length of the vector. (len * len)
//        /// @param[in] v The vector. [(x, y, z)]
//        /// @return The square of the scalar length of the vector.
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public readonly float LengthSquared()
//        {
//            return Dot(this, this);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 Subtract(Vector3 left, Vector3 right)
//        {
//            return left - right;
//        }


//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 Add(Vector3 left, Vector3 right)
//        {
//            return left + right;
//        }


//        public override bool Equals(object obj)
//        {
//            if (!(obj is Vector3))
//                return false;

//            return Equals((Vector3)obj);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public bool Equals(Vector3 other)
//        {
//            return X.Equals(other.X) &&
//                   Y.Equals(other.Y) &&
//                   Z.Equals(other.Z);
//        }


//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public override int GetHashCode()
//        {
//            return HashCode.Combine(X, Y, Z);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 Min(Vector3 value1, Vector3 value2)
//        {
//            return new Vector3(
//                (value1.X < value2.X) ? value1.X : value2.X,
//                (value1.Y < value2.Y) ? value1.Y : value2.Y,
//                (value1.Z < value2.Z) ? value1.Z : value2.Z
//            );
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 Max(Vector3 value1, Vector3 value2)
//        {
//            return new Vector3(
//                (value1.X > value2.X) ? value1.X : value2.X,
//                (value1.Y > value2.Y) ? value1.Y : value2.Y,
//                (value1.Z > value2.Z) ? value1.Z : value2.Z
//            );
//        }

//        public override string ToString()
//        {
//            return $"{X}, {Y}, {Z}";
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static bool operator ==(Vector3 left, Vector3 right)
//        {
//            return left.Equals(right);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static bool operator !=(Vector3 left, Vector3 right)
//        {
//            return !left.Equals(right);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 operator -(Vector3 left, Vector3 right)
//        {
//            return new Vector3(
//                left.X - right.X,
//                left.Y - right.Y,
//                left.Z - right.Z
//            );
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 operator +(Vector3 left, Vector3 right)
//        {
//            return new Vector3(
//                left.X + right.X,
//                left.Y + right.Y,
//                left.Z + right.Z
//            );
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 operator *(Vector3 left, Vector3 right)
//        {
//            return new Vector3(
//                left.X * right.X,
//                left.Y * right.Y,
//                left.Z * right.Z
//            );
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 operator *(Vector3 left, float right)
//        {
//            return left * new Vector3(right);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 operator *(float left, Vector3 right)
//        {
//            return right * left;
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
//        {
//            return (value1 * (1f - amount)) + (value2 * amount);
//            // return new RcVec3f(
//            //     value1.X + (value2.X - value1.X) * amount,
//            //     value1.Y + (value2.Y - value1.Y) * amount,
//            //     value1.Z + (value2.Z - value1.Z) * amount
//            // );
//        }

//        /// Returns the distance between two points.
//        /// @param[in] v1 A point. [(x, y, z)]
//        /// @param[in] v2 A point. [(x, y, z)]
//        /// @return The distance between the two points.
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static float Distance(Vector3 value1, Vector3 value2)
//        {
//            float distanceSquared = DistanceSquared(value1, value2);
//            return MathF.Sqrt(distanceSquared);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static float DistanceSquared(Vector3 value1, Vector3 value2)
//        {
//            var difference = value1 - value2;
//            return Dot(difference, difference);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static float Dot(Vector3 vector1, Vector3 vector2)
//        {
//            return (vector1.X * vector2.X) +
//                   (vector1.Y * vector2.Y) +
//                   (vector1.Z * vector2.Z);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public readonly void CopyTo(float[] array)
//        {
//            CopyTo(array, 0);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public readonly void CopyTo(float[] array, int n)
//        {
//            array[n + 0] = X;
//            array[n + 1] = Y;
//            array[n + 2] = Z;
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 Cross(Vector3 v1, Vector3 v2)
//        {
//            return new Vector3(
//                (v1.Y * v2.Z) - (v1.Z * v2.Y),
//                (v1.Z * v2.X) - (v1.X * v2.Z),
//                (v1.X * v2.Y) - (v1.Y * v2.X)
//            );
//        }

//        /// Normalizes the vector.
//        /// @param[in,out] v The vector to normalize. [(x, y, z)]
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Vector3 Normalize(Vector3 v)
//        {
//            float d = 1.0f / MathF.Sqrt(RcMath.Sqr(v.X) + RcMath.Sqr(v.Y) + RcMath.Sqr(v.Z));

//            return new Vector3(
//                v.X *= d,
//                v.Y *= d,
//                v.Z *= d
//            );
//        }
//    }
//}