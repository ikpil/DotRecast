using System;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class ClimbTrajectory : Trajectory
    {
        public override float[] apply(float[] start, float[] end, float u)
        {
            return new float[]
            {
                lerp(start[0], end[0], Math.Min(2f * u, 1f)),
                lerp(start[1], end[1], Math.Max(0f, 2f * u - 1f)),
                lerp(start[2], end[2], Math.Min(2f * u, 1f))
            };
        }
    }
}