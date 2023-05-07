using System.Collections.Generic;

namespace DotRecast.Detour.TileCache
{
    public class TempContour
    {
        public List<int> verts;
        public int nverts;
        public List<int> poly;

        public TempContour()
        {
            verts = new List<int>();
            nverts = 0;
            poly = new List<int>();
        }

        public int Npoly()
        {
            return poly.Count;
        }

        public void Clear()
        {
            nverts = 0;
            verts.Clear();
        }
    };
}