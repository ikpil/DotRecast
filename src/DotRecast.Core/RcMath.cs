/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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

namespace DotRecast.Core
{
    public static class RcMath
    {
        public const float EPS = 1e-4f;
        private static readonly float EQUAL_THRESHOLD = Sqr(1.0f / 16384.0f);

        public static float Sqr(float f)
        {
            return f * f;
        }

        public static float GetPathLen(float[] path, int npath)
        {
            float totd = 0;
            for (int i = 0; i < npath - 1; ++i)
            {
                totd += (float)Math.Sqrt(Vector3f.DistSqr(path, i * 3, (i + 1) * 3));
            }

            return totd;
        }


        public static float Step(float threshold, float v)
        {
            return v < threshold ? 0.0f : 1.0f;
        }

        public static float Clamp(float v, float min, float max)
        {
            return Math.Max(Math.Min(v, max), min);
        }

        public static int Clamp(int v, int min, int max)
        {
            return Math.Max(Math.Min(v, max), min);
        }

        public static float Lerp(float f, float g, float u)
        {
            return u * g + (1f - u) * f;
        }

        /// Performs a 'sloppy' colocation check of the specified points.
        /// @param[in] p0 A point. [(x, y, z)]
        /// @param[in] p1 A point. [(x, y, z)]
        /// @return True if the points are considered to be at the same location.
        ///
        /// Basically, this function will return true if the specified points are
        /// close enough to eachother to be considered colocated.
        public static bool VEqual(Vector3f p0, Vector3f p1)
        {
            return VEqual(p0, p1, EQUAL_THRESHOLD);
        }

        public static bool VEqual(Vector3f p0, Vector3f p1, float thresholdSqr)
        {
            float d = Vector3f.DistSqr(p0, p1);
            return d < thresholdSqr;
        }


        /// @}
        /// @name Computational geometry helper functions.
        /// @{
        /// Derives the signed xz-plane area of the triangle ABC, or the
        /// relationship of line AB to point C.
        /// @param[in] a Vertex A. [(x, y, z)]
        /// @param[in] b Vertex B. [(x, y, z)]
        /// @param[in] c Vertex C. [(x, y, z)]
        /// @return The signed xz-plane area of the triangle.
        public static float TriArea2D(float[] verts, int a, int b, int c)
        {
            float abx = verts[b] - verts[a];
            float abz = verts[b + 2] - verts[a + 2];
            float acx = verts[c] - verts[a];
            float acz = verts[c + 2] - verts[a + 2];
            return acx * abz - abx * acz;
        }

        public static float TriArea2D(Vector3f a, Vector3f b, Vector3f c)
        {
            float abx = b.x - a.x;
            float abz = b.z - a.z;
            float acx = c.x - a.x;
            float acz = c.z - a.z;
            return acx * abz - abx * acz;
        }

        /// Determines if two axis-aligned bounding boxes overlap.
        /// @param[in] amin Minimum bounds of box A. [(x, y, z)]
        /// @param[in] amax Maximum bounds of box A. [(x, y, z)]
        /// @param[in] bmin Minimum bounds of box B. [(x, y, z)]
        /// @param[in] bmax Maximum bounds of box B. [(x, y, z)]
        /// @return True if the two AABB's overlap.
        /// @see dtOverlapBounds
        public static bool OverlapQuantBounds(int[] amin, int[] amax, int[] bmin, int[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }

        /// Determines if two axis-aligned bounding boxes overlap.
        /// @param[in] amin Minimum bounds of box A. [(x, y, z)]
        /// @param[in] amax Maximum bounds of box A. [(x, y, z)]
        /// @param[in] bmin Minimum bounds of box B. [(x, y, z)]
        /// @param[in] bmax Maximum bounds of box B. [(x, y, z)]
        /// @return True if the two AABB's overlap.
        /// @see dtOverlapQuantBounds
        public static bool OverlapBounds(Vector3f amin, Vector3f amax, Vector3f bmin, Vector3f bmax)
        {
            bool overlap = true;
            overlap = (amin.x > bmax.x || amax.x < bmin.x) ? false : overlap;
            overlap = (amin.y > bmax.y || amax.y < bmin.y) ? false : overlap;
            overlap = (amin.z > bmax.z || amax.z < bmin.z) ? false : overlap;
            return overlap;
        }

        public static float DistancePtSegSqr2D(Vector3f pt, float[] verts, int p, int q, out float t)
        {
            var vp = Vector3f.Of(verts, p);
            var vq = Vector3f.Of(verts, q);
            return DistancePtSegSqr2D(pt, vp, vq, out t);
        }

        public static float DistancePtSegSqr2D(Vector3f pt, Vector3f p, Vector3f q, out float t)
        {
            float pqx = q.x - p.x;
            float pqz = q.z - p.z;
            float dx = pt.x - p.x;
            float dz = pt.z - p.z;
            float d = pqx * pqx + pqz * pqz;
            t = pqx * dx + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }

            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = p.x + t * pqx - pt.x;
            dz = p.z + t * pqz - pt.z;
            return dx * dx + dz * dz;
        }

        public static float? ClosestHeightPointTriangle(Vector3f p, Vector3f a, Vector3f b, Vector3f c)
        {
            Vector3f v0 = c.Subtract(a);
            Vector3f v1 = b.Subtract(a);
            Vector3f v2 = p.Subtract(a);

            // Compute scaled barycentric coordinates
            float denom = v0.x * v1.z - v0.z * v1.x;
            if (Math.Abs(denom) < EPS)
            {
                return null;
            }

            float u = v1.z * v2.x - v1.x * v2.z;
            float v = v0.x * v2.z - v0.z * v2.x;

            if (denom < 0)
            {
                denom = -denom;
                u = -u;
                v = -v;
            }

            // If point lies inside the triangle, return interpolated ycoord.
            if (u >= 0.0f && v >= 0.0f && (u + v) <= denom)
            {
                float h = a.y + (v0.y * u + v1.y * v) / denom;
                return h;
            }

            return null;
        }

        /// @par
        ///
        /// All points are projected onto the xz-plane, so the y-values are ignored.
        public static bool PointInPolygon(Vector3f pt, float[] verts, int nverts)
        {
            // TODO: Replace pnpoly with triArea2D tests?
            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                int vi = i * 3;
                int vj = j * 3;
                if (((verts[vi + 2] > pt.z) != (verts[vj + 2] > pt.z)) && (pt.x < (verts[vj + 0] - verts[vi + 0])
                        * (pt.z - verts[vi + 2]) / (verts[vj + 2] - verts[vi + 2]) + verts[vi + 0]))
                {
                    c = !c;
                }
            }

            return c;
        }

        public static bool DistancePtPolyEdgesSqr(Vector3f pt, float[] verts, int nverts, float[] ed, float[] et)
        {
            // TODO: Replace pnpoly with triArea2D tests?
            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                int vi = i * 3;
                int vj = j * 3;
                if (((verts[vi + 2] > pt.z) != (verts[vj + 2] > pt.z)) &&
                    (pt.x < (verts[vj + 0] - verts[vi + 0]) * (pt.z - verts[vi + 2]) / (verts[vj + 2] - verts[vi + 2]) + verts[vi + 0]))
                {
                    c = !c;
                }

                ed[j] = DistancePtSegSqr2D(pt, verts, vj, vi, out et[j]);
            }

            return c;
        }

        public static Vector2f ProjectPoly(Vector3f axis, float[] poly, int npoly)
        {
            float rmin, rmax;
            rmin = rmax = axis.Dot2D(poly, 0);
            for (int i = 1; i < npoly; ++i)
            {
                float d = axis.Dot2D(poly, i * 3);
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }

            return new Vector2f
            {
                x = rmin,
                y = rmax,
            };
        }

        public static bool OverlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return ((amin + eps) > bmax || (amax - eps) < bmin) ? false : true;
        }

        const float eps = 1e-4f;

        /// @par
        ///
        /// All vertices are projected onto the xz-plane, so the y-values are ignored.
        public static bool OverlapPolyPoly2D(float[] polya, int npolya, float[] polyb, int npolyb)
        {
            for (int i = 0, j = npolya - 1; i < npolya; j = i++)
            {
                int va = j * 3;
                int vb = i * 3;

                Vector3f n = Vector3f.Of(polya[vb + 2] - polya[va + 2], 0, -(polya[vb + 0] - polya[va + 0]));

                Vector2f aminmax = ProjectPoly(n, polya, npolya);
                Vector2f bminmax = ProjectPoly(n, polyb, npolyb);
                if (!OverlapRange(aminmax.x, aminmax.y, bminmax.x, bminmax.y, eps))
                {
                    // Found separating axis
                    return false;
                }
            }

            for (int i = 0, j = npolyb - 1; i < npolyb; j = i++)
            {
                int va = j * 3;
                int vb = i * 3;

                Vector3f n = Vector3f.Of(polyb[vb + 2] - polyb[va + 2], 0, -(polyb[vb + 0] - polyb[va + 0]));

                Vector2f aminmax = ProjectPoly(n, polya, npolya);
                Vector2f bminmax = ProjectPoly(n, polyb, npolyb);
                if (!OverlapRange(aminmax.x, aminmax.y, bminmax.x, bminmax.y, eps))
                {
                    // Found separating axis
                    return false;
                }
            }

            return true;
        }

        // Returns a random point in a convex polygon.
        // Adapted from Graphics Gems article.
        public static Vector3f RandomPointInConvexPoly(float[] pts, int npts, float[] areas, float s, float t)
        {
            // Calc triangle araes
            float areasum = 0.0f;
            for (int i = 2; i < npts; i++)
            {
                areas[i] = TriArea2D(pts, 0, (i - 1) * 3, i * 3);
                areasum += Math.Max(0.001f, areas[i]);
            }

            // Find sub triangle weighted by area.
            float thr = s * areasum;
            float acc = 0.0f;
            float u = 1.0f;
            int tri = npts - 1;
            for (int i = 2; i < npts; i++)
            {
                float dacc = areas[i];
                if (thr >= acc && thr < (acc + dacc))
                {
                    u = (thr - acc) / dacc;
                    tri = i;
                    break;
                }

                acc += dacc;
            }

            float v = (float)Math.Sqrt(t);

            float a = 1 - v;
            float b = (1 - u) * v;
            float c = u * v;
            int pa = 0;
            int pb = (tri - 1) * 3;
            int pc = tri * 3;

            return new Vector3f()
            {
                x = a * pts[pa] + b * pts[pb] + c * pts[pc],
                y = a * pts[pa + 1] + b * pts[pb + 1] + c * pts[pc + 1],
                z = a * pts[pa + 2] + b * pts[pb + 2] + c * pts[pc + 2]
            };
        }

        public static int NextPow2(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        public static int Ilog2(int v)
        {
            int r;
            int shift;
            r = (v > 0xffff ? 1 : 0) << 4;
            v >>= r;
            shift = (v > 0xff ? 1 : 0) << 3;
            v >>= shift;
            r |= shift;
            shift = (v > 0xf ? 1 : 0) << 2;
            v >>= shift;
            r |= shift;
            shift = (v > 0x3 ? 1 : 0) << 1;
            v >>= shift;
            r |= shift;
            r |= (v >> 1);
            return r;
        }


        public static IntersectResult IntersectSegmentPoly2D(Vector3f p0, Vector3f p1, float[] verts, int nverts)
        {
            IntersectResult result = new IntersectResult();
            float EPS = 0.000001f;
            var dir = p1.Subtract(p0);

            var p0v = p0;
            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                Vector3f vpj = Vector3f.Of(verts, j * 3);
                Vector3f vpi = Vector3f.Of(verts, i * 3);
                var edge = vpi.Subtract(vpj);
                var diff = p0v.Subtract(vpj);
                float n = Vector3f.Perp2D(edge, diff);
                float d = Vector3f.Perp2D(dir, edge);
                if (Math.Abs(d) < EPS)
                {
                    // S is nearly parallel to this edge
                    if (n < 0)
                    {
                        return result;
                    }
                    else
                    {
                        continue;
                    }
                }

                float t = n / d;
                if (d < 0)
                {
                    // segment S is entering across this edge
                    if (t > result.tmin)
                    {
                        result.tmin = t;
                        result.segMin = j;
                        // S enters after leaving polygon
                        if (result.tmin > result.tmax)
                        {
                            return result;
                        }
                    }
                }
                else
                {
                    // segment S is leaving across this edge
                    if (t < result.tmax)
                    {
                        result.tmax = t;
                        result.segMax = j;
                        // S leaves before entering polygon
                        if (result.tmax < result.tmin)
                        {
                            return result;
                        }
                    }
                }
            }

            result.intersects = true;
            return result;
        }

        public static int OppositeTile(int side)
        {
            return (side + 4) & 0x7;
        }


        public static bool IntersectSegSeg2D(Vector3f ap, Vector3f aq, Vector3f bp, Vector3f bq, out float s, out float t)
        {
            s = 0;
            t = 0;

            Vector3f u = aq.Subtract(ap);
            Vector3f v = bq.Subtract(bp);
            Vector3f w = ap.Subtract(bp);
            float d = Vector3f.PerpXZ(u, v);
            if (Math.Abs(d) < 1e-6f)
            {
                return false;
            }

            s = Vector3f.PerpXZ(v, w) / d;
            t = Vector3f.PerpXZ(u, w) / d;

            return true;
        }
    }
}