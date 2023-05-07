using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    /// < Max number of adaptive rings.
    public class ObstacleCircle
    {
        /** Position of the obstacle */
        public Vector3f p = new Vector3f();

        /** Velocity of the obstacle */
        public Vector3f vel = new Vector3f();

        /** Velocity of the obstacle */
        public Vector3f dvel = new Vector3f();

        /** Radius of the obstacle */
        public float rad;

        /** Use for side selection during sampling. */
        public Vector3f dp = new Vector3f();

        /** Use for side selection during sampling. */
        public Vector3f np = new Vector3f();
    }
}