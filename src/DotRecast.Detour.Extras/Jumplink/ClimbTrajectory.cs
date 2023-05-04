using System;
using DotRecast.Core;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class ClimbTrajectory : Trajectory
    {
        public override Vector3f Apply(Vector3f start, Vector3f end, float u)
        {
            return new Vector3f()
            {
                x = Lerp(start.x, end.x, Math.Min(2f * u, 1f)),
                y = Lerp(start.y, end.y, Math.Max(0f, 2f * u - 1f)),
                z = Lerp(start.z, end.z, Math.Min(2f * u, 1f))
            };
        }
    }
}