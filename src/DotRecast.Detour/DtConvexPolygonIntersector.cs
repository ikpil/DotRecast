/*
recast4j copyright (c) 2026 Piotr Piastucki piotr@recast4j.org
DotRecast Copyright (c) 2023-2026 Choi Ikpil ikpil@naver.com

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
    /**
     * Computes the intersection (common area) of two CONVEX polygons given as flat 3D coordinate arrays: {x0,y0,z0,
     * x1,y1,z1, ...}.
     *
     * Hybrid 2D/3D approach: - The actual clipping (Sutherland-Hodgman) is done purely in the (X, Z) plane, since that's
     * the plane the polygon shapes are defined in ("footprint"). - The Y coordinate of every vertex - including new
     * vertices created at edge intersections - is linearly interpolated using the same parametric factor {@code t} that
     * produced the (X, Z) intersection point, so it supports polygons whose vertices don't all share the same Y (e.g.
     * sloped/ramped floors), while the actual geometric clipping test stays 2D.
     *
     * Performance notes (this is the allocation-light version): - No {@code List}, no per-vertex {@code float[3]}/object
     * allocation. Vertices live in two flat {@code float[]} "ping-pong" buffers that are swapped after each clip edge,
     * sized once up front to the only possible upper bound (subject vertex count + clip vertex count - Sutherland-Hodgman
     * can add at most one new vertex per clip edge on top of the vertices that survive). - Per clip edge, each surviving
     * point's side value is computed once into a scratch array and reused for both its role as "current" and as "previous"
     * of the next point, instead of being recomputed twice. - Total allocation per call: 2 scratch coordinate buffers + 1
     * side array + the final result array. If this is called very frequently (e.g. once per frame per polygon pair) and
     * even that becomes a GC concern, those three scratch buffers can be pulled out into thread-local/pooled storage - ask
     * if you want that variant.
     *
     * Sutherland-Hodgman is the right choice here because both polygons are convex: clipping the subject polygon against
     * every edge of the convex clip polygon is guaranteed to produce the exact intersection in a single pass, with no need
     * for a general (Weiler-Atherton / Greiner-Hormann) clipper, which would only be necessary for
     * concave/self-intersecting polygons.
     *
     * Both input polygons are assumed to be: - convex - simple (non self-intersecting) - given as a list of vertices in
     * order (NOT closed, i.e. the first vertex is not repeated at the end), wound CLOCKWISE in (X, Z)
     *
     * The winding is validated: a polygon whose (X, Z) signed area is negative (i.e. wound counter-clockwise) causes an
     * {@link ArgumentException} to be thrown. Convexity and simplicity are assumed and not checked.
     */
    // The allocation notes above describe upstream Java. This C# port uses stack scratch
    // spans for small inputs, arrays for larger/growing buffers, and a caller-provided result span.
    // The subject + clip vertex bound holds in exact arithmetic; rounding can exceed it.
    public static class DtConvexPolygonIntersector
    {
        /**
         * Relative distance tolerance for merging duplicate vertices, expressed as a fraction of the polygon's extent (its
         * (X, Z) bounding-box diagonal). Using a fraction rather than an absolute distance keeps the test scale-invariant:
         * an absolute epsilon is below float precision for large coordinates (so near-duplicates would not merge) and too
         * coarse for small ones (so distinct vertices would collapse).
         */
        private const double DIST_EPS = 1e-5;

        /**
         * Relative area tolerance. A result whose area is smaller than this fraction of the smaller input polygon's area is
         * treated as a degenerate (zero-area) overlap and rejected. Using a fraction rather than an absolute area keeps the
         * test scale-invariant: an absolute epsilon would reject legitimate small intersections of small polygons while
         * accepting near-degenerate slivers of large ones.
         */
        private const double AREA_EPS = 1e-5;
        private const int STACK_VERTEX_CAPACITY = 64;

        // Output bound for convex polygons in exact arithmetic. Float-rounded nearly
        // collinear inputs can exceed it; those callers must provide a larger result span.
        public static int CalculateIntersectionBufferSize(int n, int m)
        {
            return checked((n + m) * 3);
        }

        /**
         * Computes the intersection of two convex polygons, clipping in the (X, Z) plane and interpolating Y at newly
         * created vertices.
         *
         * @param p
         *            first polygon, flat array of {x,y,z} triplets (subject)
         * @param q
         *            second polygon, flat array of {x,y,z} triplets (clip)
         * @param intersection
         *            receives the intersection polygon as flat {x,y,z} triplets
         * @param nin
         *            receives the coordinate count, or zero if the polygons do not overlap in an area
         * @return true if the polygons overlap in an area, or false if there is no overlap or the overlap is reduced
         *         to a point/line/edge
         * @throws ArgumentException
         *             if either polygon is wound counter-clockwise in (X, Z) (negative signed area), or the output span is too small
         */
        // The C# overload writes the polygon to intersection and its coordinate count to nin;
        // it returns false with nin = 0 instead of returning null for an empty intersection.
        public static bool Intersect(ReadOnlySpan<float> p, ReadOnlySpan<float> q, Span<float> intersection, out int nin)
        {
            nin = 0;
            int subjectCount = p.Length / 3;
            int clipCount = q.Length / 3;

            if (subjectCount < 3 || clipCount < 3)
            {
                return false;
            }
            double minInputArea = Math.Min(AreaXZ(p, subjectCount), AreaXZ(q, clipCount));
            if (minInputArea == 0)
            {
                return false;
            }
            if (minInputArea < 0)
            {
                throw new ArgumentException("Input polygons must be convex and clockwise.");
            }

            // subjectCount + clipCount is an upper bound in exact arithmetic
            // (Sutherland-Hodgman adds at most one vertex per clip edge), but
            // float rounding on nearly collinear vertices can exceed it, so the
            // buffers below are grown on demand.
            int maxVerts = subjectCount + clipCount;
            Span<float> current = maxVerts <= STACK_VERTEX_CAPACITY ? stackalloc float[maxVerts * 3] : new float[maxVerts * 3];
            Span<float> next = maxVerts <= STACK_VERTEX_CAPACITY ? stackalloc float[maxVerts * 3] : new float[maxVerts * 3];
            Span<double> sides = maxVerts <= STACK_VERTEX_CAPACITY ? stackalloc double[maxVerts] : new double[maxVerts];
            p.CopyTo(current);
            int count = subjectCount;

            for (int i = 0; i < clipCount && count > 0; i++)
            {
                int aIdx = i * 3;
                int bIdx = ((i + 1) % clipCount) * 3;
                double ax = q[aIdx], az = q[aIdx + 2];
                double abx = q[bIdx] - ax;
                double abz = q[bIdx + 2] - az;

                for (int j = 0; j < count; j++)
                {
                    int idx = j * 3;
                    double apx = current[idx] - ax;
                    double apz = current[idx + 2] - az;
                    sides[j] = abx * apz - abz * apx;
                }

                int outCount = 0;
                for (int j = 0; j < count; j++)
                {
                    int prevJ = (j == 0) ? count - 1 : j - 1;
                    double sideCurr = sides[j];
                    double sidePrev = sides[prevJ];

                    // Clip polygon is clockwise, so "inside" is the <= 0 side
                    // of each directed edge (opposite of the CCW convention).
                    // Points exactly on the edge (side == 0) count as inside;
                    // the interpolation below is well-defined at that boundary
                    // (t resolves to 0 or 1), so no epsilon margin is needed.
                    bool currInside = sideCurr <= 0;
                    bool prevInside = sidePrev <= 0;

                    int idx = j * 3;
                    int prevIdx = prevJ * 3;

                    // Each iteration writes at most two vertices (one
                    // interpolated crossing point plus the current vertex), so
                    // reserve the slots actually written by this transition and grow only here. The
                    // side scratch array is grown in lockstep so it can hold
                    // the (possibly larger) vertex count of the next edge.
                    int required = outCount + (currInside ? 1 : 0) + (currInside != prevInside ? 1 : 0);
                    if (required > next.Length / 3)
                    {
                        float[] grown = new float[checked(next.Length * 2)];
                        next.Slice(0, outCount * 3).CopyTo(grown);
                        next = grown;
                        double[] grownSides = new double[checked(sides.Length * 2)];
                        sides.Slice(0, count).CopyTo(grownSides);
                        sides = grownSides;
                    }

                    if (currInside)
                    {
                        if (!prevInside)
                        {
                            outCount = WriteInterpolated(next, outCount, current, prevIdx, idx, sidePrev, sideCurr);
                        }
                        int outIdx = outCount * 3;
                        next[outIdx] = current[idx];
                        next[outIdx + 1] = current[idx + 1];
                        next[outIdx + 2] = current[idx + 2];
                        outCount++;
                    }
                    else if (prevInside)
                    {
                        outCount = WriteInterpolated(next, outCount, current, prevIdx, idx, sidePrev, sideCurr);
                    }
                }

                count = outCount;
                Span<float> tmp = current;
                current = next;
                next = tmp;
            }

            if (count < 3)
            {
                return false;
            }

            // Merge near-duplicate vertices using a tolerance relative to the
            // polygon's own extent, so the test is scale-invariant.
            double extent = ExtentXZ(current, count);
            count = DedupeInPlace(current, count, DIST_EPS * extent);

            if (count < 3)
            {
                return false;
            }

            // Reject degenerate results (overlap reduced to a point, line or
            // edge). The threshold is relative to the input polygons' areas so
            // the test is scale-invariant.
            if (AreaXZ(current, count) < AREA_EPS * minInputArea)
            {
                return false;
            }

            int coordinateCount = count * 3;
            if (coordinateCount > intersection.Length)
            {
                throw new ArgumentException("Intersection buffer is too small.", nameof(intersection));
            }
            current.Slice(0, coordinateCount).CopyTo(intersection);
            nin = coordinateCount;
            return true;
        }

        /**
         * Writes the point where segment (prevIdx -> idx) crosses the current clip edge into {@code dest} at the next free
         * slot, interpolating X, Y and Z with the same factor {@code t} derived from the (X,Z) side values.
         */
        private static int WriteInterpolated(Span<float> dest, int outCount, ReadOnlySpan<float> src,
            int prevIdx, int idx, double sidePrev, double sideCurr)
        {
            double t = sidePrev / (sidePrev - sideCurr);
            int outIdx = outCount * 3;
            dest[outIdx] = (float)(src[prevIdx] + t * (src[idx] - src[prevIdx]));
            dest[outIdx + 1] = (float)(src[prevIdx + 1] + t * (src[idx + 1] - src[prevIdx + 1]));
            dest[outIdx + 2] = (float)(src[prevIdx + 2] + t * (src[idx + 2] - src[prevIdx + 2]));
            return outCount + 1;
        }

        /** Returns the signed area of the polygon's (X, Z) footprint (positive for clockwise). */
        public static double AreaXZ(ReadOnlySpan<float> pts, int count)
        {
            int lastIdx = (count - 1) * 3;
            double area = pts[lastIdx] * (double)pts[2] - pts[0] * (double)pts[lastIdx + 2];
            for (int idx = 0; idx < lastIdx; idx += 3)
            {
                int nextIdx = idx + 3;
                area += pts[idx] * (double)pts[nextIdx + 2] - pts[nextIdx] * (double)pts[idx + 2];
            }
            return -area / 2.0;
        }

        /** Returns the length of the (X, Z) bounding-box diagonal of the polygon. */
        private static double ExtentXZ(ReadOnlySpan<float> pts, int count)
        {
            double minX = pts[0], maxX = pts[0];
            double minZ = pts[2], maxZ = pts[2];
            for (int i = 1; i < count; i++)
            {
                int idx = i * 3;
                minX = Math.Min(minX, pts[idx]);
                maxX = Math.Max(maxX, pts[idx]);
                minZ = Math.Min(minZ, pts[idx + 2]);
                maxZ = Math.Max(maxZ, pts[idx + 2]);
            }
            double dx = maxX - minX;
            double dz = maxZ - minZ;
            return Math.Sqrt(dx * dx + dz * dz);
        }

        /** Removes consecutive duplicate vertices (including wrap-around), in place, comparing only X/Z. */
        private static int DedupeInPlace(Span<float> pts, int count, double tolerance)
        {
            double toleranceSq = tolerance * tolerance;
            int writeIdx = 0;
            for (int i = 0; i < count; i++)
            {
                int readIdx = i * 3;
                if (writeIdx == 0 || DistSqXZ(pts, readIdx, (writeIdx - 1) * 3) > toleranceSq)
                {
                    if (writeIdx != i)
                    {
                        int w = writeIdx * 3;
                        pts[w] = pts[readIdx];
                        pts[w + 1] = pts[readIdx + 1];
                        pts[w + 2] = pts[readIdx + 2];
                    }
                    writeIdx++;
                }
            }
            if (writeIdx > 1 && DistSqXZ(pts, 0, (writeIdx - 1) * 3) <= toleranceSq)
            {
                writeIdx--;
            }
            return writeIdx;
        }

        private static double DistSqXZ(ReadOnlySpan<float> pts, int idxA, int idxB)
        {
            double dx = pts[idxA] - pts[idxB];
            double dz = pts[idxA + 2] - pts[idxB + 2];
            return dx * dx + dz * dz;
        }
    }
}
