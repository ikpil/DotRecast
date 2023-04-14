using System;
using System.Diagnostics;

namespace DotRecast.Core
{
    public static class FrequencyWatch
    {
        public static readonly double Frequency = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        public static long Ticks => unchecked((long)(Stopwatch.GetTimestamp() * Frequency));
    }
}