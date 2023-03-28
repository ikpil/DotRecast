using System;
using DotRecast.Core;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class Trajectory
    {
        public float lerp(float f, float g, float u)
        {
            return u * g + (1f - u) * f;
        }

        public virtual Vector3f apply(Vector3f start, Vector3f end, float u)
        {
            throw new NotImplementedException();
        }
    }
}