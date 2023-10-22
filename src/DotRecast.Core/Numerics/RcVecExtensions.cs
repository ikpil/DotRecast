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
            return v * scale;
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
        public static float Dot2D(this RcVec3f @this, float[] v, int vi)
        {
            return @this.X * v[vi] +
                   @this.Z * v[vi + 2];
        }
    }
}