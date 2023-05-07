using System.Collections.Generic;

namespace DotRecast.Detour.TileCache
{
    public class LayerMonotoneRegion
    {
        public int area;
        public List<int> neis = new List<int>(16);
        public int regId;
        public int areaId;
    };
}