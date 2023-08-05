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
using System.IO;
using DotRecast.Core;
using DotRecast.Detour.TileCache.Io;
using NUnit.Framework;

namespace DotRecast.Detour.TileCache.Test.Io;

[Parallelizable]
public class TileCacheReaderTest
{
    private readonly DtTileCacheReader reader = new DtTileCacheReader(DtTileCacheCompressorForTestFactory.Shared);

    [Test]
    public void TestNavmesh()
    {
        using var ms = new MemoryStream(Loader.ToBytes("all_tiles_tilecache.bin"));
        using var @is = new BinaryReader(ms);
        DtTileCache tc = reader.Read(@is, 6, null);
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
        // Tile0: Tris: 1, Verts: 4 Detail Meshed: 1 Detail Verts: 0 Detail Tris: 2
        // Verts: -2.269517, 28.710686, 28.710686
        DtMeshTile tile = tc.GetNavMesh().GetTile(0);
        DtMeshData data = tile.data;
        DtMeshHeader header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(4));
        Assert.That(header.polyCount, Is.EqualTo(1));
        Assert.That(header.detailMeshCount, Is.EqualTo(1));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(2));
        Assert.That(data.polys.Length, Is.EqualTo(1));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 4));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(1));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 2));
        Assert.That(data.verts[1], Is.EqualTo(-2.269517f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(28.710686f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(28.710686f).Within(0.0001f));
        // Tile8: Tris: 7, Verts: 10 Detail Meshed: 7 Detail Verts: 0 Detail Tris: 10
        // Verts: 0.330483, 43.110687, 43.110687
        tile = tc.GetNavMesh().GetTile(8);
        data = tile.data;
        header = data.header;
        Console.WriteLine(data.header.x + "  " + data.header.y + "  " + data.header.layer);
        Assert.That(header.x, Is.EqualTo(4));
        Assert.That(header.y, Is.EqualTo(1));
        Assert.That(header.layer, Is.EqualTo(0));
        Assert.That(header.vertCount, Is.EqualTo(10));
        Assert.That(header.polyCount, Is.EqualTo(7));
        Assert.That(header.detailMeshCount, Is.EqualTo(7));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(10));
        Assert.That(data.polys.Length, Is.EqualTo(7));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 10));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(7));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 10));
        Assert.That(data.verts[1], Is.EqualTo(0.330483f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(43.110687f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(43.110687f).Within(0.0001f));
        // Tile16: Tris: 13, Verts: 33 Detail Meshed: 13 Detail Verts: 0 Detail Tris: 25
        // Verts: 1.130483, 5.610685, 6.510685
        tile = tc.GetNavMesh().GetTile(16);
        data = tile.data;
        header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(33));
        Assert.That(header.polyCount, Is.EqualTo(13));
        Assert.That(header.detailMeshCount, Is.EqualTo(13));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(25));
        Assert.That(data.polys.Length, Is.EqualTo(13));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 33));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(13));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 25));
        Assert.That(data.verts[1], Is.EqualTo(1.130483f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(5.610685f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(6.510685f).Within(0.0001f));
        // Tile29: Tris: 5, Verts: 15 Detail Meshed: 5 Detail Verts: 0 Detail Tris: 11
        // Verts: 10.330483, 10.110685, 10.110685
        tile = tc.GetNavMesh().GetTile(29);
        data = tile.data;
        header = data.header;
        Assert.That(header.vertCount, Is.EqualTo(15));
        Assert.That(header.polyCount, Is.EqualTo(5));
        Assert.That(header.detailMeshCount, Is.EqualTo(5));
        Assert.That(header.detailVertCount, Is.EqualTo(0));
        Assert.That(header.detailTriCount, Is.EqualTo(11));
        Assert.That(data.polys.Length, Is.EqualTo(5));
        Assert.That(data.verts.Length, Is.EqualTo(3 * 15));
        Assert.That(data.detailMeshes.Length, Is.EqualTo(5));
        Assert.That(data.detailVerts.Length, Is.EqualTo(0));
        Assert.That(data.detailTris.Length, Is.EqualTo(4 * 11));
        Assert.That(data.verts[1], Is.EqualTo(10.330483f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(10.110685f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(10.110685f).Within(0.0001f));
    }

    [Test]
    public void TestDungeon()
    {
        using var ms = new MemoryStream(Loader.ToBytes("dungeon_all_tiles_tilecache.bin"));
        using var @is = new BinaryReader(ms);
        DtTileCache tc = reader.Read(@is, 6, null);
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
        // Verts: 14.997294, 15.484785, 15.484785
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
        // Verts: 13.597294, 17.584785, 17.584785
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
        Assert.That(data.verts[1], Is.EqualTo(13.597294f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(17.584785f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(17.584785f).Within(0.0001f));
        // Tile16: Tris: 10, Verts: 20 Detail Meshed: 10 Detail Verts: 0 Detail Tris: 18
        // Verts: 6.197294, -22.315216, -22.315216
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
        Assert.That(data.verts[1], Is.EqualTo(6.197294f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(-22.315216f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(-22.315216f).Within(0.0001f));
        // Tile29: Tris: 1, Verts: 5 Detail Meshed: 1 Detail Verts: 0 Detail Tris: 3
        // Verts: 10.197294, 48.484783, 48.484783
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
        Assert.That(data.verts[1], Is.EqualTo(10.197294f).Within(0.0001f));
        Assert.That(data.verts[6], Is.EqualTo(48.484783f).Within(0.0001f));
        Assert.That(data.verts[9], Is.EqualTo(48.484783f).Within(0.0001f));
    }
}
