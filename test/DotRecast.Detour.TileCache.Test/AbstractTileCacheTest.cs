/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
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

using DotRecast.Core;
using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Recast.Geom;

using static DotRecast.Detour.DetourCommon;
using static DotRecast.Recast.RecastVectors;

namespace DotRecast.Detour.TileCache.Test;

public class AbstractTileCacheTest {

    private const int EXPECTED_LAYERS_PER_TILE = 4;
    private readonly float m_cellSize = 0.3f;
    private readonly float m_cellHeight = 0.2f;
    private readonly float m_agentHeight = 2.0f;
    private readonly float m_agentRadius = 0.6f;
    private readonly float m_agentMaxClimb = 0.9f;
    private readonly float m_edgeMaxError = 1.3f;
    private readonly int m_tileSize = 48;

    protected class TestTileCacheMeshProcess : TileCacheMeshProcess {
        public void process(NavMeshDataCreateParams option) {
            for (int i = 0; i < option.polyCount; ++i) {
                option.polyFlags[i] = 1;
            }
        }
    }

    public TileCache getTileCache(InputGeomProvider geom, ByteOrder order, bool cCompatibility) {
        TileCacheParams option = new TileCacheParams();
        int[] twh = Recast.Recast.calcTileCount(geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), m_cellSize, m_tileSize, m_tileSize);
        option.ch = m_cellHeight;
        option.cs = m_cellSize;
        vCopy(option.orig, geom.getMeshBoundsMin());
        option.height = m_tileSize;
        option.width = m_tileSize;
        option.walkableHeight = m_agentHeight;
        option.walkableRadius = m_agentRadius;
        option.walkableClimb = m_agentMaxClimb;
        option.maxSimplificationError = m_edgeMaxError;
        option.maxTiles = twh[0] * twh[1] * EXPECTED_LAYERS_PER_TILE;
        option.maxObstacles = 128;
        NavMeshParams navMeshParams = new NavMeshParams();
        copy(navMeshParams.orig, geom.getMeshBoundsMin());
        navMeshParams.tileWidth = m_tileSize * m_cellSize;
        navMeshParams.tileHeight = m_tileSize * m_cellSize;
        navMeshParams.maxTiles = 256;
        navMeshParams.maxPolys = 16384;
        NavMesh navMesh = new NavMesh(navMeshParams, 6);
        TileCache tc = new TileCache(option, new TileCacheStorageParams(order, cCompatibility), navMesh,
                TileCacheCompressorFactory.get(cCompatibility), new TestTileCacheMeshProcess());
        return tc;
    }

}
