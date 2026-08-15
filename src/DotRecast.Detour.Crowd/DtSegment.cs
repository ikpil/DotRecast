using DotRecast.Core.Collections;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Crowd
{
    public struct DtSegment
    {
        /** Segment start/end */
        public RcFixedArray2<RcVec3f> s;

        /** Distance for pruning. */
        public float d;
    }
}
