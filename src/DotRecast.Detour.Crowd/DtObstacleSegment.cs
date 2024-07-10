using System.Numerics;

namespace DotRecast.Detour.Crowd
{
    public struct DtObstacleSegment // TODO struct
    {
        /** End points of the obstacle segment */
        public Vector3 p;// = new RcVec3f();

        /** End points of the obstacle segment */
        public Vector3 q;// = new RcVec3f();

        public bool touch;
    }
}