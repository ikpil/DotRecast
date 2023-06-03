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
using DotRecast.Detour;

namespace DotRecast.Detour
{
    public static class NavMeshUtils
    {
        public static RcVec3f[] GetNavMeshBounds(NavMesh mesh)
        {
            RcVec3f bmin = RcVec3f.Of(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            RcVec3f bmax = RcVec3f.Of(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int t = 0; t < mesh.GetMaxTiles(); ++t)
            {
                MeshTile tile = mesh.GetTile(t);
                if (tile != null && tile.data != null)
                {
                    for (int i = 0; i < tile.data.verts.Length; i += 3)
                    {
                        bmin.x = Math.Min(bmin.x, tile.data.verts[i]);
                        bmin.y = Math.Min(bmin.y, tile.data.verts[i + 1]);
                        bmin.z = Math.Min(bmin.z, tile.data.verts[i + 2]);
                        bmax.x = Math.Max(bmax.x, tile.data.verts[i]);
                        bmax.y = Math.Max(bmax.y, tile.data.verts[i + 1]);
                        bmax.z = Math.Max(bmax.z, tile.data.verts[i + 2]);
                    }
                }
            }

            return new[] { bmin, bmax };
        }
    }
}