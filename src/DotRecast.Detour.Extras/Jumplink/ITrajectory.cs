using System;
using System.Numerics;

namespace DotRecast.Detour.Extras.Jumplink
{
    public interface ITrajectory
    {
        Vector3 Apply(Vector3 start, Vector3 end, float u);
    }
}