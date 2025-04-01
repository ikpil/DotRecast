using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Crowd
{
    public class DtObstacleSegment
    {
        /** End points of the obstacle segment */
        public Vector3 p = new Vector3();

        /** End points of the obstacle segment */
        public Vector3 q = new Vector3();

        public bool touch;
    }
}