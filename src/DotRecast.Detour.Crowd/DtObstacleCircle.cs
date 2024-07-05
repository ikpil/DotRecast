using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Crowd
{
    /// < Max number of adaptive rings.
    public struct DtObstacleCircle // TODO struct
    {
        /** Position of the obstacle */
        public RcVec3f p;

        /** Velocity of the obstacle */
        public RcVec3f vel;

        /** Velocity of the obstacle */
        public RcVec3f dvel;

        /** Radius of the obstacle */
        public float rad;

        /** Use for side selection during sampling. */
        public RcVec3f dp;

        /** Use for side selection during sampling. */
        public RcVec3f np;
    }
}