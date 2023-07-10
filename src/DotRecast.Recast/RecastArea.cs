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

    public static class RecastArea
    {
        /// @par
        ///
        /// Basically, any spans that are closer to a boundary or obstruction than the specified radius
        /// are marked as unwalkable.
        ///
        /// This method is usually called immediately after the heightfield has been built.
        ///
        /// @see rcCompactHeightfield, rcBuildCompactHeightfield, rcConfig::walkableRadius
        public static void ErodeWalkableArea(RcTelemetry ctx, int radius, RcCompactHeightfield chf)
        {
            int w = chf.width;
            int h = chf.height;
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_ERODE_AREA);

            int[] dist = new int[chf.spanCount];
            Array.Fill(dist, 255);
            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        if (chf.areas[i] == RC_NULL_AREA)
                        {
                            dist[i] = 0;
                        }
                        else
                        {
                            RcCompactSpan s = chf.spans[i];
                            int nc = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                                {
                                    int nx = x + RecastCommon.GetDirOffsetX(dir);
                                    int ny = y + RecastCommon.GetDirOffsetY(dir);
                                    int nidx = chf.cells[nx + ny * w].index + RecastCommon.GetCon(s, dir);
                                    if (chf.areas[nidx] != RC_NULL_AREA)
                                    {
                                        nc++;
                                    }
                                }
                            }

                            // At least one missing neighbour.
                            if (nc != 4)
                                dist[i] = 0;
                        }
                    }
                }
            }

            int nd;

            // Pass 1
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];

                        if (RecastCommon.GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int ax = x + RecastCommon.GetDirOffsetX(0);
                            int ay = y + RecastCommon.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 0);
                            RcCompactSpan @as = chf.spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                                dist[i] = nd;

                            // (-1,-1)
                            if (RecastCommon.GetCon(@as, 3) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastCommon.GetDirOffsetX(3);
                                int aay = ay + RecastCommon.GetDirOffsetY(3);
                                int aai = chf.cells[aax + aay * w].index + RecastCommon.GetCon(@as, 3);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                    dist[i] = nd;
                            }
                        }

                        if (RecastCommon.GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int ax = x + RecastCommon.GetDirOffsetX(3);
                            int ay = y + RecastCommon.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 3);
                            RcCompactSpan @as = chf.spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                                dist[i] = nd;

                            // (1,-1)
                            if (RecastCommon.GetCon(@as, 2) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastCommon.GetDirOffsetX(2);
                                int aay = ay + RecastCommon.GetDirOffsetY(2);
                                int aai = chf.cells[aax + aay * w].index + RecastCommon.GetCon(@as, 2);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                    dist[i] = nd;
                            }
                        }
                    }
                }
            }

            // Pass 2
            for (int y = h - 1; y >= 0; --y)
            {
                for (int x = w - 1; x >= 0; --x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];

                        if (RecastCommon.GetCon(s, 2) != RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int ax = x + RecastCommon.GetDirOffsetX(2);
                            int ay = y + RecastCommon.GetDirOffsetY(2);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 2);
                            RcCompactSpan @as = chf.spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                                dist[i] = nd;

                            // (1,1)
                            if (RecastCommon.GetCon(@as, 1) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastCommon.GetDirOffsetX(1);
                                int aay = ay + RecastCommon.GetDirOffsetY(1);
                                int aai = chf.cells[aax + aay * w].index + RecastCommon.GetCon(@as, 1);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                    dist[i] = nd;
                            }
                        }

                        if (RecastCommon.GetCon(s, 1) != RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int ax = x + RecastCommon.GetDirOffsetX(1);
                            int ay = y + RecastCommon.GetDirOffsetY(1);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 1);
                            RcCompactSpan @as = chf.spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                                dist[i] = nd;

                            // (-1,1)
                            if (RecastCommon.GetCon(@as, 0) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastCommon.GetDirOffsetX(0);
                                int aay = ay + RecastCommon.GetDirOffsetY(0);
                                int aai = chf.cells[aax + aay * w].index + RecastCommon.GetCon(@as, 0);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                    dist[i] = nd;
                            }
                        }
                    }
                }
            }

            int thr = radius * 2;
            for (int i = 0; i < chf.spanCount; ++i)
                if (dist[i] < thr)
                    chf.areas[i] = RC_NULL_AREA;
        }

        /// @par
        ///
        /// This filter is usually applied after applying area id's using functions
        /// such as #rcMarkBoxArea, #rcMarkConvexPolyArea, and #rcMarkCylinderArea.
        ///
        /// @see rcCompactHeightfield
        public static bool MedianFilterWalkableArea(RcTelemetry ctx, RcCompactHeightfield chf)
        {
            int w = chf.width;
            int h = chf.height;

            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_MEDIAN_AREA);

            int[] areas = new int[chf.spanCount];

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        if (chf.areas[i] == RC_NULL_AREA)
                        {
                            areas[i] = chf.areas[i];
                            continue;
                        }

                        int[] nei = new int[9];
                        for (int j = 0; j < 9; ++j)
                            nei[j] = chf.areas[i];

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastCommon.GetDirOffsetX(dir);
                                int ay = y + RecastCommon.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, dir);
                                if (chf.areas[ai] != RC_NULL_AREA)
                                    nei[dir * 2 + 0] = chf.areas[ai];

                                RcCompactSpan @as = chf.spans[ai];
                                int dir2 = (dir + 1) & 0x3;
                                if (RecastCommon.GetCon(@as, dir2) != RC_NOT_CONNECTED)
                                {
                                    int ax2 = ax + RecastCommon.GetDirOffsetX(dir2);
                                    int ay2 = ay + RecastCommon.GetDirOffsetY(dir2);
                                    int ai2 = chf.cells[ax2 + ay2 * w].index + RecastCommon.GetCon(@as, dir2);
                                    if (chf.areas[ai2] != RC_NULL_AREA)
                                        nei[dir * 2 + 1] = chf.areas[ai2];
                                }
                            }
                        }

                        Array.Sort(nei);
                        areas[i] = nei[4];
                    }
                }
            }

            chf.areas = areas;

            return true;
        }

        /// @par
        ///
        /// The value of spacial parameters are in world units.
        ///
        /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
        public static void MarkBoxArea(RcTelemetry ctx, float[] bmin, float[] bmax, RcAreaModification areaMod, RcCompactHeightfield chf)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_MARK_BOX_AREA);

            int minx = (int)((bmin[0] - chf.bmin.x) / chf.cs);
            int miny = (int)((bmin[1] - chf.bmin.y) / chf.ch);
            int minz = (int)((bmin[2] - chf.bmin.z) / chf.cs);
            int maxx = (int)((bmax[0] - chf.bmin.x) / chf.cs);
            int maxy = (int)((bmax[1] - chf.bmin.y) / chf.ch);
            int maxz = (int)((bmax[2] - chf.bmin.z) / chf.cs);

            if (maxx < 0)
                return;
            if (minx >= chf.width)
                return;
            if (maxz < 0)
                return;
            if (minz >= chf.height)
                return;

            if (minx < 0)
                minx = 0;
            if (maxx >= chf.width)
                maxx = chf.width - 1;
            if (minz < 0)
                minz = 0;
            if (maxz >= chf.height)
                maxz = chf.height - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    RcCompactCell c = chf.cells[x + z * chf.width];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        if (s.y >= miny && s.y <= maxy)
                        {
                            if (chf.areas[i] != RC_NULL_AREA)
                                chf.areas[i] = areaMod.Apply(chf.areas[i]);
                        }
                    }
                }
            }
        }

        static bool PointInPoly(float[] verts, RcVec3f p)
        {
            bool c = false;
            int i, j;
            for (i = 0, j = verts.Length - 3; i < verts.Length; j = i, i += 3)
            {
                int vi = i;
                int vj = j;
                if (((verts[vi + 2] > p.z) != (verts[vj + 2] > p.z))
                    && (p.x < (verts[vj] - verts[vi]) * (p.z - verts[vi + 2]) / (verts[vj + 2] - verts[vi + 2])
                        + verts[vi]))
                    c = !c;
            }

            return c;
        }

        /// @par
        ///
        /// The value of spacial parameters are in world units.
        ///
        /// The y-values of the polygon vertices are ignored. So the polygon is effectively
        /// projected onto the xz-plane at @p hmin, then extruded to @p hmax.
        ///
        /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
        public static void MarkConvexPolyArea(RcTelemetry ctx, float[] verts, float hmin, float hmax, RcAreaModification areaMod,
            RcCompactHeightfield chf)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_MARK_CONVEXPOLY_AREA);

            RcVec3f bmin = new RcVec3f();
            RcVec3f bmax = new RcVec3f();
            RcVec3f.Copy(ref bmin, verts, 0);
            RcVec3f.Copy(ref bmax, verts, 0);
            for (int i = 3; i < verts.Length; i += 3)
            {
                bmin.Min(verts, i);
                bmax.Max(verts, i);
            }

            bmin.y = hmin;
            bmax.y = hmax;

            int minx = (int)((bmin.x - chf.bmin.x) / chf.cs);
            int miny = (int)((bmin.y - chf.bmin.y) / chf.ch);
            int minz = (int)((bmin.z - chf.bmin.z) / chf.cs);
            int maxx = (int)((bmax.x - chf.bmin.x) / chf.cs);
            int maxy = (int)((bmax.y - chf.bmin.y) / chf.ch);
            int maxz = (int)((bmax.z - chf.bmin.z) / chf.cs);

            if (maxx < 0)
                return;
            if (minx >= chf.width)
                return;
            if (maxz < 0)
                return;
            if (minz >= chf.height)
                return;

            if (minx < 0)
                minx = 0;
            if (maxx >= chf.width)
                maxx = chf.width - 1;
            if (minz < 0)
                minz = 0;
            if (maxz >= chf.height)
                maxz = chf.height - 1;

            // TODO: Optimize.
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    RcCompactCell c = chf.cells[x + z * chf.width];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        if (chf.areas[i] == RC_NULL_AREA)
                            continue;
                        if (s.y >= miny && s.y <= maxy)
                        {
                            RcVec3f p = new RcVec3f();
                            p.x = chf.bmin.x + (x + 0.5f) * chf.cs;
                            p.y = 0;
                            p.z = chf.bmin.z + (z + 0.5f) * chf.cs;

                            if (PointInPoly(verts, p))
                            {
                                chf.areas[i] = areaMod.Apply(chf.areas[i]);
                            }
                        }
                    }
                }
            }
        }

        /// @par
        ///
        /// The value of spacial parameters are in world units.
        ///
        /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
        public static void MarkCylinderArea(RcTelemetry ctx, float[] pos, float r, float h, RcAreaModification areaMod, RcCompactHeightfield chf)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_MARK_CYLINDER_AREA);

            RcVec3f bmin = new RcVec3f();
            RcVec3f bmax = new RcVec3f();
            bmin.x = pos[0] - r;
            bmin.y = pos[1];
            bmin.z = pos[2] - r;
            bmax.x = pos[0] + r;
            bmax.y = pos[1] + h;
            bmax.z = pos[2] + r;
            float r2 = r * r;

            int minx = (int)((bmin.x - chf.bmin.x) / chf.cs);
            int miny = (int)((bmin.y - chf.bmin.y) / chf.ch);
            int minz = (int)((bmin.z - chf.bmin.z) / chf.cs);
            int maxx = (int)((bmax.x - chf.bmin.x) / chf.cs);
            int maxy = (int)((bmax.y - chf.bmin.y) / chf.ch);
            int maxz = (int)((bmax.z - chf.bmin.z) / chf.cs);

            if (maxx < 0)
                return;
            if (minx >= chf.width)
                return;
            if (maxz < 0)
                return;
            if (minz >= chf.height)
                return;

            if (minx < 0)
                minx = 0;
            if (maxx >= chf.width)
                maxx = chf.width - 1;
            if (minz < 0)
                minz = 0;
            if (maxz >= chf.height)
                maxz = chf.height - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    RcCompactCell c = chf.cells[x + z * chf.width];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];

                        if (chf.areas[i] == RC_NULL_AREA)
                            continue;

                        if (s.y >= miny && s.y <= maxy)
                        {
                            float sx = chf.bmin.x + (x + 0.5f) * chf.cs;
                            float sz = chf.bmin.z + (z + 0.5f) * chf.cs;
                            float dx = sx - pos[0];
                            float dz = sz - pos[2];

                            if (dx * dx + dz * dz < r2)
                            {
                                chf.areas[i] = areaMod.Apply(chf.areas[i]);
                            }
                        }
                    }
                }
            }
        }
    }
}