/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    public class CrowdTelemetry
    {
        public const int TIMING_SAMPLES = 10;
        private float _maxTimeToEnqueueRequest;
        private float _maxTimeToFindPath;
        private readonly Dictionary<string, long> _executionTimings = new Dictionary<string, long>();
        private readonly Dictionary<string, List<long>> _executionTimingSamples = new Dictionary<string, List<long>>();

        public float MaxTimeToEnqueueRequest()
        {
            return _maxTimeToEnqueueRequest;
        }

        public float MaxTimeToFindPath()
        {
            return _maxTimeToFindPath;
        }

        public Dictionary<string, long> ExecutionTimings()
        {
            return _executionTimings;
        }

        public void Start()
        {
            _maxTimeToEnqueueRequest = 0;
            _maxTimeToFindPath = 0;
            _executionTimings.Clear();
        }

        public void RecordMaxTimeToEnqueueRequest(float time)
        {
            _maxTimeToEnqueueRequest = Math.Max(_maxTimeToEnqueueRequest, time);
        }

        public void RecordMaxTimeToFindPath(float time)
        {
            _maxTimeToFindPath = Math.Max(_maxTimeToFindPath, time);
        }

        public void Start(string name)
        {
            _executionTimings.Add(name, RcFrequency.Ticks);
        }

        public void Stop(string name)
        {
            long duration = RcFrequency.Ticks - _executionTimings[name];
            if (!_executionTimingSamples.TryGetValue(name, out var s))
            {
                s = new List<long>();
                _executionTimingSamples.Add(name, s);
            }

            if (s.Count == TIMING_SAMPLES)
            {
                s.RemoveAt(0);
            }

            s.Add(duration);
            _executionTimings[name] = (long)s.Average();
        }
    }
}