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
using DotRecast.Core;
using DotRecast.Recast;

namespace DotRecast.Detour.Dynamic
{
    /**
 * Voxel raycast based on the algorithm described in
 *
 * "A Fast Voxel Traversal Algorithm for Ray Tracing" by John Amanatides and Andrew Woo
 */
    public class VoxelQuery
    {
        private readonly Vector3f origin;
        private readonly float tileWidth;
        private readonly float tileDepth;
        private readonly Func<int, int, Heightfield> heightfieldProvider;

        public VoxelQuery(Vector3f origin, float tileWidth, float tileDepth, Func<int, int, Heightfield> heightfieldProvider)
        {
            this.origin = origin;
            this.tileWidth = tileWidth;
            this.tileDepth = tileDepth;
            this.heightfieldProvider = heightfieldProvider;
        }

        /**
     * Perform raycast using voxels heightfields.
     *
     * @return Optional with hit parameter (t) or empty if no hit found
     */
        public float? raycast(Vector3f start, Vector3f end)
        {
            return traverseTiles(start, end);
        }

        private float? traverseTiles(Vector3f start, Vector3f end)
        {
            float relStartX = start[0] - origin[0];
            float relStartZ = start[2] - origin[2];
            int sx = (int)Math.Floor(relStartX / tileWidth);
            int sz = (int)Math.Floor(relStartZ / tileDepth);
            int ex = (int)Math.Floor((end[0] - origin[0]) / tileWidth);
            int ez = (int)Math.Floor((end[2] - origin[2]) / tileDepth);
            int dx = ex - sx;
            int dz = ez - sz;
            int stepX = dx < 0 ? -1 : 1;
            int stepZ = dz < 0 ? -1 : 1;
            float xRem = (tileWidth + (relStartX % tileWidth)) % tileWidth;
            float zRem = (tileDepth + (relStartZ % tileDepth)) % tileDepth;
            float tx = end[0] - start[0];
            float tz = end[2] - start[2];
            float xOffest = Math.Abs(tx < 0 ? xRem : tileWidth - xRem);
            float zOffest = Math.Abs(tz < 0 ? zRem : tileDepth - zRem);
            tx = Math.Abs(tx);
            tz = Math.Abs(tz);
            float tMaxX = xOffest / tx;
            float tMaxZ = zOffest / tz;
            float tDeltaX = tileWidth / tx;
            float tDeltaZ = tileDepth / tz;
            float t = 0;
            while (true)
            {
                float? hit = traversHeightfield(sx, sz, start, end, t, Math.Min(1, Math.Min(tMaxX, tMaxZ)));
                if (hit.HasValue)
                {
                    return hit;
                }

                if ((dx > 0 ? sx >= ex : sx <= ex) && (dz > 0 ? sz >= ez : sz <= ez))
                {
                    break;
                }

                if (tMaxX < tMaxZ)
                {
                    t = tMaxX;
                    tMaxX += tDeltaX;
                    sx += stepX;
                }
                else
                {
                    t = tMaxZ;
                    tMaxZ += tDeltaZ;
                    sz += stepZ;
                }
            }

            return null;
        }

        private float? traversHeightfield(int x, int z, Vector3f start, Vector3f end, float tMin, float tMax)
        {
            Heightfield hf = heightfieldProvider.Invoke(x, z);
            if (null != hf)
            {
                float tx = end[0] - start[0];
                float ty = end[1] - start[1];
                float tz = end[2] - start[2];
                float[] entry = { start[0] + tMin * tx, start[1] + tMin * ty, start[2] + tMin * tz };
                float[] exit = { start[0] + tMax * tx, start[1] + tMax * ty, start[2] + tMax * tz };
                float relStartX = entry[0] - hf.bmin[0];
                float relStartZ = entry[2] - hf.bmin[2];
                int sx = (int)Math.Floor(relStartX / hf.cs);
                int sz = (int)Math.Floor(relStartZ / hf.cs);
                int ex = (int)Math.Floor((exit[0] - hf.bmin[0]) / hf.cs);
                int ez = (int)Math.Floor((exit[2] - hf.bmin[2]) / hf.cs);
                int dx = ex - sx;
                int dz = ez - sz;
                int stepX = dx < 0 ? -1 : 1;
                int stepZ = dz < 0 ? -1 : 1;
                float xRem = (hf.cs + (relStartX % hf.cs)) % hf.cs;
                float zRem = (hf.cs + (relStartZ % hf.cs)) % hf.cs;
                float xOffest = Math.Abs(tx < 0 ? xRem : hf.cs - xRem);
                float zOffest = Math.Abs(tz < 0 ? zRem : hf.cs - zRem);
                tx = Math.Abs(tx);
                tz = Math.Abs(tz);
                float tMaxX = xOffest / tx;
                float tMaxZ = zOffest / tz;
                float tDeltaX = hf.cs / tx;
                float tDeltaZ = hf.cs / tz;
                float t = 0;
                while (true)
                {
                    if (sx >= 0 && sx < hf.width && sz >= 0 && sz < hf.height)
                    {
                        float y1 = start[1] + ty * (tMin + t) - hf.bmin[1];
                        float y2 = start[1] + ty * (tMin + Math.Min(tMaxX, tMaxZ)) - hf.bmin[1];
                        float ymin = Math.Min(y1, y2) / hf.ch;
                        float ymax = Math.Max(y1, y2) / hf.ch;
                        Span span = hf.spans[sx + sz * hf.width];
                        while (span != null)
                        {
                            if (span.smin <= ymin && span.smax >= ymax)
                            {
                                return Math.Min(1, tMin + t);
                            }

                            span = span.next;
                        }
                    }

                    if ((dx > 0 ? sx >= ex : sx <= ex) && (dz > 0 ? sz >= ez : sz <= ez))
                    {
                        break;
                    }

                    if (tMaxX < tMaxZ)
                    {
                        t = tMaxX;
                        tMaxX += tDeltaX;
                        sx += stepX;
                    }
                    else
                    {
                        t = tMaxZ;
                        tMaxZ += tDeltaZ;
                        sz += stepZ;
                    }
                }
            }

            return null;
        }
    }
}