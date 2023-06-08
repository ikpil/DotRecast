namespace DotRecast.Detour
{
    public class DtSegInterval
    {
        public long refs;
        public int tmin;
        public int tmax;

        public DtSegInterval(long refs, int tmin, int tmax)
        {
            this.refs = refs;
            this.tmin = tmin;
            this.tmax = tmax;
        }
    }
}