using DotRecast.Core;

namespace DotRecast.Detour
{
    public class DtNoOpDtPolygonByCircleConstraint : IDtPolygonByCircleConstraint
    {
        public static readonly DtNoOpDtPolygonByCircleConstraint Noop = new DtNoOpDtPolygonByCircleConstraint();

        private DtNoOpDtPolygonByCircleConstraint()
        {
        }

        public float[] Apply(float[] polyVerts, RcVec3f circleCenter, float radius)
        {
            return polyVerts;
        }
    }
}