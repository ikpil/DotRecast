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
using DotRecast.Core;
using DotRecast.Detour.TileCache.Io;
using static DotRecast.Core.RecastMath;

namespace DotRecast.Detour.TileCache
{
    public class TileCache
    {
        int m_tileLutSize;

        /// < Tile hash lookup size (must be pot).
        int m_tileLutMask;

        /// < Tile hash lookup mask.
        private readonly CompressedTile[] m_posLookup;

        /// < Tile hash lookup.
        private CompressedTile m_nextFreeTile;

        /// < Freelist of tiles.
        private readonly CompressedTile[] m_tiles;

        /// < List of tiles. // TODO: (PP) replace with list
        private readonly int m_saltBits;

        /// < Number of salt bits in the tile ID.
        private readonly int m_tileBits;

        /// < Number of tile bits in the tile ID.
        private readonly NavMesh m_navmesh;

        private readonly TileCacheParams m_params;
        private readonly TileCacheStorageParams m_storageParams;

        private readonly TileCacheCompressor m_tcomp;
        private readonly TileCacheMeshProcess m_tmproc;

        private readonly List<TileCacheObstacle> m_obstacles = new List<TileCacheObstacle>();
        private TileCacheObstacle m_nextFreeObstacle;

        private readonly List<ObstacleRequest> m_reqs = new List<ObstacleRequest>();
        private readonly List<long> m_update = new List<long>();

        private readonly TileCacheBuilder builder = new TileCacheBuilder();
        private readonly TileCacheLayerHeaderReader tileReader = new TileCacheLayerHeaderReader();

        private bool contains(List<long> a, long v)
        {
            return a.Contains(v);
        }

        /// Encodes a tile id.
        private long encodeTileId(int salt, int it)
        {
            return ((long)salt << m_tileBits) | it;
        }

        /// Decodes a tile salt.
        private int decodeTileIdSalt(long refs)
        {
            long saltMask = (1L << m_saltBits) - 1;
            return (int)((refs >> m_tileBits) & saltMask);
        }

        /// Decodes a tile id.
        private int decodeTileIdTile(long refs)
        {
            long tileMask = (1L << m_tileBits) - 1;
            return (int)(refs & tileMask);
        }

        /// Encodes an obstacle id.
        private long encodeObstacleId(int salt, int it)
        {
            return ((long)salt << 16) | it;
        }

        /// Decodes an obstacle salt.
        private int decodeObstacleIdSalt(long refs)
        {
            long saltMask = ((long)1 << 16) - 1;
            return (int)((refs >> 16) & saltMask);
        }

        /// Decodes an obstacle id.
        private int decodeObstacleIdObstacle(long refs)
        {
            long tileMask = ((long)1 << 16) - 1;
            return (int)(refs & tileMask);
        }

        public TileCache(TileCacheParams option, TileCacheStorageParams storageParams, NavMesh navmesh,
            TileCacheCompressor tcomp, TileCacheMeshProcess tmprocs)
        {
            m_params = option;
            m_storageParams = storageParams;
            m_navmesh = navmesh;
            m_tcomp = tcomp;
            m_tmproc = tmprocs;

            m_tileLutSize = nextPow2(m_params.maxTiles / 4);
            if (m_tileLutSize == 0)
            {
                m_tileLutSize = 1;
            }

            m_tileLutMask = m_tileLutSize - 1;
            m_tiles = new CompressedTile[m_params.maxTiles];
            m_posLookup = new CompressedTile[m_tileLutSize];
            for (int i = m_params.maxTiles - 1; i >= 0; --i)
            {
                m_tiles[i] = new CompressedTile(i);
                m_tiles[i].next = m_nextFreeTile;
                m_nextFreeTile = m_tiles[i];
            }

            m_tileBits = ilog2(nextPow2(m_params.maxTiles));
            m_saltBits = Math.Min(31, 32 - m_tileBits);
            if (m_saltBits < 10)
            {
                throw new Exception("Too few salt bits: " + m_saltBits);
            }
        }

        public CompressedTile getTileByRef(long refs)
        {
            if (refs == 0)
            {
                return null;
            }

            int tileIndex = decodeTileIdTile(refs);
            int tileSalt = decodeTileIdSalt(refs);
            if (tileIndex >= m_params.maxTiles)
            {
                return null;
            }

            CompressedTile tile = m_tiles[tileIndex];
            if (tile.salt != tileSalt)
            {
                return null;
            }

            return tile;
        }

        public List<long> getTilesAt(int tx, int ty)
        {
            List<long> tiles = new List<long>();

            // Find tile based on hash.
            int h = NavMesh.computeTileHash(tx, ty, m_tileLutMask);
            CompressedTile tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.header != null && tile.header.tx == tx && tile.header.ty == ty)
                {
                    tiles.Add(getTileRef(tile));
                }

                tile = tile.next;
            }

            return tiles;
        }

        CompressedTile getTileAt(int tx, int ty, int tlayer)
        {
            // Find tile based on hash.
            int h = NavMesh.computeTileHash(tx, ty, m_tileLutMask);
            CompressedTile tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.header != null && tile.header.tx == tx && tile.header.ty == ty && tile.header.tlayer == tlayer)
                {
                    return tile;
                }

                tile = tile.next;
            }

            return null;
        }

        public long getTileRef(CompressedTile tile)
        {
            if (tile == null)
            {
                return 0;
            }

            int it = tile.index;
            return encodeTileId(tile.salt, it);
        }

        public long getObstacleRef(TileCacheObstacle ob)
        {
            if (ob == null)
            {
                return 0;
            }

            int idx = ob.index;
            return encodeObstacleId(ob.salt, idx);
        }

        public TileCacheObstacle getObstacleByRef(long refs)
        {
            if (refs == 0)
            {
                return null;
            }

            int idx = decodeObstacleIdObstacle(refs);
            if (idx >= m_obstacles.Count)
            {
                return null;
            }

            TileCacheObstacle ob = m_obstacles[idx];
            int salt = decodeObstacleIdSalt(refs);
            if (ob.salt != salt)
            {
                return null;
            }

            return ob;
        }

        public long addTile(byte[] data, int flags)
        {
            // Make sure the data is in right format.
            ByteBuffer buf = new ByteBuffer(data);
            buf.order(m_storageParams.byteOrder);
            TileCacheLayerHeader header = tileReader.read(buf, m_storageParams.cCompatibility);
            // Make sure the location is free.
            if (getTileAt(header.tx, header.ty, header.tlayer) != null)
            {
                return 0;
            }

            // Allocate a tile.
            CompressedTile tile = null;
            if (m_nextFreeTile != null)
            {
                tile = m_nextFreeTile;
                m_nextFreeTile = tile.next;
                tile.next = null;
            }

            // Make sure we could allocate a tile.
            if (tile == null)
            {
                throw new Exception("Out of storage");
            }

            // Insert tile into the position lut.
            int h = NavMesh.computeTileHash(header.tx, header.ty, m_tileLutMask);
            tile.next = m_posLookup[h];
            m_posLookup[h] = tile;

            // Init tile.
            tile.header = header;
            tile.data = data;
            tile.compressed = align4(buf.position());
            tile.flags = flags;

            return getTileRef(tile);
        }

        private int align4(int i)
        {
            return (i + 3) & (~3);
        }

        public void removeTile(long refs)
        {
            if (refs == 0)
            {
                throw new Exception("Invalid tile ref");
            }

            int tileIndex = decodeTileIdTile(refs);
            int tileSalt = decodeTileIdSalt(refs);
            if (tileIndex >= m_params.maxTiles)
            {
                throw new Exception("Invalid tile index");
            }

            CompressedTile tile = m_tiles[tileIndex];
            if (tile.salt != tileSalt)
            {
                throw new Exception("Invalid tile salt");
            }

            // Remove tile from hash lookup.
            int h = NavMesh.computeTileHash(tile.header.tx, tile.header.ty, m_tileLutMask);
            CompressedTile prev = null;
            CompressedTile cur = m_posLookup[h];
            while (cur != null)
            {
                if (cur == tile)
                {
                    if (prev != null)
                    {
                        prev.next = cur.next;
                    }
                    else
                    {
                        m_posLookup[h] = cur.next;
                    }

                    break;
                }

                prev = cur;
                cur = cur.next;
            }

            tile.header = null;
            tile.data = null;
            tile.compressed = 0;
            tile.flags = 0;

            // Update salt, salt should never be zero.
            tile.salt = (tile.salt + 1) & ((1 << m_saltBits) - 1);
            if (tile.salt == 0)
            {
                tile.salt++;
            }

            // Add to free list.
            tile.next = m_nextFreeTile;
            m_nextFreeTile = tile;
        }

        // Cylinder obstacle
        public long addObstacle(Vector3f pos, float radius, float height)
        {
            TileCacheObstacle ob = allocObstacle();
            ob.type = TileCacheObstacle.TileCacheObstacleType.CYLINDER;

            vCopy(ref ob.pos, pos);
            ob.radius = radius;
            ob.height = height;

            return addObstacleRequest(ob).refs;
        }

        // Aabb obstacle
        public long addBoxObstacle(float[] bmin, float[] bmax)
        {
            TileCacheObstacle ob = allocObstacle();
            ob.type = TileCacheObstacle.TileCacheObstacleType.BOX;

            vCopy(ref ob.bmin, bmin);
            vCopy(ref ob.bmax, bmax);

            return addObstacleRequest(ob).refs;
        }

        // Box obstacle: can be rotated in Y
        public long addBoxObstacle(Vector3f center, Vector3f extents, float yRadians)
        {
            TileCacheObstacle ob = allocObstacle();
            ob.type = TileCacheObstacle.TileCacheObstacleType.ORIENTED_BOX;
            vCopy(ref ob.center, center);
            vCopy(ref ob.extents, extents);
            float coshalf = (float)Math.Cos(0.5f * yRadians);
            float sinhalf = (float)Math.Sin(-0.5f * yRadians);
            ob.rotAux[0] = coshalf * sinhalf;
            ob.rotAux[1] = coshalf * coshalf - 0.5f;
            return addObstacleRequest(ob).refs;
        }

        private ObstacleRequest addObstacleRequest(TileCacheObstacle ob)
        {
            ObstacleRequest req = new ObstacleRequest();
            req.action = ObstacleRequestAction.REQUEST_ADD;
            req.refs = getObstacleRef(ob);
            m_reqs.Add(req);
            return req;
        }

        public void removeObstacle(long refs)
        {
            if (refs == 0)
            {
                return;
            }

            ObstacleRequest req = new ObstacleRequest();
            req.action = ObstacleRequestAction.REQUEST_REMOVE;
            req.refs = refs;
            m_reqs.Add(req);
        }

        private TileCacheObstacle allocObstacle()
        {
            TileCacheObstacle o = m_nextFreeObstacle;
            if (o == null)
            {
                o = new TileCacheObstacle(m_obstacles.Count);
                m_obstacles.Add(o);
            }
            else
            {
                m_nextFreeObstacle = o.next;
            }

            o.state = ObstacleState.DT_OBSTACLE_PROCESSING;
            o.touched.Clear();
            o.pending.Clear();
            o.next = null;
            return o;
        }

        List<long> queryTiles(Vector3f bmin, Vector3f bmax)
        {
            List<long> results = new List<long>();
            float tw = m_params.width * m_params.cs;
            float th = m_params.height * m_params.cs;
            int tx0 = (int)Math.Floor((bmin[0] - m_params.orig[0]) / tw);
            int tx1 = (int)Math.Floor((bmax[0] - m_params.orig[0]) / tw);
            int ty0 = (int)Math.Floor((bmin[2] - m_params.orig[2]) / th);
            int ty1 = (int)Math.Floor((bmax[2] - m_params.orig[2]) / th);
            for (int ty = ty0; ty <= ty1; ++ty)
            {
                for (int tx = tx0; tx <= tx1; ++tx)
                {
                    List<long> tiles = getTilesAt(tx, ty);
                    foreach (long i in tiles)
                    {
                        CompressedTile tile = m_tiles[decodeTileIdTile(i)];
                        Vector3f tbmin = new Vector3f();
                        Vector3f tbmax = new Vector3f();
                        calcTightTileBounds(tile.header, ref tbmin, ref tbmax);
                        if (overlapBounds(bmin, bmax, tbmin, tbmax))
                        {
                            results.Add(i);
                        }
                    }
                }
            }

            return results;
        }

        /**
     * Updates the tile cache by rebuilding tiles touched by unfinished obstacle requests.
     *
     * @return Returns true if the tile cache is fully up to date with obstacle requests and tile rebuilds. If the tile
     *         cache is up to date another (immediate) call to update will have no effect; otherwise another call will
     *         continue processing obstacle requests and tile rebuilds.
     */
        public bool update()
        {
            if (0 == m_update.Count)
            {
                // Process requests.
                foreach (ObstacleRequest req in m_reqs)
                {
                    int idx = decodeObstacleIdObstacle(req.refs);
                    if (idx >= m_obstacles.Count)
                    {
                        continue;
                    }

                    TileCacheObstacle ob = m_obstacles[idx];
                    int salt = decodeObstacleIdSalt(req.refs);
                    if (ob.salt != salt)
                    {
                        continue;
                    }

                    if (req.action == ObstacleRequestAction.REQUEST_ADD)
                    {
                        // Find touched tiles.
                        Vector3f bmin = new Vector3f();
                        Vector3f bmax = new Vector3f();
                        getObstacleBounds(ob, ref bmin, ref bmax);
                        ob.touched = queryTiles(bmin, bmax);
                        // Add tiles to update list.
                        ob.pending.Clear();
                        foreach (long j in ob.touched)
                        {
                            if (!contains(m_update, j))
                            {
                                m_update.Add(j);
                            }

                            ob.pending.Add(j);
                        }
                    }
                    else if (req.action == ObstacleRequestAction.REQUEST_REMOVE)
                    {
                        // Prepare to remove obstacle.
                        ob.state = ObstacleState.DT_OBSTACLE_REMOVING;
                        // Add tiles to update list.
                        ob.pending.Clear();
                        foreach (long j in ob.touched)
                        {
                            if (!contains(m_update, j))
                            {
                                m_update.Add(j);
                            }

                            ob.pending.Add(j);
                        }
                    }
                }

                m_reqs.Clear();
            }

            // Process updates
            if (0 < m_update.Count)
            {
                long refs = m_update[0];
                m_update.RemoveAt(0);
                // Build mesh
                buildNavMeshTile(refs);

                // Update obstacle states.
                for (int i = 0; i < m_obstacles.Count; ++i)
                {
                    TileCacheObstacle ob = m_obstacles[i];
                    if (ob.state == ObstacleState.DT_OBSTACLE_PROCESSING
                        || ob.state == ObstacleState.DT_OBSTACLE_REMOVING)
                    {
                        // Remove handled tile from pending list.
                        ob.pending.Remove(refs);

                        // If all pending tiles processed, change state.
                        if (0 == ob.pending.Count)
                        {
                            if (ob.state == ObstacleState.DT_OBSTACLE_PROCESSING)
                            {
                                ob.state = ObstacleState.DT_OBSTACLE_PROCESSED;
                            }
                            else if (ob.state == ObstacleState.DT_OBSTACLE_REMOVING)
                            {
                                ob.state = ObstacleState.DT_OBSTACLE_EMPTY;
                                // Update salt, salt should never be zero.
                                ob.salt = (ob.salt + 1) & ((1 << 16) - 1);
                                if (ob.salt == 0)
                                {
                                    ob.salt++;
                                }

                                // Return obstacle to free list.
                                ob.next = m_nextFreeObstacle;
                                m_nextFreeObstacle = ob;
                            }
                        }
                    }
                }
            }

            return 0 == m_update.Count && 0 == m_reqs.Count;
        }

        public void buildNavMeshTile(long refs)
        {
            int idx = decodeTileIdTile(refs);
            if (idx > m_params.maxTiles)
            {
                throw new Exception("Invalid tile index");
            }

            CompressedTile tile = m_tiles[idx];
            int salt = decodeTileIdSalt(refs);
            if (tile.salt != salt)
            {
                throw new Exception("Invalid tile salt");
            }

            int walkableClimbVx = (int)(m_params.walkableClimb / m_params.ch);

            // Decompress tile layer data.
            TileCacheLayer layer = decompressTile(tile);

            // Rasterize obstacles.
            for (int i = 0; i < m_obstacles.Count; ++i)
            {
                TileCacheObstacle ob = m_obstacles[i];
                if (ob.state == ObstacleState.DT_OBSTACLE_EMPTY || ob.state == ObstacleState.DT_OBSTACLE_REMOVING)
                {
                    continue;
                }

                if (contains(ob.touched, refs))
                {
                    if (ob.type == TileCacheObstacle.TileCacheObstacleType.CYLINDER)
                    {
                        builder.markCylinderArea(layer, tile.header.bmin, m_params.cs, m_params.ch, ob.pos, ob.radius, ob.height, 0);
                    }
                    else if (ob.type == TileCacheObstacle.TileCacheObstacleType.BOX)
                    {
                        builder.markBoxArea(layer, tile.header.bmin, m_params.cs, m_params.ch, ob.bmin, ob.bmax, 0);
                    }
                    else if (ob.type == TileCacheObstacle.TileCacheObstacleType.ORIENTED_BOX)
                    {
                        builder.markBoxArea(layer, tile.header.bmin, m_params.cs, m_params.ch, ob.center, ob.extents, ob.rotAux, 0);
                    }
                }
            }

            // Build navmesh
            builder.buildTileCacheRegions(layer, walkableClimbVx);
            TileCacheContourSet lcset = builder.buildTileCacheContours(layer, walkableClimbVx,
                m_params.maxSimplificationError);
            TileCachePolyMesh polyMesh = builder.buildTileCachePolyMesh(lcset, m_navmesh.getMaxVertsPerPoly());
            // Early out if the mesh tile is empty.
            if (polyMesh.npolys == 0)
            {
                m_navmesh.removeTile(m_navmesh.getTileRefAt(tile.header.tx, tile.header.ty, tile.header.tlayer));
                return;
            }

            NavMeshDataCreateParams option = new NavMeshDataCreateParams();
            option.verts = polyMesh.verts;
            option.vertCount = polyMesh.nverts;
            option.polys = polyMesh.polys;
            option.polyAreas = polyMesh.areas;
            option.polyFlags = polyMesh.flags;
            option.polyCount = polyMesh.npolys;
            option.nvp = m_navmesh.getMaxVertsPerPoly();
            option.walkableHeight = m_params.walkableHeight;
            option.walkableRadius = m_params.walkableRadius;
            option.walkableClimb = m_params.walkableClimb;
            option.tileX = tile.header.tx;
            option.tileZ = tile.header.ty;
            option.tileLayer = tile.header.tlayer;
            option.cs = m_params.cs;
            option.ch = m_params.ch;
            option.buildBvTree = false;
            option.bmin = tile.header.bmin;
            option.bmax = tile.header.bmax;
            if (m_tmproc != null)
            {
                m_tmproc.process(option);
            }

            MeshData meshData = NavMeshBuilder.createNavMeshData(option);
            // Remove existing tile.
            m_navmesh.removeTile(m_navmesh.getTileRefAt(tile.header.tx, tile.header.ty, tile.header.tlayer));
            // Add new tile, or leave the location empty. if (navData) { // Let the
            if (meshData != null)
            {
                m_navmesh.addTile(meshData, 0, 0);
            }
        }

        public TileCacheLayer decompressTile(CompressedTile tile)
        {
            TileCacheLayer layer = builder.decompressTileCacheLayer(m_tcomp, tile.data, m_storageParams.byteOrder,
                m_storageParams.cCompatibility);
            return layer;
        }

        void calcTightTileBounds(TileCacheLayerHeader header, ref Vector3f bmin, ref Vector3f bmax)
        {
            float cs = m_params.cs;
            bmin[0] = header.bmin[0] + header.minx * cs;
            bmin[1] = header.bmin[1];
            bmin[2] = header.bmin[2] + header.miny * cs;
            bmax[0] = header.bmin[0] + (header.maxx + 1) * cs;
            bmax[1] = header.bmax[1];
            bmax[2] = header.bmin[2] + (header.maxy + 1) * cs;
        }

        void getObstacleBounds(TileCacheObstacle ob, ref Vector3f bmin, ref Vector3f bmax)
        {
            if (ob.type == TileCacheObstacle.TileCacheObstacleType.CYLINDER)
            {
                bmin[0] = ob.pos[0] - ob.radius;
                bmin[1] = ob.pos[1];
                bmin[2] = ob.pos[2] - ob.radius;
                bmax[0] = ob.pos[0] + ob.radius;
                bmax[1] = ob.pos[1] + ob.height;
                bmax[2] = ob.pos[2] + ob.radius;
            }
            else if (ob.type == TileCacheObstacle.TileCacheObstacleType.BOX)
            {
                vCopy(ref bmin, ob.bmin);
                vCopy(ref bmax, ob.bmax);
            }
            else if (ob.type == TileCacheObstacle.TileCacheObstacleType.ORIENTED_BOX)
            {
                float maxr = 1.41f * Math.Max(ob.extents[0], ob.extents[2]);
                bmin[0] = ob.center[0] - maxr;
                bmax[0] = ob.center[0] + maxr;
                bmin[1] = ob.center[1] - ob.extents[1];
                bmax[1] = ob.center[1] + ob.extents[1];
                bmin[2] = ob.center[2] - maxr;
                bmax[2] = ob.center[2] + maxr;
            }
        }

        public TileCacheParams getParams()
        {
            return m_params;
        }

        public TileCacheCompressor getCompressor()
        {
            return m_tcomp;
        }

        public int getTileCount()
        {
            return m_params.maxTiles;
        }

        public CompressedTile getTile(int i)
        {
            return m_tiles[i];
        }

        public NavMesh getNavMesh()
        {
            return m_navmesh;
        }
    }
}