using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Numerics
{
    public struct RcVec2f
    {
        public float X;
        public float Y;

        public static RcVec2f Zero { get; } = new RcVec2f { X = 0, Y = 0 };

        public RcVec2f(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RcVec2f))
                return false;

            return Equals((RcVec2f)obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RcVec2f other)
        {
            return X.Equals(other.X) &&
                   Y.Equals(other.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            int hash = X.GetHashCode();
            hash = RcHashCodes.CombineHashCodes(hash, Y.GetHashCode());
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RcVec2f left, RcVec2f right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RcVec2f left, RcVec2f right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }
}