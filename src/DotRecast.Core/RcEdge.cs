namespace DotRecast.Core
{
    public unsafe struct RcEdge
    {
        public fixed int vert[2];
        public fixed int polyEdge[2];
        public fixed int poly[2];
    }
}