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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Recast.Geom;
using NUnit.Framework;

namespace DotRecast.Recast.Test;

using static RecastConstants;

public class RecastTileMeshTest
{
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
    private const PartitionType m_partitionType = PartitionType.WATERSHED;
    private const int m_tileSize = 32;

    [Test]
    public void testDungeon()
    {
        testBuild("dungeon.obj");
    }

    public void testBuild(string filename)
    {
        InputGeomProvider geom = ObjImporter.load(Loader.ToBytes(filename));
        RecastBuilder builder = new RecastBuilder();
        RecastConfig cfg = new RecastConfig(true, m_tileSize, m_tileSize, RecastConfig.calcBorder(m_agentRadius, m_cellSize),
            m_partitionType, m_cellSize, m_cellHeight, m_agentMaxSlope, true, true, true, m_agentHeight, m_agentRadius,
            m_agentMaxClimb, m_regionMinArea, m_regionMergeArea, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly, true,
            m_detailSampleDist, m_detailSampleMaxError, SampleAreaModifications.SAMPLE_AREAMOD_GROUND);
        RecastBuilderConfig bcfg = new RecastBuilderConfig(cfg, geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), 7, 8);
        RecastBuilderResult rcResult = builder.build(geom, bcfg);
        Assert.That(rcResult.getMesh().npolys, Is.EqualTo(1));
        Assert.That(rcResult.getMesh().nverts, Is.EqualTo(5));
        bcfg = new RecastBuilderConfig(cfg, geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), 6, 9);
        rcResult = builder.build(geom, bcfg);
        Assert.That(rcResult.getMesh().npolys, Is.EqualTo(2));
        Assert.That(rcResult.getMesh().nverts, Is.EqualTo(7));
        bcfg = new RecastBuilderConfig(cfg, geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), 2, 9);
        rcResult = builder.build(geom, bcfg);
        Assert.That(rcResult.getMesh().npolys, Is.EqualTo(2));
        Assert.That(rcResult.getMesh().nverts, Is.EqualTo(9));
        bcfg = new RecastBuilderConfig(cfg, geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), 4, 3);
        rcResult = builder.build(geom, bcfg);
        Assert.That(rcResult.getMesh().npolys, Is.EqualTo(3));
        Assert.That(rcResult.getMesh().nverts, Is.EqualTo(6));
        bcfg = new RecastBuilderConfig(cfg, geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), 2, 8);
        rcResult = builder.build(geom, bcfg);
        Assert.That(rcResult.getMesh().npolys, Is.EqualTo(5));
        Assert.That(rcResult.getMesh().nverts, Is.EqualTo(17));
        bcfg = new RecastBuilderConfig(cfg, geom.getMeshBoundsMin(), geom.getMeshBoundsMax(), 0, 8);
        rcResult = builder.build(geom, bcfg);
        Assert.That(rcResult.getMesh().npolys, Is.EqualTo(6));
        Assert.That(rcResult.getMesh().nverts, Is.EqualTo(15));
    }

    [Test]
    public void testPerformance()
    {
        InputGeomProvider geom = ObjImporter.load(Loader.ToBytes("dungeon.obj"));
        RecastBuilder builder = new RecastBuilder();
        RecastConfig cfg = new RecastConfig(true, m_tileSize, m_tileSize, RecastConfig.calcBorder(m_agentRadius, m_cellSize),
            m_partitionType, m_cellSize, m_cellHeight, m_agentMaxSlope, true, true, true, m_agentHeight, m_agentRadius,
            m_agentMaxClimb, m_regionMinArea, m_regionMergeArea, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly, true,
            m_detailSampleDist, m_detailSampleMaxError, SampleAreaModifications.SAMPLE_AREAMOD_GROUND);
        for (int i = 0; i < 4; i++)
        {
            build(geom, builder, cfg, 1, true);
            build(geom, builder, cfg, 4, true);
        }

        long t1 = Stopwatch.GetTimestamp();
        for (int i = 0; i < 4; i++)
        {
            build(geom, builder, cfg, 1, false);
        }

        long t2 = Stopwatch.GetTimestamp();
        for (int i = 0; i < 4; i++)
        {
            build(geom, builder, cfg, 4, false);
        }

        long t3 = Stopwatch.GetTimestamp();
        Console.WriteLine(" Time ST : " + (t2 - t1) / 1000000);
        Console.WriteLine(" Time MT : " + (t3 - t2) / 1000000);
    }

    private void build(InputGeomProvider geom, RecastBuilder builder, RecastConfig cfg, int threads, bool validate)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        List<RecastBuilderResult> tiles = new();
        var task = builder.buildTilesAsync(geom, cfg, threads, tiles, Task.Factory, cts.Token);
        if (validate)
        {
            RecastBuilderResult rcResult = getTile(tiles, 7, 8);
            Assert.That(rcResult.getMesh().npolys, Is.EqualTo(1));
            Assert.That(rcResult.getMesh().nverts, Is.EqualTo(5));
            rcResult = getTile(tiles, 6, 9);
            Assert.That(rcResult.getMesh().npolys, Is.EqualTo(2));
            Assert.That(rcResult.getMesh().nverts, Is.EqualTo(7));
            rcResult = getTile(tiles, 2, 9);
            Assert.That(rcResult.getMesh().npolys, Is.EqualTo(2));
            Assert.That(rcResult.getMesh().nverts, Is.EqualTo(9));
            rcResult = getTile(tiles, 4, 3);
            Assert.That(rcResult.getMesh().npolys, Is.EqualTo(3));
            Assert.That(rcResult.getMesh().nverts, Is.EqualTo(6));
            rcResult = getTile(tiles, 2, 8);
            Assert.That(rcResult.getMesh().npolys, Is.EqualTo(5));
            Assert.That(rcResult.getMesh().nverts, Is.EqualTo(17));
            rcResult = getTile(tiles, 0, 8);
            Assert.That(rcResult.getMesh().npolys, Is.EqualTo(6));
            Assert.That(rcResult.getMesh().nverts, Is.EqualTo(15));
        }

        try
        {
            cts.Cancel();
            //executor.awaitTermination(1000, TimeUnit.HOURS);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private RecastBuilderResult getTile(List<RecastBuilderResult> tiles, int x, int z)
    {
        return tiles.FirstOrDefault(tile => tile.tileX == x && tile.tileZ == z);
    }
}