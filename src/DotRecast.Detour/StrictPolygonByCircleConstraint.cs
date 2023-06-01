using System;
using DotRecast.Core;

namespace DotRecast.Detour
{
    using static DotRecast.Core.RcMath;

    /**
     * Calculate the intersection between a polygon and a circle. A dodecagon is used as an approximation of the circle.
     */
    public class StrictPolygonByCircleConstraint : IPolygonByCircleConstraint
    {
        private const int CIRCLE_SEGMENTS = 12;
        private static float[] unitCircle;

        public float[] Aply(float[] verts, Vector3f center, float radius)
        {
            float radiusSqr = radius * radius;
            int outsideVertex = -1;
            for (int pv = 0; pv < verts.Length; pv += 3)
            {
                if (Vector3f.Dist2DSqr(center, verts, pv) > radiusSqr)
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

            float[] qCircle = Circle(center, radius);
            float[] intersection = ConvexConvexIntersection.Intersect(verts, qCircle);
            if (intersection == null && DetourCommon.PointInPolygon(center, verts, verts.Length / 3))
            {
                // circle inside polygon
                return qCircle;
            }

            return intersection;
        }

        private float[] Circle(Vector3f center, float radius)
        {
            if (unitCircle == null)
            {
                unitCircle = new float[CIRCLE_SEGMENTS * 3];
                for (int i = 0; i < CIRCLE_SEGMENTS; i++)
                {
                    double a = i * Math.PI * 2 / CIRCLE_SEGMENTS;
                    unitCircle[3 * i] = (float)Math.Cos(a);
                    unitCircle[3 * i + 1] = 0;
                    unitCircle[3 * i + 2] = (float)-Math.Sin(a);
                }
            }

            float[] circle = new float[12 * 3];
            for (int i = 0; i < CIRCLE_SEGMENTS * 3; i += 3)
            {
                circle[i] = unitCircle[i] * radius + center.x;
                circle[i + 1] = center.y;
                circle[i + 2] = unitCircle[i + 2] * radius + center.z;
            }

            return circle;
        }
    }
}