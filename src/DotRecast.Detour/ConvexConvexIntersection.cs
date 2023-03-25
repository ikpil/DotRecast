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

namespace DotRecast.Detour
{
    using static DotRecast.Core.RecastMath;

    /**
 * Convex-convex intersection based on "Computational Geometry in C" by Joseph O'Rourke
 */
    public static class ConvexConvexIntersection
    {
        private static readonly float EPSILON = 0.0001f;

        private enum InFlag
        {
            Pin,
            Qin,
            Unknown,
        }

        private enum Intersection
        {
            None,
            Single,
            Overlap,
        }

        public static float[] intersect(float[] p, float[] q)
        {
            int n = p.Length / 3;
            int m = q.Length / 3;
            float[] inters = new float[Math.Max(m, n) * 3 * 3];
            int ii = 0;
            /* Initialize variables. */
            float[] a = new float[3];
            float[] b = new float[3];
            float[] a1 = new float[3];
            float[] b1 = new float[3];

            int aa = 0;
            int ba = 0;
            int ai = 0;
            int bi = 0;

            InFlag f = InFlag.Unknown;
            bool FirstPoint = true;
            float[] ip = new float[3];
            float[] iq = new float[3];

            do
            {
                vCopy(a, p, 3 * (ai % n));
                vCopy(b, q, 3 * (bi % m));
                vCopy(a1, p, 3 * ((ai + n - 1) % n)); // prev a
                vCopy(b1, q, 3 * ((bi + m - 1) % m)); // prev b

                float[] A = vSub(a, a1);
                float[] B = vSub(b, b1);

                float cross = B[0] * A[2] - A[0] * B[2]; // triArea2D({0, 0}, A, B);
                float aHB = triArea2D(b1, b, a);
                float bHA = triArea2D(a1, a, b);
                if (Math.Abs(cross) < EPSILON)
                {
                    cross = 0f;
                }

                bool parallel = cross == 0f;
                Intersection code = parallel ? parallelInt(a1, a, b1, b, ip, iq) : segSegInt(a1, a, b1, b, ip, iq);

                if (code == Intersection.Single)
                {
                    if (FirstPoint)
                    {
                        FirstPoint = false;
                        aa = ba = 0;
                    }

                    ii = addVertex(inters, ii, ip);
                    f = inOut(f, aHB, bHA);
                }

                /*-----Advance rules-----*/

                /* Special case: A & B overlap and oppositely oriented. */
                if (code == Intersection.Overlap && vDot2D(A, B) < 0)
                {
                    ii = addVertex(inters, ii, ip);
                    ii = addVertex(inters, ii, iq);
                    break;
                }

                /* Special case: A & B parallel and separated. */
                if (parallel && aHB < 0f && bHA < 0f)
                {
                    return null;
                }
                /* Special case: A & B collinear. */
                else if (parallel && Math.Abs(aHB) < EPSILON && Math.Abs(bHA) < EPSILON)
                {
                    /* Advance but do not output point. */
                    if (f == InFlag.Pin)
                    {
                        ba++;
                        bi++;
                    }
                    else
                    {
                        aa++;
                        ai++;
                    }
                }
                /* Generic cases. */
                else if (cross >= 0)
                {
                    if (bHA > 0)
                    {
                        if (f == InFlag.Pin)
                        {
                            ii = addVertex(inters, ii, a);
                        }

                        aa++;
                        ai++;
                    }
                    else
                    {
                        if (f == InFlag.Qin)
                        {
                            ii = addVertex(inters, ii, b);
                        }

                        ba++;
                        bi++;
                    }
                }
                else
                {
                    if (aHB > 0)
                    {
                        if (f == InFlag.Qin)
                        {
                            ii = addVertex(inters, ii, b);
                        }

                        ba++;
                        bi++;
                    }
                    else
                    {
                        if (f == InFlag.Pin)
                        {
                            ii = addVertex(inters, ii, a);
                        }

                        aa++;
                        ai++;
                    }
                }
                /* Quit when both adv. indices have cycled, or one has cycled twice. */
            } while ((aa < n || ba < m) && aa < 2 * n && ba < 2 * m);

            /* Deal with special cases: not implemented. */
            if (f == InFlag.Unknown)
            {
                return null;
            }

            float[] copied = new float[ii];
            Array.Copy(inters, copied, ii);
            return copied;
        }

        private static int addVertex(float[] inters, int ii, float[] p)
        {
            if (ii > 0)
            {
                if (inters[ii - 3] == p[0] && inters[ii - 2] == p[1] && inters[ii - 1] == p[2])
                {
                    return ii;
                }

                if (inters[0] == p[0] && inters[1] == p[1] && inters[2] == p[2])
                {
                    return ii;
                }
            }

            inters[ii] = p[0];
            inters[ii + 1] = p[1];
            inters[ii + 2] = p[2];
            return ii + 3;
        }

        private static InFlag inOut(InFlag inflag, float aHB, float bHA)
        {
            if (aHB > 0)
            {
                return InFlag.Pin;
            }
            else if (bHA > 0)
            {
                return InFlag.Qin;
            }

            return inflag;
        }

        private static Intersection segSegInt(float[] a, float[] b, float[] c, float[] d, float[] p, float[] q)
        {
            var isec = intersectSegSeg2D(a, b, c, d);
            if (null != isec)
            {
                float s = isec.Item1;
                float t = isec.Item2;
                if (s >= 0.0f && s <= 1.0f && t >= 0.0f && t <= 1.0f)
                {
                    p[0] = a[0] + (b[0] - a[0]) * s;
                    p[1] = a[1] + (b[1] - a[1]) * s;
                    p[2] = a[2] + (b[2] - a[2]) * s;
                    return Intersection.Single;
                }
            }

            return Intersection.None;
        }

        private static Intersection parallelInt(float[] a, float[] b, float[] c, float[] d, float[] p, float[] q)
        {
            if (between(a, b, c) && between(a, b, d))
            {
                vCopy(p, c);
                vCopy(q, d);
                return Intersection.Overlap;
            }

            if (between(c, d, a) && between(c, d, b))
            {
                vCopy(p, a);
                vCopy(q, b);
                return Intersection.Overlap;
            }

            if (between(a, b, c) && between(c, d, b))
            {
                vCopy(p, c);
                vCopy(q, b);
                return Intersection.Overlap;
            }

            if (between(a, b, c) && between(c, d, a))
            {
                vCopy(p, c);
                vCopy(q, a);
                return Intersection.Overlap;
            }

            if (between(a, b, d) && between(c, d, b))
            {
                vCopy(p, d);
                vCopy(q, b);
                return Intersection.Overlap;
            }

            if (between(a, b, d) && between(c, d, a))
            {
                vCopy(p, d);
                vCopy(q, a);
                return Intersection.Overlap;
            }

            return Intersection.None;
        }

        private static bool between(float[] a, float[] b, float[] c)
        {
            if (Math.Abs(a[0] - b[0]) > Math.Abs(a[2] - b[2]))
            {
                return ((a[0] <= c[0]) && (c[0] <= b[0])) || ((a[0] >= c[0]) && (c[0] >= b[0]));
            }
            else
            {
                return ((a[2] <= c[2]) && (c[2] <= b[2])) || ((a[2] >= c[2]) && (c[2] >= b[2]));
            }
        }
    }
}