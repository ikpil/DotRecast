/*
+recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

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

using static DotRecast.Core.RecastMath;
using static DotRecast.Recast.RecastConstants;
using static DotRecast.Recast.RecastVectors;

namespace DotRecast.Recast
{
    public class RecastFilledVolumeRasterization
    {
        private const float EPSILON = 0.00001f;
        private static readonly int[] BOX_EDGES = new[] { 0, 1, 0, 2, 0, 4, 1, 3, 1, 5, 2, 3, 2, 6, 3, 7, 4, 5, 4, 6, 5, 7, 6, 7 };

        public static void rasterizeSphere(Heightfield hf, Vector3f center, float radius, int area, int flagMergeThr, Telemetry ctx)
        {
            ctx.startTimer("RASTERIZE_SPHERE");
            float[] bounds =
            {
                center[0] - radius, center[1] - radius, center[2] - radius, center[0] + radius, center[1] + radius,
                center[2] + radius
            };
            rasterizationFilledShape(hf, bounds, area, flagMergeThr,
                rectangle => intersectSphere(rectangle, center, radius * radius));
            ctx.stopTimer("RASTERIZE_SPHERE");
        }

        public static void rasterizeCapsule(Heightfield hf, Vector3f start, Vector3f end, float radius, int area, int flagMergeThr,
            Telemetry ctx)
        {
            ctx.startTimer("RASTERIZE_CAPSULE");
            float[] bounds =
            {
                Math.Min(start[0], end[0]) - radius, Math.Min(start[1], end[1]) - radius,
                Math.Min(start[2], end[2]) - radius, Math.Max(start[0], end[0]) + radius, Math.Max(start[1], end[1]) + radius,
                Math.Max(start[2], end[2]) + radius
            };
            Vector3f axis = Vector3f.Of(end[0] - start[0], end[1] - start[1], end[2] - start[2]);
            rasterizationFilledShape(hf, bounds, area, flagMergeThr,
                rectangle => intersectCapsule(rectangle, start, end, axis, radius * radius));
            ctx.stopTimer("RASTERIZE_CAPSULE");
        }

        public static void rasterizeCylinder(Heightfield hf, Vector3f start, Vector3f end, float radius, int area, int flagMergeThr,
            Telemetry ctx)
        {
            ctx.startTimer("RASTERIZE_CYLINDER");
            float[] bounds =
            {
                Math.Min(start[0], end[0]) - radius, Math.Min(start[1], end[1]) - radius,
                Math.Min(start[2], end[2]) - radius, Math.Max(start[0], end[0]) + radius, Math.Max(start[1], end[1]) + radius,
                Math.Max(start[2], end[2]) + radius
            };
            Vector3f axis = Vector3f.Of(end[0] - start[0], end[1] - start[1], end[2] - start[2]);
            rasterizationFilledShape(hf, bounds, area, flagMergeThr,
                rectangle => intersectCylinder(rectangle, start, end, axis, radius * radius));
            ctx.stopTimer("RASTERIZE_CYLINDER");
        }

        public static void rasterizeBox(Heightfield hf, float[] center, float[][] halfEdges, int area, int flagMergeThr,
            Telemetry ctx)
        {
            ctx.startTimer("RASTERIZE_BOX");
            float[][] normals =
            {
                new[] { halfEdges[0][0], halfEdges[0][1], halfEdges[0][2] },
                new[] { halfEdges[1][0], halfEdges[1][1], halfEdges[1][2] },
                new[] { halfEdges[2][0], halfEdges[2][1], halfEdges[2][2] }
            };
            normalize(normals[0]);
            normalize(normals[1]);
            normalize(normals[2]);

            float[] vertices = new float[8 * 3];
            float[] bounds = new float[]
            {
                float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity,
                float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity
            };
            for (int i = 0; i < 8; ++i)
            {
                float s0 = (i & 1) != 0 ? 1f : -1f;
                float s1 = (i & 2) != 0 ? 1f : -1f;
                float s2 = (i & 4) != 0 ? 1f : -1f;
                vertices[i * 3 + 0] = center[0] + s0 * halfEdges[0][0] + s1 * halfEdges[1][0] + s2 * halfEdges[2][0];
                vertices[i * 3 + 1] = center[1] + s0 * halfEdges[0][1] + s1 * halfEdges[1][1] + s2 * halfEdges[2][1];
                vertices[i * 3 + 2] = center[2] + s0 * halfEdges[0][2] + s1 * halfEdges[1][2] + s2 * halfEdges[2][2];
                bounds[0] = Math.Min(bounds[0], vertices[i * 3 + 0]);
                bounds[1] = Math.Min(bounds[1], vertices[i * 3 + 1]);
                bounds[2] = Math.Min(bounds[2], vertices[i * 3 + 2]);
                bounds[3] = Math.Max(bounds[3], vertices[i * 3 + 0]);
                bounds[4] = Math.Max(bounds[4], vertices[i * 3 + 1]);
                bounds[5] = Math.Max(bounds[5], vertices[i * 3 + 2]);
            }

            float[][] planes = ArrayUtils.Of<float>(6, 4);
            for (int i = 0; i < 6; i++)
            {
                float m = i < 3 ? -1 : 1;
                int vi = i < 3 ? 0 : 7;
                planes[i][0] = m * normals[i % 3][0];
                planes[i][1] = m * normals[i % 3][1];
                planes[i][2] = m * normals[i % 3][2];
                planes[i][3] = vertices[vi * 3] * planes[i][0] + vertices[vi * 3 + 1] * planes[i][1]
                                                               + vertices[vi * 3 + 2] * planes[i][2];
            }

            rasterizationFilledShape(hf, bounds, area, flagMergeThr, rectangle => intersectBox(rectangle, vertices, planes));
            ctx.stopTimer("RASTERIZE_BOX");
        }

        public static void rasterizeConvex(Heightfield hf, float[] vertices, int[] triangles, int area, int flagMergeThr,
            Telemetry ctx)
        {
            ctx.startTimer("RASTERIZE_CONVEX");
            float[] bounds = new float[] { vertices[0], vertices[1], vertices[2], vertices[0], vertices[1], vertices[2] };
            for (int i = 0; i < vertices.Length; i += 3)
            {
                bounds[0] = Math.Min(bounds[0], vertices[i + 0]);
                bounds[1] = Math.Min(bounds[1], vertices[i + 1]);
                bounds[2] = Math.Min(bounds[2], vertices[i + 2]);
                bounds[3] = Math.Max(bounds[3], vertices[i + 0]);
                bounds[4] = Math.Max(bounds[4], vertices[i + 1]);
                bounds[5] = Math.Max(bounds[5], vertices[i + 2]);
            }


            float[][] planes = ArrayUtils.Of<float>(triangles.Length, 4);
            float[][] triBounds = ArrayUtils.Of<float>(triangles.Length / 3, 4);
            for (int i = 0, j = 0; i < triangles.Length; i += 3, j++)
            {
                int a = triangles[i] * 3;
                int b = triangles[i + 1] * 3;
                int c = triangles[i + 2] * 3;
                float[] ab = { vertices[b] - vertices[a], vertices[b + 1] - vertices[a + 1], vertices[b + 2] - vertices[a + 2] };
                float[] ac = { vertices[c] - vertices[a], vertices[c + 1] - vertices[a + 1], vertices[c + 2] - vertices[a + 2] };
                float[] bc = { vertices[c] - vertices[b], vertices[c + 1] - vertices[b + 1], vertices[c + 2] - vertices[b + 2] };
                float[] ca = { vertices[a] - vertices[c], vertices[a + 1] - vertices[c + 1], vertices[a + 2] - vertices[c + 2] };
                plane(planes, i, ab, ac, vertices, a);
                plane(planes, i + 1, planes[i], bc, vertices, b);
                plane(planes, i + 2, planes[i], ca, vertices, c);

                float s = 1.0f / (vertices[a] * planes[i + 1][0] + vertices[a + 1] * planes[i + 1][1]
                                                                 + vertices[a + 2] * planes[i + 1][2] - planes[i + 1][3]);
                planes[i + 1][0] *= s;
                planes[i + 1][1] *= s;
                planes[i + 1][2] *= s;
                planes[i + 1][3] *= s;

                s = 1.0f / (vertices[b] * planes[i + 2][0] + vertices[b + 1] * planes[i + 2][1] + vertices[b + 2] * planes[i + 2][2]
                            - planes[i + 2][3]);
                planes[i + 2][0] *= s;
                planes[i + 2][1] *= s;
                planes[i + 2][2] *= s;
                planes[i + 2][3] *= s;

                triBounds[j][0] = Math.Min(Math.Min(vertices[a], vertices[b]), vertices[c]);
                triBounds[j][1] = Math.Min(Math.Min(vertices[a + 2], vertices[b + 2]), vertices[c + 2]);
                triBounds[j][2] = Math.Max(Math.Max(vertices[a], vertices[b]), vertices[c]);
                triBounds[j][3] = Math.Max(Math.Max(vertices[a + 2], vertices[b + 2]), vertices[c + 2]);
            }

            rasterizationFilledShape(hf, bounds, area, flagMergeThr,
                rectangle => intersectConvex(rectangle, triangles, vertices, planes, triBounds));
            ctx.stopTimer("RASTERIZE_CONVEX");
        }

        private static void plane(float[][] planes, int p, float[] v1, float[] v2, float[] vertices, int vert)
        {
            RecastVectors.cross(planes[p], v1, v2);
            planes[p][3] = planes[p][0] * vertices[vert] + planes[p][1] * vertices[vert + 1] + planes[p][2] * vertices[vert + 2];
        }

        private static void rasterizationFilledShape(Heightfield hf, float[] bounds, int area, int flagMergeThr,
            Func<float[], float[]> intersection)
        {
            if (!overlapBounds(hf.bmin, hf.bmax, bounds))
            {
                return;
            }

            bounds[3] = Math.Min(bounds[3], hf.bmax[0]);
            bounds[5] = Math.Min(bounds[5], hf.bmax[2]);
            bounds[0] = Math.Max(bounds[0], hf.bmin[0]);
            bounds[2] = Math.Max(bounds[2], hf.bmin[2]);

            if (bounds[3] <= bounds[0] || bounds[4] <= bounds[1] || bounds[5] <= bounds[2])
            {
                return;
            }

            float ics = 1.0f / hf.cs;
            float ich = 1.0f / hf.ch;
            int xMin = (int)((bounds[0] - hf.bmin[0]) * ics);
            int zMin = (int)((bounds[2] - hf.bmin[2]) * ics);
            int xMax = Math.Min(hf.width - 1, (int)((bounds[3] - hf.bmin[0]) * ics));
            int zMax = Math.Min(hf.height - 1, (int)((bounds[5] - hf.bmin[2]) * ics));
            float[] rectangle = new float[5];
            rectangle[4] = hf.bmin[1];
            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    rectangle[0] = x * hf.cs + hf.bmin[0];
                    rectangle[1] = z * hf.cs + hf.bmin[2];
                    rectangle[2] = rectangle[0] + hf.cs;
                    rectangle[3] = rectangle[1] + hf.cs;
                    float[] h = intersection.Invoke(rectangle);
                    if (h != null)
                    {
                        int smin = (int)Math.Floor((h[0] - hf.bmin[1]) * ich);
                        int smax = (int)Math.Ceiling((h[1] - hf.bmin[1]) * ich);
                        if (smin != smax)
                        {
                            int ismin = clamp(smin, 0, SPAN_MAX_HEIGHT);
                            int ismax = clamp(smax, ismin + 1, SPAN_MAX_HEIGHT);
                            RecastRasterization.addSpan(hf, x, z, ismin, ismax, area, flagMergeThr);
                        }
                    }
                }
            }
        }

        private static float[] intersectSphere(float[] rectangle, Vector3f center, float radiusSqr)
        {
            float x = Math.Max(rectangle[0], Math.Min(center[0], rectangle[2]));
            float y = rectangle[4];
            float z = Math.Max(rectangle[1], Math.Min(center[2], rectangle[3]));

            float mx = x - center[0];
            float my = y - center[1];
            float mz = z - center[2];

            float b = my; // dot(m, d) d = (0, 1, 0)
            float c = lenSqr(mx, my, mz) - radiusSqr;
            if (c > 0.0f && b > 0.0f)
            {
                return null;
            }

            float discr = b * b - c;
            if (discr < 0.0f)
            {
                return null;
            }

            float discrSqrt = (float)Math.Sqrt(discr);
            float tmin = -b - discrSqrt;
            float tmax = -b + discrSqrt;

            if (tmin < 0.0f)
            {
                tmin = 0.0f;
            }

            return new float[] { y + tmin, y + tmax };
        }

        private static float[] intersectCapsule(float[] rectangle, Vector3f start, Vector3f end, Vector3f axis, float radiusSqr)
        {
            float[] s = mergeIntersections(intersectSphere(rectangle, start, radiusSqr), intersectSphere(rectangle, end, radiusSqr));
            float axisLen2dSqr = axis[0] * axis[0] + axis[2] * axis[2];
            if (axisLen2dSqr > EPSILON)
            {
                s = slabsCylinderIntersection(rectangle, start, end, axis, radiusSqr, s);
            }

            return s;
        }

        private static float[] intersectCylinder(float[] rectangle, Vector3f start, Vector3f end, Vector3f axis, float radiusSqr)
        {
            float[] s = mergeIntersections(
                rayCylinderIntersection(new float[]
                {
                    clamp(start[0], rectangle[0], rectangle[2]), rectangle[4],
                    clamp(start[2], rectangle[1], rectangle[3])
                }, start, axis, radiusSqr),
                rayCylinderIntersection(new float[]
                {
                    clamp(end[0], rectangle[0], rectangle[2]), rectangle[4],
                    clamp(end[2], rectangle[1], rectangle[3])
                }, start, axis, radiusSqr));
            float axisLen2dSqr = axis[0] * axis[0] + axis[2] * axis[2];
            if (axisLen2dSqr > EPSILON)
            {
                s = slabsCylinderIntersection(rectangle, start, end, axis, radiusSqr, s);
            }

            if (axis[1] * axis[1] > EPSILON)
            {
                float[][] rectangleOnStartPlane = ArrayUtils.Of<float>(4, 3);
                float[][] rectangleOnEndPlane = ArrayUtils.Of<float>(4, 3);
                float ds = dot(axis, start);
                float de = dot(axis, end);
                for (int i = 0; i < 4; i++)
                {
                    float x = rectangle[(i + 1) & 2];
                    float z = rectangle[(i & 2) + 1];
                    Vector3f a = Vector3f.Of(x, rectangle[4], z);
                    float dotAxisA = dot(axis, a);
                    float t = (ds - dotAxisA) / axis[1];
                    rectangleOnStartPlane[i][0] = x;
                    rectangleOnStartPlane[i][1] = rectangle[4] + t;
                    rectangleOnStartPlane[i][2] = z;
                    t = (de - dotAxisA) / axis[1];
                    rectangleOnEndPlane[i][0] = x;
                    rectangleOnEndPlane[i][1] = rectangle[4] + t;
                    rectangleOnEndPlane[i][2] = z;
                }

                for (int i = 0; i < 4; i++)
                {
                    s = cylinderCapIntersection(start, radiusSqr, s, i, rectangleOnStartPlane);
                    s = cylinderCapIntersection(end, radiusSqr, s, i, rectangleOnEndPlane);
                }
            }

            return s;
        }

        private static float[] cylinderCapIntersection(Vector3f start, float radiusSqr, float[] s, int i, float[][] rectangleOnPlane)
        {
            int j = (i + 1) % 4;
            // Ray against sphere intersection
            float[] m = { rectangleOnPlane[i][0] - start[0], rectangleOnPlane[i][1] - start[1], rectangleOnPlane[i][2] - start[2] };
            float[] d =
            {
                rectangleOnPlane[j][0] - rectangleOnPlane[i][0], rectangleOnPlane[j][1] - rectangleOnPlane[i][1],
                rectangleOnPlane[j][2] - rectangleOnPlane[i][2]
            };
            float dl = dot(d, d);
            float b = dot(m, d) / dl;
            float c = (dot(m, m) - radiusSqr) / dl;
            float discr = b * b - c;
            if (discr > EPSILON)
            {
                float discrSqrt = (float)Math.Sqrt(discr);
                float t1 = -b - discrSqrt;
                float t2 = -b + discrSqrt;
                if (t1 <= 1 && t2 >= 0)
                {
                    t1 = Math.Max(0, t1);
                    t2 = Math.Min(1, t2);
                    float y1 = rectangleOnPlane[i][1] + t1 * d[1];
                    float y2 = rectangleOnPlane[i][1] + t2 * d[1];
                    float[] y = { Math.Min(y1, y2), Math.Max(y1, y2) };
                    s = mergeIntersections(s, y);
                }
            }

            return s;
        }

        private static float[] slabsCylinderIntersection(float[] rectangle, Vector3f start, Vector3f end, Vector3f axis, float radiusSqr,
            float[] s)
        {
            if (Math.Min(start[0], end[0]) < rectangle[0])
            {
                s = mergeIntersections(s, xSlabCylinderIntersection(rectangle, start, axis, radiusSqr, rectangle[0]));
            }

            if (Math.Max(start[0], end[0]) > rectangle[2])
            {
                s = mergeIntersections(s, xSlabCylinderIntersection(rectangle, start, axis, radiusSqr, rectangle[2]));
            }

            if (Math.Min(start[2], end[2]) < rectangle[1])
            {
                s = mergeIntersections(s, zSlabCylinderIntersection(rectangle, start, axis, radiusSqr, rectangle[1]));
            }

            if (Math.Max(start[2], end[2]) > rectangle[3])
            {
                s = mergeIntersections(s, zSlabCylinderIntersection(rectangle, start, axis, radiusSqr, rectangle[3]));
            }

            return s;
        }

        private static float[] xSlabCylinderIntersection(float[] rectangle, Vector3f start, Vector3f axis, float radiusSqr, float x)
        {
            return rayCylinderIntersection(xSlabRayIntersection(rectangle, start, axis, x), start, axis, radiusSqr);
        }

        private static float[] xSlabRayIntersection(float[] rectangle, Vector3f start, Vector3f direction, float x)
        {
            // 2d intersection of plane and segment
            float t = (x - start[0]) / direction[0];
            float z = clamp(start[2] + t * direction[2], rectangle[1], rectangle[3]);
            return new float[] { x, rectangle[4], z };
        }

        private static float[] zSlabCylinderIntersection(float[] rectangle, Vector3f start, Vector3f axis, float radiusSqr, float z)
        {
            return rayCylinderIntersection(zSlabRayIntersection(rectangle, start, axis, z), start, axis, radiusSqr);
        }

        private static float[] zSlabRayIntersection(float[] rectangle, Vector3f start, Vector3f direction, float z)
        {
            // 2d intersection of plane and segment
            float t = (z - start[2]) / direction[2];
            float x = clamp(start[0] + t * direction[0], rectangle[0], rectangle[2]);
            return new float[] { x, rectangle[4], z };
        }

        // Based on Christer Ericsons's "Real-Time Collision Detection"
        private static float[] rayCylinderIntersection(float[] point, Vector3f start, Vector3f axis, float radiusSqr)
        {
            Vector3f d = axis;
            Vector3f m = Vector3f.Of(point[0] - start[0], point[1] - start[1], point[2] - start[2]);
            // float[] n = { 0, 1, 0 };
            float md = dot(m, d);
            // float nd = dot(n, d);
            float nd = axis[1];
            float dd = dot(d, d);

            // float nn = dot(n, n);
            float nn = 1;
            // float mn = dot(m, n);
            float mn = m[1];
            // float a = dd * nn - nd * nd;
            float a = dd - nd * nd;
            float k = dot(m, m) - radiusSqr;
            float c = dd * k - md * md;
            if (Math.Abs(a) < EPSILON)
            {
                // Segment runs parallel to cylinder axis
                if (c > 0.0f)
                {
                    return null; // ’a’ and thus the segment lie outside cylinder
                }

                // Now known that segment intersects cylinder; figure out how it intersects
                float tt1 = -mn / nn; // Intersect segment against ’p’ endcap
                float tt2 = (nd - mn) / nn; // Intersect segment against ’q’ endcap
                return new float[] { point[1] + Math.Min(tt1, tt2), point[1] + Math.Max(tt1, tt2) };
            }

            float b = dd * mn - nd * md;
            float discr = b * b - a * c;
            if (discr < 0.0f)
            {
                return null; // No real roots; no intersection
            }

            float discSqrt = (float)Math.Sqrt(discr);
            float t1 = (-b - discSqrt) / a;
            float t2 = (-b + discSqrt) / a;

            if (md + t1 * nd < 0.0f)
            {
                // Intersection outside cylinder on ’p’ side
                t1 = -md / nd;
                if (k + t1 * (2 * mn + t1 * nn) > 0.0f)
                {
                    return null;
                }
            }
            else if (md + t1 * nd > dd)
            {
                // Intersection outside cylinder on ’q’ side
                t1 = (dd - md) / nd;
                if (k + dd - 2 * md + t1 * (2 * (mn - nd) + t1 * nn) > 0.0f)
                {
                    return null;
                }
            }

            if (md + t2 * nd < 0.0f)
            {
                // Intersection outside cylinder on ’p’ side
                t2 = -md / nd;
                if (k + t2 * (2 * mn + t2 * nn) > 0.0f)
                {
                    return null;
                }
            }
            else if (md + t2 * nd > dd)
            {
                // Intersection outside cylinder on ’q’ side
                t2 = (dd - md) / nd;
                if (k + dd - 2 * md + t2 * (2 * (mn - nd) + t2 * nn) > 0.0f)
                {
                    return null;
                }
            }

            return new float[] { point[1] + Math.Min(t1, t2), point[1] + Math.Max(t1, t2) };
        }

        private static float[] intersectBox(float[] rectangle, float[] vertices, float[][] planes)
        {
            float yMin = float.PositiveInfinity;
            float yMax = float.NegativeInfinity;
            // check intersection with rays starting in box vertices first
            for (int i = 0; i < 8; i++)
            {
                int vi = i * 3;
                if (vertices[vi] >= rectangle[0] && vertices[vi] < rectangle[2] && vertices[vi + 2] >= rectangle[1]
                    && vertices[vi + 2] < rectangle[3])
                {
                    yMin = Math.Min(yMin, vertices[vi + 1]);
                    yMax = Math.Max(yMax, vertices[vi + 1]);
                }
            }

            // check intersection with rays starting in rectangle vertices
            float[] point = new float[] { 0, rectangle[1], 0 };
            for (int i = 0; i < 4; i++)
            {
                point[0] = ((i & 1) == 0) ? rectangle[0] : rectangle[2];
                point[2] = ((i & 2) == 0) ? rectangle[1] : rectangle[3];
                for (int j = 0; j < 6; j++)
                {
                    if (Math.Abs(planes[j][1]) > EPSILON)
                    {
                        float dotNormalPoint = dot(planes[j], point);
                        float t = (planes[j][3] - dotNormalPoint) / planes[j][1];
                        float y = point[1] + t;
                        bool valid = true;
                        for (int k = 0; k < 6; k++)
                        {
                            if (k != j)
                            {
                                if (point[0] * planes[k][0] + y * planes[k][1] + point[2] * planes[k][2] > planes[k][3])
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }

                        if (valid)
                        {
                            yMin = Math.Min(yMin, y);
                            yMax = Math.Max(yMax, y);
                        }
                    }
                }
            }

            // check intersection with box edges
            for (int i = 0; i < BOX_EDGES.Length; i += 2)
            {
                int vi = BOX_EDGES[i] * 3;
                int vj = BOX_EDGES[i + 1] * 3;
                float x = vertices[vi];
                float z = vertices[vi + 2];
                // edge slab intersection
                float y = vertices[vi + 1];
                float dx = vertices[vj] - x;
                float dy = vertices[vj + 1] - y;
                float dz = vertices[vj + 2] - z;
                if (Math.Abs(dx) > EPSILON)
                {
                    float? iy = xSlabSegmentIntersection(rectangle, x, y, z, dx, dy, dz, rectangle[0]);
                    if (iy != null)
                    {
                        yMin = Math.Min(yMin, iy.Value);
                        yMax = Math.Max(yMax, iy.Value);
                    }

                    iy = xSlabSegmentIntersection(rectangle, x, y, z, dx, dy, dz, rectangle[2]);
                    if (iy != null)
                    {
                        yMin = Math.Min(yMin, iy.Value);
                        yMax = Math.Max(yMax, iy.Value);
                    }
                }

                if (Math.Abs(dz) > EPSILON)
                {
                    float? iy = zSlabSegmentIntersection(rectangle, x, y, z, dx, dy, dz, rectangle[1]);
                    if (iy != null)
                    {
                        yMin = Math.Min(yMin, iy.Value);
                        yMax = Math.Max(yMax, iy.Value);
                    }

                    iy = zSlabSegmentIntersection(rectangle, x, y, z, dx, dy, dz, rectangle[3]);
                    if (iy != null)
                    {
                        yMin = Math.Min(yMin, iy.Value);
                        yMax = Math.Max(yMax, iy.Value);
                    }
                }
            }

            if (yMin <= yMax)
            {
                return new float[] { yMin, yMax };
            }

            return null;
        }

        private static float[] intersectConvex(float[] rectangle, int[] triangles, float[] verts, float[][] planes,
            float[][] triBounds)
        {
            float imin = float.PositiveInfinity;
            float imax = float.NegativeInfinity;
            for (int tr = 0, tri = 0; tri < triangles.Length; tr++, tri += 3)
            {
                if (triBounds[tr][0] > rectangle[2] || triBounds[tr][2] < rectangle[0] || triBounds[tr][1] > rectangle[3]
                    || triBounds[tr][3] < rectangle[1])
                {
                    continue;
                }

                if (Math.Abs(planes[tri][1]) < EPSILON)
                {
                    continue;
                }

                for (int i = 0; i < 3; i++)
                {
                    int vi = triangles[tri + i] * 3;
                    int vj = triangles[tri + (i + 1) % 3] * 3;
                    float x = verts[vi];
                    float z = verts[vi + 2];
                    // triangle vertex
                    if (x >= rectangle[0] && x <= rectangle[2] && z >= rectangle[1] && z <= rectangle[3])
                    {
                        imin = Math.Min(imin, verts[vi + 1]);
                        imax = Math.Max(imax, verts[vi + 1]);
                    }

                    // triangle slab intersection
                    float y = verts[vi + 1];
                    float dx = verts[vj] - x;
                    float dy = verts[vj + 1] - y;
                    float dz = verts[vj + 2] - z;
                    if (Math.Abs(dx) > EPSILON)
                    {
                        float? iy = xSlabSegmentIntersection(rectangle, x, y, z, dx, dy, dz, rectangle[0]);
                        if (iy != null)
                        {
                            imin = Math.Min(imin, iy.Value);
                            imax = Math.Max(imax, iy.Value);
                        }

                        iy = xSlabSegmentIntersection(rectangle, x, y, z, dx, dy, dz, rectangle[2]);
                        if (iy != null)
                        {
                            imin = Math.Min(imin, iy.Value);
                            imax = Math.Max(imax, iy.Value);
                        }
                    }

                    if (Math.Abs(dz) > EPSILON)
                    {
                        float? iy = zSlabSegmentIntersection(rectangle, x, y, z, dx, dy, dz, rectangle[1]);
                        if (iy != null)
                        {
                            imin = Math.Min(imin, iy.Value);
                            imax = Math.Max(imax, iy.Value);
                        }

                        iy = zSlabSegmentIntersection(rectangle, x, y, z, dx, dy, dz, rectangle[3]);
                        if (iy != null)
                        {
                            imin = Math.Min(imin, iy.Value);
                            imax = Math.Max(imax, iy.Value);
                        }
                    }
                }

                // rectangle vertex
                float[] point = new float[] { 0, rectangle[1], 0 };
                for (int i = 0; i < 4; i++)
                {
                    point[0] = ((i & 1) == 0) ? rectangle[0] : rectangle[2];
                    point[2] = ((i & 2) == 0) ? rectangle[1] : rectangle[3];
                    float? y = rayTriangleIntersection(point, tri, planes);
                    if (y != null)
                    {
                        imin = Math.Min(imin, y.Value);
                        imax = Math.Max(imax, y.Value);
                    }
                }
            }

            if (imin < imax)
            {
                return new float[] { imin, imax };
            }

            return null;
        }

        private static float? xSlabSegmentIntersection(float[] rectangle, float x, float y, float z, float dx, float dy, float dz,
            float slabX)
        {
            float x2 = x + dx;
            if ((x < slabX && x2 > slabX) || (x > slabX && x2 < slabX))
            {
                float t = (slabX - x) / dx;
                float iz = z + dz * t;
                if (iz >= rectangle[1] && iz <= rectangle[3])
                {
                    return y + dy * t;
                }
            }

            return null;
        }

        private static float? zSlabSegmentIntersection(float[] rectangle, float x, float y, float z, float dx, float dy, float dz,
            float slabZ)
        {
            float z2 = z + dz;
            if ((z < slabZ && z2 > slabZ) || (z > slabZ && z2 < slabZ))
            {
                float t = (slabZ - z) / dz;
                float ix = x + dx * t;
                if (ix >= rectangle[0] && ix <= rectangle[2])
                {
                    return y + dy * t;
                }
            }

            return null;
        }

        private static float? rayTriangleIntersection(float[] point, int plane, float[][] planes)
        {
            float t = (planes[plane][3] - dot(planes[plane], point)) / planes[plane][1];
            float[] s = { point[0], point[1] + t, point[2] };
            float u = dot(s, planes[plane + 1]) - planes[plane + 1][3];
            if (u < 0.0f || u > 1.0f)
            {
                return null;
            }

            float v = dot(s, planes[plane + 2]) - planes[plane + 2][3];
            if (v < 0.0f)
            {
                return null;
            }

            float w = 1f - u - v;
            if (w < 0.0f)
            {
                return null;
            }

            return s[1];
        }

        private static float[] mergeIntersections(float[] s1, float[] s2)
        {
            if (s1 == null)
            {
                return s2;
            }

            if (s2 == null)
            {
                return s1;
            }

            return new float[] { Math.Min(s1[0], s2[0]), Math.Max(s1[1], s2[1]) };
        }

        private static float lenSqr(float dx, float dy, float dz)
        {
            return dx * dx + dy * dy + dz * dz;
        }

        private static bool overlapBounds(Vector3f amin, Vector3f amax, float[] bounds)
        {
            bool overlap = true;
            overlap = (amin[0] > bounds[3] || amax[0] < bounds[0]) ? false : overlap;
            overlap = (amin[1] > bounds[4]) ? false : overlap;
            overlap = (amin[2] > bounds[5] || amax[2] < bounds[2]) ? false : overlap;
            return overlap;
        }
    }
}