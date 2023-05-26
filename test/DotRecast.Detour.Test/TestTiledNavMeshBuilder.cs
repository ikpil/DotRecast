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

using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Recast;
using DotRecast.Recast.Geom;


namespace DotRecast.Detour.Test;

public class TestTiledNavMeshBuilder
{
    private readonly NavMesh navMesh;
    private const float m_cellSize = 0.3f;
    private const float m_cellHeight = 0.2f;
    private const float m_agentHeight = 2.0f;
    private const float m_agentRadius = 0.6f;
    private const float m_agentMaxClimb = 0.9f;
    private const float m_agentMaxSlope = 45.0f;
    private const int m_regionMinSize = 8;
    private const int m_regionMergeSize = 20;
    private const float m_regionMinArea = m_regionMinSize * m_regionMinSize * m_cellSize * m_cellSize;
    private const float m_regionMergeArea = m_regionMergeSize * m_regionMergeSize * m_cellSize * m_cellSize;
    private const float m_edgeMaxLen = 12.0f;
    private const float m_edgeMaxError = 1.3f;
    private const int m_vertsPerPoly = 6;
    private const float m_detailSampleDist = 6.0f;
    private const float m_detailSampleMaxError = 1.0f;
    private const int m_tileSize = 32;

    public TestTiledNavMeshBuilder() :
        this(ObjImporter.Load(Loader.ToBytes("dungeon.obj")),
            PartitionType.WATERSHED, m_cellSize, m_cellHeight, m_agentHeight, m_agentRadius, m_agentMaxClimb, m_agentMaxSlope,
            m_regionMinSize, m_regionMergeSize, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly, m_detailSampleDist,
            m_detailSampleMaxError, m_tileSize)
    {
    }

    public TestTiledNavMeshBuilder(IInputGeomProvider m_geom, PartitionType m_partitionType, float m_cellSize, float m_cellHeight,
        float m_agentHeight, float m_agentRadius, float m_agentMaxClimb, float m_agentMaxSlope, int m_regionMinSize,
        int m_regionMergeSize, float m_edgeMaxLen, float m_edgeMaxError, int m_vertsPerPoly, float m_detailSampleDist,
        float m_detailSampleMaxError, int m_tileSize)
    {
        // Create empty nav mesh
        NavMeshParams navMeshParams = new NavMeshParams();
        navMeshParams.orig = m_geom.GetMeshBoundsMin();
        navMeshParams.tileWidth = m_tileSize * m_cellSize;
        navMeshParams.tileHeight = m_tileSize * m_cellSize;
        navMeshParams.maxTiles = 128;
        navMeshParams.maxPolys = 32768;
        navMesh = new NavMesh(navMeshParams, 6);

        // Build all tiles
        RecastConfig cfg = new RecastConfig(true, m_tileSize, m_tileSize, RecastConfig.CalcBorder(m_agentRadius, m_cellSize),
            m_partitionType, m_cellSize, m_cellHeight, m_agentMaxSlope, true, true, true, m_agentHeight, m_agentRadius,
            m_agentMaxClimb, m_regionMinArea, m_regionMergeArea, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly, true,
            m_detailSampleDist, m_detailSampleMaxError, SampleAreaModifications.SAMPLE_AREAMOD_GROUND);
        RecastBuilder rcBuilder = new RecastBuilder();
        List<RecastBuilderResult> rcResult = rcBuilder.BuildTiles(m_geom, cfg, null);

        // Add tiles to nav mesh

        foreach (RecastBuilderResult result in rcResult)
        {
            PolyMesh pmesh = result.GetMesh();
            if (pmesh.npolys == 0)
            {
                continue;
            }

            for (int i = 0; i < pmesh.npolys; ++i)
            {
                pmesh.flags[i] = 1;
            }

            NavMeshDataCreateParams option = new NavMeshDataCreateParams();
            option.verts = pmesh.verts;
            option.vertCount = pmesh.nverts;
            option.polys = pmesh.polys;
            option.polyAreas = pmesh.areas;
            option.polyFlags = pmesh.flags;
            option.polyCount = pmesh.npolys;
            option.nvp = pmesh.nvp;
            PolyMeshDetail dmesh = result.GetMeshDetail();
            option.detailMeshes = dmesh.meshes;
            option.detailVerts = dmesh.verts;
            option.detailVertsCount = dmesh.nverts;
            option.detailTris = dmesh.tris;
            option.detailTriCount = dmesh.ntris;
            option.walkableHeight = m_agentHeight;
            option.walkableRadius = m_agentRadius;
            option.walkableClimb = m_agentMaxClimb;
            option.bmin = pmesh.bmin;
            option.bmax = pmesh.bmax;
            option.cs = m_cellSize;
            option.ch = m_cellHeight;
            option.tileX = result.tileX;
            option.tileZ = result.tileZ;
            option.buildBvTree = true;
            navMesh.AddTile(NavMeshBuilder.CreateNavMeshData(option), 0, 0);
        }
    }

    public NavMesh GetNavMesh()
    {
        return navMesh;
    }
}