//using System;
//using System.Runtime.CompilerServices;

//namespace System.Numerics
//{
//    public struct Vector2
//    {
//        public float X;
//        public float Y;

//        public static readonly Vector2 Zero = new Vector2 { X = 0, Y = 0 };

//        public Vector2(float x, float y)
//        {
//            X = x;
//            Y = y;
//        }

//        public override bool Equals(object obj)
//        {
//            if (!(obj is Vector2))
//                return false;

//            return Equals((Vector2)obj);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public bool Equals(Vector2 other)
//        {
//            return X.Equals(other.X) &&
//                   Y.Equals(other.Y);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public override int GetHashCode()
//        {
//            int hash = X.GetHashCode();
//            hash = RcHashCodes.CombineHashCodes(hash, Y.GetHashCode());
//            return hash;
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static bool operator ==(Vector2 left, Vector2 right)
//        {
//            return left.Equals(right);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static bool operator !=(Vector2 left, Vector2 right)
//        {
//            return !left.Equals(right);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public override string ToString()
//        {
//            return $"{X}, {Y}";
//        }
//    }
//}