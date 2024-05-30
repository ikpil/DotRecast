using System;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class Trajectory
    {
        public virtual RcVec3f Apply(RcVec3f start, RcVec3f end, float u)
        {
            throw new NotImplementedException();
        }
    }
}