using System;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour
{
    public class DtNoOpDtPolygonByCircleConstraint : IDtPolygonByCircleConstraint
    {
        public static readonly DtNoOpDtPolygonByCircleConstraint Shared = new DtNoOpDtPolygonByCircleConstraint();

        private DtNoOpDtPolygonByCircleConstraint()
        {
        }

        public int Apply(Span<float> polyVerts, RcVec3f circleCenter, float radius, out Span<float> constrainedVerts)
        {
            constrainedVerts = polyVerts;
            return polyVerts.Length;
        }
    }
}