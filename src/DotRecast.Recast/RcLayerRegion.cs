using System.Collections.Generic;

namespace DotRecast.Recast
{
    public class RcLayerRegion
    {
        public int id;
        public int layerId; // Layer ID
        public bool @base; // Flag indicating if the region is the base of merged regions.
        public int ymin, ymax;
        public List<int> layers; // Layer count
        public List<int> neis; // Neighbour count

        public RcLayerRegion(int i)
        {
            id = i;
            ymin = 0xFFFF;
            layerId = 0xff;
            layers = new List<int>();
            neis = new List<int>();
        }
    };
}