using System;

namespace DotRecast.Recast
{
    public readonly struct RcTelemetryTick
    {
        public readonly string Key;
        public readonly long Ticks;
        public long Millis => Ticks / TimeSpan.TicksPerMillisecond;

        public RcTelemetryTick(string key, long ticks)
        {
            Key = key;
            Ticks = ticks;
        }
    }
}