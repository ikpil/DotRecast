using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.TileCache;
using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Recast.Geom;
using DotRecast.Recast.Toolset.Geom;

namespace DotRecast.Recast.Toolset.Tools
{
    public class ObstacleToolImpl : ISampleTool
    {
        private Sample _sample;
        private readonly IDtTileCacheCompressorFactory _compFactory;
        private readonly IDtTileCacheMeshProcess _mProc;
        private DtTileCache _tc;

        public ObstacleToolImpl(IDtTileCacheCompressorFactory compFactory, IDtTileCacheMeshProcess meshProcessor = null)
        {
            _compFactory = compFactory;
            _mProc = meshProcessor ?? new DemoDtTileCacheMeshProcess();
        }

        public string GetName()
        {
            return "Create Temp Obstacles";
        }

        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }

        public void ClearAllTempObstacles()
        {
        }

        public void RemoveTempObstacle(RcVec3f sp, RcVec3f sq)
        {
            // ..
        }

        public void AddTempObstacle(RcVec3f pos)
        {
            //p[1] -= 0.5f;
            //m_tileCache->addObstacle(p, 1.0f, 2.0f, 0);
        }

        public DtTileCache GetTileCache(IInputGeomProvider geom, RcByteOrder order, bool cCompatibility)
        {
            // DtTileCacheParams option = new DtTileCacheParams();
            // RcUtils.CalcTileCount(geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax(), m_cellSize, m_tileSize, m_tileSize, out var tw, out var th);
            // option.ch = m_cellHeight;
            // option.cs = m_cellSize;
            // option.orig = geom.GetMeshBoundsMin();
            // option.height = m_tileSize;
            // option.width = m_tileSize;
            // option.walkableHeight = m_agentHeight;
            // option.walkableRadius = m_agentRadius;
            // option.walkableClimb = m_agentMaxClimb;
            // option.maxSimplificationError = m_edgeMaxError;
            // option.maxTiles = tw * th * EXPECTED_LAYERS_PER_TILE;
            // option.maxObstacles = 128;
            //
            // DtNavMeshParams navMeshParams = new DtNavMeshParams();
            // navMeshParams.orig = geom.GetMeshBoundsMin();
            // navMeshParams.tileWidth = m_tileSize * m_cellSize;
            // navMeshParams.tileHeight = m_tileSize * m_cellSize;
            // navMeshParams.maxTiles = 256;
            // navMeshParams.maxPolys = 16384;
            //
            // var navMesh = new DtNavMesh(navMeshParams, 6);
            // var comp = _compFactory.Get(cCompatibility);
            // var storageParams = new DtTileCacheStorageParams(order, cCompatibility);
            // var process = new TestTileCacheMeshProcess();
            // DtTileCache tc = new DtTileCache(option, storageParams, navMesh, comp, process);
            // return tc;
            return null;
        }
    }
}