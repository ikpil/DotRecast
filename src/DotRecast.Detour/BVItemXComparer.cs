using System.Collections.Generic;

namespace DotRecast.Detour
{
    public class BVItemXComparer : IComparer<BVItem>
    {
        public int Compare(BVItem a, BVItem b)
        {
            return a.bmin[0].CompareTo(b.bmin[0]);
        }
    }
}