using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Recast.Geom
{
    public class RcChunkyTriMeshNode
    {
        public Vector2 bmin;
        public Vector2 bmax;
        public int i;
        public int[] tris;
    }
}