using System;
using DotRecast.Core;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class ClimbTrajectory : Trajectory
    {
        public override Vector3f apply(Vector3f start, Vector3f end, float u)
        {
            return new Vector3f()
            {
                x = lerp(start[0], end[0], Math.Min(2f * u, 1f)),
                y = lerp(start[1], end[1], Math.Max(0f, 2f * u - 1f)),
                z = lerp(start[2], end[2], Math.Min(2f * u, 1f))
            };
        }
    }
}