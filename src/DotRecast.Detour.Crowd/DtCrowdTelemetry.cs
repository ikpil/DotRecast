
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotRecast.Core;
using DotRecast.Core.Buffers;

namespace DotRecast.Detour.Crowd
{
    public class DtCrowdTelemetry
    {
        public const int TIMING_SAMPLES = 10;
        private float _maxTimeToEnqueueRequest;
        private float _maxTimeToFindPath;

#if PROFILE
        private readonly Dictionary<DtCrowdTimerLabel, long> _executionTimings = new Dictionary<DtCrowdTimerLabel, long>();
        private readonly Dictionary<DtCrowdTimerLabel, RcCyclicBuffer<long>> _executionTimingSamples = new Dictionary<DtCrowdTimerLabel, RcCyclicBuffer<long>>();
#endif

        public float MaxTimeToEnqueueRequest()
        {
            return _maxTimeToEnqueueRequest;
        }

        public float MaxTimeToFindPath()
        {
            return _maxTimeToFindPath;
        }

        public IEnumerable<RcTelemetryTick> ToExecutionTimings()
        {
#if PROFILE
            foreach (var e in _executionTimings)
            {
                yield return new RcTelemetryTick(e.Key.Label, e.Value);
            }
#else
            yield return default;
#endif
        }

        [Conditional("PROFILE")]
        public void Start()
        {
#if PROFILE
            _maxTimeToEnqueueRequest = 0;
            _maxTimeToFindPath = 0;
            _executionTimings.Clear();
#endif
        }

        [Conditional("PROFILE")]
        public void RecordMaxTimeToEnqueueRequest(float time)
        {
#if PROFILE
            _maxTimeToEnqueueRequest = Math.Max(_maxTimeToEnqueueRequest, time);
#endif
        }

        [Conditional("PROFILE")]
        public void RecordMaxTimeToFindPath(float time)
        {
#if PROFILE
            _maxTimeToFindPath = Math.Max(_maxTimeToFindPath, time);
#endif
        }

        internal DtCrowdScopedTimer ScopedTimer(DtCrowdTimerLabel label)
        {
            return new DtCrowdScopedTimer(this, label);
        }

        [Conditional("PROFILE")]
        internal void Start(DtCrowdTimerLabel name)
        {
#if PROFILE
            //_executionTimings.Add(name, RcFrequency.Ticks);
            _executionTimings[name] = RcFrequency.Ticks;
#endif
        }

        [Conditional("PROFILE")]
        internal void Stop(DtCrowdTimerLabel name)
        {
#if PROFILE
            long duration = RcFrequency.Ticks - _executionTimings[name];
            if (!_executionTimingSamples.TryGetValue(name, out var cb))
            {
                cb = new RcCyclicBuffer<long>(TIMING_SAMPLES);
                _executionTimingSamples.Add(name, cb);
            }

            cb.PushBack(duration);
            _executionTimings[name] = (long)cb.Average();
#endif
        }
    }
}

