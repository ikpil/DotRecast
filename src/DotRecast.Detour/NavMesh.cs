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
using System.Collections.Generic;
using System.Collections.Immutable;
using DotRecast.Core;

namespace DotRecast.Detour
{


using static DetourCommon;

public class NavMesh {

    public const int DT_SALT_BITS = 16;
    public const int DT_TILE_BITS = 28;
    public const int DT_POLY_BITS = 20;
    public const int DT_DETAIL_EDGE_BOUNDARY = 0x01;

    /// A flag that indicates that an entity links to an external entity.
    /// (E.g. A polygon edge is a portal that links to another polygon.)
    public const int DT_EXT_LINK = 0x8000;

    /// A value that indicates the entity does not link to anything.
    public const int DT_NULL_LINK = unchecked((int)0xffffffff);

    /// A flag that indicates that an off-mesh connection can be traversed in
    /// both directions. (Is bidirectional.)
    public const int DT_OFFMESH_CON_BIDIR = 1;

    /// The maximum number of user defined area ids.
    public const int DT_MAX_AREAS = 64;

    /// Limit raycasting during any angle pahfinding
    /// The limit is given as a multiple of the character radius
    public const float DT_RAY_CAST_LIMIT_PROPORTIONS = 50.0f;

    private readonly NavMeshParams m_params; /// < Current initialization params. TODO: do not store this info twice.
    private readonly float[] m_orig; /// < Origin of the tile (0,0)
    // float m_orig[3]; ///< Origin of the tile (0,0)
    float m_tileWidth, m_tileHeight; /// < Dimensions of each tile.
    int m_maxTiles; /// < Max number of tiles.
    private readonly int m_tileLutMask; /// < Tile hash lookup mask.
    private readonly Dictionary<int, List<MeshTile>> posLookup = new Dictionary<int, List<MeshTile>>();
    private readonly LinkedList<MeshTile> availableTiles = new LinkedList<MeshTile>();
    private readonly MeshTile[] m_tiles; /// < List of tiles.
    /** The maximum number of vertices per navigation polygon. */
    private readonly int m_maxVertPerPoly;
    private int m_tileCount;

    /**
     * The maximum number of tiles supported by the navigation mesh.
     *
     * @return The maximum number of tiles supported by the navigation mesh.
     */
    public int getMaxTiles() {
        return m_maxTiles;
    }

    /**
     * Returns tile in the tile array.
     */
    public MeshTile getTile(int i) {
        return m_tiles[i];
    }

    /**
     * Gets the polygon reference for the tile's base polygon.
     *
     * @param tile
     *            The tile.
     * @return The polygon reference for the base polygon in the specified tile.
     */
    public long getPolyRefBase(MeshTile tile) {
        if (tile == null) {
            return 0;
        }
        int it = tile.index;
        return encodePolyId(tile.salt, it, 0);
    }

    /**
     * Derives a standard polygon reference.
     *
     * @note This function is generally meant for internal use only.
     * @param salt
     *            The tile's salt value.
     * @param it
     *            The index of the tile.
     * @param ip
     *            The index of the polygon within the tile.
     * @return encoded polygon reference
     */
    public static long encodePolyId(int salt, int it, int ip) {
        return (((long) salt) << (DT_POLY_BITS + DT_TILE_BITS)) | ((long) it << DT_POLY_BITS) | ip;
    }

    /// Decodes a standard polygon reference.
    /// @note This function is generally meant for internal use only.
    /// @param[in] ref The polygon reference to decode.
    /// @param[out] salt The tile's salt value.
    /// @param[out] it The index of the tile.
    /// @param[out] ip The index of the polygon within the tile.
    /// @see #encodePolyId
    static int[] decodePolyId(long refs) {
        int salt;
        int it;
        int ip;
        long saltMask = (1L << DT_SALT_BITS) - 1;
        long tileMask = (1L << DT_TILE_BITS) - 1;
        long polyMask = (1L << DT_POLY_BITS) - 1;
        salt = (int) ((refs >> (DT_POLY_BITS + DT_TILE_BITS)) & saltMask);
        it = (int) ((refs >> DT_POLY_BITS) & tileMask);
        ip = (int) (refs & polyMask);
        return new int[] { salt, it, ip };
    }

    /// Extracts a tile's salt value from the specified polygon reference.
    /// @note This function is generally meant for internal use only.
    /// @param[in] ref The polygon reference.
    /// @see #encodePolyId
    static int decodePolyIdSalt(long refs) {
        long saltMask = (1L << DT_SALT_BITS) - 1;
        return (int) ((refs >> (DT_POLY_BITS + DT_TILE_BITS)) & saltMask);
    }

    /// Extracts the tile's index from the specified polygon reference.
    /// @note This function is generally meant for internal use only.
    /// @param[in] ref The polygon reference.
    /// @see #encodePolyId
    public static int decodePolyIdTile(long refs) {
        long tileMask = (1L << DT_TILE_BITS) - 1;
        return (int) ((refs >> DT_POLY_BITS) & tileMask);
    }

    /// Extracts the polygon's index (within its tile) from the specified
    /// polygon reference.
    /// @note This function is generally meant for internal use only.
    /// @param[in] ref The polygon reference.
    /// @see #encodePolyId
    static int decodePolyIdPoly(long refs) {
        long polyMask = (1L << DT_POLY_BITS) - 1;
        return (int) (refs & polyMask);
    }

    private int allocLink(MeshTile tile) {
        if (tile.linksFreeList == DT_NULL_LINK) {
            Link link = new Link();
            link.next = DT_NULL_LINK;
            tile.links.Add(link);
            return tile.links.Count - 1;
        }
        int linkIdx = tile.linksFreeList;
        tile.linksFreeList = tile.links[linkIdx].next;
        return linkIdx;
    }

    private void freeLink(MeshTile tile, int link) {
        tile.links[link].next = tile.linksFreeList;
        tile.linksFreeList = link;
    }

    /**
     * Calculates the tile grid location for the specified world position.
     *
     * @param pos
     *            The world position for the query. [(x, y, z)]
     * @return 2-element int array with (tx,ty) tile location
     */
    public int[] calcTileLoc(float[] pos) {
        int tx = (int) Math.Floor((pos[0] - m_orig[0]) / m_tileWidth);
        int ty = (int) Math.Floor((pos[2] - m_orig[2]) / m_tileHeight);
        return new int[] { tx, ty };
    }

    public Result<Tuple<MeshTile, Poly>> getTileAndPolyByRef(long refs) {
        if (refs == 0) {
            return Results.invalidParam<Tuple<MeshTile, Poly>>("ref = 0");
        }
        int[] saltitip = decodePolyId(refs);
        int salt = saltitip[0];
        int it = saltitip[1];
        int ip = saltitip[2];
        if (it >= m_maxTiles) {
            return Results.invalidParam<Tuple<MeshTile, Poly>>("tile > m_maxTiles");
        }
        if (m_tiles[it].salt != salt || m_tiles[it].data.header == null) {
            return Results.invalidParam<Tuple<MeshTile, Poly>>("Invalid salt or header");
        }
        if (ip >= m_tiles[it].data.header.polyCount) {
            return Results.invalidParam<Tuple<MeshTile, Poly>>("poly > polyCount");
        }
        return Results.success(Tuple.Create(m_tiles[it], m_tiles[it].data.polys[ip]));
    }

    /// @par
    ///
    /// @warning Only use this function if it is known that the provided polygon
    /// reference is valid. This function is faster than #getTileAndPolyByRef,
    /// but
    /// it does not validate the reference.
    public Tuple<MeshTile, Poly> getTileAndPolyByRefUnsafe(long refs) {
        int[] saltitip = decodePolyId(refs);
        int it = saltitip[1];
        int ip = saltitip[2];
        return Tuple.Create(m_tiles[it], m_tiles[it].data.polys[ip]);
    }

    public bool isValidPolyRef(long refs) {
        if (refs == 0) {
            return false;
        }
        int[] saltitip = decodePolyId(refs);
        int salt = saltitip[0];
        int it = saltitip[1];
        int ip = saltitip[2];
        if (it >= m_maxTiles) {
            return false;
        }
        if (m_tiles[it].salt != salt || m_tiles[it].data == null) {
            return false;
        }
        if (ip >= m_tiles[it].data.header.polyCount) {
            return false;
        }
        return true;
    }

    public NavMeshParams getParams() {
        return m_params;
    }

    public NavMesh(MeshData data, int maxVertsPerPoly, int flags) 
        : this(getNavMeshParams(data), maxVertsPerPoly) 
    {
        addTile(data, flags, 0);
    }

    public NavMesh(NavMeshParams option, int maxVertsPerPoly) {
        m_params = option;
        m_orig = option.orig;
        m_tileWidth = option.tileWidth;
        m_tileHeight = option.tileHeight;
        // Init tiles
        m_maxTiles = option.maxTiles;
        m_maxVertPerPoly = maxVertsPerPoly;
        m_tileLutMask = Math.Max(1, nextPow2(option.maxTiles)) - 1;
        m_tiles = new MeshTile[m_maxTiles];
        for (int i = 0; i < m_maxTiles; i++) {
            m_tiles[i] = new MeshTile(i);
            m_tiles[i].salt = 1;
            availableTiles.AddLast(m_tiles[i]);
        }

    }

    private static NavMeshParams getNavMeshParams(MeshData data) {
        NavMeshParams option = new NavMeshParams();
        vCopy(option.orig, data.header.bmin);
        option.tileWidth = data.header.bmax[0] - data.header.bmin[0];
        option.tileHeight = data.header.bmax[2] - data.header.bmin[2];
        option.maxTiles = 1;
        option.maxPolys = data.header.polyCount;
        return option;
    }

    // TODO: These methods are duplicates from dtNavMeshQuery, but are needed
    // for off-mesh connection finding.

    List<long> queryPolygonsInTile(MeshTile tile, float[] qmin, float[] qmax) {
        List<long> polys = new List<long>();
        if (tile.data.bvTree != null) {
            int nodeIndex = 0;
            float[] tbmin = tile.data.header.bmin;
            float[] tbmax = tile.data.header.bmax;
            float qfac = tile.data.header.bvQuantFactor;
            // Calculate quantized box
            int[] bmin = new int[3];
            int[] bmax = new int[3];
            // dtClamp query box to world box.
            float minx = clamp(qmin[0], tbmin[0], tbmax[0]) - tbmin[0];
            float miny = clamp(qmin[1], tbmin[1], tbmax[1]) - tbmin[1];
            float minz = clamp(qmin[2], tbmin[2], tbmax[2]) - tbmin[2];
            float maxx = clamp(qmax[0], tbmin[0], tbmax[0]) - tbmin[0];
            float maxy = clamp(qmax[1], tbmin[1], tbmax[1]) - tbmin[1];
            float maxz = clamp(qmax[2], tbmin[2], tbmax[2]) - tbmin[2];
            // Quantize
            bmin[0] = (int) (qfac * minx) & 0x7ffffffe;
            bmin[1] = (int) (qfac * miny) & 0x7ffffffe;
            bmin[2] = (int) (qfac * minz) & 0x7ffffffe;
            bmax[0] = (int) (qfac * maxx + 1) | 1;
            bmax[1] = (int) (qfac * maxy + 1) | 1;
            bmax[2] = (int) (qfac * maxz + 1) | 1;

            // Traverse tree
            long @base = getPolyRefBase(tile);
            int end = tile.data.header.bvNodeCount;
            while (nodeIndex < end) {
                BVNode node = tile.data.bvTree[nodeIndex];
                bool overlap = overlapQuantBounds(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap) {
                    polys.Add(@base | node.i);
                }

                if (overlap || isLeafNode) {
                    nodeIndex++;
                } else {
                    int escapeIndex = -node.i;
                    nodeIndex += escapeIndex;
                }
            }

            return polys;
        } else {
            float[] bmin = new float[3];
            float[] bmax = new float[3];
            long @base = getPolyRefBase(tile);
            for (int i = 0; i < tile.data.header.polyCount; ++i) {
                Poly p = tile.data.polys[i];
                // Do not return off-mesh connection polygons.
                if (p.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
                    continue;
                }
                // Calc polygon bounds.
                int v = p.verts[0] * 3;
                vCopy(bmin, tile.data.verts, v);
                vCopy(bmax, tile.data.verts, v);
                for (int j = 1; j < p.vertCount; ++j) {
                    v = p.verts[j] * 3;
                    vMin(bmin, tile.data.verts, v);
                    vMax(bmax, tile.data.verts, v);
                }
                if (overlapBounds(qmin, qmax, bmin, bmax)) {
                    polys.Add(@base | i);
                }
            }
            return polys;
        }
    }

    public long updateTile(MeshData data, int flags) {
        long refs = getTileRefAt(data.header.x, data.header.y, data.header.layer);
        refs = removeTile(refs);
        return addTile(data, flags, refs);
    }

    /// Adds a tile to the navigation mesh.
    /// @param[in] data Data for the new tile mesh. (See: #dtCreateNavMeshData)
    /// @param[in] dataSize Data size of the new tile mesh.
    /// @param[in] flags Tile flags. (See: #dtTileFlags)
    /// @param[in] lastRef The desired reference for the tile. (When reloading a
    /// tile.) [opt] [Default: 0]
    /// @param[out] result The tile reference. (If the tile was succesfully
    /// added.) [opt]
    /// @return The status flags for the operation.
    /// @par
    ///
    /// The add operation will fail if the data is in the wrong format, the
    /// allocated tile
    /// space is full, or there is a tile already at the specified reference.
    ///
    /// The lastRef parameter is used to restore a tile with the same tile
    /// reference it had previously used. In this case the #long's for the
    /// tile will be restored to the same values they were before the tile was
    /// removed.
    ///
    /// The nav mesh assumes exclusive access to the data passed and will make
    /// changes to the dynamic portion of the data. For that reason the data
    /// should not be reused in other nav meshes until the tile has been successfully
    /// removed from this nav mesh.
    ///
    /// @see dtCreateNavMeshData, #removeTile
    public long addTile(MeshData data, int flags, long lastRef) {
        // Make sure the data is in right format.
        MeshHeader header = data.header;

        // Make sure the location is free.
        if (getTileAt(header.x, header.y, header.layer) != null) {
            throw new Exception("Tile already exists");
        }

        // Allocate a tile.
        MeshTile tile = null;
        if (lastRef == 0) {
            // Make sure we could allocate a tile.
            if (0 == availableTiles.Count) {
                throw new Exception("Could not allocate a tile");
            }

            tile = availableTiles.First?.Value;
            availableTiles.RemoveFirst();
            m_tileCount++;
        } else {
            // Try to relocate the tile to specific index with same salt.
            int tileIndex = decodePolyIdTile(lastRef);
            if (tileIndex >= m_maxTiles) {
                throw new Exception("Tile index too high");
            }
            // Try to find the specific tile id from the free list.
            MeshTile target = m_tiles[tileIndex];
            // Remove from freelist
            if (!availableTiles.Remove(target)) {
                // Could not find the correct location.
                throw new Exception("Could not find tile");
            }
            tile = target;
            // Restore salt.
            tile.salt = decodePolyIdSalt(lastRef);
        }

        tile.data = data;
        tile.flags = flags;
        tile.links.Clear();
        tile.polyLinks = new int[data.polys.Length];
        Array.Fill(tile.polyLinks, NavMesh.DT_NULL_LINK);

        // Insert tile into the position lut.
        getTileListByPos(header.x, header.y).Add(tile);

        // Patch header pointers.

        // If there are no items in the bvtree, reset the tree pointer.
        if (tile.data.bvTree != null && tile.data.bvTree.Length == 0) {
            tile.data.bvTree = null;
        }

        // Init tile.

        connectIntLinks(tile);
        // Base off-mesh connections to their starting polygons and connect connections inside the tile.
        baseOffMeshLinks(tile);
        connectExtOffMeshLinks(tile, tile, -1);

        // Connect with layers in current tile.
        List<MeshTile> neis = getTilesAt(header.x, header.y);
        for (int j = 0; j < neis.Count; ++j) {
            if (neis[j] == tile) {
                continue;
            }
            connectExtLinks(tile, neis[j], -1);
            connectExtLinks(neis[j], tile, -1);
            connectExtOffMeshLinks(tile, neis[j], -1);
            connectExtOffMeshLinks(neis[j], tile, -1);
        }

        // Connect with neighbour tiles.
        for (int i = 0; i < 8; ++i) {
            neis = getNeighbourTilesAt(header.x, header.y, i);
            for (int j = 0; j < neis.Count; ++j) {
                connectExtLinks(tile, neis[j], i);
                connectExtLinks(neis[j], tile, oppositeTile(i));
                connectExtOffMeshLinks(tile, neis[j], i);
                connectExtOffMeshLinks(neis[j], tile, oppositeTile(i));
            }
        }

        return getTileRef(tile);
    }

    /// Removes the specified tile from the navigation mesh.
    /// @param[in] ref The reference of the tile to remove.
    /// @param[out] data Data associated with deleted tile.
    /// @param[out] dataSize Size of the data associated with deleted tile.
    ///
    /// This function returns the data for the tile so that, if desired,
    /// it can be added back to the navigation mesh at a later point.
    ///
    /// @see #addTile
    public long removeTile(long refs) {
        if (refs == 0) {
            return 0;
        }
        int tileIndex = decodePolyIdTile(refs);
        int tileSalt = decodePolyIdSalt(refs);
        if (tileIndex >= m_maxTiles) {
            throw new Exception("Invalid tile index");
        }
        MeshTile tile = m_tiles[tileIndex];
        if (tile.salt != tileSalt) {
            throw new Exception("Invalid tile salt");
        }

        // Remove tile from hash lookup.
        getTileListByPos(tile.data.header.x, tile.data.header.y).Remove(tile);

        // Remove connections to neighbour tiles.
        // Create connections with neighbour tiles.

        // Disconnect from other layers in current tile.
        List<MeshTile> nneis = getTilesAt(tile.data.header.x, tile.data.header.y);
        foreach (MeshTile j in nneis) {
            if (j == tile) {
                continue;
            }
            unconnectLinks(j, tile);
        }

        // Disconnect from neighbour tiles.
        for (int i = 0; i < 8; ++i) {
            nneis = getNeighbourTilesAt(tile.data.header.x, tile.data.header.y, i);
            foreach (MeshTile j in nneis) {
                unconnectLinks(j, tile);
            }
        }
        // Reset tile.
        tile.data = null;

        tile.flags = 0;
        tile.links.Clear();
        tile.linksFreeList=NavMesh.DT_NULL_LINK;

        // Update salt, salt should never be zero.
        tile.salt = (tile.salt + 1) & ((1 << DT_SALT_BITS) - 1);
        if (tile.salt == 0) {
            tile.salt++;
        }

        // Add to free list.
        availableTiles.AddFirst(tile);
        m_tileCount--;
        return getTileRef(tile);
    }

    /// Builds internal polygons links for a tile.
    void connectIntLinks(MeshTile tile) {
        if (tile == null) {
            return;
        }

        long @base = getPolyRefBase(tile);

        for (int i = 0; i < tile.data.header.polyCount; ++i) {
            Poly poly = tile.data.polys[i];
            tile.polyLinks[poly.index] = DT_NULL_LINK;

            if (poly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
                continue;
            }

            // Build edge links backwards so that the links will be
            // in the linked list from lowest index to highest.
            for (int j = poly.vertCount - 1; j >= 0; --j) {
                // Skip hard and non-internal edges.
                if (poly.neis[j] == 0 || (poly.neis[j] & DT_EXT_LINK) != 0) {
                    continue;
                }

                int idx = allocLink(tile);
                Link link = tile.links[idx];
                link.refs = @base | (poly.neis[j] - 1);
                link.edge = j;
                link.side = 0xff;
                link.bmin = link.bmax = 0;
                // Add to linked list.
                link.next = tile.polyLinks[poly.index];
                tile.polyLinks[poly.index] = idx;
            }
        }
    }

    void unconnectLinks(MeshTile tile, MeshTile target) {
        if (tile == null || target == null) {
            return;
        }

        int targetNum = decodePolyIdTile(getTileRef(target));

        for (int i = 0; i < tile.data.header.polyCount; ++i) {
            Poly poly = tile.data.polys[i];
            int j = tile.polyLinks[poly.index];
            int pj = DT_NULL_LINK;
            while (j != DT_NULL_LINK) {
                if (decodePolyIdTile(tile.links[j].refs) == targetNum) {
                    // Remove link.
                    int nj = tile.links[j].next;
                    if (pj == DT_NULL_LINK) {
                        tile.polyLinks[poly.index] = nj;
                    } else {
                        tile.links[pj].next = nj;
                    }
                    freeLink(tile, j);
                    j = nj;
                } else {
                    // Advance
                    pj = j;
                    j = tile.links[j].next;
                }
            }
        }
    }

    void connectExtLinks(MeshTile tile, MeshTile target, int side) {
        if (tile == null) {
            return;
        }

        // Connect border links.
        for (int i = 0; i < tile.data.header.polyCount; ++i) {
            Poly poly = tile.data.polys[i];

            // Create new links.
            // short m = DT_EXT_LINK | (short)side;

            int nv = poly.vertCount;
            for (int j = 0; j < nv; ++j) {
                // Skip non-portal edges.
                if ((poly.neis[j] & DT_EXT_LINK) == 0) {
                    continue;
                }

                int dir = poly.neis[j] & 0xff;
                if (side != -1 && dir != side) {
                    continue;
                }

                // Create new links
                int va = poly.verts[j] * 3;
                int vb = poly.verts[(j + 1) % nv] * 3;
                IList<Tuple<long,float,float>> connectedPolys = findConnectingPolys(tile.data.verts, va, vb, target,
                        oppositeTile(dir));
                foreach (Tuple<long,float,float> connectedPoly in connectedPolys) {
                    int idx = allocLink(tile);
                    Link link = tile.links[idx];
                    link.refs = connectedPoly.Item1;
                    link.edge = j;
                    link.side = dir;

                    link.next = tile.polyLinks[poly.index];
                    tile.polyLinks[poly.index] = idx;

                    // Compress portal limits to a byte value.
                    if (dir == 0 || dir == 4) {
                        float tmin = (connectedPoly.Item2 - tile.data.verts[va + 2])
                                / (tile.data.verts[vb + 2] - tile.data.verts[va + 2]);
                        float tmax = (connectedPoly.Item3 - tile.data.verts[va + 2])
                                / (tile.data.verts[vb + 2] - tile.data.verts[va + 2]);
                        if (tmin > tmax) {
                            float temp = tmin;
                            tmin = tmax;
                            tmax = temp;
                        }
                        link.bmin = (int)Math.Round(clamp(tmin, 0.0f, 1.0f) * 255.0f);
                        link.bmax = (int)Math.Round(clamp(tmax, 0.0f, 1.0f) * 255.0f);
                    } else if (dir == 2 || dir == 6) {
                        float tmin = (connectedPoly.Item2 - tile.data.verts[va])
                                / (tile.data.verts[vb] - tile.data.verts[va]);
                        float tmax = (connectedPoly.Item3 - tile.data.verts[va])
                                / (tile.data.verts[vb] - tile.data.verts[va]);
                        if (tmin > tmax) {
                            float temp = tmin;
                            tmin = tmax;
                            tmax = temp;
                        }
                        link.bmin = (int)Math.Round(clamp(tmin, 0.0f, 1.0f) * 255.0f);
                        link.bmax = (int)Math.Round(clamp(tmax, 0.0f, 1.0f) * 255.0f);
                    }
                }
            }
        }
    }

    void connectExtOffMeshLinks(MeshTile tile, MeshTile target, int side) {
        if (tile == null) {
            return;
        }

        // Connect off-mesh links.
        // We are interested on links which land from target tile to this tile.
        int oppositeSide = (side == -1) ? 0xff : oppositeTile(side);

        for (int i = 0; i < target.data.header.offMeshConCount; ++i) {
            OffMeshConnection targetCon = target.data.offMeshCons[i];
            if (targetCon.side != oppositeSide) {
                continue;
            }

            Poly targetPoly = target.data.polys[targetCon.poly];
            // Skip off-mesh connections which start location could not be
            // connected at all.
            if (target.polyLinks[targetPoly.index] == DT_NULL_LINK) {
                continue;
            }

            float[] ext = new float[] { targetCon.rad, target.data.header.walkableClimb, targetCon.rad };

            // Find polygon to connect to.
            float[] p = new float[3];
            p[0] = targetCon.pos[3];
            p[1] = targetCon.pos[4];
            p[2] = targetCon.pos[5];
            FindNearestPolyResult nearest = findNearestPolyInTile(tile, p, ext);
            long refs = nearest.getNearestRef();
            if (refs == 0) {
                continue;
            }
            float[] nearestPt = nearest.getNearestPos();
            // findNearestPoly may return too optimistic results, further check
            // to make sure.

            if (sqr(nearestPt[0] - p[0]) + sqr(nearestPt[2] - p[2]) > sqr(targetCon.rad)) {
                continue;
            }
            // Make sure the location is on current mesh.
            target.data.verts[targetPoly.verts[1] * 3] = nearestPt[0];
            target.data.verts[targetPoly.verts[1] * 3 + 1] = nearestPt[1];
            target.data.verts[targetPoly.verts[1] * 3 + 2] = nearestPt[2];

            // Link off-mesh connection to target poly.
            int idx = allocLink(target);
            Link link = target.links[idx];
            link.refs = refs;
            link.edge = 1;
            link.side = oppositeSide;
            link.bmin = link.bmax = 0;
            // Add to linked list.
            link.next = target.polyLinks[targetPoly.index];
            target.polyLinks[targetPoly.index] = idx;

            // Link target poly to off-mesh connection.
            if ((targetCon.flags & DT_OFFMESH_CON_BIDIR) != 0) {
                int tidx = allocLink(tile);
                int landPolyIdx = decodePolyIdPoly(refs);
                Poly landPoly = tile.data.polys[landPolyIdx];
                link = tile.links[tidx];
                link.refs = getPolyRefBase(target) | (targetCon.poly);
                link.edge = 0xff;
                link.side = (side == -1 ? 0xff : side);
                link.bmin = link.bmax = 0;
                // Add to linked list.
                link.next = tile.polyLinks[landPoly.index];
                tile.polyLinks[landPoly.index] = tidx;
            }
        }
    }

    private IList<Tuple<long, float, float>> findConnectingPolys(float[] verts, int va, int vb, MeshTile tile, int side) {
        if (tile == null) {
            return ImmutableArray<Tuple<long, float, float>>.Empty;
        }
        List<Tuple<long, float, float>> result = new List<Tuple<long, float, float>>();
        float[] amin = new float[2];
        float[] amax = new float[2];
        calcSlabEndPoints(verts, va, vb, amin, amax, side);
        float apos = getSlabCoord(verts, va, side);

        // Remove links pointing to 'side' and compact the links array.
        float[] bmin = new float[2];
        float[] bmax = new float[2];
        int m = DT_EXT_LINK | side;
        long @base = getPolyRefBase(tile);

        for (int i = 0; i < tile.data.header.polyCount; ++i) {
            Poly poly = tile.data.polys[i];
            int nv = poly.vertCount;
            for (int j = 0; j < nv; ++j) {
                // Skip edges which do not point to the right side.
                if (poly.neis[j] != m) {
                    continue;
                }
                int vc = poly.verts[j] * 3;
                int vd = poly.verts[(j + 1) % nv] * 3;
                float bpos = getSlabCoord(tile.data.verts, vc, side);
                // Segments are not close enough.
                if (Math.Abs(apos - bpos) > 0.01f) {
                    continue;
                }

                // Check if the segments touch.
                calcSlabEndPoints(tile.data.verts, vc, vd, bmin, bmax, side);

                if (!overlapSlabs(amin, amax, bmin, bmax, 0.01f, tile.data.header.walkableClimb)) {
                    continue;
                }

                // Add return value.
                result.Add(Tuple.Create(@base | i, Math.Max(amin[0], bmin[0]), Math.Min(amax[0], bmax[0])));
                break;
            }
        }
        return result;
    }

    static float getSlabCoord(float[] verts, int va, int side) {
        if (side == 0 || side == 4) {
            return verts[va];
        } else if (side == 2 || side == 6) {
            return verts[va + 2];
        }
        return 0;
    }

    static void calcSlabEndPoints(float[] verts, int va, int vb, float[] bmin, float[] bmax, int side) {
        if (side == 0 || side == 4) {
            if (verts[va + 2] < verts[vb + 2]) {
                bmin[0] = verts[va + 2];
                bmin[1] = verts[va + 1];
                bmax[0] = verts[vb + 2];
                bmax[1] = verts[vb + 1];
            } else {
                bmin[0] = verts[vb + 2];
                bmin[1] = verts[vb + 1];
                bmax[0] = verts[va + 2];
                bmax[1] = verts[va + 1];
            }
        } else if (side == 2 || side == 6) {
            if (verts[va + 0] < verts[vb + 0]) {
                bmin[0] = verts[va + 0];
                bmin[1] = verts[va + 1];
                bmax[0] = verts[vb + 0];
                bmax[1] = verts[vb + 1];
            } else {
                bmin[0] = verts[vb + 0];
                bmin[1] = verts[vb + 1];
                bmax[0] = verts[va + 0];
                bmax[1] = verts[va + 1];
            }
        }
    }

    bool overlapSlabs(float[] amin, float[] amax, float[] bmin, float[] bmax, float px, float py) {
        // Check for horizontal overlap.
        // The segment is shrunken a little so that slabs which touch
        // at end points are not connected.
        float minx = Math.Max(amin[0] + px, bmin[0] + px);
        float maxx = Math.Min(amax[0] - px, bmax[0] - px);
        if (minx > maxx) {
            return false;
        }

        // Check vertical overlap.
        float ad = (amax[1] - amin[1]) / (amax[0] - amin[0]);
        float ak = amin[1] - ad * amin[0];
        float bd = (bmax[1] - bmin[1]) / (bmax[0] - bmin[0]);
        float bk = bmin[1] - bd * bmin[0];
        float aminy = ad * minx + ak;
        float amaxy = ad * maxx + ak;
        float bminy = bd * minx + bk;
        float bmaxy = bd * maxx + bk;
        float dmin = bminy - aminy;
        float dmax = bmaxy - amaxy;

        // Crossing segments always overlap.
        if (dmin * dmax < 0) {
            return true;
        }

        // Check for overlap at endpoints.
        float thr = (py * 2) * (py * 2);
        if (dmin * dmin <= thr || dmax * dmax <= thr) {
            return true;
        }

        return false;
    }

    /**
     * Builds internal polygons links for a tile.
     *
     * @param tile
     */
    void baseOffMeshLinks(MeshTile tile) {
        if (tile == null) {
            return;
        }

        long @base = getPolyRefBase(tile);

        // Base off-mesh connection start points.
        for (int i = 0; i < tile.data.header.offMeshConCount; ++i) {
            OffMeshConnection con = tile.data.offMeshCons[i];
            Poly poly = tile.data.polys[con.poly];

            float[] ext = new float[] { con.rad, tile.data.header.walkableClimb, con.rad };

            // Find polygon to connect to.
            FindNearestPolyResult nearestPoly = findNearestPolyInTile(tile, con.pos, ext);
            long refs = nearestPoly.getNearestRef();
            if (refs == 0) {
                continue;
            }
            float[] p = con.pos; // First vertex
            float[] nearestPt = nearestPoly.getNearestPos();
            // findNearestPoly may return too optimistic results, further check
            // to make sure.
            if (sqr(nearestPt[0] - p[0]) + sqr(nearestPt[2] - p[2]) > sqr(con.rad)) {
                continue;
            }
            // Make sure the location is on current mesh.
            tile.data.verts[poly.verts[0] * 3] = nearestPt[0];
            tile.data.verts[poly.verts[0] * 3 + 1] = nearestPt[1];
            tile.data.verts[poly.verts[0] * 3 + 2] = nearestPt[2];

            // Link off-mesh connection to target poly.
            int idx = allocLink(tile);
            Link link = tile.links[idx];
            link.refs = refs;
            link.edge = 0;
            link.side = 0xff;
            link.bmin = link.bmax = 0;
            // Add to linked list.
            link.next = tile.polyLinks[poly.index];
            tile.polyLinks[poly.index] = idx;

            // Start end-point is always connect back to off-mesh connection.
            int tidx = allocLink(tile);
            int landPolyIdx = decodePolyIdPoly(refs);
            Poly landPoly = tile.data.polys[landPolyIdx];
            link = tile.links[tidx];
            link.refs = @base | (con.poly);
            link.edge = 0xff;
            link.side = 0xff;
            link.bmin = link.bmax = 0;
            // Add to linked list.
            link.next = tile.polyLinks[landPoly.index];
            tile.polyLinks[landPoly.index] = tidx;
        }
    }

    /**
     * Returns closest point on polygon.
     *
     * @param ref
     * @param pos
     * @return
     */
    float[] closestPointOnDetailEdges(MeshTile tile, Poly poly, float[] pos, bool onlyBoundary) {
        int ANY_BOUNDARY_EDGE = (DT_DETAIL_EDGE_BOUNDARY << 0) | (DT_DETAIL_EDGE_BOUNDARY << 2)
                | (DT_DETAIL_EDGE_BOUNDARY << 4);
        int ip = poly.index;
        float dmin = float.MaxValue;
        float tmin = 0;
        float[] pmin = null;
        float[] pmax = null;

        if (tile.data.detailMeshes != null) {

            PolyDetail pd = tile.data.detailMeshes[ip];
            for (int i = 0; i < pd.triCount; i++) {
                int ti = (pd.triBase + i) * 4;
                int[] tris = tile.data.detailTris;
                if (onlyBoundary && (tris[ti + 3] & ANY_BOUNDARY_EDGE) == 0) {
                    continue;
                }

                float[][] v = new float[3][];
                for (int j = 0; j < 3; ++j) {
                    if (tris[ti + j] < poly.vertCount) {
                        int index = poly.verts[tris[ti + j]] * 3;
                        v[j] = new float[] { tile.data.verts[index], tile.data.verts[index + 1],
                                tile.data.verts[index + 2] };
                    } else {
                        int index = (pd.vertBase + (tris[ti + j] - poly.vertCount)) * 3;
                        v[j] = new float[] { tile.data.detailVerts[index], tile.data.detailVerts[index + 1],
                                tile.data.detailVerts[index + 2] };
                    }
                }

                for (int k = 0, j = 2; k < 3; j = k++) {
                    if ((getDetailTriEdgeFlags(tris[ti + 3], j) & DT_DETAIL_EDGE_BOUNDARY) == 0
                            && (onlyBoundary || tris[ti + j] < tris[ti + k])) {
                        // Only looking at boundary edges and this is internal, or
                        // this is an inner edge that we will see again or have already seen.
                        continue;
                    }

                    Tuple<float, float> dt = distancePtSegSqr2D(pos, v[j], v[k]);
                    float d = dt.Item1;
                    float t = dt.Item2;
                    if (d < dmin) {
                        dmin = d;
                        tmin = t;
                        pmin = v[j];
                        pmax = v[k];
                    }
                }
            }
        } else {
            float[][] v = ArrayUtils.Of<float>(2, 3);
            for (int j = 0; j < poly.vertCount; ++j) {
                int k = (j + 1) % poly.vertCount;
                v[0][0] = tile.data.verts[poly.verts[j] * 3];
                v[0][1] = tile.data.verts[poly.verts[j] * 3 + 1];
                v[0][2] = tile.data.verts[poly.verts[j] * 3 + 2];
                v[1][0] = tile.data.verts[poly.verts[k] * 3];
                v[1][1] = tile.data.verts[poly.verts[k] * 3 + 1];
                v[1][2] = tile.data.verts[poly.verts[k] * 3 + 2];

                Tuple<float, float> dt = distancePtSegSqr2D(pos, v[0], v[1]);
                float d = dt.Item1;
                float t = dt.Item2;
                if (d < dmin) {
                    dmin = d;
                    tmin = t;
                    pmin = v[0];
                    pmax = v[1];
                }
            }
        }

        return vLerp(pmin, pmax, tmin);
    }

    public float? getPolyHeight(MeshTile tile, Poly poly, float[] pos) {
        // Off-mesh connections do not have detail polys and getting height
        // over them does not make sense.
        if (poly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
            return null;
        }

        int ip = poly.index;

        float[] verts = new float[m_maxVertPerPoly * 3];
        int nv = poly.vertCount;
        for (int i = 0; i < nv; ++i) {
            Array.Copy(tile.data.verts, poly.verts[i] * 3, verts, i * 3, 3);
        }

        if (!pointInPolygon(pos, verts, nv)) {
            return null;
        }

        // Find height at the location.
        if (tile.data.detailMeshes != null) {
            PolyDetail pd = tile.data.detailMeshes[ip];
            for (int j = 0; j < pd.triCount; ++j) {
                int t = (pd.triBase + j) * 4;
                float[][] v = new float[3][];
                for (int k = 0; k < 3; ++k) {
                    if (tile.data.detailTris[t + k] < poly.vertCount) {
                        int index = poly.verts[tile.data.detailTris[t + k]] * 3;
                        v[k] = new float[] { tile.data.verts[index], tile.data.verts[index + 1],
                                tile.data.verts[index + 2] };
                    } else {
                        int index = (pd.vertBase + (tile.data.detailTris[t + k] - poly.vertCount)) * 3;
                        v[k] = new float[] { tile.data.detailVerts[index], tile.data.detailVerts[index + 1],
                                tile.data.detailVerts[index + 2] };
                    }
                }
                float? h = closestHeightPointTriangle(pos, v[0], v[1], v[2]);
                if (null != h) {
                    return h;
                }
            }
        } else {
            float[][] v = ArrayUtils.Of<float>(3, 3);
            v[0][0] = tile.data.verts[poly.verts[0] * 3];
            v[0][1] = tile.data.verts[poly.verts[0] * 3 + 1];
            v[0][2] = tile.data.verts[poly.verts[0] * 3 + 2];
            for (int j = 1; j < poly.vertCount - 1; ++j) {
                for (int k = 0; k < 2; ++k) {
                    v[k + 1][0] = tile.data.verts[poly.verts[j + k] * 3];
                    v[k + 1][1] = tile.data.verts[poly.verts[j + k] * 3 + 1];
                    v[k + 1][2] = tile.data.verts[poly.verts[j + k] * 3 + 2];
                }
                float? h = closestHeightPointTriangle(pos, v[0], v[1], v[2]);
                if (null != h) {
                    return h;
                }
            }
        }

        // If all triangle checks failed above (can happen with degenerate triangles
        // or larger floating point values) the point is on an edge, so just select
        // closest. This should almost never happen so the extra iteration here is
        // ok.
        float[] closest = closestPointOnDetailEdges(tile, poly, pos, false);
        return closest[1];
    }

    public ClosestPointOnPolyResult closestPointOnPoly(long refs, float[] pos) {
        Tuple<MeshTile, Poly> tileAndPoly = getTileAndPolyByRefUnsafe(refs);
        MeshTile tile = tileAndPoly.Item1;
        Poly poly = tileAndPoly.Item2;
        float[] closest = new float[3];
        vCopy(closest, pos);
        float? h = getPolyHeight(tile, poly, pos);
        if (null != h) {
            closest[1] = h.Value;
            return new ClosestPointOnPolyResult(true, closest);
        }

        // Off-mesh connections don't have detail polygons.
        if (poly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
            int i = poly.verts[0] * 3;
            float[] v0 = new float[] { tile.data.verts[i], tile.data.verts[i + 1], tile.data.verts[i + 2] };
            i = poly.verts[1] * 3;
            float[] v1 = new float[] { tile.data.verts[i], tile.data.verts[i + 1], tile.data.verts[i + 2] };
            Tuple<float, float> dt = distancePtSegSqr2D(pos, v0, v1);
            return new ClosestPointOnPolyResult(false, vLerp(v0, v1, dt.Item2));
        }
        // Outside poly that is not an offmesh connection.
        return new ClosestPointOnPolyResult(false, closestPointOnDetailEdges(tile, poly, pos, true));
    }

    FindNearestPolyResult findNearestPolyInTile(MeshTile tile, float[] center, float[] extents) {
        float[] nearestPt = null;
        bool overPoly = false;
        float[] bmin = vSub(center, extents);
        float[] bmax = vAdd(center, extents);

        // Get nearby polygons from proximity grid.
        List<long> polys = queryPolygonsInTile(tile, bmin, bmax);

        // Find nearest polygon amongst the nearby polygons.
        long nearest = 0;
        float nearestDistanceSqr = float.MaxValue;
        for (int i = 0; i < polys.Count; ++i) {
            long refs = polys[i];
            float d;
            ClosestPointOnPolyResult cpp = closestPointOnPoly(refs, center);
            bool posOverPoly = cpp.isPosOverPoly();
            float[] closestPtPoly = cpp.getClosest();

            // If a point is directly over a polygon and closer than
            // climb height, favor that instead of straight line nearest point.
            float[] diff = vSub(center, closestPtPoly);
            if (posOverPoly) {
                d = Math.Abs(diff[1]) - tile.data.header.walkableClimb;
                d = d > 0 ? d * d : 0;
            } else {
                d = vLenSqr(diff);
            }
            if (d < nearestDistanceSqr) {
                nearestPt = closestPtPoly;
                nearestDistanceSqr = d;
                nearest = refs;
                overPoly = posOverPoly;
            }
        }
        return new FindNearestPolyResult(nearest, nearestPt, overPoly);
    }

    MeshTile getTileAt(int x, int y, int layer) {
        foreach (MeshTile tile in getTileListByPos(x, y)) {
            if (tile.data.header != null && tile.data.header.x == x && tile.data.header.y == y
                    && tile.data.header.layer == layer) {
                return tile;
            }
        }
        return null;
    }

    List<MeshTile> getNeighbourTilesAt(int x, int y, int side) {
        int nx = x, ny = y;
        switch (side) {
        case 0:
            nx++;
            break;
        case 1:
            nx++;
            ny++;
            break;
        case 2:
            ny++;
            break;
        case 3:
            nx--;
            ny++;
            break;
        case 4:
            nx--;
            break;
        case 5:
            nx--;
            ny--;
            break;
        case 6:
            ny--;
            break;
        case 7:
            nx++;
            ny--;
            break;
        }
        return getTilesAt(nx, ny);
    }

    public List<MeshTile> getTilesAt(int x, int y) {
        List<MeshTile> tiles = new List<MeshTile>();
        foreach (MeshTile tile in getTileListByPos(x, y)) {
            if (tile.data.header != null && tile.data.header.x == x && tile.data.header.y == y) {
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    public long getTileRefAt(int x, int y, int layer) {
        return getTileRef(getTileAt(x, y, layer));
    }

    public MeshTile getTileByRef(long refs) {
        if (refs == 0) {
            return null;
        }
        int tileIndex = decodePolyIdTile(refs);
        int tileSalt = decodePolyIdSalt(refs);
        if (tileIndex >= m_maxTiles) {
            return null;
        }
        MeshTile tile = m_tiles[tileIndex];
        if (tile.salt != tileSalt) {
            return null;
        }
        return tile;
    }

    public long getTileRef(MeshTile tile) {
        if (tile == null) {
            return 0;
        }
        return encodePolyId(tile.salt, tile.index, 0);
    }

    public static int computeTileHash(int x, int y, int mask) {
        uint h1 = 0x8da6b343; // Large multiplicative constants;
        uint h2 = 0xd8163841; // here arbitrarily chosen primes
        uint n = h1 * (uint)x + h2 * (uint)y;
        return (int)(n & mask);
    }

    /// @par
    ///
    /// Off-mesh connections are stored in the navigation mesh as special
    /// 2-vertex
    /// polygons with a single edge. At least one of the vertices is expected to
    /// be
    /// inside a normal polygon. So an off-mesh connection is "entered" from a
    /// normal polygon at one of its endpoints. This is the polygon identified
    /// by
    /// the prevRef parameter.
    public Result<Tuple<float[], float[]>> getOffMeshConnectionPolyEndPoints(long prevRef, long polyRef) {
        if (polyRef == 0) {
            return Results.invalidParam<Tuple<float[], float[]>>("polyRef = 0");
        }

        // Get current polygon
        int[] saltitip = decodePolyId(polyRef);
        int salt = saltitip[0];
        int it = saltitip[1];
        int ip = saltitip[2];
        if (it >= m_maxTiles) {
            return Results.invalidParam<Tuple<float[], float[]>>("Invalid tile ID > max tiles");
        }
        if (m_tiles[it].salt != salt || m_tiles[it].data.header == null) {
            return Results.invalidParam<Tuple<float[], float[]>>("Invalid salt or missing tile header");
        }
        MeshTile tile = m_tiles[it];
        if (ip >= tile.data.header.polyCount) {
            return Results.invalidParam<Tuple<float[], float[]>>("Invalid poly ID > poly count");
        }
        Poly poly = tile.data.polys[ip];

        // Make sure that the current poly is indeed off-mesh link.
        if (poly.getType() != Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
            return Results.invalidParam<Tuple<float[], float[]>>("Invalid poly type");
        }

        // Figure out which way to hand out the vertices.
        int idx0 = 0, idx1 = 1;

        // Find link that points to first vertex.
        for (int i = tile.polyLinks[poly.index]; i != DT_NULL_LINK; i = tile.links[i].next) {
            if (tile.links[i].edge == 0) {
                if (tile.links[i].refs != prevRef) {
                    idx0 = 1;
                    idx1 = 0;
                }
                break;
            }
        }
        float[] startPos = new float[3];
        float[] endPos = new float[3];
        vCopy(startPos, tile.data.verts, poly.verts[idx0] * 3);
        vCopy(endPos, tile.data.verts, poly.verts[idx1] * 3);
        return Results.success(Tuple.Create(startPos, endPos));

    }

    public int getMaxVertsPerPoly() {
        return m_maxVertPerPoly;
    }

    public int getTileCount() {
        return m_tileCount;
    }

    public Status setPolyFlags(long refs, int flags) {
        if (refs == 0) {
            return Status.FAILURE;
        }
        int[] saltTilePoly = decodePolyId(refs);
        int salt = saltTilePoly[0];
        int it = saltTilePoly[1];
        int ip = saltTilePoly[2];
        if (it >= m_maxTiles) {
            return Status.FAILURE_INVALID_PARAM;
        }
        if (m_tiles[it].salt != salt || m_tiles[it].data == null || m_tiles[it].data.header == null) {
            return Status.FAILURE_INVALID_PARAM;
        }
        MeshTile tile = m_tiles[it];
        if (ip >= tile.data.header.polyCount) {
            return Status.FAILURE_INVALID_PARAM;
        }
        Poly poly = tile.data.polys[ip];

        // Change flags.
        poly.flags = flags;
        return Status.SUCCSESS;
    }

    public Result<int> getPolyFlags(long refs) {
        if (refs == 0) {
            return Results.failure<int>();
        }
        int[] saltTilePoly = decodePolyId(refs);
        int salt = saltTilePoly[0];
        int it = saltTilePoly[1];
        int ip = saltTilePoly[2];
        if (it >= m_maxTiles) {
            return Results.invalidParam<int>();
        }
        if (m_tiles[it].salt != salt || m_tiles[it].data == null || m_tiles[it].data.header == null) {
            return Results.invalidParam<int>();
        }
        MeshTile tile = m_tiles[it];
        if (ip >= tile.data.header.polyCount) {
            return Results.invalidParam<int>();
        }
        Poly poly = tile.data.polys[ip];

        return Results.success(poly.flags);
    }

    public Status setPolyArea(long refs, char area) {
        if (refs == 0) {
            return Status.FAILURE;
        }
        int[] saltTilePoly = decodePolyId(refs);
        int salt = saltTilePoly[0];
        int it = saltTilePoly[1];
        int ip = saltTilePoly[2];
        if (it >= m_maxTiles) {
            return Status.FAILURE;
        }
        if (m_tiles[it].salt != salt || m_tiles[it].data == null || m_tiles[it].data.header == null) {
            return Status.FAILURE_INVALID_PARAM;
        }
        MeshTile tile = m_tiles[it];
        if (ip >= tile.data.header.polyCount) {
            return Status.FAILURE_INVALID_PARAM;
        }
        Poly poly = tile.data.polys[ip];

        poly.setArea(area);

        return Status.SUCCSESS;
    }

    public Result<int> getPolyArea(long refs) {
        if (refs == 0) {
            return Results.failure<int>();
        }
        int[] saltTilePoly = decodePolyId(refs);
        int salt = saltTilePoly[0];
        int it = saltTilePoly[1];
        int ip = saltTilePoly[2];
        if (it >= m_maxTiles) {
            return Results.invalidParam<int>();
        }
        if (m_tiles[it].salt != salt || m_tiles[it].data == null || m_tiles[it].data.header == null) {
            return Results.invalidParam<int>();
        }
        MeshTile tile = m_tiles[it];
        if (ip >= tile.data.header.polyCount) {
            return Results.invalidParam<int>();
        }
        Poly poly = tile.data.polys[ip];

        return Results.success(poly.getArea());
    }

    /**
     * Get flags for edge in detail triangle.
     *
     * @param triFlags
     *            The flags for the triangle (last component of detail vertices above).
     * @param edgeIndex
     *            The index of the first vertex of the edge. For instance, if 0,
     * @return flags for edge AB.
     */
    public static int getDetailTriEdgeFlags(int triFlags, int edgeIndex) {
        return (triFlags >> (edgeIndex * 2)) & 0x3;
    }

    private List<MeshTile> getTileListByPos(int x, int z)
    {
        var tileHash = computeTileHash(x, z, m_tileLutMask);
        if (!posLookup.TryGetValue(tileHash, out var tiles))
        {
            tiles = new List<MeshTile>();
            posLookup.Add(tileHash, tiles);
        }

        return tiles;
    }
}

}