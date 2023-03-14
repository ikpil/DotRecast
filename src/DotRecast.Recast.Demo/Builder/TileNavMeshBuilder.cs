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
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Recast.Demo.Geom;

namespace DotRecast.Recast.Demo.Builder;

public class TileNavMeshBuilder : AbstractNavMeshBuilder {

    public TileNavMeshBuilder() {
    }

    public Tuple<IList<RecastBuilderResult>, NavMesh> build(DemoInputGeomProvider m_geom, PartitionType m_partitionType,
            float m_cellSize, float m_cellHeight, float m_agentHeight, float m_agentRadius, float m_agentMaxClimb,
            float m_agentMaxSlope, int m_regionMinSize, int m_regionMergeSize, float m_edgeMaxLen, float m_edgeMaxError,
            int m_vertsPerPoly, float m_detailSampleDist, float m_detailSampleMaxError, bool filterLowHangingObstacles,
            bool filterLedgeSpans, bool filterWalkableLowHeightSpans, int tileSize) {

        List<RecastBuilderResult> rcResult = buildRecastResult(m_geom, m_partitionType, m_cellSize, m_cellHeight, m_agentHeight,
                m_agentRadius, m_agentMaxClimb, m_agentMaxSlope, m_regionMinSize, m_regionMergeSize, m_edgeMaxLen, m_edgeMaxError,
                m_vertsPerPoly, m_detailSampleDist, m_detailSampleMaxError, filterLowHangingObstacles, filterLedgeSpans,
                filterWalkableLowHeightSpans, tileSize);
        return Tuple.Create((IList<RecastBuilderResult>) rcResult,
                buildNavMesh(m_geom,
                        buildMeshData(m_geom, m_cellSize, m_cellHeight, m_agentHeight, m_agentRadius, m_agentMaxClimb, rcResult),
                        m_cellSize, tileSize, m_vertsPerPoly));
    }

    private List<RecastBuilderResult> buildRecastResult(DemoInputGeomProvider m_geom, PartitionType m_partitionType,
            float m_cellSize, float m_cellHeight, float m_agentHeight, float m_agentRadius, float m_agentMaxClimb,
            float m_agentMaxSlope, int m_regionMinSize, int m_regionMergeSize, float m_edgeMaxLen, float m_edgeMaxError,
            int m_vertsPerPoly, float m_detailSampleDist, float m_detailSampleMaxError, bool filterLowHangingObstacles,
            bool filterLedgeSpans, bool filterWalkableLowHeightSpans, int tileSize) {
        RecastConfig cfg = new RecastConfig(true, tileSize, tileSize, RecastConfig.calcBorder(m_agentRadius, m_cellSize),
                m_partitionType, m_cellSize, m_cellHeight, m_agentMaxSlope, filterLowHangingObstacles, filterLedgeSpans,
                filterWalkableLowHeightSpans, m_agentHeight, m_agentRadius, m_agentMaxClimb,
                m_regionMinSize * m_regionMinSize * m_cellSize * m_cellSize,
                m_regionMergeSize * m_regionMergeSize * m_cellSize * m_cellSize, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly,
                true, m_detailSampleDist, m_detailSampleMaxError, SampleAreaModifications.SAMPLE_AREAMOD_WALKABLE);
        RecastBuilder rcBuilder = new RecastBuilder();
        return rcBuilder.buildTiles(m_geom, cfg, Task.Factory);
    }

    private NavMesh buildNavMesh(DemoInputGeomProvider geom, List<MeshData> meshData, float cellSize, int tileSize,
            int vertsPerPoly) {
        NavMeshParams navMeshParams = new NavMeshParams();
        navMeshParams.orig[0] = geom.getMeshBoundsMin()[0];
        navMeshParams.orig[1] = geom.getMeshBoundsMin()[1];
        navMeshParams.orig[2] = geom.getMeshBoundsMin()[2];
        navMeshParams.tileWidth = tileSize * cellSize;
        navMeshParams.tileHeight = tileSize * cellSize;

        // snprintf(text, 64, "Tiles %d x %d", tw, th);

        navMeshParams.maxTiles = getMaxTiles(geom, cellSize, tileSize);
        navMeshParams.maxPolys = getMaxPolysPerTile(geom, cellSize, tileSize);
        NavMesh navMesh = new NavMesh(navMeshParams, vertsPerPoly);
        meshData.forEach(md => navMesh.addTile(md, 0, 0));
        return navMesh;
    }

    public int getMaxTiles(DemoInputGeomProvider geom, float cellSize, int tileSize) {
        int tileBits = getTileBits(geom, cellSize, tileSize);
        return 1 << tileBits;
    }

    public int getMaxPolysPerTile(DemoInputGeomProvider geom, float cellSize, int tileSize) {
        int polyBits = 22 - getTileBits(geom, cellSize, tileSize);
        return 1 << polyBits;
    }

    private int getTileBits(DemoInputGeomProvider geom, float cellSize, int tileSize) {
        int[] wh = Recast.calcGridSize(geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), cellSize);
        int tw = (wh[0] + tileSize - 1) / tileSize;
        int th = (wh[1] + tileSize - 1) / tileSize;
        int tileBits = Math.Min(DetourCommon.ilog2(DetourCommon.nextPow2(tw * th)), 14);
        return tileBits;
    }

    public int[] getTiles(DemoInputGeomProvider geom, float cellSize, int tileSize) {
        int[] wh = Recast.calcGridSize(geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), cellSize);
        int tw = (wh[0] + tileSize - 1) / tileSize;
        int th = (wh[1] + tileSize - 1) / tileSize;
        return new int[] { tw, th };
    }

    private List<MeshData> buildMeshData(DemoInputGeomProvider m_geom, float m_cellSize, float m_cellHeight, float m_agentHeight,
            float m_agentRadius, float m_agentMaxClimb, List<RecastBuilderResult> rcResult) {

        // Add tiles to nav mesh
        List<MeshData> meshData = new();
        foreach (RecastBuilderResult result in rcResult) {
            int x = result.tileX;
            int z = result.tileZ;
            NavMeshDataCreateParams option = getNavMeshCreateParams(m_geom, m_cellSize, m_cellHeight, m_agentHeight,
                    m_agentRadius, m_agentMaxClimb, result);
            option.tileX = x;
            option.tileZ = z;
            MeshData md = NavMeshBuilder.createNavMeshData(option);
            if (md != null) {
                meshData.Add(updateAreaAndFlags(md));
            }
        }
        return meshData;
    }

}
