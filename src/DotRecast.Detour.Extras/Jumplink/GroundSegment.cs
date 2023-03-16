namespace DotRecast.Detour.Extras.Jumplink
{
    public class GroundSegment
    {
        public readonly float[] p = new float[3];
        public readonly float[] q = new float[3];
        public GroundSample[] gsamples;
        public float height;
    }
}