namespace DotRecast.Core
{
    public class IntersectResult
    {
        public bool intersects;
        public float tmin;
        public float tmax = 1f;
        public int segMin = -1;
        public int segMax = -1;
    }
}