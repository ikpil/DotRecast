/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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
using System.Linq;
using DotRecast.Core;
using static DotRecast.Recast.RcConstants;


namespace DotRecast.Recast
{
    using static RcCommons;


    public static class RcCompacts
    {
        private const int MAX_LAYERS = RC_NOT_CONNECTED - 1;
        private const int MAX_HEIGHT = RcConstants.RC_SPAN_MAX_HEIGHT;

        /// @}
        /// @name Compact Heightfield Functions
        /// @see rcCompactHeightfield
        /// @{
        /// Builds a compact heightfield representing open space, from a heightfield representing solid space.
        ///
        /// This is just the beginning of the process of fully building a compact heightfield.
        /// Various filters may be applied, then the distance field and regions built.
        /// E.g: #rcBuildDistanceField and #rcBuildRegions
        ///
        /// See the #rcConfig documentation for more information on the configuration parameters.
        ///
        /// @see rcAllocCompactHeightfield, rcHeightfield, rcCompactHeightfield, rcConfig
        /// @ingroup recast
        /// 
        /// @param[in,out]	context				The build context to use during the operation.
        /// @param[in]		walkableHeight		Minimum floor to 'ceiling' height that will still allow the floor area 
        /// 									to be considered walkable. [Limit: >= 3] [Units: vx]
        /// @param[in]		walkableClimb		Maximum ledge height that is considered to still be traversable. 
        /// 									[Limit: >=0] [Units: vx]
        /// @param[in]		heightfield			The heightfield to be compacted.
        /// @param[out]		compactHeightfield	The resulting compact heightfield. (Must be pre-allocated.)
        /// @returns True if the operation completed successfully.
        public static RcCompactHeightfield BuildCompactHeightfield(RcContext context, int walkableHeight, int walkableClimb, RcHeightfield heightfield)
        {
            using var timer = context.ScopedTimer(RcTimerLabel.RC_TIMER_BUILD_COMPACTHEIGHTFIELD);

            int xSize = heightfield.width;
            int zSize = heightfield.height;
            int spanCount = GetHeightFieldSpanCount(context, heightfield);

            // Fill in header.
            RcCompactHeightfield compactHeightfield = new RcCompactHeightfield();
            compactHeightfield.width = xSize;
            compactHeightfield.height = zSize;
            compactHeightfield.borderSize = heightfield.borderSize;
            compactHeightfield.spanCount = spanCount;
            compactHeightfield.walkableHeight = walkableHeight;
            compactHeightfield.walkableClimb = walkableClimb;
            compactHeightfield.maxRegions = 0;
            compactHeightfield.bmin = heightfield.bmin;
            compactHeightfield.bmax = heightfield.bmax;
            compactHeightfield.bmax.Y += walkableHeight * heightfield.ch;
            compactHeightfield.cs = heightfield.cs;
            compactHeightfield.ch = heightfield.ch;
            compactHeightfield.cells = new RcCompactCell[xSize * zSize];
            //chf.spans = new RcCompactSpan[spanCount];
            compactHeightfield.areas = new int[spanCount];

            var tempSpans = Enumerable
                .Range(0, spanCount)
                .Select(x => RcCompactSpanBuilder.NewBuilder())
                .ToArray();

            // Fill in cells and spans.
            int idx = 0;
            for (int y = 0; y < zSize; ++y)
            {
                for (int x = 0; x < xSize; ++x)
                {
                    RcSpan s = heightfield.spans[x + y * xSize];
                    // If there are no spans at this cell, just leave the data to index=0, count=0.
                    if (s == null)
                        continue;

                    int tmpIdx = idx;
                    int tmpCount = 0;
                    while (s != null)
                    {
                        if (s.area != RC_NULL_AREA)
                        {
                            int bot = s.smax;
                            int top = s.next != null ? (int)s.next.smin : MAX_HEIGHT;
                            tempSpans[idx].y = Math.Clamp(bot, 0, MAX_HEIGHT);
                            tempSpans[idx].h = Math.Clamp(top - bot, 0, MAX_HEIGHT);
                            compactHeightfield.areas[idx] = s.area;
                            idx++;
                            tmpCount++;
                        }

                        s = s.next;
                    }

                    compactHeightfield.cells[x + y * xSize] = new RcCompactCell(tmpIdx, tmpCount);
                }
            }

            // Find neighbour connections.
            int tooHighNeighbour = 0;
            for (int y = 0; y < zSize; ++y)
            {
                for (int x = 0; x < xSize; ++x)
                {
                    ref RcCompactCell c = ref compactHeightfield.cells[x + y * xSize];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        ref RcCompactSpanBuilder s = ref tempSpans[i];

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            SetCon(s, dir, RC_NOT_CONNECTED);
                            int nx = x + GetDirOffsetX(dir);
                            int ny = y + GetDirOffsetY(dir);
                            // First check that the neighbour cell is in bounds.
                            if (nx < 0 || ny < 0 || nx >= xSize || ny >= zSize)
                                continue;

                            // Iterate over all neighbour spans and check if any of the is
                            // accessible from current cell.
                            ref RcCompactCell nc = ref compactHeightfield.cells[nx + ny * xSize];
                            for (int k = nc.index, nk = nc.index + nc.count; k < nk; ++k)
                            {
                                ref RcCompactSpanBuilder ns = ref tempSpans[k];
                                int bot = Math.Max(s.y, ns.y);
                                int top = Math.Min(s.y + s.h, ns.y + ns.h);

                                // Check that the gap between the spans is walkable,
                                // and that the climb height between the gaps is not too high.
                                if ((top - bot) >= walkableHeight && MathF.Abs(ns.y - s.y) <= walkableClimb)
                                {
                                    // Mark direction as walkable.
                                    int lidx = k - nc.index;
                                    if (lidx < 0 || lidx > MAX_LAYERS)
                                    {
                                        tooHighNeighbour = Math.Max(tooHighNeighbour, lidx);
                                        continue;
                                    }

                                    SetCon(s, dir, lidx);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (tooHighNeighbour > MAX_LAYERS)
            {
                throw new Exception("rcBuildCompactHeightfield: Heightfield has too many layers " + tooHighNeighbour
                                                                                                  + " (max: " + MAX_LAYERS + ")");
            }

            compactHeightfield.spans = tempSpans.Select(x => x.Build()).ToArray();

            return compactHeightfield;
        }

        /// Returns the number of spans contained in the specified heightfield.
        ///  @ingroup recast
        ///  @param[in,out]	context		The build context to use during the operation.
        ///  @param[in]		heightfield	An initialized heightfield.
        ///  @returns The number of spans in the heightfield.
        private static int GetHeightFieldSpanCount(RcContext context, RcHeightfield heightfield)
        {
            int numCols = heightfield.width * heightfield.height;
            int spanCount = 0;
            for (int columnIndex = 0; columnIndex < numCols; ++columnIndex)
            {
                for (RcSpan span = heightfield.spans[columnIndex]; span != null; span = span.next)
                {
                    if (span.area != RC_NULL_AREA)
                    {
                        spanCount++;
                    }
                }
            }

            return spanCount;
        }
    }
}