/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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
using System.IO.Compression;
using System.Numerics;
using DotRecast.Core;

namespace DotRecast.Detour.Extras.Unity.Astar
{


public class GraphMeshDataReader : ZipBinaryReader {

    public const float INT_PRECISION_FACTOR = 1000f;

    public GraphMeshData read(ZipArchive file, string filename, GraphMeta meta, int maxVertPerPoly) {
        ByteBuffer buffer = toByteBuffer(file, filename);
        int tileXCount = buffer.getInt();
        if (tileXCount < 0) {
            return null;
        }
        int tileZCount = buffer.getInt();
        MeshData[] tiles = new MeshData[tileXCount * tileZCount];
        for (int z = 0; z < tileZCount; z++) {
            for (int x = 0; x < tileXCount; x++) {
                int tileIndex = x + z * tileXCount;
                int tx = buffer.getInt();
                int tz = buffer.getInt();
                if (tx != x || tz != z) {
                    throw new ArgumentException("Inconsistent tile positions");
                }

                tiles[tileIndex] = new MeshData();
                int width = buffer.getInt();
                int depth = buffer.getInt();

                int trisCount = buffer.getInt();
                int[] tris = new int[trisCount];
                for (int i = 0; i < tris.Length; i++) {
                    tris[i] = buffer.getInt();
                }

                int vertsCount = buffer.getInt();
                float[] verts = new float[3 * vertsCount];
                for (int i = 0; i < verts.Length; i++) {
                    verts[i] = buffer.getInt() / INT_PRECISION_FACTOR;
                }

                int[] vertsInGraphSpace = new int[3 * buffer.getInt()];
                for (int i = 0; i < vertsInGraphSpace.Length; i++) {
                    vertsInGraphSpace[i] = buffer.getInt();
                }

                int nodeCount = buffer.getInt();
                Poly[] nodes = new Poly[nodeCount];
                PolyDetail[] detailNodes = new PolyDetail[nodeCount];
                float[] detailVerts = new float[0];
                int[] detailTris = new int[4 * nodeCount];
                int vertMask = getVertMask(vertsCount);
                float ymin = float.PositiveInfinity;
                float ymax = float.NegativeInfinity;
                for (int i = 0; i < nodes.Length; i++) {
                    nodes[i] = new Poly(i, maxVertPerPoly);
                    nodes[i].vertCount = 3;
                    // XXX: What can we do with the penalty?
                    int penalty = buffer.getInt();
                    nodes[i].flags = buffer.getInt();
                    nodes[i].verts[0] = buffer.getInt() & vertMask;
                    nodes[i].verts[1] = buffer.getInt() & vertMask;
                    nodes[i].verts[2] = buffer.getInt() & vertMask;
                    ymin = Math.Min(ymin, verts[nodes[i].verts[0] * 3 + 1]);
                    ymin = Math.Min(ymin, verts[nodes[i].verts[1] * 3 + 1]);
                    ymin = Math.Min(ymin, verts[nodes[i].verts[2] * 3 + 1]);
                    ymax = Math.Max(ymax, verts[nodes[i].verts[0] * 3 + 1]);
                    ymax = Math.Max(ymax, verts[nodes[i].verts[1] * 3 + 1]);
                    ymax = Math.Max(ymax, verts[nodes[i].verts[2] * 3 + 1]);
                    detailNodes[i] = new PolyDetail();
                    detailNodes[i].vertBase = 0;
                    detailNodes[i].vertCount = 0;
                    detailNodes[i].triBase = i;
                    detailNodes[i].triCount = 1;
                    detailTris[4 * i] = 0;
                    detailTris[4 * i + 1] = 1;
                    detailTris[4 * i + 2] = 2;
                    // Bit for each edge that belongs to poly boundary, basically all edges marked as boundary as it is
                    // a triangle
                    detailTris[4 * i + 3] = (1 << 4) | (1 << 2) | 1;
                }

                tiles[tileIndex].verts = verts;
                tiles[tileIndex].polys = nodes;
                tiles[tileIndex].detailMeshes = detailNodes;
                tiles[tileIndex].detailVerts = detailVerts;
                tiles[tileIndex].detailTris = detailTris;
                MeshHeader header = new MeshHeader();
                header.magic = MeshHeader.DT_NAVMESH_MAGIC;
                header.version = MeshHeader.DT_NAVMESH_VERSION;
                header.x = x;
                header.y = z;
                header.polyCount = nodeCount;
                header.vertCount = vertsCount;
                header.detailMeshCount = nodeCount;
                header.detailTriCount = nodeCount;
                header.maxLinkCount = nodeCount * 3 * 2; // XXX: Needed by Recast, not needed by recast4j
                header.bmin[0] = meta.forcedBoundsCenter.x - 0.5f * meta.forcedBoundsSize.x
                        + meta.cellSize * meta.tileSizeX * x;
                header.bmin[1] = ymin;
                header.bmin[2] = meta.forcedBoundsCenter.z - 0.5f * meta.forcedBoundsSize.z
                        + meta.cellSize * meta.tileSizeZ * z;
                header.bmax[0] = meta.forcedBoundsCenter.x - 0.5f * meta.forcedBoundsSize.x
                        + meta.cellSize * meta.tileSizeX * (x + 1);
                header.bmax[1] = ymax;
                header.bmax[2] = meta.forcedBoundsCenter.z - 0.5f * meta.forcedBoundsSize.z
                        + meta.cellSize * meta.tileSizeZ * (z + 1);
                header.bvQuantFactor = 1.0f / meta.cellSize;
                header.offMeshBase = nodeCount;
                header.walkableClimb = meta.walkableClimb;
                header.walkableHeight = meta.walkableHeight;
                header.walkableRadius = meta.characterRadius;
                tiles[tileIndex].header = header;
            }
        }
        return new GraphMeshData(tileXCount, tileZCount, tiles);
    }

    public static int highestOneBit(uint i)
    {
        i |= (i >>  1);
        i |= (i >>  2);
        i |= (i >>  4);
        i |= (i >>  8);
        i |= (i >>  16);
        return (int)(i - (i >> 1));
    }
    
    // See NavmeshBase.cs: ASTAR_RECAST_LARGER_TILES
    private int getVertMask(int vertsCount)
    {
        int vertMask = 1 << highestOneBit((uint)vertsCount);
        if (vertMask != vertsCount) {
            vertMask *= 2;
        }
        vertMask--;
        return vertMask;
    }

}

}