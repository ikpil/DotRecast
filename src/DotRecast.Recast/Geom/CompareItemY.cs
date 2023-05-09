using System.Collections.Generic;

namespace DotRecast.Recast.Geom
{
    public class CompareItemY : IComparer<BoundsItem>
    {
        public int Compare(BoundsItem a, BoundsItem b)
        {
            return a.bmin.y.CompareTo(b.bmin.y);
        }
    }
}