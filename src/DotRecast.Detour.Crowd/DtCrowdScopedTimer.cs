
using System;

namespace DotRecast.Detour.Crowd
{
    internal readonly ref struct DtCrowdScopedTimer
    {
#if PROFILE
        private readonly DtCrowdTimerLabel _label;
        private readonly DtCrowdTelemetry _telemetry;
#endif

        internal DtCrowdScopedTimer(DtCrowdTelemetry telemetry, DtCrowdTimerLabel label)
        {
#if PROFILE
            _telemetry = telemetry;
            _label = label;

            _telemetry.Start(_label);
#endif
        }

        public void Dispose()
        {
#if PROFILE
            _telemetry.Stop(_label);
#endif
        }
    }
}
