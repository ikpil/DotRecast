/*
recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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
using System.IO;
using DotRecast.Core;
using System.Numerics;
using DotRecast.Detour.Io;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using NUnit.Framework;


namespace DotRecast.Detour.Test.Io;

public class MeshSetReaderWriterTest
{
    private readonly DtMeshSetWriter writer = new DtMeshSetWriter();
    private readonly DtMeshSetReader reader = new DtMeshSetReader();
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
    private const int m_maxTiles = 128;
    private const int m_maxPolysPerTile = 0x8000;

    [Test]
    public void Test()
    {
        IInputGeomProvider geom = SimpleInputGeomProvider.LoadFile("dungeon.obj");

        NavMeshSetHeader header = new NavMeshSetHeader();
        header.magic = NavMeshSetHeader.NAVMESHSET_MAGIC;
        header.version = NavMeshSetHeader.NAVMESHSET_VERSION;
        header.option.orig = geom.GetMeshBoundsMin();
        header.option.tileWidth = m_tileSize * m_cellSize;
        header.option.tileHeight = m_tileSize * m_cellSize;
        header.option.maxTiles = m_maxTiles;
        header.option.maxPolys = m_maxPolysPerTile;
        header.numTiles = 0;
        DtNavMesh mesh = new DtNavMesh();
        mesh.Init(header.option, 6);

        Vector3 bmin = geom.GetMeshBoundsMin();
        Vector3 bmax = geom.GetMeshBoundsMax();
        RcRecast.CalcTileCount(bmin, bmax, m_cellSize, m_tileSize, m_tileSize, out var tw, out var th);
        for (int y = 0; y < th; ++y)
        {
            for (int x = 0; x < tw; ++x)
            {
                RcConfig cfg = new RcConfig(true, m_tileSize, m_tileSize,
                    RcConfig.CalcBorder(m_agentRadius, m_cellSize),
                    RcPartition.WATERSHED,
                    m_cellSize, m_cellHeight,
                    m_agentMaxSlope, m_agentHeight, m_agentRadius, m_agentMaxClimb,
                    m_regionMinArea, m_regionMergeArea,
                    m_edgeMaxLen, m_edgeMaxError,
                    m_vertsPerPoly,
                    m_detailSampleDist, m_detailSampleMaxError,
                    true, true, true,
                    SampleAreaModifications.SAMPLE_AREAMOD_GROUND, true);
                RcBuilderConfig bcfg = new RcBuilderConfig(cfg, bmin, bmax, x, y);
                TestDetourBuilder db = new TestDetourBuilder();
                DtMeshData data = db.Build(geom, bcfg, m_agentHeight, m_agentRadius, m_agentMaxClimb, x, y, true);
                if (data != null)
                {
                    mesh.RemoveTile(mesh.GetTileRefAt(x, y, 0));
                    mesh.AddTile(data, 0, 0, out _);
                }
            }
        }

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        writer.Write(bw, mesh, RcByteOrder.LITTLE_ENDIAN, true);
        ms.Seek(0, SeekOrigin.Begin);

        using var br = new BinaryReader(ms);
        mesh = reader.Read(br, 6);
        Assert.That(mesh.GetMaxTiles(), Is.EqualTo(128));
        Assert.That(mesh.GetParams().maxPolys, Is.EqualTo(0x8000));
        Assert.That(mesh.GetParams().tileWidth, Is.EqualTo(9.6f).Within(0.001f));

        const int MAX_NEIS = 32;
        DtMeshTile[] tiles = new DtMeshTile[MAX_NEIS];
        int nneis = 0;

        nneis = mesh.GetTilesAt(6, 9, tiles, MAX_NEIS);
        Assert.That(nneis, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(2));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(7 * 3));

        nneis = mesh.GetTilesAt(2, 9, tiles, MAX_NEIS);
        Assert.That(nneis, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(2));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(9 * 3));

        nneis = mesh.GetTilesAt(4, 3, tiles, MAX_NEIS);
        Assert.That(nneis, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(3));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(6 * 3));

        nneis = mesh.GetTilesAt(2, 8, tiles, MAX_NEIS);
        Assert.That(nneis, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(5));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(17 * 3));
    }
}