using System;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class ClimbTrajectory : Trajectory
    {
        public override RcVec3f Apply(RcVec3f start, RcVec3f end, float u)
        {
            return new RcVec3f()
            {
                X = Lerp(start.X, end.X, Math.Min(2f * u, 1f)),
                Y = Lerp(start.Y, end.Y, Math.Max(0f, 2f * u - 1f)),
                Z = Lerp(start.Z, end.Z, Math.Min(2f * u, 1f))
            };
        }
    }
}