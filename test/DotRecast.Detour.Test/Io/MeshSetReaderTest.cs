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

using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;
using NUnit.Framework;

namespace DotRecast.Detour.Test.Io;

public class MeshSetReaderTest
{
    private readonly DtMeshSetReader reader = new DtMeshSetReader();

    [Test]
    public void TestNavmesh()
    {
        byte[] @is = RcIO.ReadFileIfFound("all_tiles_navmesh.bin");
        using var ms = new MemoryStream(@is);
        using var br = new BinaryReader(ms);
        DtNavMesh mesh = reader.Read(br, 6);
        Assert.That(mesh.GetMaxTiles(), Is.EqualTo(128));
        Assert.That(mesh.GetParams().maxPolys, Is.EqualTo(0x8000));
        Assert.That(mesh.GetParams().tileWidth, Is.EqualTo(9.6f).Within(0.001f));

        const int MAX_NEIS = 32;
        DtMeshTile[] tiles = new DtMeshTile[MAX_NEIS];
        int nneis = 0;

        nneis = mesh.GetTilesAt(4, 7, tiles, MAX_NEIS);
        Assert.That(nneis, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(7));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(22 * 3));

        nneis = mesh.GetTilesAt(1, 6, tiles, MAX_NEIS);
        Assert.That(nneis, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(7));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(26 * 3));

        nneis = mesh.GetTilesAt(6, 2, tiles, MAX_NEIS);
        Assert.That(nneis, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(1));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(4 * 3));

        nneis = mesh.GetTilesAt(7, 6, tiles, MAX_NEIS);
        Assert.That(nneis, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(8));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(24 * 3));
    }

    [Test]
    public void TestDungeon()
    {
        byte[] @is = RcIO.ReadFileIfFound("dungeon_all_tiles_navmesh.bin");
        using var ms = new MemoryStream(@is);
        using var br = new BinaryReader(ms);

        DtNavMesh mesh = reader.Read(br, 6);
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

    [Test]
    public void TestDungeon32Bit()
    {
        byte[] @is = RcIO.ReadFileIfFound("dungeon_all_tiles_navmesh_32bit.bin");
        using var ms = new MemoryStream(@is);
        using var br = new BinaryReader(ms);

        DtNavMesh mesh = reader.Read32Bit(br, 6);
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