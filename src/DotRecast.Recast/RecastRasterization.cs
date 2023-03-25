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
using static DotRecast.Core.RecastMath;
using static DotRecast.Recast.RecastConstants;

namespace DotRecast.Recast
{

    public class RecastRasterization
    {
        /**
     * Check whether two bounding boxes overlap
     *
     * @param amin
     *            Min axis extents of bounding box A
     * @param amax
     *            Max axis extents of bounding box A
     * @param bmin
     *            Min axis extents of bounding box B
     * @param bmax
     *            Max axis extents of bounding box B
     * @returns true if the two bounding boxes overlap. False otherwise
     */
        private static bool overlapBounds(float[] amin, float[] amax, float[] bmin, float[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }

        /**
     * Adds a span to the heightfield. If the new span overlaps existing spans, it will merge the new span with the
     * existing ones. The span addition can be set to favor flags. If the span is merged to another span and the new
     * spanMax is within flagMergeThreshold units from the existing span, the span flags are merged.
     *
     * @param heightfield
     *            An initialized heightfield.
     * @param x
     *            The width index where the span is to be added. [Limits: 0 <= value < Heightfield::width]
     * @param y
     *            The height index where the span is to be added. [Limits: 0 <= value < Heightfield::height]
     * @param spanMin
     *            The minimum height of the span. [Limit: < spanMax] [Units: vx]
     * @param spanMax
     *            The minimum height of the span. [Limit: <= RecastConstants.SPAN_MAX_HEIGHT] [Units: vx]
     * @param areaId
     *            The area id of the span. [Limit: <= WALKABLE_AREA)
     * @param flagMergeThreshold
     *            The merge theshold. [Limit: >= 0] [Units: vx]
     * @see Heightfield, Span.
     */
        public static void addSpan(Heightfield heightfield, int x, int y, int spanMin, int spanMax, int areaId,
            int flagMergeThreshold)
        {
            int idx = x + y * heightfield.width;

            Span s = new Span();
            s.smin = spanMin;
            s.smax = spanMax;
            s.area = areaId;
            s.next = null;

            // Empty cell, add the first span.
            if (heightfield.spans[idx] == null)
            {
                heightfield.spans[idx] = s;
                return;
            }

            Span prev = null;
            Span cur = heightfield.spans[idx];

            // Insert and merge spans.
            while (cur != null)
            {
                if (cur.smin > s.smax)
                {
                    // Current span is further than the new span, break.
                    break;
                }
                else if (cur.smax < s.smin)
                {
                    // Current span is before the new span advance.
                    prev = cur;
                    cur = cur.next;
                }
                else
                {
                    // Merge spans.
                    if (cur.smin < s.smin)
                        s.smin = cur.smin;
                    if (cur.smax > s.smax)
                        s.smax = cur.smax;

                    // Merge flags.
                    if (Math.Abs(s.smax - cur.smax) <= flagMergeThreshold)
                        s.area = Math.Max(s.area, cur.area);

                    // Remove current span.
                    Span next = cur.next;
                    if (prev != null)
                        prev.next = next;
                    else
                        heightfield.spans[idx] = next;
                    cur = next;
                }
            }

            // Insert new span.
            if (prev != null)
            {
                s.next = prev.next;
                prev.next = s;
            }
            else
            {
                s.next = heightfield.spans[idx];
                heightfield.spans[idx] = s;
            }
        }

        /**
     * Divides a convex polygon of max 12 vertices into two convex polygons across a separating axis.
     *
     * @param inVerts
     *            The input polygon vertices
     * @param inVertsOffset
     *            The offset of the first polygon vertex
     * @param inVertsCount
     *            The number of input polygon vertices
     * @param outVerts1
     *            The offset of the resulting polygon 1's vertices
     * @param outVerts2
     *            The offset of the resulting polygon 2's vertices
     * @param axisOffset
     *            The offset along the specified axis
     * @param axis
     *            The separating axis
     * @return The number of resulting polygon 1 and polygon 2 vertices
     */
        private static int[] dividePoly(float[] inVerts, int inVertsOffset, int inVertsCount, int outVerts1, int outVerts2, float axisOffset,
            int axis)
        {
            float[] d = new float[12];
            for (int i = 0; i < inVertsCount; ++i)
                d[i] = axisOffset - inVerts[inVertsOffset + i * 3 + axis];

            int m = 0, n = 0;
            for (int i = 0, j = inVertsCount - 1; i < inVertsCount; j = i, ++i)
            {
                bool ina = d[j] >= 0;
                bool inb = d[i] >= 0;
                if (ina != inb)
                {
                    float s = d[j] / (d[j] - d[i]);
                    inVerts[outVerts1 + m * 3 + 0] = inVerts[inVertsOffset + j * 3 + 0]
                                                     + (inVerts[inVertsOffset + i * 3 + 0] - inVerts[inVertsOffset + j * 3 + 0]) * s;
                    inVerts[outVerts1 + m * 3 + 1] = inVerts[inVertsOffset + j * 3 + 1]
                                                     + (inVerts[inVertsOffset + i * 3 + 1] - inVerts[inVertsOffset + j * 3 + 1]) * s;
                    inVerts[outVerts1 + m * 3 + 2] = inVerts[inVertsOffset + j * 3 + 2]
                                                     + (inVerts[inVertsOffset + i * 3 + 2] - inVerts[inVertsOffset + j * 3 + 2]) * s;
                    RecastVectors.copy(inVerts, outVerts2 + n * 3, inVerts, outVerts1 + m * 3);
                    m++;
                    n++;
                    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
                    // since these were already added above
                    if (d[i] > 0)
                    {
                        RecastVectors.copy(inVerts, outVerts1 + m * 3, inVerts, inVertsOffset + i * 3);
                        m++;
                    }
                    else if (d[i] < 0)
                    {
                        RecastVectors.copy(inVerts, outVerts2 + n * 3, inVerts, inVertsOffset + i * 3);
                        n++;
                    }
                }
                else // same side
                {
                    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
                    if (d[i] >= 0)
                    {
                        RecastVectors.copy(inVerts, outVerts1 + m * 3, inVerts, inVertsOffset + i * 3);
                        m++;
                        if (d[i] != 0)
                            continue;
                    }

                    RecastVectors.copy(inVerts, outVerts2 + n * 3, inVerts, inVertsOffset + i * 3);
                    n++;
                }
            }

            return new int[] { m, n };
        }

        /**
     * Rasterize a single triangle to the heightfield. This code is extremely hot, so much care should be given to
     * maintaining maximum perf here.
     *
     * @param verts
     *            An array with vertex coordinates [(x, y, z) * N]
     * @param v0
     *            Index of triangle vertex 0, will be multiplied by 3 to get vertex coordinates
     * @param v1
     *            Triangle vertex 1 index
     * @param v2
     *            Triangle vertex 2 index
     * @param area
     *            The area ID to assign to the rasterized spans
     * @param hf
     *            Heightfield to rasterize into
     * @param hfBBMin
     *            The min extents of the heightfield bounding box
     * @param hfBBMax
     *            The max extents of the heightfield bounding box
     * @param cellSize
     *            The x and z axis size of a voxel in the heightfield
     * @param inverseCellSize
     *            1 / cellSize
     * @param inverseCellHeight
     *            1 / cellHeight
     * @param flagMergeThreshold
     *            The threshold in which area flags will be merged
     */
        private static void rasterizeTri(float[] verts, int v0, int v1, int v2, int area, Heightfield hf, float[] hfBBMin,
            float[] hfBBMax, float cellSize, float inverseCellSize, float inverseCellHeight, int flagMergeThreshold)
        {
            float[] tmin = new float[3];
            float[] tmax = new float[3];
            float by = hfBBMax[1] - hfBBMin[1];

            // Calculate the bounding box of the triangle.
            RecastVectors.copy(tmin, verts, v0 * 3);
            RecastVectors.copy(tmax, verts, v0 * 3);
            RecastVectors.min(tmin, verts, v1 * 3);
            RecastVectors.min(tmin, verts, v2 * 3);
            RecastVectors.max(tmax, verts, v1 * 3);
            RecastVectors.max(tmax, verts, v2 * 3);

            // If the triangle does not touch the bbox of the heightfield, skip the triagle.
            if (!overlapBounds(hfBBMin, hfBBMax, tmin, tmax))
                return;

            // Calculate the footprint of the triangle on the grid's y-axis
            int z0 = (int)((tmin[2] - hfBBMin[2]) * inverseCellSize);
            int z1 = (int)((tmax[2] - hfBBMin[2]) * inverseCellSize);

            int w = hf.width;
            int h = hf.height;
            // use -1 rather than 0 to cut the polygon properly at the start of the tile
            z0 = clamp(z0, -1, h - 1);
            z1 = clamp(z1, 0, h - 1);

            // Clip the triangle into all grid cells it touches.
            float[] buf = new float[7 * 3 * 4];
            int @in = 0;
            int inRow = 7 * 3;
            int p1 = inRow + 7 * 3;
            int p2 = p1 + 7 * 3;

            RecastVectors.copy(buf, 0, verts, v0 * 3);
            RecastVectors.copy(buf, 3, verts, v1 * 3);
            RecastVectors.copy(buf, 6, verts, v2 * 3);
            int nvRow, nvIn = 3;

            for (int z = z0; z <= z1; ++z)
            {
                // Clip polygon to row. Store the remaining polygon as well
                float cellZ = hfBBMin[2] + z * cellSize;
                int[] nvrowin = dividePoly(buf, @in, nvIn, inRow, p1, cellZ + cellSize, 2);
                nvRow = nvrowin[0];
                nvIn = nvrowin[1];
                {
                    int temp = @in;
                    @in = p1;
                    p1 = temp;
                }
                if (nvRow < 3)
                    continue;

                if (z < 0)
                {
                    continue;
                }

                // find the horizontal bounds in the row
                float minX = buf[inRow], maxX = buf[inRow];
                for (int i = 1; i < nvRow; ++i)
                {
                    float v = buf[inRow + i * 3];
                    minX = Math.Min(minX, v);
                    maxX = Math.Max(maxX, v);
                }

                int x0 = (int)((minX - hfBBMin[0]) * inverseCellSize);
                int x1 = (int)((maxX - hfBBMin[0]) * inverseCellSize);
                if (x1 < 0 || x0 >= w)
                {
                    continue;
                }

                x0 = clamp(x0, -1, w - 1);
                x1 = clamp(x1, 0, w - 1);

                int nv, nv2 = nvRow;
                for (int x = x0; x <= x1; ++x)
                {
                    // Clip polygon to column. store the remaining polygon as well
                    float cx = hfBBMin[0] + x * cellSize;
                    int[] nvnv2 = dividePoly(buf, inRow, nv2, p1, p2, cx + cellSize, 0);
                    nv = nvnv2[0];
                    nv2 = nvnv2[1];
                    {
                        int temp = inRow;
                        inRow = p2;
                        p2 = temp;
                    }
                    if (nv < 3)
                        continue;
                    if (x < 0)
                    {
                        continue;
                    }

                    // Calculate min and max of the span.
                    float spanMin = buf[p1 + 1];
                    float spanMax = buf[p1 + 1];
                    for (int i = 1; i < nv; ++i)
                    {
                        spanMin = Math.Min(spanMin, buf[p1 + i * 3 + 1]);
                        spanMax = Math.Max(spanMax, buf[p1 + i * 3 + 1]);
                    }

                    spanMin -= hfBBMin[1];
                    spanMax -= hfBBMin[1];
                    // Skip the span if it is outside the heightfield bbox
                    if (spanMax < 0.0f)
                        continue;
                    if (spanMin > by)
                        continue;
                    // Clamp the span to the heightfield bbox.
                    if (spanMin < 0.0f)
                        spanMin = 0;
                    if (spanMax > by)
                        spanMax = by;

                    // Snap the span to the heightfield height grid.
                    int spanMinCellIndex = clamp((int)Math.Floor(spanMin * inverseCellHeight), 0, SPAN_MAX_HEIGHT);
                    int spanMaxCellIndex = clamp((int)Math.Ceiling(spanMax * inverseCellHeight), spanMinCellIndex + 1, SPAN_MAX_HEIGHT);

                    addSpan(hf, x, z, spanMinCellIndex, spanMaxCellIndex, area, flagMergeThreshold);
                }
            }
        }

        /**
     * Rasterizes a single triangle into the specified heightfield. Calling this for each triangle in a mesh is less
     * efficient than calling rasterizeTriangles. No spans will be added if the triangle does not overlap the
     * heightfield grid.
     *
     * @param heightfield
     *            An initialized heightfield.
     * @param verts
     *            An array with vertex coordinates [(x, y, z) * N]
     * @param v0
     *            Index of triangle vertex 0, will be multiplied by 3 to get vertex coordinates
     * @param v1
     *            Triangle vertex 1 index
     * @param v2
     *            Triangle vertex 2 index
     * @param areaId
     *            The area id of the triangle. [Limit: <= WALKABLE_AREA)
     * @param flagMergeThreshold
     *            The distance where the walkable flag is favored over the non-walkable flag. [Limit: >= 0] [Units: vx]
     * @see Heightfield
     */
        public static void rasterizeTriangle(Heightfield heightfield, float[] verts, int v0, int v1, int v2, int area,
            int flagMergeThreshold, Telemetry ctx)
        {
            ctx.startTimer("RASTERIZE_TRIANGLES");

            float inverseCellSize = 1.0f / heightfield.cs;
            float inverseCellHeight = 1.0f / heightfield.ch;
            rasterizeTri(verts, v0, v1, v2, area, heightfield, heightfield.bmin, heightfield.bmax, heightfield.cs, inverseCellSize,
                inverseCellHeight, flagMergeThreshold);

            ctx.stopTimer("RASTERIZE_TRIANGLES");
        }

        /**
     * Rasterizes an indexed triangle mesh into the specified heightfield. Spans will only be added for triangles that
     * overlap the heightfield grid.
     *
     * @param heightfield
     *            An initialized heightfield.
     * @param verts
     *            The vertices. [(x, y, z) * N]
     * @param tris
     *            The triangle indices. [(vertA, vertB, vertC) * nt]
     * @param areaIds
     *            The area id's of the triangles. [Limit: <= WALKABLE_AREA] [Size: numTris]
     * @param numTris
     *            The number of triangles.
     * @param flagMergeThreshold
     *            The distance where the walkable flag is favored over the non-walkable flag. [Limit: >= 0] [Units: vx]
     * @see Heightfield
     */
        public static void rasterizeTriangles(Heightfield heightfield, float[] verts, int[] tris, int[] areaIds, int numTris,
            int flagMergeThreshold, Telemetry ctx)
        {
            ctx.startTimer("RASTERIZE_TRIANGLES");

            float inverseCellSize = 1.0f / heightfield.cs;
            float inverseCellHeight = 1.0f / heightfield.ch;
            for (int triIndex = 0; triIndex < numTris; ++triIndex)
            {
                int v0 = tris[triIndex * 3 + 0];
                int v1 = tris[triIndex * 3 + 1];
                int v2 = tris[triIndex * 3 + 2];
                rasterizeTri(verts, v0, v1, v2, areaIds[triIndex], heightfield, heightfield.bmin, heightfield.bmax, heightfield.cs,
                    inverseCellSize, inverseCellHeight, flagMergeThreshold);
            }

            ctx.stopTimer("RASTERIZE_TRIANGLES");
        }

        /**
     * Rasterizes a triangle list into the specified heightfield. Expects each triangle to be specified as three
     * sequential vertices of 3 floats. Spans will only be added for triangles that overlap the heightfield grid.
     *
     * @param heightfield
     *            An initialized heightfield.
     * @param verts
     *            The vertices. [(x, y, z) * numVerts]
     * @param areaIds
     *            The area id's of the triangles. [Limit: <= WALKABLE_AREA] [Size: numTris]
     * @param tris
     *            The triangle indices. [(vertA, vertB, vertC) * nt]
     * @param numTris
     *            The number of triangles.
     * @param flagMergeThreshold
     *            The distance where the walkable flag is favored over the non-walkable flag. [Limit: >= 0] [Units: vx]
     * @see Heightfield
     */
        public static void rasterizeTriangles(Heightfield heightfield, float[] verts, int[] areaIds, int numTris,
            int flagMergeThreshold, Telemetry ctx)
        {
            ctx.startTimer("RASTERIZE_TRIANGLES");

            float inverseCellSize = 1.0f / heightfield.cs;
            float inverseCellHeight = 1.0f / heightfield.ch;
            for (int triIndex = 0; triIndex < numTris; ++triIndex)
            {
                int v0 = (triIndex * 3 + 0);
                int v1 = (triIndex * 3 + 1);
                int v2 = (triIndex * 3 + 2);
                rasterizeTri(verts, v0, v1, v2, areaIds[triIndex], heightfield, heightfield.bmin, heightfield.bmax, heightfield.cs,
                    inverseCellSize, inverseCellHeight, flagMergeThreshold);
            }

            ctx.stopTimer("RASTERIZE_TRIANGLES");
        }
    }
}