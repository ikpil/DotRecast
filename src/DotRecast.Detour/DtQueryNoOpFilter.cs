using System.Numerics;

namespace DotRecast.Detour
{
    public class DtQueryNoOpFilter : IDtQueryFilter
    {
        public static readonly DtQueryNoOpFilter Shared = new DtQueryNoOpFilter();

        private DtQueryNoOpFilter()
        {
        }

        public bool PassFilter(long refs, DtMeshTile tile, DtPoly poly)
        {
            return true;
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