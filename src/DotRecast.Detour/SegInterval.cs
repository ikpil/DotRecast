namespace DotRecast.Detour
{
    public class SegInterval
    {
        public long refs;
        public int tmin;
        public int tmax;

        public SegInterval(long refs, int tmin, int tmax)
        {
            this.refs = refs;
            this.tmin = tmin;
            this.tmax = tmax;
        }
    }
}