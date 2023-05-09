using System.Collections.Generic;

namespace DotRecast.Recast.Geom
{
    public class CompareItemX : IComparer<BoundsItem>
    {
        public int Compare(BoundsItem a, BoundsItem b)
        {
            return a.bmin.x.CompareTo(b.bmin.x);
        }
    }
}