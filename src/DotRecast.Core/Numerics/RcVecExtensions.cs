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
    }
}