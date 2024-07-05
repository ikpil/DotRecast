using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Crowd
{
    public unsafe struct DtSegment
    {
        /** Segment start/end */
        //public RcVec3f[] s = new RcVec3f[2];
        public fixed float s[2 * 3];

        /** Distance for pruning. */
        public float d;
    }
}