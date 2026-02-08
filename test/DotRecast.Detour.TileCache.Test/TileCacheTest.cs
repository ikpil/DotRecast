/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
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

using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Recast.Geom;
using NUnit.Framework;

namespace DotRecast.Detour.TileCache.Test;


public class TileCacheTest : AbstractTileCacheTest
{
    [Test]
    public void TestFastLz()
    {
        TestDungeon(RcByteOrder.LITTLE_ENDIAN, false);
        TestDungeon(RcByteOrder.LITTLE_ENDIAN, true);
        TestDungeon(RcByteOrder.BIG_ENDIAN, false);
        TestDungeon(RcByteOrder.BIG_ENDIAN, true);
        Test(RcByteOrder.LITTLE_ENDIAN, false);
        Test(RcByteOrder.LITTLE_ENDIAN, true);
        Test(RcByteOrder.BIG_ENDIAN, false);
        Test(RcByteOrder.BIG_ENDIAN, true);
    }

    [Test]
    public void TestLZ4()
    {
        TestDungeon(RcByteOrder.LITTLE_ENDIAN, false);
        TestDungeon(RcByteOrder.LITTLE_ENDIAN, true);
        TestDungeon(RcByteOrder.BIG_ENDIAN, false);
        TestDungeon(RcByteOrder.BIG_ENDIAN, true);
        Test(RcByteOrder.LITTLE_ENDIAN, false);
        Test(RcByteOrder.LITTLE_ENDIAN, true);
        Test(RcByteOrder.BIG_ENDIAN, false);
        Test(RcByteOrder.BIG_ENDIAN, true);
    }

    private void TestDungeon(RcByteOrder order, bool cCompatibility)
    {
        IRcInputGeomProvider geom = RcSampleInputGeomProvider.LoadFile("dungeon.obj");
        DtTileCache tc = GetTileCache(geom, order, cCompatibility);
        TestTileLayerBuilder layerBuilder = new TestTileLayerBuilder(geom);
        List<byte[]> layers = layerBuilder.Build(order, cCompatibility, 1);
        int cacheLayerCount = 0;
        int cacheCompressedSize = 0;
        int cacheRawSize = 0;
        foreach (byte[] layer in layers)
        {
            long refs = tc.AddTile(layer, 0);
            tc.BuildNavMeshTile(refs);
            cacheLayerCount++;
            cacheCompressedSize += layer.Length;
            cacheRawSize += 4 * 48 * 48 + 56; // FIXME
        }

        Console.WriteLine("Compressor: " + tc.GetCompressor().GetType().Name + " C Compatibility: " + cCompatibility
                          + " Layers: " + cacheLayerCount + " Raw Size: " + cacheRawSize + " Compressed: " + cacheCompressedSize);
        Assert.That(tc.GetNavMesh().GetMaxTiles(), Is.EqualTo(256));
        Assert.That(tc.GetNavMesh().GetParams().maxPolys, Is.EqualTo(16384));
        Assert.That(tc.GetNavMesh().GetParams().tileWidth, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.GetNavMesh().GetParams().tileHeight, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.GetNavMesh().GetMaxVertsPerPoly(), Is.EqualTo(6));
        Assert.That(tc.GetParams().cs, Is.EqualTo(0.3f));
        Assert.That(tc.GetParams().ch, Is.EqualTo(0.2f));
        Assert.That(tc.GetParams().walkableClimb, Is.EqualTo(0.9f));
        Assert.That(tc.GetParams().walkableHeight, Is.EqualTo(2f));
        Assert.That(tc.GetParams().walkableRadius, Is.EqualTo(0.6f));
        Assert.That(tc.GetParams().width, Is.EqualTo(48));
        Assert.That(tc.GetParams().maxTiles, Is.EqualTo(6 * 7 * 4));
        Assert.That(tc.GetParams().maxObstacles, Is.EqualTo(128));
        Assert.That(tc.GetTileCount(), Is.EqualTo(168));

        // Tile0: Tris: 8, Verts: 18 Detail Meshed: 8 Detail Verts: 0 Detail Tris: 14
        DtMeshTile tile = tc.GetNavMesh().GetTile(0);
        DtMeshData data = tile.data;
        DtMeshHeader header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(18));
        Assert.That(header.polyCount, Is.EqualTo(8));
        Assert.That(header.detailMeshCount, Is.EqualTo(8));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(14));
        Assert.That(data.polys.Length, Is.EqualTo(8));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 18));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(8));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 14));
        Assert.That(data.verts[1], Is.EqualTo(14.997294f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(15.484785f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(15.484785f).Within(0.0001f));
        // Tile8: Tris: 3, Verts: 8 Detail Meshed: 3 Detail Verts: 0 Detail Tris: 6
        tile = tc.GetNavMesh().GetTile(8);
        data = tile.data;
        header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(8));
        Assert.That(header.polyCount, Is.EqualTo(3));
        Assert.That(header.detailMeshCount, Is.EqualTo(3));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(6));
        Assert.That(data.polys.Length, Is.EqualTo(3));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 8));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(3));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 6));
        // Tile16: Tris: 10, Verts: 20 Detail Meshed: 10 Detail Verts: 0 Detail Tris: 18
        tile = tc.GetNavMesh().GetTile(16);
        data = tile.data;
        header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(20));
        Assert.That(header.polyCount, Is.EqualTo(10));
        Assert.That(header.detailMeshCount, Is.EqualTo(10));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(18));
        Assert.That(data.polys.Length, Is.EqualTo(10));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 20));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(10));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 18));
        // Tile29: Tris: 1, Verts: 5 Detail Meshed: 1 Detail Verts: 0 Detail Tris: 3
        tile = tc.GetNavMesh().GetTile(29);
        data = tile.data;
        header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(5));
        Assert.That(header.polyCount, Is.EqualTo(1));
        Assert.That(header.detailMeshCount, Is.EqualTo(1));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(3));
        Assert.That(data.polys.Length, Is.EqualTo(1));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 5));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(1));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 3));
    }

    private void Test(RcByteOrder order, bool cCompatibility)
    {
        IRcInputGeomProvider geom = RcSampleInputGeomProvider.LoadFile("nav_test.obj");
        DtTileCache tc = GetTileCache(geom, order, cCompatibility);
        TestTileLayerBuilder layerBuilder = new TestTileLayerBuilder(geom);
        List<byte[]> layers = layerBuilder.Build(order, cCompatibility, 1);
        int cacheLayerCount = 0;
        int cacheCompressedSize = 0;
        int cacheRawSize = 0;
        foreach (byte[] layer in layers)
        {
            long refs = tc.AddTile(layer, 0);
            tc.BuildNavMeshTile(refs);
            cacheLayerCount++;
            cacheCompressedSize += layer.Length;
            cacheRawSize += 4 * 48 * 48 + 56;
        }

        Console.WriteLine("Compressor: " + tc.GetCompressor().GetType().Name + " C Compatibility: " + cCompatibility
                          + " Layers: " + cacheLayerCount + " Raw Size: " + cacheRawSize + " Compressed: " + cacheCompressedSize);
    }

    [Test]
    public void TestPerformance()
    {
        int threads = Environment.ProcessorCount;
        RcByteOrder order = RcByteOrder.LITTLE_ENDIAN;
        bool cCompatibility = false;

        IRcInputGeomProvider geom = RcSampleInputGeomProvider.LoadFile("dungeon.obj");
        TestTileLayerBuilder layerBuilder = new TestTileLayerBuilder(geom);
        for (int i = 0; i < 4; i++)
        {
            layerBuilder.Build(order, cCompatibility, 1);
            layerBuilder.Build(order, cCompatibility, threads);
        }

        long t1 = RcFrequency.Ticks;
        List<byte[]> layers = null;
        for (int i = 0; i < 8; i++)
        {
            layers = layerBuilder.Build(order, cCompatibility, 1);
        }

        long t2 = RcFrequency.Ticks;
        for (int i = 0; i < 8; i++)
        {
            layers = layerBuilder.Build(order, cCompatibility, threads);
        }

        long t3 = RcFrequency.Ticks;
        Console.WriteLine(" Time ST : " + (t2 - t1) / TimeSpan.TicksPerMillisecond);
        Console.WriteLine(" Time MT : " + (t3 - t2) / TimeSpan.TicksPerMillisecond);
        DtTileCache tc = GetTileCache(geom, order, cCompatibility);
        foreach (byte[] layer in layers)
        {
            long refs = tc.AddTile(layer, 0);
            tc.BuildNavMeshTile(refs);
        }

        Assert.That(tc.GetNavMesh().GetMaxTiles(), Is.EqualTo(256));
        Assert.That(tc.GetNavMesh().GetParams().maxPolys, Is.EqualTo(16384));
        Assert.That(tc.GetNavMesh().GetParams().tileWidth, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.GetNavMesh().GetParams().tileHeight, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.GetNavMesh().GetMaxVertsPerPoly(), Is.EqualTo(6));
        Assert.That(tc.GetParams().cs, Is.EqualTo(0.3f));
        Assert.That(tc.GetParams().ch, Is.EqualTo(0.2f));
        Assert.That(tc.GetParams().walkableClimb, Is.EqualTo(0.9f));
        Assert.That(tc.GetParams().walkableHeight, Is.EqualTo(2f));
        Assert.That(tc.GetParams().walkableRadius, Is.EqualTo(0.6f));
        Assert.That(tc.GetParams().width, Is.EqualTo(48));
        Assert.That(tc.GetParams().maxTiles, Is.EqualTo(6 * 7 * 4));
        Assert.That(tc.GetParams().maxObstacles, Is.EqualTo(128));
        Assert.That(tc.GetTileCount(), Is.EqualTo(168));
        // Tile0: Tris: 8, Verts: 18 Detail Meshed: 8 Detail Verts: 0 Detail Tris: 14
        DtMeshTile tile = tc.GetNavMesh().GetTile(0);
        DtMeshData data = tile.data;
        DtMeshHeader header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(18));
        Assert.That(header.polyCount, Is.EqualTo(8));
        Assert.That(header.detailMeshCount, Is.EqualTo(8));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(14));
        Assert.That(data.polys.Length, Is.EqualTo(8));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 18));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(8));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 14));
        Assert.That(data.verts[1], Is.EqualTo(14.997294f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(15.484785f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(15.484785f).Within(0.0001f));
        // Tile8: Tris: 3, Verts: 8 Detail Meshed: 3 Detail Verts: 0 Detail Tris: 6
        tile = tc.GetNavMesh().GetTile(8);
        data = tile.data;
        header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(8));
        Assert.That(header.polyCount, Is.EqualTo(3));
        Assert.That(header.detailMeshCount, Is.EqualTo(3));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(6));
        Assert.That(data.polys.Length, Is.EqualTo(3));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 8));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(3));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 6));
        // Tile16: Tris: 10, Verts: 20 Detail Meshed: 10 Detail Verts: 0 Detail Tris: 18
        tile = tc.GetNavMesh().GetTile(16);
        data = tile.data;
        header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(20));
        Assert.That(header.polyCount, Is.EqualTo(10));
        Assert.That(header.detailMeshCount, Is.EqualTo(10));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(18));
        Assert.That(data.polys.Length, Is.EqualTo(10));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 20));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(10));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 18));
        // Tile29: Tris: 1, Verts: 5 Detail Meshed: 1 Detail Verts: 0 Detail Tris: 3
        tile = tc.GetNavMesh().GetTile(29);
        data = tile.data;
        header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(5));
        Assert.That(header.polyCount, Is.EqualTo(1));
        Assert.That(header.detailMeshCount, Is.EqualTo(1));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(3));
        Assert.That(data.polys.Length, Is.EqualTo(1));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 5));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(1));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 3));
    }
}