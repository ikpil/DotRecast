using System.Numerics;

namespace DotRecast.Detour.Crowd
{
    public struct DtObstacleSegment
    {
        /** End points of the obstacle segment */
        public Vector3 p;

        /** End points of the obstacle segment */
        public Vector3 q;

        public bool touch;
    }
}