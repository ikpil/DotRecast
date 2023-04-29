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
                x = lerp(start.x, end.x, Math.Min(2f * u, 1f)),
                y = lerp(start.y, end.y, Math.Max(0f, 2f * u - 1f)),
                z = lerp(start.z, end.z, Math.Min(2f * u, 1f))
            };
        }
    }
}