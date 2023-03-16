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

using System.Collections.Generic;
using DotRecast.Recast.Geom;

namespace DotRecast.Recast
{
    public class RecastVoxelization
    {
        public static Heightfield buildSolidHeightfield(InputGeomProvider geomProvider, RecastBuilderConfig builderCfg,
            Telemetry ctx)
        {
            RecastConfig cfg = builderCfg.cfg;

            // Allocate voxel heightfield where we rasterize our input data to.
            Heightfield solid = new Heightfield(builderCfg.width, builderCfg.height, builderCfg.bmin, builderCfg.bmax, cfg.cs,
                cfg.ch, cfg.borderSize);

            // Allocate array that can hold triangle area types.
            // If you have multiple meshes you need to process, allocate
            // and array which can hold the max number of triangles you need to
            // process.

            // Find triangles which are walkable based on their slope and rasterize
            // them.
            // If your input data is multiple meshes, you can transform them here,
            // calculate
            // the are type for each of the meshes and rasterize them.
            foreach (TriMesh geom in geomProvider.meshes())
            {
                float[] verts = geom.getVerts();
                if (cfg.useTiles)
                {
                    float[] tbmin = new float[2];
                    float[] tbmax = new float[2];
                    tbmin[0] = builderCfg.bmin[0];
                    tbmin[1] = builderCfg.bmin[2];
                    tbmax[0] = builderCfg.bmax[0];
                    tbmax[1] = builderCfg.bmax[2];
                    List<ChunkyTriMeshNode> nodes = geom.getChunksOverlappingRect(tbmin, tbmax);
                    foreach (ChunkyTriMeshNode node in nodes)
                    {
                        int[] tris = node.tris;
                        int ntris = tris.Length / 3;
                        int[] m_triareas = Recast.markWalkableTriangles(ctx, cfg.walkableSlopeAngle, verts, tris, ntris,
                            cfg.walkableAreaMod);
                        RecastRasterization.rasterizeTriangles(solid, verts, tris, m_triareas, ntris, cfg.walkableClimb, ctx);
                    }
                }
                else
                {
                    int[] tris = geom.getTris();
                    int ntris = tris.Length / 3;
                    int[] m_triareas = Recast.markWalkableTriangles(ctx, cfg.walkableSlopeAngle, verts, tris, ntris,
                        cfg.walkableAreaMod);
                    RecastRasterization.rasterizeTriangles(solid, verts, tris, m_triareas, ntris, cfg.walkableClimb, ctx);
                }
            }

            return solid;
        }
    }
}