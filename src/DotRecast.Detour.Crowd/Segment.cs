using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    public class Segment
    {
        /** Segment start/end */
        public RcVec3f[] s = new RcVec3f[2];

        /** Distance for pruning. */
        public float d;
    }
}