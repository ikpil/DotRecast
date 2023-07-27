using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotRecast.Core;
using DotRecast.Detour.TileCache;
using DotRecast.Recast.DemoTool.Builder;

namespace DotRecast.Recast.DemoTool.Tools
{
    public class TileToolImpl : ISampleTool
    {
        private Sample _sample;

        public string GetName()
        {
            return "Create Tiles";
        }

        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }

        public bool BuildTile(RcVec3f pos, out long tileBuildTicks, out int tileTriCount, out int tileMemUsage)
        {
            var settings = _sample.GetSettings();
            var geom = _sample.GetInputGeom();
            var navMesh = _sample.GetNavMesh();

            tileBuildTicks = 0;
            tileTriCount = 0;
            tileMemUsage = 0;

            if (null == settings || null == geom || navMesh == null)
                return false;

            float ts = settings.tileSize * settings.cellSize;

            RcVec3f bmin = geom.GetMeshBoundsMin();
            RcVec3f bmax = geom.GetMeshBoundsMax();

            int tx = (int)((pos.x - bmin[0]) / ts);
            int ty = (int)((pos.z - bmin[2]) / ts);

            RcConfig cfg = new RcConfig(
                true,
                settings.tileSize,
                settings.tileSize,
                RcConfig.CalcBorder(settings.agentRadius, settings.cellSize),
                settings.partitioning,
                settings.cellSize,
                settings.cellHeight,
                settings.agentMaxSlope,
                settings.filterLowHangingObstacles,
                settings.filterLedgeSpans,
                settings.filterWalkableLowHeightSpans,
                settings.agentHeight,
                settings.agentRadius,
                settings.agentMaxClimb,
                settings.minRegionSize * settings.minRegionSize * settings.cellSize * settings.cellSize,
                settings.mergedRegionSize * settings.mergedRegionSize * settings.cellSize * settings.cellSize,
                settings.edgeMaxLen,
                settings.edgeMaxError,
                settings.vertsPerPoly,
                true,
                settings.detailSampleDist,
                settings.detailSampleMaxError,
                SampleAreaModifications.SAMPLE_AREAMOD_WALKABLE
            );

            var rb = new RecastBuilder();
            var result = rb.BuildTile(geom, cfg, bmin, bmax, tx, ty, new RcAtomicInteger(0), 1);

            var tb = new TileNavMeshBuilder();
            var meshData = tb.BuildMeshData(geom,
                settings.cellSize, settings.cellHeight, settings.agentHeight, settings.agentRadius, settings.agentMaxClimb,
                ImmutableArray.Create(result)
            ).First();

            navMesh.UpdateTile(meshData, 0);


            var telemetry = result.GetTelemetry();
            tileBuildTicks = telemetry.ToList().Sum(x => x.Ticks);
            tileTriCount = 0; // ...
            tileMemUsage = 0; // ...

            return true;
        }

        public bool RemoveTile(RcVec3f pos)
        {
            var settings = _sample.GetSettings();
            var geom = _sample.GetInputGeom();
            var navMesh = _sample.GetNavMesh();

            if (null == settings || null == geom || navMesh == null)
                return false;

            float ts = settings.tileSize * settings.cellSize;

            var bmin = geom.GetMeshBoundsMin();

            int tx = (int)((pos.x - bmin[0]) / ts);
            int ty = (int)((pos.z - bmin[2]) / ts);

            var tileRef = navMesh.GetTileRefAt(tx, ty, 0);
            navMesh.RemoveTile(tileRef);

            return true;
        }
    }
}