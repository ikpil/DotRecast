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
using DotRecast.Core;
using DotRecast.Core.Numerics;
using static DotRecast.Recast.RcConstants;

namespace DotRecast.Recast
{
    public static class RcRasterizations
    {
        /// Check whether two bounding boxes overlap
        ///
        /// @param[in]	aMin	Min axis extents of bounding box A
        /// @param[in]	aMax	Max axis extents of bounding box A
        /// @param[in]	bMin	Min axis extents of bounding box B
        /// @param[in]	bMax	Max axis extents of bounding box B
        /// @returns true if the two bounding boxes overlap.  False otherwise.
        private static bool OverlapBounds(RcVec3f aMin, RcVec3f aMax, RcVec3f bMin, RcVec3f bMax)
        {
            return
                aMin.X <= bMax.X && aMax.X >= bMin.X &&
                aMin.Y <= bMax.Y && aMax.Y >= bMin.Y &&
                aMin.Z <= bMax.Z && aMax.Z >= bMin.Z;
        }


        /// Adds a span to the heightfield.  If the new span overlaps existing spans,
        /// it will merge the new span with the existing ones.
        ///
        /// @param[in]	heightfield					Heightfield to add spans to
        /// @param[in]	x					The new span's column cell x index
        /// @param[in]	z					The new span's column cell z index
        /// @param[in]	min					The new span's minimum cell index
        /// @param[in]	max					The new span's maximum cell index
        /// @param[in]	areaID				The new span's area type ID
        /// @param[in]	flagMergeThreshold	How close two spans maximum extents need to be to merge area type IDs
        public static void AddSpan(RcHeightfield heightfield, int x, int z, int min, int max, int areaID, int flagMergeThreshold)
        {
            // Create the new span.
            RcSpan newSpan = new RcSpan();
            newSpan.smin = min;
            newSpan.smax = max;
            newSpan.area = areaID;
            newSpan.next = null;

            int columnIndex = x + z * heightfield.width;

            // Empty cell, add the first span.
            if (heightfield.spans[columnIndex] == null)
            {
                heightfield.spans[columnIndex] = newSpan;
                return;
            }

            RcSpan previousSpan = null;
            RcSpan currentSpan = heightfield.spans[columnIndex];

            // Insert the new span, possibly merging it with existing spans.
            while (currentSpan != null)
            {
                if (currentSpan.smin > newSpan.smax)
                {
                    // Current span is further than the new span, break.
                    break;
                }

                if (currentSpan.smax < newSpan.smin)
                {
                    // Current span is completely before the new span.  Keep going.
                    previousSpan = currentSpan;
                    currentSpan = currentSpan.next;
                }
                else
                {
                    // The new span overlaps with an existing span.  Merge them.
                    if (currentSpan.smin < newSpan.smin)
                    {
                        newSpan.smin = currentSpan.smin;
                    }

                    if (currentSpan.smax > newSpan.smax)
                    {
                        newSpan.smax = currentSpan.smax;
                    }

                    // Merge flags.
                    if (MathF.Abs(newSpan.smax - currentSpan.smax) <= flagMergeThreshold)
                    {
                        // Higher area ID numbers indicate higher resolution priority.
                        newSpan.area = Math.Max(newSpan.area, currentSpan.area);
                    }

                    // Remove the current span since it's now merged with newSpan.
                    // Keep going because there might be other overlapping spans that also need to be merged.
                    RcSpan next = currentSpan.next;
                    if (previousSpan != null)
                    {
                        previousSpan.next = next;
                    }
                    else
                    {
                        heightfield.spans[columnIndex] = next;
                    }

                    currentSpan = next;
                }
            }

            // Insert new span after prev
            if (previousSpan != null)
            {
                newSpan.next = previousSpan.next;
                previousSpan.next = newSpan;
            }
            else
            {
                // This span should go before the others in the list
                newSpan.next = heightfield.spans[columnIndex];
                heightfield.spans[columnIndex] = newSpan;
            }
        }

        /// Divides a convex polygon of max 12 vertices into two convex polygons
        /// across a separating axis.
        /// 
        /// @param[in]	inVerts			The input polygon vertices
        /// @param[in]	inVertsCount	The number of input polygon vertices
        /// @param[out]	outVerts1		Resulting polygon 1's vertices
        /// @param[out]	outVerts1Count	The number of resulting polygon 1 vertices
        /// @param[out]	outVerts2		Resulting polygon 2's vertices
        /// @param[out]	outVerts2Count	The number of resulting polygon 2 vertices
        /// @param[in]	axisOffset		THe offset along the specified axis
        /// @param[in]	axis			The separating axis
        private static void DividePoly(float[] inVerts, int inVertsOffset, int inVertsCount,
            int outVerts1, out int outVerts1Count,
            int outVerts2, out int outVerts2Count,
            float axisOffset, int axis)
        {
            float[] d = new float[12];

            // How far positive or negative away from the separating axis is each vertex.
            for (int inVert = 0; inVert < inVertsCount; ++inVert)
            {
                d[inVert] = axisOffset - inVerts[inVertsOffset + inVert * 3 + axis];
            }

            int poly1Vert = 0;
            int poly2Vert = 0;
            for (int inVertA = 0, inVertB = inVertsCount - 1; inVertA < inVertsCount; inVertB = inVertA, ++inVertA)
            {
                bool ina = d[inVertB] >= 0;
                bool inb = d[inVertA] >= 0;
                if (ina != inb)
                {
                    float s = d[inVertB] / (d[inVertB] - d[inVertA]);
                    inVerts[outVerts1 + poly1Vert * 3 + 0] = inVerts[inVertsOffset + inVertB * 3 + 0] +
                                                             (inVerts[inVertsOffset + inVertA * 3 + 0] - inVerts[inVertsOffset + inVertB * 3 + 0]) * s;
                    inVerts[outVerts1 + poly1Vert * 3 + 1] = inVerts[inVertsOffset + inVertB * 3 + 1] +
                                                             (inVerts[inVertsOffset + inVertA * 3 + 1] - inVerts[inVertsOffset + inVertB * 3 + 1]) * s;
                    inVerts[outVerts1 + poly1Vert * 3 + 2] = inVerts[inVertsOffset + inVertB * 3 + 2] +
                                                             (inVerts[inVertsOffset + inVertA * 3 + 2] - inVerts[inVertsOffset + inVertB * 3 + 2]) * s;
                    RcVecUtils.Copy(inVerts, outVerts2 + poly2Vert * 3, inVerts, outVerts1 + poly1Vert * 3);
                    poly1Vert++;
                    poly2Vert++;
                    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
                    // since these were already added above
                    if (d[inVertA] > 0)
                    {
                        RcVecUtils.Copy(inVerts, outVerts1 + poly1Vert * 3, inVerts, inVertsOffset + inVertA * 3);
                        poly1Vert++;
                    }
                    else if (d[inVertA] < 0)
                    {
                        RcVecUtils.Copy(inVerts, outVerts2 + poly2Vert * 3, inVerts, inVertsOffset + inVertA * 3);
                        poly2Vert++;
                    }
                }
                else // same side
                {
                    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
                    if (d[inVertA] >= 0)
                    {
                        RcVecUtils.Copy(inVerts, outVerts1 + poly1Vert * 3, inVerts, inVertsOffset + inVertA * 3);
                        poly1Vert++;
                        if (d[inVertA] != 0)
                            continue;
                    }

                    RcVecUtils.Copy(inVerts, outVerts2 + poly2Vert * 3, inVerts, inVertsOffset + inVertA * 3);
                    poly2Vert++;
                }
            }

            outVerts1Count = poly1Vert;
            outVerts2Count = poly2Vert;
        }

        ///	Rasterize a single triangle to the heightfield.
        ///
        ///	This code is extremely hot, so much care should be given to maintaining maximum perf here.
        /// 
        /// @param[in] 	v0					Triangle vertex 0
        /// @param[in] 	v1					Triangle vertex 1
        /// @param[in] 	v2					Triangle vertex 2
        /// @param[in] 	areaID				The area ID to assign to the rasterized spans
        /// @param[in] 	heightfield			Heightfield to rasterize into
        /// @param[in] 	heightfieldBBMin	The min extents of the heightfield bounding box
        /// @param[in] 	heightfieldBBMax	The max extents of the heightfield bounding box
        /// @param[in] 	cellSize			The x and z axis size of a voxel in the heightfield
        /// @param[in] 	inverseCellSize		1 / cellSize
        /// @param[in] 	inverseCellHeight	1 / cellHeight
        /// @param[in] 	flagMergeThreshold	The threshold in which area flags will be merged 
        /// @returns true if the operation completes successfully.  false if there was an error adding spans to the heightfield.
        private static void RasterizeTri(float[] verts, int v0, int v1, int v2, int area, RcHeightfield heightfield,
            RcVec3f heightfieldBBMin, RcVec3f heightfieldBBMax,
            float cellSize, float inverseCellSize, float inverseCellHeight,
            int flagMergeThreshold)
        {
            float by = heightfieldBBMax.Y - heightfieldBBMin.Y;

            // Calculate the bounding box of the triangle.
            RcVec3f tmin = RcVecUtils.Create(verts, v0 * 3);
            RcVec3f tmax = RcVecUtils.Create(verts, v0 * 3);
            tmin = RcVecUtils.Min(tmin, verts, v1 * 3);
            tmin = RcVecUtils.Min(tmin, verts, v2 * 3);
            tmax = RcVecUtils.Max(tmax, verts, v1 * 3);
            tmax = RcVecUtils.Max(tmax, verts, v2 * 3);

            // If the triangle does not touch the bbox of the heightfield, skip the triagle.
            if (!OverlapBounds(heightfieldBBMin, heightfieldBBMax, tmin, tmax))
                return;

            // Calculate the footprint of the triangle on the grid's y-axis
            int z0 = (int)((tmin.Z - heightfieldBBMin.Z) * inverseCellSize);
            int z1 = (int)((tmax.Z - heightfieldBBMin.Z) * inverseCellSize);

            int w = heightfield.width;
            int h = heightfield.height;
            // use -1 rather than 0 to cut the polygon properly at the start of the tile
            z0 = Math.Clamp(z0, -1, h - 1);
            z1 = Math.Clamp(z1, 0, h - 1);

            // Clip the triangle into all grid cells it touches.
            float[] buf = new float[7 * 3 * 4];
            int @in = 0;
            int inRow = 7 * 3;
            int p1 = inRow + 7 * 3;
            int p2 = p1 + 7 * 3;

            RcVecUtils.Copy(buf, 0, verts, v0 * 3);
            RcVecUtils.Copy(buf, 3, verts, v1 * 3);
            RcVecUtils.Copy(buf, 6, verts, v2 * 3);
            int nvRow, nvIn = 3;

            for (int z = z0; z <= z1; ++z)
            {
                // Clip polygon to row. Store the remaining polygon as well
                float cellZ = heightfieldBBMin.Z + z * cellSize;
                DividePoly(buf, @in, nvIn, inRow, out nvRow, p1, out nvIn, cellZ + cellSize, 2);
                (@in, p1) = (p1, @in);

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

                int x0 = (int)((minX - heightfieldBBMin.X) * inverseCellSize);
                int x1 = (int)((maxX - heightfieldBBMin.X) * inverseCellSize);
                if (x1 < 0 || x0 >= w)
                {
                    continue;
                }

                x0 = Math.Clamp(x0, -1, w - 1);
                x1 = Math.Clamp(x1, 0, w - 1);

                int nv, nv2 = nvRow;
                for (int x = x0; x <= x1; ++x)
                {
                    // Clip polygon to column. store the remaining polygon as well
                    float cx = heightfieldBBMin.X + x * cellSize;
                    DividePoly(buf, inRow, nv2, p1, out nv, p2, out nv2, cx + cellSize, 0);
                    (inRow, p2) = (p2, inRow);

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

                    spanMin -= heightfieldBBMin.Y;
                    spanMax -= heightfieldBBMin.Y;
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
                    int spanMinCellIndex = Math.Clamp((int)MathF.Floor(spanMin * inverseCellHeight), 0, RC_SPAN_MAX_HEIGHT);
                    int spanMaxCellIndex = Math.Clamp((int)MathF.Ceiling(spanMax * inverseCellHeight), spanMinCellIndex + 1, RC_SPAN_MAX_HEIGHT);

                    AddSpan(heightfield, x, z, spanMinCellIndex, spanMaxCellIndex, area, flagMergeThreshold);
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
        public static void RasterizeTriangle(RcHeightfield heightfield, float[] verts, int v0, int v1, int v2, int area,
            int flagMergeThreshold, RcTelemetry ctx)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);

            float inverseCellSize = 1.0f / heightfield.cs;
            float inverseCellHeight = 1.0f / heightfield.ch;
            RasterizeTri(verts, v0, v1, v2, area, heightfield, heightfield.bmin, heightfield.bmax, heightfield.cs, inverseCellSize,
                inverseCellHeight, flagMergeThreshold);
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
        public static void RasterizeTriangles(RcHeightfield heightfield, float[] verts, int[] tris, int[] areaIds, int numTris,
            int flagMergeThreshold, RcTelemetry ctx)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);

            float inverseCellSize = 1.0f / heightfield.cs;
            float inverseCellHeight = 1.0f / heightfield.ch;
            for (int triIndex = 0; triIndex < numTris; ++triIndex)
            {
                int v0 = tris[triIndex * 3 + 0];
                int v1 = tris[triIndex * 3 + 1];
                int v2 = tris[triIndex * 3 + 2];
                RasterizeTri(verts, v0, v1, v2, areaIds[triIndex], heightfield, heightfield.bmin, heightfield.bmax, heightfield.cs,
                    inverseCellSize, inverseCellHeight, flagMergeThreshold);
            }
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
        public static void RasterizeTriangles(RcHeightfield heightfield, float[] verts, int[] areaIds, int numTris,
            int flagMergeThreshold, RcTelemetry ctx)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_RASTERIZE_TRIANGLES);

            float inverseCellSize = 1.0f / heightfield.cs;
            float inverseCellHeight = 1.0f / heightfield.ch;
            for (int triIndex = 0; triIndex < numTris; ++triIndex)
            {
                int v0 = (triIndex * 3 + 0);
                int v1 = (triIndex * 3 + 1);
                int v2 = (triIndex * 3 + 2);
                RasterizeTri(verts, v0, v1, v2, areaIds[triIndex], heightfield, heightfield.bmin, heightfield.bmax, heightfield.cs,
                    inverseCellSize, inverseCellHeight, flagMergeThreshold);
            }
        }
    }
}