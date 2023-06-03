using DotRecast.Core;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class NoOpFilter : IQueryFilter
    {
        public bool PassFilter(long refs, MeshTile tile, Poly poly)
        {
            return true;
        }

        public float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, MeshTile prevTile, Poly prevPoly, long curRef,
            MeshTile curTile, Poly curPoly, long nextRef, MeshTile nextTile, Poly nextPoly)
        {
            return 0;
        }
    }
}