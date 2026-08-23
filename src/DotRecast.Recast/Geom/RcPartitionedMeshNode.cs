using DotRecast.Core.Numerics;

namespace DotRecast.Recast.Geom
{
    public class RcPartitionedMeshNode
    {
        public RcVec2f bmin;
        public RcVec2f bmax;
        public int i;
        public int[] tris;
    }
}