using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Crowd
{
    /// < Max number of adaptive rings.
    public class DtObstacleCircle
    {
        /** Position of the obstacle */
        public Vector3 p = new Vector3();

        /** Velocity of the obstacle */
        public Vector3 vel = new Vector3();

        /** Velocity of the obstacle */
        public Vector3 dvel = new Vector3();

        /** Radius of the obstacle */
        public float rad;

        /** Use for side selection during sampling. */
        public Vector3 dp = new Vector3();

        /** Use for side selection during sampling. */
        public Vector3 np = new Vector3();
    }
}