using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Crowd
{
    public class DtSegment // TODO struct
    {
        /** Segment start/end */
        public RcVec3f[] s = new RcVec3f[2];

        /** Distance for pruning. */
        public float d;
    }
}