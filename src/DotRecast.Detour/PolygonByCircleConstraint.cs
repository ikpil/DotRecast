/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using DotRecast.Core;

namespace DotRecast.Detour
{
    using static DotRecast.Core.RecastMath;

    public interface PolygonByCircleConstraint
    {
        float[] aply(float[] polyVerts, Vector3f circleCenter, float radius);

        public static PolygonByCircleConstraint noop()
        {
            return new NoOpPolygonByCircleConstraint();
        }

        public static PolygonByCircleConstraint strict()
        {
            return new StrictPolygonByCircleConstraint();
        }

        public class NoOpPolygonByCircleConstraint : PolygonByCircleConstraint
        {
            public float[] aply(float[] polyVerts, Vector3f circleCenter, float radius)
            {
                return polyVerts;
            }
        }

        /**
     * Calculate the intersection between a polygon and a circle. A dodecagon is used as an approximation of the circle.
     */
        public class StrictPolygonByCircleConstraint : PolygonByCircleConstraint
        {
            private const int CIRCLE_SEGMENTS = 12;
            private static float[] unitCircle;

            public float[] aply(float[] verts, Vector3f center, float radius)
            {
                float radiusSqr = radius * radius;
                int outsideVertex = -1;
                for (int pv = 0; pv < verts.Length; pv += 3)
                {
                    if (vDist2DSqr(center, verts, pv) > radiusSqr)
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

                float[] qCircle = circle(center, radius);
                float[] intersection = ConvexConvexIntersection.intersect(verts, qCircle);
                if (intersection == null && pointInPolygon(center, verts, verts.Length / 3))
                {
                    // circle inside polygon
                    return qCircle;
                }

                return intersection;
            }

            private float[] circle(Vector3f center, float radius)
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
                    circle[i] = unitCircle[i] * radius + center[0];
                    circle[i + 1] = center[1];
                    circle[i + 2] = unitCircle[i + 2] * radius + center[2];
                }

                return circle;
            }
        }
    }
}