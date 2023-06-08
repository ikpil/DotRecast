using DotRecast.Core;

namespace DotRecast.Detour
{
    public class DtQueryNoOpFilter : IDtQueryFilter
    {
        public bool PassFilter(long refs, DtMeshTile tile, DtPoly poly)
        {
            return true;
        }

        public float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef,
            DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            return 0;
        }
    }
}