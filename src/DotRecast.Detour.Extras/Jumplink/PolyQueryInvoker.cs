using System;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class PolyQueryInvoker : IDtPolyQuery
    {
        private readonly Action<DtMeshTile, DtPoly[], long[], int> _callback;

        public PolyQueryInvoker(Action<DtMeshTile, DtPoly[], long[], int> callback)
        {
            _callback = callback;
        }

        public void Process(DtMeshTile tile, DtPoly[] poly, long[] refs, int count)
        {
            _callback?.Invoke(tile, poly, refs, count);
        }
    }
}