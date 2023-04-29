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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Recast;

namespace DotRecast.Detour.Dynamic
{
    public class DynamicNavMesh
    {
        public const int MAX_VERTS_PER_POLY = 6;
        public readonly DynamicNavMeshConfig config;
        private readonly RecastBuilder builder;
        private readonly Dictionary<long, DynamicTile> _tiles = new Dictionary<long, DynamicTile>();
        private readonly Telemetry telemetry;
        private readonly NavMeshParams navMeshParams;
        private readonly BlockingCollection<UpdateQueueItem> updateQueue = new BlockingCollection<UpdateQueueItem>();
        private readonly AtomicLong currentColliderId = new AtomicLong(0);
        private NavMesh _navMesh;
        private bool dirty = true;

        public DynamicNavMesh(VoxelFile voxelFile)
        {
            config = new DynamicNavMeshConfig(voxelFile.useTiles, voxelFile.tileSizeX, voxelFile.tileSizeZ, voxelFile.cellSize);
            config.walkableHeight = voxelFile.walkableHeight;
            config.walkableRadius = voxelFile.walkableRadius;
            config.walkableClimb = voxelFile.walkableClimb;
            config.walkableSlopeAngle = voxelFile.walkableSlopeAngle;
            config.maxSimplificationError = voxelFile.maxSimplificationError;
            config.maxEdgeLen = voxelFile.maxEdgeLen;
            config.minRegionArea = voxelFile.minRegionArea;
            config.regionMergeArea = voxelFile.regionMergeArea;
            config.vertsPerPoly = voxelFile.vertsPerPoly;
            config.buildDetailMesh = voxelFile.buildMeshDetail;
            config.detailSampleDistance = voxelFile.detailSampleDistance;
            config.detailSampleMaxError = voxelFile.detailSampleMaxError;
            builder = new RecastBuilder();
            navMeshParams = new NavMeshParams();
            navMeshParams.orig.x = voxelFile.bounds[0];
            navMeshParams.orig.y = voxelFile.bounds[1];
            navMeshParams.orig.z = voxelFile.bounds[2];
            navMeshParams.tileWidth = voxelFile.cellSize * voxelFile.tileSizeX;
            navMeshParams.tileHeight = voxelFile.cellSize * voxelFile.tileSizeZ;
            navMeshParams.maxTiles = voxelFile.tiles.Count;
            navMeshParams.maxPolys = 0x8000;
            foreach (var t in voxelFile.tiles)
            {
                _tiles.Add(lookupKey(t.tileX, t.tileZ), new DynamicTile(t));
            }

            ;
            telemetry = new Telemetry();
        }

        public NavMesh navMesh()
        {
            return _navMesh;
        }

        /**
     * Voxel queries require checkpoints to be enabled in {@link DynamicNavMeshConfig}
     */
        public VoxelQuery voxelQuery()
        {
            return new VoxelQuery(navMeshParams.orig, navMeshParams.tileWidth, navMeshParams.tileHeight, lookupHeightfield);
        }

        private Heightfield lookupHeightfield(int x, int z)
        {
            return getTileAt(x, z)?.checkpoint.heightfield;
        }

        public long addCollider(Collider collider)
        {
            long cid = currentColliderId.IncrementAndGet();
            updateQueue.Add(new AddColliderQueueItem(cid, collider, getTiles(collider.bounds())));
            return cid;
        }

        public void removeCollider(long colliderId)
        {
            updateQueue.Add(new RemoveColliderQueueItem(colliderId, getTilesByCollider(colliderId)));
        }

        /**
     * Perform full build of the nav mesh
     */
        public void build()
        {
            processQueue();
            rebuild(_tiles.Values);
        }

        /**
     * Perform incremental update of the nav mesh
     */
        public bool update()
        {
            return rebuild(processQueue());
        }

        private bool rebuild(ICollection<DynamicTile> stream)
        {
            foreach (var dynamicTile in stream)
                rebuild(dynamicTile);
            return updateNavMesh();
        }

        private HashSet<DynamicTile> processQueue()
        {
            var items = consumeQueue();
            foreach (var item in items)
            {
                process(item);
            }

            return items.SelectMany(i => i.affectedTiles()).ToHashSet();
        }

        private List<UpdateQueueItem> consumeQueue()
        {
            List<UpdateQueueItem> items = new List<UpdateQueueItem>();
            while (updateQueue.TryTake(out var item))
            {
                items.Add(item);
            }

            return items;
        }

        private void process(UpdateQueueItem item)
        {
            foreach (var tile in item.affectedTiles())
            {
                item.process(tile);
            }
        }

        /**
     * Perform full build concurrently using the given {@link ExecutorService}
     */
        public Task<bool> build(TaskFactory executor)
        {
            processQueue();
            return rebuild(_tiles.Values, executor);
        }

        /**
     * Perform incremental update concurrently using the given {@link ExecutorService}
     */
        public Task<bool> update(TaskFactory executor)
        {
            return rebuild(processQueue(), executor);
        }

        private Task<bool> rebuild(ICollection<DynamicTile> tiles, TaskFactory executor)
        {
            var tasks = tiles.Select(tile => executor.StartNew(() => rebuild(tile))).ToArray();
            return Task.WhenAll(tasks).ContinueWith(k => updateNavMesh());
        }

        private ICollection<DynamicTile> getTiles(float[] bounds)
        {
            if (bounds == null)
            {
                return _tiles.Values;
            }

            int minx = (int)Math.Floor((bounds[0] - navMeshParams.orig.x) / navMeshParams.tileWidth);
            int minz = (int)Math.Floor((bounds[2] - navMeshParams.orig.z) / navMeshParams.tileHeight);
            int maxx = (int)Math.Floor((bounds[3] - navMeshParams.orig.x) / navMeshParams.tileWidth);
            int maxz = (int)Math.Floor((bounds[5] - navMeshParams.orig.z) / navMeshParams.tileHeight);
            List<DynamicTile> tiles = new List<DynamicTile>();
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    DynamicTile tile = getTileAt(x, z);
                    if (tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }

            return tiles;
        }

        private List<DynamicTile> getTilesByCollider(long cid)
        {
            return _tiles.Values.Where(t => t.containsCollider(cid)).ToList();
        }

        private void rebuild(DynamicTile tile)
        {
            NavMeshDataCreateParams option = new NavMeshDataCreateParams();
            option.walkableHeight = config.walkableHeight;
            dirty = dirty | tile.build(builder, config, telemetry);
        }

        private bool updateNavMesh()
        {
            if (dirty)
            {
                NavMesh navMesh = new NavMesh(navMeshParams, MAX_VERTS_PER_POLY);
                foreach (var t in _tiles.Values)
                    t.addTo(navMesh);

                this._navMesh = navMesh;
                dirty = false;
                return true;
            }

            return false;
        }

        private DynamicTile getTileAt(int x, int z)
        {
            return _tiles.TryGetValue(lookupKey(x, z), out var tile)
                ? tile
                : null;
        }

        private long lookupKey(long x, long z)
        {
            return (z << 32) | x;
        }

        public List<VoxelTile> voxelTiles()
        {
            return _tiles.Values.Select(t => t.voxelTile).ToList();
        }

        public List<RecastBuilderResult> recastResults()
        {
            return _tiles.Values.Select(t => t.recastResult).ToList();
        }
    }
}
