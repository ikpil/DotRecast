using System;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour
{
    // Calculate the intersection between a polygon and a circle. A dodecagon is used as an approximation of the circle.
    public class DtStrictDtPolygonByCircleConstraint : IDtPolygonByCircleConstraint
    {
        private const int CIRCLE_SEGMENTS = 12;
        private static readonly float[] UnitCircle = CreateCircle();

        public static readonly IDtPolygonByCircleConstraint Shared = new DtStrictDtPolygonByCircleConstraint();

        private DtStrictDtPolygonByCircleConstraint()
        {
        }

        public static float[] CreateCircle()
        {
            var temp = new float[CIRCLE_SEGMENTS * 3];
            for (int i = 0; i < CIRCLE_SEGMENTS; i++)
            {
                float a = i * MathF.PI * 2 / CIRCLE_SEGMENTS;
                temp[3 * i] = MathF.Cos(a);
                temp[3 * i + 1] = 0;
                temp[3 * i + 2] = -MathF.Sin(a);
            }

            return temp;
        }

        public static void ScaleCircle(Span<float> src, RcVec3f center, float radius, Span<float> dst)
        {
            for (int i = 0; i < CIRCLE_SEGMENTS; i++)
            {
                dst[3 * i] = src[3 * i] * radius + center.X;
                dst[3 * i + 1] = center.Y;
                dst[3 * i + 2] = src[3 * i + 2] * radius + center.Z;
            }
        }


        public float[] Apply(float[] verts, RcVec3f center, float radius)
        {
            float radiusSqr = radius * radius;
            int outsideVertex = -1;
            for (int pv = 0; pv < verts.Length; pv += 3)
            {
                if (RcVec.Dist2DSqr(center, verts, pv) > radiusSqr)
                {
                    outsideVertex = pv;
                    break;
                }
            }

            if (outsideVertex == -1)
            {
                // polygon inside circle
                return verts;
            }

            Span<float> qCircle = stackalloc float[UnitCircle.Length];
            ScaleCircle(UnitCircle, center, radius, qCircle);
            float[] intersection = DtConvexConvexIntersections.Intersect(verts, qCircle);
            if (intersection == null && DtUtils.PointInPolygon(center, verts, verts.Length / 3))
            {
                // circle inside polygon
                return qCircle.ToArray();
            }

            return intersection;
        }
    }
}