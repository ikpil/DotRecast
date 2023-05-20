using System.Collections.Generic;
using DotRecast.Core;
using static DotRecast.Core.RcMath;

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

        public EdgeSampler(JumpEdge edge, Trajectory trajectory)
        {
            this.trajectory = trajectory;
            ax = edge.sq.Subtract(edge.sp);
            ax.Normalize();
            az.Set(ax.z, 0, -ax.x);
            az.Normalize();
            ay.Set(0, 1, 0);
        }
    }
}