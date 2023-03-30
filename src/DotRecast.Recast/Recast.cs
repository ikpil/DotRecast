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
    using static RecastConstants;

    public class Recast
    {
        void calcBounds(float[] verts, int nv, float[] bmin, float[] bmax)
        {
            for (int i = 0; i < 3; i++)
            {
                bmin[i] = verts[i];
                bmax[i] = verts[i];
            }

            for (int i = 1; i < nv; ++i)
            {
                for (int j = 0; j < 3; j++)
                {
                    bmin[j] = Math.Min(bmin[j], verts[i * 3 + j]);
                    bmax[j] = Math.Max(bmax[j], verts[i * 3 + j]);
                }
            }
            // Calculate bounding box.
        }

        public static int[] calcGridSize(float[] bmin, float[] bmax, float cs)
        {
            return new int[] { (int)((bmax[0] - bmin[0]) / cs + 0.5f), (int)((bmax[2] - bmin[2]) / cs + 0.5f) };
        }
        
        public static int[] calcGridSize(Vector3f bmin, float[] bmax, float cs)
        {
            return new int[] { (int)((bmax[0] - bmin[0]) / cs + 0.5f), (int)((bmax[2] - bmin[2]) / cs + 0.5f) };
        }
        
        public static int[] calcGridSize(Vector3f bmin, Vector3f bmax, float cs)
        {
            return new int[] { (int)((bmax[0] - bmin[0]) / cs + 0.5f), (int)((bmax[2] - bmin[2]) / cs + 0.5f) };
        }


        public static int[] calcTileCount(Vector3f bmin, Vector3f bmax, float cs, int tileSizeX, int tileSizeZ)
        {
            int[] gwd = calcGridSize(bmin, bmax, cs);
            int gw = gwd[0];
            int gd = gwd[1];
            int tw = (gw + tileSizeX - 1) / tileSizeX;
            int td = (gd + tileSizeZ - 1) / tileSizeZ;
            return new int[] { tw, td };
        }

        /// @par
        ///
        /// Modifies the area id of all triangles with a slope below the specified value.
        ///
        /// See the #rcConfig documentation for more information on the configuration parameters.
        ///
        /// @see rcHeightfield, rcClearUnwalkableTriangles, rcRasterizeTriangles
        public static int[] markWalkableTriangles(Telemetry ctx, float walkableSlopeAngle, float[] verts, int[] tris, int nt,
            AreaModification areaMod)
        {
            int[] areas = new int[nt];
            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * Math.PI);
            Vector3f norm = new Vector3f();
            for (int i = 0; i < nt; ++i)
            {
                int tri = i * 3;
                calcTriNormal(verts, tris[tri], tris[tri + 1], tris[tri + 2], ref norm);
                // Check if the face is walkable.
                if (norm[1] > walkableThr)
                    areas[i] = areaMod.apply(areas[i]);
            }

            return areas;
        }

        static void calcTriNormal(float[] verts, int v0, int v1, int v2, float[] norm)
        {
            Vector3f e0 = new Vector3f();
            Vector3f e1 = new Vector3f();
            RecastVectors.sub(ref e0, verts, v1 * 3, v0 * 3);
            RecastVectors.sub(ref e1, verts, v2 * 3, v0 * 3);
            RecastVectors.cross(norm, e0, e1);
            RecastVectors.normalize(norm);
        }
        
        static void calcTriNormal(float[] verts, int v0, int v1, int v2, ref Vector3f norm)
        {
            Vector3f e0 = new Vector3f();
            Vector3f e1 = new Vector3f();
            RecastVectors.sub(ref e0, verts, v1 * 3, v0 * 3);
            RecastVectors.sub(ref e1, verts, v2 * 3, v0 * 3);
            RecastVectors.cross(ref norm, e0, e1);
            RecastVectors.normalize(ref norm);
        }


        /// @par
        ///
        /// Only sets the area id's for the unwalkable triangles. Does not alter the
        /// area id's for walkable triangles.
        ///
        /// See the #rcConfig documentation for more information on the configuration parameters.
        ///
        /// @see rcHeightfield, rcClearUnwalkableTriangles, rcRasterizeTriangles
        public static void clearUnwalkableTriangles(Telemetry ctx, float walkableSlopeAngle, float[] verts, int nv,
            int[] tris, int nt, int[] areas)
        {
            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * Math.PI);

            Vector3f norm = new Vector3f();

            for (int i = 0; i < nt; ++i)
            {
                int tri = i * 3;
                calcTriNormal(verts, tris[tri], tris[tri + 1], tris[tri + 2], ref norm);
                // Check if the face is walkable.
                if (norm[1] <= walkableThr)
                    areas[i] = RC_NULL_AREA;
            }
        }
    }
}