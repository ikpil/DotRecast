namespace DotRecast.Detour
{
    public unsafe struct BVItem
    {
        public fixed int bmin[3];
        public fixed int bmax[3];
        public int i;
    };
}