using System.Numerics;

namespace DotRecast.Detour.Crowd
{
    /// < Max number of adaptive rings.
    public struct DtObstacleCircle
    {
        /** Position of the obstacle */
        public Vector3 p;

        /** Velocity of the obstacle */
        public Vector3 vel;

        /** Velocity of the obstacle */
        public Vector3 dvel;

        /** Radius of the obstacle */
        public float rad;

        /** Use for side selection during sampling. */
        public Vector3 dp;

        /** Use for side selection during sampling. */
        public Vector3 np;
    }
}