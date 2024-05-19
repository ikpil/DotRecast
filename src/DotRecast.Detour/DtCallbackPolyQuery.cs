using System;

namespace DotRecast.Detour
{
    public class DtCallbackPolyQuery : IDtPolyQuery
    {
        private readonly Action<DtMeshTile, DtPoly[], long[], int> _callback;

        public DtCallbackPolyQuery(Action<DtMeshTile, DtPoly[], long[], int> callback)
        {
            _callback = callback;
        }

        public void Process(DtMeshTile tile, DtPoly[] poly, long[] refs, int count)
        {
            _callback?.Invoke(tile, poly, refs, count);
        }
    }
}