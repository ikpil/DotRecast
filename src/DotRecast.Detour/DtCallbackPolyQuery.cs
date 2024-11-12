using System;

namespace DotRecast.Detour
{
    public struct DtCallbackPolyQuery : IDtPolyQuery
    {
        private readonly Action<DtMeshTile, DtPoly, long> _callback;

        public DtCallbackPolyQuery(Action<DtMeshTile, DtPoly, long> callback)
        {
            _callback = callback;
        }

        public void Process(DtMeshTile tile, DtPoly[] poly, Span<long> refs, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                _callback?.Invoke(tile, poly[i], refs[i]);
            }
        }
    }
}