

using System;

namespace DotRecast.Core
{
    public readonly ref struct RcScopedTimer // try final 对性能的影响可以忽略不计
    {
#if PROFILE
        private readonly RcContext _context;
        private readonly RcTimerLabel _label;
#endif        

        internal RcScopedTimer(RcContext context, RcTimerLabel label)
        {
#if PROFILE
            _context = context;
            _label = label;
            
            _context.StartTimer(_label);
#endif
        }

        public void Dispose()
        {
#if PROFILE
            _context.StopTimer(_label);
#endif
        }
    }
}

