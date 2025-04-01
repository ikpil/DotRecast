using System;
using DotRecast.Core;
using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class ClimbTrajectory : ITrajectory
    {
        public Vector3 Apply(Vector3 start, Vector3 end, float u)
        {
            return new Vector3()
            {
                X = RcMath.Lerp(start.X, end.X, Math.Min(2f * u, 1f)),
                Y = RcMath.Lerp(start.Y, end.Y, Math.Max(0f, 2f * u - 1f)),
                Z = RcMath.Lerp(start.Z, end.Z, Math.Min(2f * u, 1f))
            };
        }
    }
}