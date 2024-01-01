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

namespace DotRecast.Recast
{
    using static RcConstants;
    using static RcCommons;

    public static class RcFilters
    {
        /// @par
        ///
        /// Allows the formation of walkable regions that will flow over low lying
        /// objects such as curbs, and up structures such as stairways.
        ///
        /// Two neighboring spans are walkable if: <tt>RcAbs(currentSpan.smax - neighborSpan.smax) < walkableClimb</tt>
        ///
        /// @warning Will override the effect of #rcFilterLedgeSpans. So if both filters are used, call
        /// #rcFilterLedgeSpans after calling this filter.
        ///
        /// @see rcHeightfield, rcConfig
        public static void FilterLowHangingWalkableObstacles(RcTelemetry ctx, int walkableClimb, RcHeightfield solid)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_FILTER_LOW_OBSTACLES);

            int w = solid.width;
            int h = solid.height;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcSpan ps = null;
                    bool previousWalkable = false;
                    int previousArea = RC_NULL_AREA;

                    for (RcSpan s = solid.spans[x + y * w]; s != null; ps = s, s = s.next)
                    {
                        bool walkable = s.area != RC_NULL_AREA;
                        // If current span is not walkable, but there is walkable
                        // span just below it, mark the span above it walkable too.
                        if (!walkable && previousWalkable)
                        {
                            if (MathF.Abs(s.smax - ps.smax) <= walkableClimb)
                                s.area = previousArea;
                        }

                        // Copy walkable flag so that it cannot propagate
                        // past multiple non-walkable objects.
                        previousWalkable = walkable;
                        previousArea = s.area;
                    }
                }
            }
        }

        /// @par
        ///
        /// A ledge is a span with one or more neighbors whose maximum is further away than @p walkableClimb
        /// from the current span's maximum.
        /// This method removes the impact of the overestimation of conservative voxelization
        /// so the resulting mesh will not have regions hanging in the air over ledges.
        ///
        /// A span is a ledge if: <tt>RcAbs(currentSpan.smax - neighborSpan.smax) > walkableClimb</tt>
        ///
        /// @see rcHeightfield, rcConfig
        public static void FilterLedgeSpans(RcTelemetry ctx, int walkableHeight, int walkableClimb, RcHeightfield heightfield)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_FILTER_BORDER);

            int xSize = heightfield.width;
            int zSize = heightfield.height;

            // Mark border spans.
            for (int z = 0; z < zSize; ++z)
            {
                for (int x = 0; x < xSize; ++x)
                {
                    for (RcSpan span = heightfield.spans[x + z * xSize]; span != null; span = span.next)
                    {
                        // Skip non walkable spans.
                        if (span.area == RC_NULL_AREA)
                        {
                            continue;
                        }

                        int bot = (span.smax);
                        int top = span.next != null ? span.next.smin : SPAN_MAX_HEIGHT;

                        // Find neighbours minimum height.
                        int minNeighborHeight = SPAN_MAX_HEIGHT;

                        // Min and max height of accessible neighbours.
                        int accessibleNeighborMinHeight = span.smax;
                        int accessibleNeighborMaxHeight = span.smax;

                        for (int direction = 0; direction < 4; ++direction)
                        {
                            int dx = x + GetDirOffsetX(direction);
                            int dz = z + GetDirOffsetY(direction);
                            // Skip neighbours which are out of bounds.
                            if (dx < 0 || dz < 0 || dx >= xSize || dz >= zSize)
                            {
                                minNeighborHeight = (-walkableClimb - 1);
                                break;
                            }

                            // From minus infinity to the first span.
                            RcSpan neighborSpan = heightfield.spans[dx + dz * xSize];
                            int neighborTop = neighborSpan != null ? neighborSpan.smin : SPAN_MAX_HEIGHT;
                            
                            // Skip neightbour if the gap between the spans is too small.
                            if (Math.Min(top, neighborTop) - bot >= walkableHeight)
                            {
                                minNeighborHeight = (-walkableClimb - 1);
                                break;
                            }

                            // Rest of the spans.
                            for (neighborSpan = heightfield.spans[dx + dz * xSize]; neighborSpan != null; neighborSpan = neighborSpan.next)
                            {
                                int neighborBot = neighborSpan.smax;
                                neighborTop = neighborSpan.next != null ? neighborSpan.next.smin : SPAN_MAX_HEIGHT;
                                
                                // Skip neightbour if the gap between the spans is too small.
                                if (Math.Min(top, neighborTop) - Math.Max(bot, neighborBot) >= walkableHeight)
                                {
                                    int accessibleNeighbourHeight = neighborBot - bot;
                                    minNeighborHeight = Math.Min(minNeighborHeight, accessibleNeighbourHeight);

                                    // Find min/max accessible neighbour height. 
                                    if (MathF.Abs(accessibleNeighbourHeight) <= walkableClimb)
                                    {
                                        if (neighborBot < accessibleNeighborMinHeight) accessibleNeighborMinHeight = neighborBot;
                                        if (neighborBot > accessibleNeighborMaxHeight) accessibleNeighborMaxHeight = neighborBot;
                                    }
                                    else if (accessibleNeighbourHeight < -walkableClimb)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        // The current span is close to a ledge if the drop to any
                        // neighbour span is less than the walkableClimb.
                        if (minNeighborHeight < -walkableClimb)
                        {
                            span.area = RC_NULL_AREA;
                        }

                        // If the difference between all neighbours is too large,
                        // we are at steep slope, mark the span as ledge.
                        if ((accessibleNeighborMaxHeight - accessibleNeighborMinHeight) > walkableClimb)
                        {
                            span.area = RC_NULL_AREA;
                        }
                    }
                }
            }
        }

        /// @par
        ///
        /// For this filter, the clearance above the span is the distance from the span's
        /// maximum to the next higher span's minimum. (Same grid column.)
        ///
        /// @see rcHeightfield, rcConfig
        public static void FilterWalkableLowHeightSpans(RcTelemetry ctx, int walkableHeight, RcHeightfield solid)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_FILTER_WALKABLE);

            int w = solid.width;
            int h = solid.height;

            // Remove walkable flag from spans which do not have enough
            // space above them for the agent to stand there.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (RcSpan s = solid.spans[x + y * w]; s != null; s = s.next)
                    {
                        int bot = (s.smax);
                        int top = s.next != null ? s.next.smin : SPAN_MAX_HEIGHT;
                        if ((top - bot) < walkableHeight)
                            s.area = RC_NULL_AREA;
                    }
                }
            }
        }
    }
}