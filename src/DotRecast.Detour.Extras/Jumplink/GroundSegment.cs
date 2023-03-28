using DotRecast.Core;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class GroundSegment
    {
        public Vector3f p = new Vector3f();
        public Vector3f q = new Vector3f();
        public GroundSample[] gsamples;
        public float height;
    }
}