using System.Collections.Generic;
using static DotRecast.Core.RecastMath;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class EdgeSampler
    {
        public readonly GroundSegment start = new GroundSegment();
        public readonly List<GroundSegment> end = new List<GroundSegment>();
        public readonly Trajectory trajectory;

        public readonly Vector3f ax = new Vector3f();
        public readonly Vector3f ay = new Vector3f();
        public readonly Vector3f az = new Vector3f();

        public EdgeSampler(Edge edge, Trajectory trajectory)
        {
            this.trajectory = trajectory;
            vCopy(ax, vSub(edge.sq, edge.sp));
            vNormalize(ax);
            vSet(az, ax[2], 0, -ax[0]);
            vNormalize(az);
            vSet(ay, 0, 1, 0);
        }
    }
}