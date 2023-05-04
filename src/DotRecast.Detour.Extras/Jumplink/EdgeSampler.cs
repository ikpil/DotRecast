using System.Collections.Generic;
using DotRecast.Core;
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
            ax = VSub(edge.sq, edge.sp);
            VNormalize(ref ax);
            VSet(ref az, ax.z, 0, -ax.x);
            VNormalize(ref az);
            VSet(ref ay, 0, 1, 0);
        }
    }
}