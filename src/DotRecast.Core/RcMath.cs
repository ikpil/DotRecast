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

        public static float VDistSqr(Vector3f v1, float[] v2, int i)
        {
            float dx = v2[i] - v1.x;
            float dy = v2[i + 1] - v1.y;
            float dz = v2[i + 2] - v1.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static float VDistSqr(Vector3f v1, Vector3f v2)
        {
            float dx = v2.x - v1.x;
            float dy = v2.y - v1.y;
            float dz = v2.z - v1.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static float VDistSqr(float[] v, int i, int j)
        {
            float dx = v[i] - v[j];
            float dy = v[i + 1] - v[j + 1];
            float dz = v[i + 2] - v[j + 2];
            return dx * dx + dy * dy + dz * dz;
        }



        public static float Sqr(float f)
        {
            return f * f;
        }

        public static float GetPathLen(float[] path, int npath)
        {
            float totd = 0;
            for (int i = 0; i < npath - 1; ++i)
            {
                totd += (float)Math.Sqrt(VDistSqr(path, i * 3, (i + 1) * 3));
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


        /// Performs a scaled vector addition. (@p v1 + (@p v2 * @p s))
        /// @param[out] dest The result vector. [(x, y, z)]
        /// @param[in] v1 The base vector. [(x, y, z)]
        /// @param[in] v2 The vector to scale and add to @p v1. [(x, y, z)]
        /// @param[in] s The amount to scale @p v2 by before adding to @p v1.
        public static Vector3f VMad(Vector3f v1, Vector3f v2, float s)
        {
            Vector3f dest = new Vector3f();
            dest.x = v1.x + v2.x * s;
            dest.y = v1.y + v2.y * s;
            dest.z = v1.z + v2.z * s;
            return dest;
        }

        /// Performs a linear interpolation between two vectors. (@p v1 toward @p
        /// v2)
        /// @param[out] dest The result vector. [(x, y, x)]
        /// @param[in] v1 The starting vector.
        /// @param[in] v2 The destination vector.
        /// @param[in] t The interpolation factor. [Limits: 0 <= value <= 1.0]
        public static Vector3f VLerp(float[] verts, int v1, int v2, float t)
        {
            return new Vector3f(
                verts[v1 + 0] + (verts[v2 + 0] - verts[v1 + 0]) * t,
                verts[v1 + 1] + (verts[v2 + 1] - verts[v1 + 1]) * t,
                verts[v1 + 2] + (verts[v2 + 2] - verts[v1 + 2]) * t
            );
        }


        public static void VSet(ref Vector3f @out, float a, float b, float c)
        {
            @out.x = a;
            @out.y = b;
            @out.z = c;
        }

        public static void VCopy(float[] @out, Vector3f @in)
        {
            @out[0] = @in.x;
            @out[1] = @in.y;
            @out[2] = @in.z;
        }

        public static void VCopy(ref Vector3f @out, float[] @in)
        {
            @out.x = @in[0];
            @out.y = @in[1];
            @out.z = @in[2];
        }

        public static void VCopy(ref Vector3f @out, float[] @in, int i)
        {
            @out.x = @in[i];
            @out.y = @in[i + 1];
            @out.z = @in[i + 2];
        }

        public static void VMin(ref Vector3f @out, float[] @in, int i)
        {
            @out.x = Math.Min(@out.x, @in[i]);
            @out.y = Math.Min(@out.y, @in[i + 1]);
            @out.z = Math.Min(@out.z, @in[i + 2]);
        }


        public static void VMax(ref Vector3f @out, float[] @in, int i)
        {
            @out.x = Math.Max(@out.x, @in[i]);
            @out.y = Math.Max(@out.y, @in[i + 1]);
            @out.z = Math.Max(@out.z, @in[i + 2]);
        }


        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        public static float VDistSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }


        /// Derives the square of the scalar length of the vector. (len * len)
        /// @param[in] v The vector. [(x, y, z)]
        /// @return The square of the scalar length of the vector.
        public static float VLenSqr(Vector3f v)
        {
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }


        /// Derives the distance between the specified points on the xz-plane.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the point on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        public static float VDist2D(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        public static float VDist2D(Vector3f v1, Vector3f v2)
        {
            float dx = v2.x - v1.x;
            float dz = v2.z - v1.z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }


        public static float VDist2DSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return dx * dx + dz * dz;
        }

        public static float VDist2DSqr(Vector3f v1, Vector3f v2)
        {
            float dx = v2.x - v1.x;
            float dz = v2.z - v1.z;
            return dx * dx + dz * dz;
        }


        public static float VDist2DSqr(Vector3f p, float[] verts, int i)
        {
            float dx = verts[i] - p.x;
            float dz = verts[i + 2] - p.z;
            return dx * dx + dz * dz;
        }

        /// Normalizes the vector.
        /// @param[in,out] v The vector to normalize. [(x, y, z)]
        public static void VNormalize(float[] v)
        {
            float d = (float)(1.0f / Math.Sqrt(Sqr(v[0]) + Sqr(v[1]) + Sqr(v[2])));
            if (d != 0)
            {
                v[0] *= d;
                v[1] *= d;
                v[2] *= d;
            }
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
            float d = VDistSqr(p0, p1);
            return d < thresholdSqr;
        }


        /// Derives the xz-plane 2D perp product of the two vectors. (uz*vx - ux*vz)
        /// @param[in] u The LHV vector [(x, y, z)]
        /// @param[in] v The RHV vector [(x, y, z)]
        /// @return The dot product on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        public static float VPerp2D(float[] u, float[] v)
        {
            return u[2] * v[0] - u[0] * v[2];
        }

        public static float VPerp2D(Vector3f u, Vector3f v)
        {
            return u.z * v.x - u.x * v.z;
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

        public static float TriArea2D(float[] a, float[] b, float[] c)
        {
            float abx = b[0] - a[0];
            float abz = b[2] - a[2];
            float acx = c[0] - a[0];
            float acz = c[2] - a[2];
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

        public static float TriArea2D(Vector3f a, float[] b, Vector3f c)
        {
            float abx = b[0] - a.x;
            float abz = b[2] - a.z;
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
        public static bool OverlapBounds(float[] amin, float[] amax, float[] bmin, float[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }

        public static bool OverlapBounds(Vector3f amin, Vector3f amax, Vector3f bmin, Vector3f bmax)
        {
            bool overlap = true;
            overlap = (amin.x > bmax.x || amax.x < bmin.x) ? false : overlap;
            overlap = (amin.y > bmax.y || amax.y < bmin.y) ? false : overlap;
            overlap = (amin.z > bmax.z || amax.z < bmin.z) ? false : overlap;
            return overlap;
        }

        public static Tuple<float, float> DistancePtSegSqr2D(Vector3f pt, Vector3f p, Vector3f q)
        {
            float pqx = q.x - p.x;
            float pqz = q.z - p.z;
            float dx = pt.x - p.x;
            float dz = pt.z - p.z;
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
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
            return Tuple.Create(dx * dx + dz * dz, t);
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
                if (((verts[vi + 2] > pt.z) != (verts[vj + 2] > pt.z)) && (pt.x < (verts[vj + 0] - verts[vi + 0])
                        * (pt.z - verts[vi + 2]) / (verts[vj + 2] - verts[vi + 2]) + verts[vi + 0]))
                {
                    c = !c;
                }

                Tuple<float, float> edet = DistancePtSegSqr2D(pt, verts, vj, vi);
                ed[j] = edet.Item1;
                et[j] = edet.Item2;
            }

            return c;
        }

        public static float[] ProjectPoly(Vector3f axis, float[] poly, int npoly)
        {
            float rmin, rmax;
            rmin = rmax = axis.Dot2D(poly, 0);
            for (int i = 1; i < npoly; ++i)
            {
                float d = axis.Dot2D(poly, i * 3);
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }

            return new float[] { rmin, rmax };
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

                float[] aminmax = ProjectPoly(n, polya, npolya);
                float[] bminmax = ProjectPoly(n, polyb, npolyb);
                if (!OverlapRange(aminmax[0], aminmax[1], bminmax[0], bminmax[1], eps))
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

                float[] aminmax = ProjectPoly(n, polya, npolya);
                float[] bminmax = ProjectPoly(n, polyb, npolyb);
                if (!OverlapRange(aminmax[0], aminmax[1], bminmax[0], bminmax[1], eps))
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
                float n = VPerp2D(edge, diff);
                float d = VPerp2D(dir, edge);
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

        public static Tuple<float, float> DistancePtSegSqr2D(Vector3f pt, SegmentVert verts, int p, int q)
        {
            float pqx = verts[q + 0] - verts[p + 0];
            float pqz = verts[q + 2] - verts[p + 2];
            float dx = pt.x - verts[p + 0];
            float dz = pt.z - verts[p + 2];
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
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

            dx = verts[p + 0] + t * pqx - pt.x;
            dz = verts[p + 2] + t * pqz - pt.z;
            return Tuple.Create(dx * dx + dz * dz, t);
        }


        public static Tuple<float, float> DistancePtSegSqr2D(Vector3f pt, float[] verts, int p, int q)
        {
            float pqx = verts[q + 0] - verts[p + 0];
            float pqz = verts[q + 2] - verts[p + 2];
            float dx = pt.x - verts[p + 0];
            float dz = pt.z - verts[p + 2];
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
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

            dx = verts[p + 0] + t * pqx - pt.x;
            dz = verts[p + 2] + t * pqz - pt.z;
            return Tuple.Create(dx * dx + dz * dz, t);
        }

        public static int OppositeTile(int side)
        {
            return (side + 4) & 0x7;
        }

        public static float VperpXZ(float[] a, float[] b)
        {
            return a[0] * b[2] - a[2] * b[0];
        }

        public static float VperpXZ(Vector3f a, Vector3f b)
        {
            return a.x * b.z - a.z * b.x;
        }

        public static Tuple<float, float> IntersectSegSeg2D(Vector3f ap, Vector3f aq, Vector3f bp, Vector3f bq)
        {
            Vector3f u = aq.Subtract(ap);
            Vector3f v = bq.Subtract(bp);
            Vector3f w = ap.Subtract(bp);
            float d = VperpXZ(u, v);
            if (Math.Abs(d) < 1e-6f)
            {
                return null;
            }

            float s = VperpXZ(v, w) / d;
            float t = VperpXZ(u, w) / d;
            return Tuple.Create(s, t);
        }

        public static Vector3f VScale(Vector3f @in, float scale)
        {
            var @out = new Vector3f();
            @out.x = @in.x * scale;
            @out.y = @in.y * scale;
            @out.z = @in.z * scale;
            return @out;
        }


        /// Checks that the specified vector's components are all finite.
        /// @param[in] v A point. [(x, y, z)]
        /// @return True if all of the point's components are finite, i.e. not NaN
        /// or any of the infinities.
        public static bool VIsFinite(float[] v)
        {
            return float.IsFinite(v[0]) && float.IsFinite(v[1]) && float.IsFinite(v[2]);
        }

        public static bool VIsFinite(Vector3f v)
        {
            return float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
        }

        /// Checks that the specified vector's 2D components are finite.
        /// @param[in] v A point. [(x, y, z)]
        public static bool VIsFinite2D(float[] v)
        {
            return float.IsFinite(v[0]) && float.IsFinite(v[2]);
        }

        public static bool VIsFinite2D(Vector3f v)
        {
            return float.IsFinite(v.x) && float.IsFinite(v.z);
        }
    }
}