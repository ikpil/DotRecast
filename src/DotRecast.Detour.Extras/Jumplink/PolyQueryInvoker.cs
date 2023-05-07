using System;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class PolyQueryInvoker : IPolyQuery
    {
        public readonly Action<MeshTile, Poly, long> _callback;

        public PolyQueryInvoker(Action<MeshTile, Poly, long> callback)
        {
            _callback = callback;
        }

        public void Process(MeshTile tile, Poly poly, long refs)
        {
            _callback?.Invoke(tile, poly, refs);
        }
    }
}