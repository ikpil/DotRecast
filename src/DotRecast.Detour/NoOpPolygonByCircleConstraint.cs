using DotRecast.Core;

namespace DotRecast.Detour
{
    public class NoOpPolygonByCircleConstraint : IPolygonByCircleConstraint
    {
        public static readonly NoOpPolygonByCircleConstraint Noop = new NoOpPolygonByCircleConstraint();

        private NoOpPolygonByCircleConstraint()
        {
        }

        public float[] Apply(float[] polyVerts, RcVec3f circleCenter, float radius)
        {
            return polyVerts;
        }
    }
}