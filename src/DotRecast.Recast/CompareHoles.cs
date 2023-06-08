using System.Collections.Generic;

namespace DotRecast.Recast
{
    public class CompareHoles : IComparer<RcContourHole>
    {
        public int Compare(RcContourHole a, RcContourHole b)
        {
            if (a.minx == b.minx)
            {
                return a.minz.CompareTo(b.minz);
            }
            else
            {
                return a.minx.CompareTo(b.minx);
            }
        }
    }
}