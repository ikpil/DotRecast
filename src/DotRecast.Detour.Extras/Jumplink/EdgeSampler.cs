using System.Collections.Generic;
using System.Numerics;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class EdgeSampler
    {
        public readonly GroundSegment start = new GroundSegment();
        public readonly List<GroundSegment> end = new List<GroundSegment>();
        public readonly ITrajectory trajectory;

        public readonly Vector3 ax = new Vector3();
        public readonly Vector3 ay = new Vector3();
        public readonly Vector3 az = new Vector3();

        public EdgeSampler(JumpEdge edge, ITrajectory trajectory)
        {
            this.trajectory = trajectory;
            ax = Vector3.Subtract(edge.sq, edge.sp);
            ax = Vector3.Normalize(ax);
            
            az = new Vector3(ax.Z, 0, -ax.X);
            az = Vector3.Normalize(az);
            
            ay = new Vector3(0, 1, 0);
        }
    }
}