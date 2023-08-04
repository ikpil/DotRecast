using System.Collections.Generic;

namespace DotRecast.Recast
{
    public class RcPotentialDiagonalComparer : IComparer<RcPotentialDiagonal>
    {
        public int Compare(RcPotentialDiagonal va, RcPotentialDiagonal vb)
        {
            RcPotentialDiagonal a = va;
            RcPotentialDiagonal b = vb;
            return a.dist.CompareTo(b.dist);
        }
    }
}