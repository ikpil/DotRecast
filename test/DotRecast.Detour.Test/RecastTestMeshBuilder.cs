/*
recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

using DotRecast.Core;
using DotRecast.Recast;
using DotRecast.Recast.Geom;

namespace DotRecast.Detour.Test;

public class RecastTestMeshBuilder
{
    private readonly DtMeshData meshData;
    private const float m_cellSize = 0.3f;
    private const float m_cellHeight = 0.2f;
    private const float m_agentHeight = 2.0f;
    private const float m_agentRadius = 0.6f;
    private const float m_agentMaxClimb = 0.9f;
    private const float m_agentMaxSlope = 45.0f;
    private const int m_regionMinSize = 8;
    private const int m_regionMergeSize = 20;
    private const float m_edgeMaxLen = 12.0f;
    private const float m_edgeMaxError = 1.3f;
    private const int m_vertsPerPoly = 6;
    private const float m_detailSampleDist = 6.0f;
    private const float m_detailSampleMaxError = 1.0f;

    public RecastTestMeshBuilder()
        : this(SimpleInputGeomProvider.LoadFile("dungeon.obj"),
            RcPartition.WATERSHED,
            m_cellSize, m_cellHeight,
            m_agentMaxSlope, m_agentHeight, m_agentRadius, m_agentMaxClimb,
            m_regionMinSize, m_regionMergeSize,
            m_edgeMaxLen, m_edgeMaxError,
            m_vertsPerPoly,
            m_detailSampleDist, m_detailSampleMaxError)
    {
    }

    public RecastTestMeshBuilder(IInputGeomProvider geom,
        RcPartition partition,
        float cellSize, float cellHeight,
        float agentMaxSlope, float agentHeight, float agentRadius, float agentMaxClimb,
        int regionMinSize, int regionMergeSize,
        float edgeMaxLen, float edgeMaxError,
        int vertsPerPoly,
        float detailSampleDist, float detailSampleMaxError)
    {
        RcConfig cfg = new RcConfig(
            partition,
            cellSize, cellHeight,
            agentMaxSlope, agentHeight, agentRadius, agentMaxClimb,
            regionMinSize, regionMergeSize,
            edgeMaxLen, edgeMaxError,
            vertsPerPoly,
            detailSampleDist, detailSampleMaxError,
            true, true, true,
            SampleAreaModifications.SAMPLE_AREAMOD_GROUND, true);
        RcBuilderConfig bcfg = new RcBuilderConfig(cfg, geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax());
        RcBuilder rcBuilder = new RcBuilder();
        RcBuilderResult rcResult = rcBuilder.Build(geom, bcfg);
        RcPolyMesh m_pmesh = rcResult.GetMesh();
        for (int i = 0; i < m_pmesh.npolys; ++i)
        {
            m_pmesh.flags[i] = 1;
        }

        RcPolyMeshDetail m_dmesh = rcResult.GetMeshDetail();
        DtNavMeshCreateParams option = new DtNavMeshCreateParams();
        option.verts = m_pmesh.verts;
        option.vertCount = m_pmesh.nverts;
        option.polys = m_pmesh.polys;
        option.polyAreas = m_pmesh.areas;
        option.polyFlags = m_pmesh.flags;
        option.polyCount = m_pmesh.npolys;
        option.nvp = m_pmesh.nvp;
        option.detailMeshes = m_dmesh.meshes;
        option.detailVerts = m_dmesh.verts;
        option.detailVertsCount = m_dmesh.nverts;
        option.detailTris = m_dmesh.tris;
        option.detailTriCount = m_dmesh.ntris;
        option.walkableHeight = agentHeight;
        option.walkableRadius = agentRadius;
        option.walkableClimb = agentMaxClimb;
        option.bmin = m_pmesh.bmin;
        option.bmax = m_pmesh.bmax;
        option.cs = cellSize;
        option.ch = cellHeight;
        option.buildBvTree = true;

        option.offMeshConVerts = new float[6];
        option.offMeshConVerts[0] = 0.1f;
        option.offMeshConVerts[1] = 0.2f;
        option.offMeshConVerts[2] = 0.3f;
        option.offMeshConVerts[3] = 0.4f;
        option.offMeshConVerts[4] = 0.5f;
        option.offMeshConVerts[5] = 0.6f;
        option.offMeshConRad = new float[1];
        option.offMeshConRad[0] = 0.1f;
        option.offMeshConDir = new int[1];
        option.offMeshConDir[0] = 1;
        option.offMeshConAreas = new int[1];
        option.offMeshConAreas[0] = 2;
        option.offMeshConFlags = new int[1];
        option.offMeshConFlags[0] = 12;
        option.offMeshConUserID = new int[1];
        option.offMeshConUserID[0] = 0x4567;
        option.offMeshConCount = 1;
        meshData = DtNavMeshBuilder.CreateNavMeshData(option);
    }

    public DtMeshData GetMeshData()
    {
        return meshData;
    }
}