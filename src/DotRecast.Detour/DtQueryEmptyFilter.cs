using System.Numerics;

namespace DotRecast.Detour
{
    public class DtQueryEmptyFilter : IDtQueryFilter
    {
        public static readonly DtQueryEmptyFilter Shared = new DtQueryEmptyFilter();

        private DtQueryEmptyFilter()
        {
        }

        public bool PassFilter(long refs, DtMeshTile tile, DtPoly poly)
        {
            return false;
        }

        public float GetCost(Vector3 pa, Vector3 pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef,
            DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            return 0;
        }

        public float GetAreaCost(int i)
        {
            return 0;
        }

        public void SetAreaCost(int i, float cost)
        {
            
        }
    }
}