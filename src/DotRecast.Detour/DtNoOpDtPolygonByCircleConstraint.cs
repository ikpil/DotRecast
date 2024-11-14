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

        public Span<float> Apply(Span<float> polyVerts, RcVec3f circleCenter, float radius, Span<float> resultBuffer)
        {
            var result = resultBuffer.Slice(0, polyVerts.Length);
            polyVerts.CopyTo(result);
            return result;
        }
    }
}