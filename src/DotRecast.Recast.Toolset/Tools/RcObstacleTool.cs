using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.TileCache;
using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Recast.Geom;
using DotRecast.Recast.Toolset.Geom;

namespace DotRecast.Recast.Toolset.Tools
{
    public class RcObstacleTool : IRcToolable
    {
        private readonly IDtTileCacheCompressorFactory _comp;
        private readonly DemoDtTileCacheMeshProcess _proc;
        private DtTileCache _tc;

        public RcObstacleTool(IDtTileCacheCompressorFactory comp)
        {
            _comp = comp;
            _proc = new DemoDtTileCacheMeshProcess();
        }

        public string GetName()
        {
            return "Create Temp Obstacles";
        }

        public bool Build(IInputGeomProvider geom, RcNavMeshBuildSettings setting, RcByteOrder order, bool cCompatibility)
        {
            DtStatus status;

            if (null == geom || null == geom.GetMesh())
            {
                //m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: No vertices and triangles.");
                return false;
            }
            
            _proc.Init(geom);
            
            // Init cache
            var bmin = geom.GetMeshBoundsMin();
            var bmax = geom.GetMeshBoundsMax();
            RcUtils.CalcGridSize(bmin, bmax, setting.cellSize, out var gw, out var gh);
            int ts = setting.tileSize;
            int tw = (gw + ts-1) / ts;
            int th = (gh + ts-1) / ts;
            
            // Generation params.
            // RcConfig cfg = new RcConfig();
            // cfg.cs = m_cellSize;
            // cfg.ch = m_cellHeight;
            // cfg.walkableSlopeAngle = m_agentMaxSlope;
            // cfg.walkableHeight = (int)ceilf(m_agentHeight / cfg.ch);
            // cfg.walkableClimb = (int)floorf(m_agentMaxClimb / cfg.ch);
            // cfg.walkableRadius = (int)ceilf(m_agentRadius / cfg.cs);
            // cfg.maxEdgeLen = (int)(m_edgeMaxLen / m_cellSize);
            // cfg.maxSimplificationError = m_edgeMaxError;
            // cfg.minRegionArea = (int)rcSqr(m_regionMinSize);		// Note: area = size*size
            // cfg.mergeRegionArea = (int)rcSqr(m_regionMergeSize);	// Note: area = size*size
            // cfg.maxVertsPerPoly = (int)m_vertsPerPoly;
            // cfg.tileSize = (int)m_tileSize;
            // cfg.borderSize = cfg.walkableRadius + 3; // Reserve enough padding.
            // cfg.width = cfg.tileSize + cfg.borderSize*2;
            // cfg.height = cfg.tileSize + cfg.borderSize*2;
            // cfg.detailSampleDist = m_detailSampleDist < 0.9f ? 0 : m_cellSize * m_detailSampleDist;
            // cfg.detailSampleMaxError = m_cellHeight * m_detailSampleMaxError;
            // rcVcopy(cfg.bmin, bmin);
            // rcVcopy(cfg.bmax, bmax);

            _tc = CreateTileCache(geom, setting, tw, th, order, cCompatibility);

            for (int y = 0; y < th; ++y)
            {
                for (int x = 0; x < tw; ++x)
                {
                    // TileCacheData tiles[MAX_LAYERS];
                    // memset(tiles, 0, sizeof(tiles));
                    // int ntiles = rasterizeTileLayers(x, y, cfg, tiles, MAX_LAYERS);
                    //
                    // for (int i = 0; i < ntiles; ++i)
                    // {
                    //     TileCacheData* tile = &tiles[i];
                    //     status = m_tileCache->addTile(tile->data, tile->dataSize, DT_COMPRESSEDTILE_FREE_DATA, 0);
                    //     if (dtStatusFailed(status))
                    //     {
                    //         dtFree(tile->data);
                    //         tile->data = 0;
                    //         continue;
                    //     }
                    //
                    //     m_cacheLayerCount++;
                    //     m_cacheCompressedSize += tile->dataSize;
                    //     m_cacheRawSize += calcLayerBufferSize(tcparams.width, tcparams.height);
                    // }
                }
            }

            return true;
        }

        public void ClearAllTempObstacles()
        {
            if (null == _tc)
                return;

            for (int i = 0; i < _tc.GetObstacleCount(); ++i)
            {
                DtTileCacheObstacle ob = _tc.GetObstacle(i);
                if (ob.state == DtObstacleState.DT_OBSTACLE_EMPTY)
                    continue;

                _tc.RemoveObstacle(_tc.GetObstacleRef(ob));
            }
        }

        public void RemoveTempObstacle(RcVec3f sp, RcVec3f sq)
        {
            if (null == _tc)
                return;

            //DtObstacleRef refs = hitTestObstacle(m_tileCache, sp, sq);
            //_tc.RemoveObstacle(refs);
        }

        public long AddTempObstacle(RcVec3f p)
        {
            if (null == _tc)
                return 0;

            p.y -= 0.5f;
            return _tc.AddObstacle(p, 1.0f, 2.0f);
        }

        public DtTileCache GetTileCache()
        {
            return _tc;
        }

        public DtTileCache CreateTileCache(IInputGeomProvider geom, RcNavMeshBuildSettings setting, int tw, int th, RcByteOrder order, bool cCompatibility)
        {
            DtTileCacheParams option = new DtTileCacheParams();
            option.ch = setting.cellHeight;
            option.cs = setting.cellSize;
            option.orig = geom.GetMeshBoundsMin();
            option.height = setting.tileSize;
            option.width = setting.tileSize;
            option.walkableHeight = setting.agentHeight;
            option.walkableRadius = setting.agentRadius;
            option.walkableClimb = setting.agentMaxClimb;
            option.maxSimplificationError = setting.edgeMaxError;
            option.maxTiles = tw * th * 4; // for test EXPECTED_LAYERS_PER_TILE;
            option.maxObstacles = 128;

            DtNavMeshParams navMeshParams = new DtNavMeshParams();
            navMeshParams.orig = geom.GetMeshBoundsMin();
            navMeshParams.tileWidth = setting.tileSize * setting.cellSize;
            navMeshParams.tileHeight = setting.tileSize * setting.cellSize;
            navMeshParams.maxTiles = 256; // ..
            navMeshParams.maxPolys = 16384;

            var navMesh = new DtNavMesh(navMeshParams, 6);
            var comp = _comp.Create(cCompatibility ? 0 : 1);
            var storageParams = new DtTileCacheStorageParams(order, cCompatibility);
            DtTileCache tc = new DtTileCache(option, storageParams, navMesh, comp, _proc);
            return tc;
        }
    }
}