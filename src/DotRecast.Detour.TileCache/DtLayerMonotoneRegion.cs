using System.Collections.Generic;

namespace DotRecast.Detour.TileCache
{
    public unsafe struct DtLayerMonotoneRegion
    {
        public const int DT_LAYER_MAX_NEIS = 16;

        public int area;
        public fixed byte neis[DT_LAYER_MAX_NEIS];
        public byte nneis;

        public byte regId;
        public byte areaId;
    }
}