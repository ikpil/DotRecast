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
using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;
using NUnit.Framework;

namespace DotRecast.Detour.Test.Io;

[Parallelizable]
public class MeshSetReaderTest
{
    private readonly MeshSetReader reader = new MeshSetReader();

    [Test]
    public void testNavmesh()
    {
        byte[] @is = Loader.ToBytes("all_tiles_navmesh.bin");
        using var ms = new MemoryStream(@is);
        using var bris = new BinaryReader(ms);
        NavMesh mesh = reader.read(bris, 6);
        Assert.That(mesh.getMaxTiles(), Is.EqualTo(128));
        Assert.That(mesh.getParams().maxPolys, Is.EqualTo(0x8000));
        Assert.That(mesh.getParams().tileWidth, Is.EqualTo(9.6f).Within(0.001f));
        List<MeshTile> tiles = mesh.getTilesAt(4, 7);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(7));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(22 * 3));
        tiles = mesh.getTilesAt(1, 6);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(7));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(26 * 3));
        tiles = mesh.getTilesAt(6, 2);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(1));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(4 * 3));
        tiles = mesh.getTilesAt(7, 6);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(8));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(24 * 3));
    }

    [Test]
    public void testDungeon()
    {
        byte[] @is = Loader.ToBytes("dungeon_all_tiles_navmesh.bin");
        using var ms = new MemoryStream(@is);
        using var bris = new BinaryReader(ms);

        NavMesh mesh = reader.read(bris, 6);
        Assert.That(mesh.getMaxTiles(), Is.EqualTo(128));
        Assert.That(mesh.getParams().maxPolys, Is.EqualTo(0x8000));
        Assert.That(mesh.getParams().tileWidth, Is.EqualTo(9.6f).Within(0.001f));
        List<MeshTile> tiles = mesh.getTilesAt(6, 9);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(2));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(7 * 3));
        tiles = mesh.getTilesAt(2, 9);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(2));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(9 * 3));
        tiles = mesh.getTilesAt(4, 3);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(3));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(6 * 3));
        tiles = mesh.getTilesAt(2, 8);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(5));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(17 * 3));
    }

    [Test]
    public void testDungeon32Bit()
    {
        byte[] @is = Loader.ToBytes("dungeon_all_tiles_navmesh_32bit.bin");
        using var ms = new MemoryStream(@is);
        using var bris = new BinaryReader(ms);

        NavMesh mesh = reader.read32Bit(bris, 6);
        Assert.That(mesh.getMaxTiles(), Is.EqualTo(128));
        Assert.That(mesh.getParams().maxPolys, Is.EqualTo(0x8000));
        Assert.That(mesh.getParams().tileWidth, Is.EqualTo(9.6f).Within(0.001f));
        List<MeshTile> tiles = mesh.getTilesAt(6, 9);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(2));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(7 * 3));
        tiles = mesh.getTilesAt(2, 9);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(2));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(9 * 3));
        tiles = mesh.getTilesAt(4, 3);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(3));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(6 * 3));
        tiles = mesh.getTilesAt(2, 8);
        Assert.That(tiles.Count, Is.EqualTo(1));
        Assert.That(tiles[0].data.polys.Length, Is.EqualTo(5));
        Assert.That(tiles[0].data.verts.Length, Is.EqualTo(17 * 3));
    }
}