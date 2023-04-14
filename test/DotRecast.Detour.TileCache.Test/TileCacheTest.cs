/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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
using DotRecast.Core;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using NUnit.Framework;

namespace DotRecast.Detour.TileCache.Test;

public class TileCacheTest : AbstractTileCacheTest
{
    [Test]
    public void testFastLz()
    {
        testDungeon(ByteOrder.LITTLE_ENDIAN, false);
        testDungeon(ByteOrder.LITTLE_ENDIAN, true);
        testDungeon(ByteOrder.BIG_ENDIAN, false);
        testDungeon(ByteOrder.BIG_ENDIAN, true);
        test(ByteOrder.LITTLE_ENDIAN, false);
        test(ByteOrder.LITTLE_ENDIAN, true);
        test(ByteOrder.BIG_ENDIAN, false);
        test(ByteOrder.BIG_ENDIAN, true);
    }

    [Test]
    public void testLZ4()
    {
        testDungeon(ByteOrder.LITTLE_ENDIAN, false);
        testDungeon(ByteOrder.LITTLE_ENDIAN, true);
        testDungeon(ByteOrder.BIG_ENDIAN, false);
        testDungeon(ByteOrder.BIG_ENDIAN, true);
        test(ByteOrder.LITTLE_ENDIAN, false);
        test(ByteOrder.LITTLE_ENDIAN, true);
        test(ByteOrder.BIG_ENDIAN, false);
        test(ByteOrder.BIG_ENDIAN, true);
    }

    private void testDungeon(ByteOrder order, bool cCompatibility)
    {
        InputGeomProvider geom = ObjImporter.load(Loader.ToBytes("dungeon.obj"));
        TileCache tc = getTileCache(geom, order, cCompatibility);
        TestTileLayerBuilder layerBuilder = new TestTileLayerBuilder(geom);
        List<byte[]> layers = layerBuilder.build(order, cCompatibility, 1);
        int cacheLayerCount = 0;
        int cacheCompressedSize = 0;
        int cacheRawSize = 0;
        foreach (byte[] layer in layers)
        {
            long refs = tc.addTile(layer, 0);
            tc.buildNavMeshTile(refs);
            cacheLayerCount++;
            cacheCompressedSize += layer.Length;
            cacheRawSize += 4 * 48 * 48 + 56; // FIXME
        }

        Console.WriteLine("Compressor: " + tc.getCompressor().GetType().Name + " C Compatibility: " + cCompatibility
                          + " Layers: " + cacheLayerCount + " Raw Size: " + cacheRawSize + " Compressed: " + cacheCompressedSize);
        Assert.That(tc.getNavMesh().getMaxTiles(), Is.EqualTo(256));
        Assert.That(tc.getNavMesh().getParams().maxPolys, Is.EqualTo(16384));
        Assert.That(tc.getNavMesh().getParams().tileWidth, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.getNavMesh().getParams().tileHeight, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.getNavMesh().getMaxVertsPerPoly(), Is.EqualTo(6));
        Assert.That(tc.getParams().cs, Is.EqualTo(0.3f));
        Assert.That(tc.getParams().ch, Is.EqualTo(0.2f));
        Assert.That(tc.getParams().walkableClimb, Is.EqualTo(0.9f));
        Assert.That(tc.getParams().walkableHeight, Is.EqualTo(2f));
        Assert.That(tc.getParams().walkableRadius, Is.EqualTo(0.6f));
        Assert.That(tc.getParams().width, Is.EqualTo(48));
        Assert.That(tc.getParams().maxTiles, Is.EqualTo(6 * 7 * 4));
        Assert.That(tc.getParams().maxObstacles, Is.EqualTo(128));
        Assert.That(tc.getTileCount(), Is.EqualTo(168));

        // Tile0: Tris: 8, Verts: 18 Detail Meshed: 8 Detail Verts: 0 Detail Tris: 14
        MeshTile tile = tc.getNavMesh().getTile(0);
        MeshData data = tile.data;
        MeshHeader header = data.header;
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
        tile = tc.getNavMesh().getTile(8);
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
        tile = tc.getNavMesh().getTile(16);
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
        tile = tc.getNavMesh().getTile(29);
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

    private void test(ByteOrder order, bool cCompatibility)
    {
        InputGeomProvider geom = ObjImporter.load(Loader.ToBytes("nav_test.obj"));
        TileCache tc = getTileCache(geom, order, cCompatibility);
        TestTileLayerBuilder layerBuilder = new TestTileLayerBuilder(geom);
        List<byte[]> layers = layerBuilder.build(order, cCompatibility, 1);
        int cacheLayerCount = 0;
        int cacheCompressedSize = 0;
        int cacheRawSize = 0;
        foreach (byte[] layer in layers)
        {
            long refs = tc.addTile(layer, 0);
            tc.buildNavMeshTile(refs);
            cacheLayerCount++;
            cacheCompressedSize += layer.Length;
            cacheRawSize += 4 * 48 * 48 + 56;
        }

        Console.WriteLine("Compressor: " + tc.getCompressor().GetType().Name + " C Compatibility: " + cCompatibility
                          + " Layers: " + cacheLayerCount + " Raw Size: " + cacheRawSize + " Compressed: " + cacheCompressedSize);
    }

    [Test]
    public void testPerformance()
    {
        int threads = 4;
        ByteOrder order = ByteOrder.LITTLE_ENDIAN;
        bool cCompatibility = false;
        InputGeomProvider geom = ObjImporter.load(Loader.ToBytes("dungeon.obj"));
        TestTileLayerBuilder layerBuilder = new TestTileLayerBuilder(geom);
        for (int i = 0; i < 4; i++)
        {
            layerBuilder.build(order, cCompatibility, 1);
            layerBuilder.build(order, cCompatibility, threads);
        }

        long t1 = FrequencyWatch.Ticks;
        List<byte[]> layers = null;
        for (int i = 0; i < 8; i++)
        {
            layers = layerBuilder.build(order, cCompatibility, 1);
        }

        long t2 = FrequencyWatch.Ticks;
        for (int i = 0; i < 8; i++)
        {
            layers = layerBuilder.build(order, cCompatibility, threads);
        }

        long t3 = FrequencyWatch.Ticks;
        Console.WriteLine(" Time ST : " + (t2 - t1) / TimeSpan.TicksPerMillisecond);
        Console.WriteLine(" Time MT : " + (t3 - t2) / TimeSpan.TicksPerMillisecond);
        TileCache tc = getTileCache(geom, order, cCompatibility);
        foreach (byte[] layer in layers)
        {
            long refs = tc.addTile(layer, 0);
            tc.buildNavMeshTile(refs);
        }

        Assert.That(tc.getNavMesh().getMaxTiles(), Is.EqualTo(256));
        Assert.That(tc.getNavMesh().getParams().maxPolys, Is.EqualTo(16384));
        Assert.That(tc.getNavMesh().getParams().tileWidth, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.getNavMesh().getParams().tileHeight, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.getNavMesh().getMaxVertsPerPoly(), Is.EqualTo(6));
        Assert.That(tc.getParams().cs, Is.EqualTo(0.3f));
        Assert.That(tc.getParams().ch, Is.EqualTo(0.2f));
        Assert.That(tc.getParams().walkableClimb, Is.EqualTo(0.9f));
        Assert.That(tc.getParams().walkableHeight, Is.EqualTo(2f));
        Assert.That(tc.getParams().walkableRadius, Is.EqualTo(0.6f));
        Assert.That(tc.getParams().width, Is.EqualTo(48));
        Assert.That(tc.getParams().maxTiles, Is.EqualTo(6 * 7 * 4));
        Assert.That(tc.getParams().maxObstacles, Is.EqualTo(128));
        Assert.That(tc.getTileCount(), Is.EqualTo(168));
        // Tile0: Tris: 8, Verts: 18 Detail Meshed: 8 Detail Verts: 0 Detail Tris: 14
        MeshTile tile = tc.getNavMesh().getTile(0);
        MeshData data = tile.data;
        MeshHeader header = data.header;
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
        tile = tc.getNavMesh().getTile(8);
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
        tile = tc.getNavMesh().getTile(16);
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
        tile = tc.getNavMesh().getTile(29);
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