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
using System.Collections.Immutable;
using DotRecast.Detour;
using DotRecast.Recast.Demo.Geom;

namespace DotRecast.Recast.Demo.Builder;

public class SoloNavMeshBuilder : AbstractNavMeshBuilder
{
    public Tuple<IList<RecastBuilderResult>, NavMesh> Build(DemoInputGeomProvider m_geom, PartitionType m_partitionType,
        float m_cellSize, float m_cellHeight, float m_agentHeight, float m_agentRadius, float m_agentMaxClimb,
        float m_agentMaxSlope, int m_regionMinSize, int m_regionMergeSize, float m_edgeMaxLen, float m_edgeMaxError,
        int m_vertsPerPoly, float m_detailSampleDist, float m_detailSampleMaxError, bool filterLowHangingObstacles,
        bool filterLedgeSpans, bool filterWalkableLowHeightSpans)
    {
        RecastBuilderResult rcResult = BuildRecastResult(m_geom, m_partitionType, m_cellSize, m_cellHeight, m_agentHeight,
            m_agentRadius, m_agentMaxClimb, m_agentMaxSlope, m_regionMinSize, m_regionMergeSize, m_edgeMaxLen, m_edgeMaxError,
            m_vertsPerPoly, m_detailSampleDist, m_detailSampleMaxError, filterLowHangingObstacles, filterLedgeSpans,
            filterWalkableLowHeightSpans);
        return Tuple.Create(ImmutableArray.Create(rcResult) as IList<RecastBuilderResult>,
            BuildNavMesh(
                BuildMeshData(m_geom, m_cellSize, m_cellHeight, m_agentHeight, m_agentRadius, m_agentMaxClimb, rcResult),
                m_vertsPerPoly));
    }

    private NavMesh BuildNavMesh(MeshData meshData, int m_vertsPerPoly)
    {
        return new NavMesh(meshData, m_vertsPerPoly, 0);
    }

    private RecastBuilderResult BuildRecastResult(DemoInputGeomProvider m_geom, PartitionType m_partitionType, float m_cellSize,
        float m_cellHeight, float m_agentHeight, float m_agentRadius, float m_agentMaxClimb, float m_agentMaxSlope,
        int m_regionMinSize, int m_regionMergeSize, float m_edgeMaxLen, float m_edgeMaxError, int m_vertsPerPoly,
        float m_detailSampleDist, float m_detailSampleMaxError, bool filterLowHangingObstacles, bool filterLedgeSpans,
        bool filterWalkableLowHeightSpans)
    {
        RecastConfig cfg = new RecastConfig(m_partitionType, m_cellSize, m_cellHeight, m_agentMaxSlope, filterLowHangingObstacles,
            filterLedgeSpans, filterWalkableLowHeightSpans, m_agentHeight, m_agentRadius, m_agentMaxClimb, m_regionMinSize,
            m_regionMergeSize, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly, m_detailSampleDist, m_detailSampleMaxError,
            SampleAreaModifications.SAMPLE_AREAMOD_WALKABLE, true);
        RecastBuilderConfig bcfg = new RecastBuilderConfig(cfg, m_geom.GetMeshBoundsMin(), m_geom.GetMeshBoundsMax());
        RecastBuilder rcBuilder = new RecastBuilder();
        return rcBuilder.Build(m_geom, bcfg);
    }

    private MeshData BuildMeshData(DemoInputGeomProvider m_geom, float m_cellSize, float m_cellHeight, float m_agentHeight,
        float m_agentRadius, float m_agentMaxClimb, RecastBuilderResult rcResult)
    {
        NavMeshDataCreateParams option = GetNavMeshCreateParams(m_geom, m_cellSize, m_cellHeight, m_agentHeight, m_agentRadius,
            m_agentMaxClimb, rcResult);
        return UpdateAreaAndFlags(NavMeshBuilder.CreateNavMeshData(option));
    }
}