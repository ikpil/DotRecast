using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DotRecast.Core.Buffers
{
    public static class RcCyclicBuffers
    {
        public static long Sum(this RcCyclicBuffer<long> source)
        {
            var buffer = source.GetBufferSpan();
            var result = 0L;
            if (Vector.IsHardwareAccelerated)
            {
                var vectors = MemoryMarshal.Cast<long, Vector<long>>(buffer);
                var vecSum = Vector<long>.Zero;
                foreach (var vec in vectors)
                    vecSum += vec;

                result = Vector.Dot(vecSum, Vector<long>.One);
                var remainder = source.Size % Vector<long>.Count;
                buffer = buffer[^remainder..];
            }
            
            foreach (var val in buffer)
                result += val;
            
            return result;
        }

        public static double Average(this RcCyclicBuffer<long> source)
        {
            if (0 >= source.Size)
                return 0;

            return source.Sum() / (double)source.Size;
        }
        
        public static long Min(this RcCyclicBuffer<long> source)
        {
            var buffer = source.GetBufferSpan();
            var result = long.MaxValue;
            
            if (Vector.IsHardwareAccelerated)
            {
                var vectors = MemoryMarshal.Cast<long, Vector<long>>(buffer);
                var vecMin = Vector<long>.One * result;
                
                foreach (var vec in vectors)
                    vecMin = Vector.Min(vecMin, vec);

                for (int i = 0; i < Vector<long>.Count; i++)
                    result = Math.Min(result, vecMin[i]);
                
                var remainder = source.Size % Vector<long>.Count;
                buffer = buffer[^remainder..];
            }
            
            foreach (var val in buffer)
                result = Math.Min(result, val);

            return result;
        }
        
        public static long Max(this RcCyclicBuffer<long> source)
        {
            var buffer = source.GetBufferSpan();
            var result = long.MinValue;
            
            if (Vector.IsHardwareAccelerated)
            {
                var vectors = MemoryMarshal.Cast<long, Vector<long>>(buffer);
                var vecMax = Vector<long>.One * result;
                
                foreach (var vec in vectors)
                    vecMax = Vector.Max(vecMax, vec);

                for (int i = 0; i < Vector<long>.Count; i++)
                    result = Math.Max(result, vecMax[i]);
                
                var remainder = source.Size % Vector<long>.Count;
                buffer = buffer[^remainder..];
            }
            
            foreach (var val in buffer)
                result = Math.Max(result, val);

            return result;
        }
    }
}