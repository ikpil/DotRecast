namespace DotRecast.Detour.Extras.Jumplink
{
    public class GroundSegment
    {
        public readonly Vector3f p = new Vector3f();
        public readonly Vector3f q = new Vector3f();
        public GroundSample[] gsamples;
        public float height;
    }
}