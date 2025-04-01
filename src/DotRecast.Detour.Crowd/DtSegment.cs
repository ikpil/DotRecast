using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Crowd
{
    public class DtSegment
    {
        /** Segment start/end */
        public Vector3[] s = new Vector3[2];

        /** Distance for pruning. */
        public float d;
    }
}