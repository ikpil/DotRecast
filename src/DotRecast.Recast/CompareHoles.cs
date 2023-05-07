using System.Collections.Generic;

namespace DotRecast.Recast
{
    public class CompareHoles : IComparer<ContourHole>
    {
        public int Compare(ContourHole a, ContourHole b)
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