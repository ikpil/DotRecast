using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    public class ObstacleSegment
    {
        /** End points of the obstacle segment */
        public Vector3f p = new Vector3f();

        /** End points of the obstacle segment */
        public Vector3f q = new Vector3f();

        public bool touch;
    }
}