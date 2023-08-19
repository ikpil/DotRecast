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

using System.Collections.Generic;
using System.IO;
using DotRecast.Core;
using DotRecast.Detour.TileCache.Io;
using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using NUnit.Framework;

namespace DotRecast.Detour.TileCache.Test.Io;

[Parallelizable]
public class TileCacheReaderWriterTest : AbstractTileCacheTest
{
    private readonly DtTileCacheReader reader = new DtTileCacheReader(DtTileCacheCompressorFactory.Shared);
    private readonly DtTileCacheWriter writer = new DtTileCacheWriter(DtTileCacheCompressorFactory.Shared);

    [Test]
    public void TestFastLz()
    {
        TestDungeon(false);
        TestDungeon(true);
    }

    [Test]
    public void TestLZ4()
    {
        TestDungeon(true);
        TestDungeon(false);
    }

    private void TestDungeon(bool cCompatibility)
    {
        IInputGeomProvider geom = ObjImporter.Load(Loader.ToBytes("dungeon.obj"));
        TestTileLayerBuilder layerBuilder = new TestTileLayerBuilder(geom);
        List<byte[]> layers = layerBuilder.Build(RcByteOrder.LITTLE_ENDIAN, cCompatibility, 1);
        DtTileCache tc = GetTileCache(geom, RcByteOrder.LITTLE_ENDIAN, cCompatibility);
        foreach (byte[] layer in layers)
        {
            long refs = tc.AddTile(layer, 0);
            tc.BuildNavMeshTile(refs);
        }

        using var msw = new MemoryStream();
        using var bw = new BinaryWriter(msw);
        writer.Write(bw, tc, RcByteOrder.LITTLE_ENDIAN, cCompatibility);

        using var msr = new MemoryStream(msw.ToArray());
        using var br = new BinaryReader(msr);
        tc = reader.Read(br, 6, null);
        Assert.That(tc.GetNavMesh().GetMaxTiles(), Is.EqualTo(256));
        Assert.That(tc.GetNavMesh().GetParams().maxPolys, Is.EqualTo(16384));
        Assert.That(tc.GetNavMesh().GetParams().tileWidth, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.GetNavMesh().GetParams().tileHeight, Is.EqualTo(14.4f).Within(0.001f));
        Assert.That(tc.GetNavMesh().GetMaxVertsPerPoly(), Is.EqualTo(6));
        Assert.That(tc.GetParams().cs, Is.EqualTo(0.3f).Within(0.0f));
        Assert.That(tc.GetParams().ch, Is.EqualTo(0.2f).Within(0.0f));
        Assert.That(tc.GetParams().walkableClimb, Is.EqualTo(0.9f).Within(0.0f));
        Assert.That(tc.GetParams().walkableHeight, Is.EqualTo(2f).Within(0.0f));
        Assert.That(tc.GetParams().walkableRadius, Is.EqualTo(0.6f).Within(0.0f));
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