using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class GroundSegment
    {
        public Vector3 p;
        public Vector3 q;
        public GroundSample[] gsamples;
        public float height;
    }
}