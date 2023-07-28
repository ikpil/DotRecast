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
using System.Collections.Generic;
using System.Threading.Tasks;
using DotRecast.Detour;
using DotRecast.Recast.DemoTool.Geom;

namespace DotRecast.Recast.DemoTool.Builder
{
    public class TileNavMeshBuilder
    {
        public TileNavMeshBuilder()
        {
        }

        public NavMeshBuildResult Build(DemoInputGeomProvider geom, RcNavMeshBuildSetting settings)
        {
            return Build(geom,
                RcPartitionType.OfValue(settings.partitioning), settings.cellSize, settings.cellHeight, settings.agentHeight,
                settings.agentRadius, settings.agentMaxClimb, settings.agentMaxSlope,
                settings.minRegionSize, settings.mergedRegionSize,
                settings.edgeMaxLen, settings.edgeMaxError,
                settings.vertsPerPoly, settings.detailSampleDist, settings.detailSampleMaxError,
                settings.filterLowHangingObstacles, settings.filterLedgeSpans, settings.filterWalkableLowHeightSpans,
                settings.tileSize);
        }

        public NavMeshBuildResult Build(DemoInputGeomProvider geom, RcPartition partitionType,
            float cellSize, float cellHeight, float agentHeight, float agentRadius, float agentMaxClimb,
            float agentMaxSlope, int regionMinSize, int regionMergeSize, float edgeMaxLen, float edgeMaxError,
            int vertsPerPoly, float detailSampleDist, float detailSampleMaxError, bool filterLowHangingObstacles,
            bool filterLedgeSpans, bool filterWalkableLowHeightSpans, int tileSize)
        {
            List<RecastBuilderResult> results = BuildRecastResult(geom, partitionType, cellSize, cellHeight, agentHeight,
                agentRadius, agentMaxClimb, agentMaxSlope, regionMinSize, regionMergeSize, edgeMaxLen, edgeMaxError,
                vertsPerPoly, detailSampleDist, detailSampleMaxError, filterLowHangingObstacles, filterLedgeSpans,
                filterWalkableLowHeightSpans, tileSize);
            var tileMeshData = BuildMeshData(geom, cellSize, cellHeight, agentHeight, agentRadius, agentMaxClimb, results);
            var tileNavMesh = BuildNavMesh(geom, tileMeshData, cellSize, tileSize, vertsPerPoly);
            return new NavMeshBuildResult(results, tileNavMesh);
        }

        public List<RecastBuilderResult> BuildRecastResult(DemoInputGeomProvider geom, RcPartition partitionType,
            float cellSize, float cellHeight, float agentHeight, float agentRadius, float agentMaxClimb,
            float agentMaxSlope, int regionMinSize, int regionMergeSize, float edgeMaxLen, float edgeMaxError,
            int vertsPerPoly, float detailSampleDist, float detailSampleMaxError, bool filterLowHangingObstacles,
            bool filterLedgeSpans, bool filterWalkableLowHeightSpans, int tileSize)
        {
            RcConfig cfg = new RcConfig(true, tileSize, tileSize, RcConfig.CalcBorder(agentRadius, cellSize),
                partitionType, cellSize, cellHeight, agentMaxSlope, filterLowHangingObstacles, filterLedgeSpans,
                filterWalkableLowHeightSpans, agentHeight, agentRadius, agentMaxClimb,
                regionMinSize * regionMinSize * cellSize * cellSize,
                regionMergeSize * regionMergeSize * cellSize * cellSize, edgeMaxLen, edgeMaxError, vertsPerPoly,
                true, detailSampleDist, detailSampleMaxError, SampleAreaModifications.SAMPLE_AREAMOD_WALKABLE);
            RecastBuilder rcBuilder = new RecastBuilder();
            return rcBuilder.BuildTiles(geom, cfg, Task.Factory);
        }

        public DtNavMesh BuildNavMesh(DemoInputGeomProvider geom, List<DtMeshData> meshData, float cellSize, int tileSize, int vertsPerPoly)
        {
            DtNavMeshParams navMeshParams = new DtNavMeshParams();
            navMeshParams.orig = geom.GetMeshBoundsMin();
            navMeshParams.tileWidth = tileSize * cellSize;
            navMeshParams.tileHeight = tileSize * cellSize;

            // Snprintf(text, 64, "Tiles %d x %d", tw, th);

            navMeshParams.maxTiles = GetMaxTiles(geom, cellSize, tileSize);
            navMeshParams.maxPolys = GetMaxPolysPerTile(geom, cellSize, tileSize);
            DtNavMesh navMesh = new DtNavMesh(navMeshParams, vertsPerPoly);
            meshData.ForEach(md => navMesh.AddTile(md, 0, 0));
            return navMesh;
        }

        public List<DtMeshData> BuildMeshData(DemoInputGeomProvider geom, float cellSize, float cellHeight, float agentHeight,
            float agentRadius, float agentMaxClimb, IList<RecastBuilderResult> results)
        {
            // Add tiles to nav mesh
            List<DtMeshData> meshData = new List<DtMeshData>();
            foreach (RecastBuilderResult result in results)
            {
                int x = result.tileX;
                int z = result.tileZ;
                DtNavMeshCreateParams option = DemoNavMeshBuilder
                    .GetNavMeshCreateParams(geom, cellSize, cellHeight, agentHeight, agentRadius, agentMaxClimb, result);

                option.tileX = x;
                option.tileZ = z;
                DtMeshData md = NavMeshBuilder.CreateNavMeshData(option);
                if (md != null)
                {
                    meshData.Add(DemoNavMeshBuilder.UpdateAreaAndFlags(md));
                }
            }

            return meshData;
        }


        public int GetMaxTiles(DemoInputGeomProvider geom, float cellSize, int tileSize)
        {
            int tileBits = GetTileBits(geom, cellSize, tileSize);
            return 1 << tileBits;
        }

        public int GetMaxPolysPerTile(DemoInputGeomProvider geom, float cellSize, int tileSize)
        {
            int polyBits = 22 - GetTileBits(geom, cellSize, tileSize);
            return 1 << polyBits;
        }

        private int GetTileBits(DemoInputGeomProvider geom, float cellSize, int tileSize)
        {
            Recast.CalcGridSize(geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax(), cellSize, out var gw, out var gh);
            int tw = (gw + tileSize - 1) / tileSize;
            int th = (gh + tileSize - 1) / tileSize;
            int tileBits = Math.Min(DetourCommon.Ilog2(DetourCommon.NextPow2(tw * th)), 14);
            return tileBits;
        }

        public int[] GetTiles(DemoInputGeomProvider geom, float cellSize, int tileSize)
        {
            Recast.CalcGridSize(geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax(), cellSize, out var gw, out var gh);
            int tw = (gw + tileSize - 1) / tileSize;
            int th = (gh + tileSize - 1) / tileSize;
            return new int[] { tw, th };
        }
    }
}