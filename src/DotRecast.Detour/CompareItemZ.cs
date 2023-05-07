using System.Collections.Generic;

namespace DotRecast.Detour
{
    public class CompareItemZ : IComparer<BVItem>
    {
        public int Compare(BVItem a, BVItem b)
        {
            return a.bmin[2].CompareTo(b.bmin[2]);
        }
    }
}