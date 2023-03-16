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

using DotRecast.Core;
using DotRecast.Detour;

namespace DotRecast.Recast.Demo.Geom;

/**
 * Simple helper to find an intersection between a ray and a nav mesh
 */
public class NavMeshRaycast
{
    public static float? raycast(NavMesh mesh, float[] src, float[] dst)
    {
        for (int t = 0; t < mesh.getMaxTiles(); ++t)
        {
            MeshTile tile = mesh.getTile(t);
            if (tile != null && tile.data != null)
            {
                float? intersection = raycast(tile, src, dst);
                if (null != intersection)
                {
                    return intersection;
                }
            }
        }

        return null;
    }

    private static float? raycast(MeshTile tile, float[] sp, float[] sq)
    {
        for (int i = 0; i < tile.data.header.polyCount; ++i)
        {
            Poly p = tile.data.polys[i];
            if (p.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                continue;
            }

            PolyDetail pd = tile.data.detailMeshes[i];

            if (pd != null)
            {
                float[][] verts = ArrayUtils.Of<float>(3, 3);
                for (int j = 0; j < pd.triCount; ++j)
                {
                    int t = (pd.triBase + j) * 4;
                    for (int k = 0; k < 3; ++k)
                    {
                        int v = tile.data.detailTris[t + k];
                        if (v < p.vertCount)
                        {
                            verts[k][0] = tile.data.verts[p.verts[v] * 3];
                            verts[k][1] = tile.data.verts[p.verts[v] * 3 + 1];
                            verts[k][2] = tile.data.verts[p.verts[v] * 3 + 2];
                        }
                        else
                        {
                            verts[k][0] = tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3];
                            verts[k][1] = tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 1];
                            verts[k][2] = tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 2];
                        }
                    }

                    float? intersection = Intersections.intersectSegmentTriangle(sp, sq, verts[0], verts[1], verts[2]);
                    if (null != intersection)
                    {
                        return intersection;
                    }
                }
            }
            else
            {
                // FIXME: Use Poly if PolyDetail is unavailable
            }
        }

        return null;
    }
}