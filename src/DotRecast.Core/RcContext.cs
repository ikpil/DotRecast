/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotRecast.Core
{
    /// Provides an interface for optional logging and performance tracking of the Recast 
    /// build process.
    /// 
    /// This class does not provide logging or timer functionality on its 
    /// own.  Both must be provided by a concrete implementation 
    /// by overriding the protected member functions.  Also, this class does not 
    /// provide an interface for extracting log messages. (Only adding them.) 
    /// So concrete implementations must provide one.
    ///
    /// If no logging or timers are required, just pass an instance of this 
    /// class through the Recast build process.
    /// 
    /// @ingroup recast
    public class RcContext
    {
#if PROFILE
        private readonly ThreadLocal<Dictionary<string, RcAtomicLong>> _timerStart;
        private readonly ConcurrentDictionary<string, RcAtomicLong> _timerAccum;
#endif

        public RcContext()
        {
#if PROFILE
            _timerStart = new(() => new(32));
            _timerAccum = new(Environment.ProcessorCount, 32);
#endif
        }

        public RcScopedTimer ScopedTimer(RcTimerLabel label)
        {
            return new RcScopedTimer(this, label);
        }

        [Conditional("PROFILE")]
        public void StartTimer(RcTimerLabel label)
        {
#if PROFILE
            _timerStart.Value[label.Name] = new RcAtomicLong(RcFrequency.Ticks);
#endif
        }

        [Conditional("PROFILE")]
        public void StopTimer(RcTimerLabel label)
        {
#if PROFILE
            _timerAccum
               .GetOrAdd(label.Name, _ => new RcAtomicLong(0))
               .AddAndGet(RcFrequency.Ticks - _timerStart.Value?[label.Name].Read() ?? 0);
#endif
        }

        public void Warn(string message)
        {
            Console.WriteLine(message);
        }

        public List<RcTelemetryTick> ToList()
        {
#if PROFILE
            return _timerAccum
                .Select(x => new RcTelemetryTick(x.Key, x.Value.Read()))
                .ToList();
#else
            return new List<RcTelemetryTick>();
#endif
        }
    }
}