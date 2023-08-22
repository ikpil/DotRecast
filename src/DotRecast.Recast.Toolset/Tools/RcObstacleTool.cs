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
        private readonly IDtTileCacheCompressorFactory _compFactory;
        private readonly IDtTileCacheMeshProcess _proc;
        private DtTileCache _tc;

        public RcObstacleTool(IDtTileCacheCompressorFactory compFactory, IDtTileCacheMeshProcess meshProcessor = null)
        {
            _compFactory = compFactory;
            _proc = meshProcessor ?? new DemoDtTileCacheMeshProcess();
        }

        public string GetName()
        {
            return "Create Temp Obstacles";
        }

        public void Build(IInputGeomProvider geom, RcNavMeshBuildSettings setting, RcByteOrder order, bool cCompatibility)
        {
            _tc = CreateTileCache(geom, setting, order, cCompatibility);
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

        public DtTileCache CreateTileCache(IInputGeomProvider geom, RcNavMeshBuildSettings setting, RcByteOrder order, bool cCompatibility)
        {
            RcUtils.CalcTileCount(geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax(),
                setting.cellSize, setting.tileSize, setting.tileSize,
                out var tw, out var th
            );

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
            var comp = _compFactory.Create(cCompatibility ? 0 : 1);
            var storageParams = new DtTileCacheStorageParams(order, cCompatibility);
            DtTileCache tc = new DtTileCache(option, storageParams, navMesh, comp, _proc);
            return tc;
        }
    }
}