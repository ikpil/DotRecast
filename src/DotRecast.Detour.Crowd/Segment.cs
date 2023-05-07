using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    public class Segment
    {
        /** Segment start/end */
        public Vector3f[] s = new Vector3f[2];

        /** Distance for pruning. */
        public float d;
    }
}