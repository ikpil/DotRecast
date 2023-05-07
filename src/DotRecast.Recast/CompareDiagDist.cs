using System.Collections.Generic;

namespace DotRecast.Recast
{
    public class CompareDiagDist : IComparer<PotentialDiagonal>
    {
        public int Compare(PotentialDiagonal va, PotentialDiagonal vb)
        {
            PotentialDiagonal a = va;
            PotentialDiagonal b = vb;
            return a.dist.CompareTo(b.dist);
        }
    }
}