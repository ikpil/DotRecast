using System;
using System.Runtime.CompilerServices;
using DotRecast.Core.Numerics;

namespace DotRecast.Core.Numerics
{
    public static class RcVecExtensions
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
            return new RcVec3f(
                v.X * scale,
                v.Y * scale,
                v.Z * scale
            );
        }
    }
}