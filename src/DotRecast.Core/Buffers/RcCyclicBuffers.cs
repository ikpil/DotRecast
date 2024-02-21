﻿namespace DotRecast.Core.Buffers
{
    public static class RcCyclicBuffers
    {
        public static long Sum(this RcCyclicBuffer<long> source)
        {
            long sum = 0;
            checked
            {
                // NOTE: SIMD would be nice here
                foreach (var x in source)
                {
                    sum += x;
                }
            }

            return sum;
        }

        public static double Average(this RcCyclicBuffer<long> source)
        {
            if (0 >= source.Size)
                return 0;

            return source.Sum() / (double)source.Size;
        }
        
        public static long Min(this RcCyclicBuffer<long> source)
        {
            if (0 >= source.Size)
                return 0;
            
            long minValue = long.MaxValue;
            foreach (var x in source)
            {
                if (x < minValue)
                    minValue = x;
            }

            return minValue;
        }
        
        public static long Max(this RcCyclicBuffer<long> source)
        {
            if (0 >= source.Size)
                return 0;
            
            long maxValue = long.MinValue;
            foreach (var x in source)
            {
                if (x > maxValue)
                    maxValue = x;
            }

            return maxValue;
        }
    }
}