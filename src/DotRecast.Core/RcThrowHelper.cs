using System;
using System.Runtime.CompilerServices;
using DotRecast.Core.Collections;

namespace DotRecast.Core
{
    public static class RcThrowHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowExceptionIfIndexOutOfRange(int index, int size)
        {
            if (0 > index || index >= size)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range - size({size})");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StackOverflow()
        {
            var array_128_512_1 = RcStackArray128<RcStackArray512<float>>.Empty; // 128 * 512 = 65536
            var array_128_512_2 = RcStackArray128<RcStackArray512<float>>.Empty; // 128 * 512 = 65536

            var array_32_512_1 = RcStackArray32<RcStackArray512<float>>.Empty; // 32 * 512 = 16384

            var array_16_512_1 = RcStackArray16<RcStackArray512<float>>.Empty; // 16 * 512 = 8192

            var array_8_512_1 = RcStackArray8<RcStackArray512<float>>.Empty; // 8 * 512 = 4196
            var array_4_256_1 = RcStackArray4<RcStackArray256<float>>.Empty; // 4 * 256 = 1024
            var array_4_64_1 = RcStackArray4<RcStackArray64<float>>.Empty; // 4 * 64 = 256
        
            //
            var array_2_8_1 = RcStackArray2<RcStackArray8<float>>.Empty; // 2 * 8 = 16
            var array_2_4_1 = RcStackArray2<RcStackArray2<float>>.Empty; // 2 * 2 = 4
            
            float f1 = 0.0f; // 1
            //float f2 = 0.0f; // my system stack overflow!
        }

 
    }
}