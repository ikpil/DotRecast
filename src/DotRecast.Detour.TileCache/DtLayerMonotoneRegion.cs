using System.Collections.Generic;

namespace DotRecast.Detour.TileCache
{
    public class DtLayerMonotoneRegion
    {
        public const int DT_LAYER_MAX_NEIS = 16;

        public int area;
        public List<byte> neis = new List<byte>(DT_LAYER_MAX_NEIS);
        public byte regId;
        public byte areaId;
    };
}