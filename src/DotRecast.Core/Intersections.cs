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
using static DotRecast.Core.RcMath;

namespace DotRecast.Core
{
    public static class Intersections
    {
        public static float? IntersectSegmentTriangle(Vector3f sp, Vector3f sq, Vector3f a, Vector3f b, Vector3f c)
        {
            float v, w;
            Vector3f ab = b.Subtract(a);
            Vector3f ac = c.Subtract(a);
            Vector3f qp = sp.Subtract(sq);

            // Compute triangle normal. Can be precalculated or cached if
            // intersecting multiple segments against the same triangle
            Vector3f norm = VCross(ab, ac);

            // Compute denominator d. If d <= 0, segment is parallel to or points
            // away from triangle, so exit early
            float d = VDot(qp, norm);
            if (d <= 0.0f)
            {
                return null;
            }

            // Compute intersection t value of pq with plane of triangle. A ray
            // intersects iff 0 <= t. Segment intersects iff 0 <= t <= 1. Delay
            // dividing by d until intersection has been found to pierce triangle
            Vector3f ap = sp.Subtract(a);
            float t = VDot(ap, norm);
            if (t < 0.0f)
            {
                return null;
            }

            if (t > d)
            {
                return null; // For segment; exclude this code line for a ray test
            }

            // Compute barycentric coordinate components and test if within bounds
            Vector3f e = VCross(qp, ap);
            v = VDot(ac, e);
            if (v < 0.0f || v > d)
            {
                return null;
            }

            w = -VDot(ab, e);
            if (w < 0.0f || v + w > d)
            {
                return null;
            }

            // Segment/ray intersects triangle. Perform delayed division
            t /= d;

            return t;
        }

        public static float[] IntersectSegmentAABB(Vector3f sp, Vector3f sq, Vector3f amin, Vector3f amax)
        {
            float EPS = 1e-6f;

            Vector3f d = new Vector3f();
            d.x = sq.x - sp.x;
            d.y = sq.y - sp.y;
            d.z = sq.z - sp.z;
            float tmin = 0.0f;
            float tmax = 1.0f;

            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(d[i]) < EPS)
                {
                    if (sp[i] < amin[i] || sp[i] > amax[i])
                    {
                        return null;
                    }
                }
                else
                {
                    float ood = 1.0f / d[i];
                    float t1 = (amin[i] - sp[i]) * ood;
                    float t2 = (amax[i] - sp[i]) * ood;
                    if (t1 > t2)
                    {
                        float tmp = t1;
                        t1 = t2;
                        t2 = tmp;
                    }

                    if (t1 > tmin)
                    {
                        tmin = t1;
                    }

                    if (t2 < tmax)
                    {
                        tmax = t2;
                    }

                    if (tmin > tmax)
                    {
                        return null;
                    }
                }
            }

            return new float[] { tmin, tmax };
        }
    }
}