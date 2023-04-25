/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

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
using DotRecast.Detour.Dynamic.Io;
using NUnit.Framework;

namespace DotRecast.Detour.Dynamic.Test.Io;

[Parallelizable]
public class VoxelFileReaderTest
{
    [Test]
    public void shouldReadSingleTileFile()
    {
        byte[] bytes = Loader.ToBytes("test.voxels");
        using var ms = new MemoryStream(bytes);
        using var bis = new BinaryReader(ms);

        VoxelFileReader reader = new VoxelFileReader();
        VoxelFile f = reader.read(bis);
        Assert.That(f.useTiles, Is.False);
        Assert.That(f.bounds, Is.EqualTo(new float[] { -100.0f, 0f, -100f, 100f, 5f, 100f }));
        Assert.That(f.cellSize, Is.EqualTo(0.25f));
        Assert.That(f.walkableRadius, Is.EqualTo(0.5f));
        Assert.That(f.walkableHeight, Is.EqualTo(2f));
        Assert.That(f.walkableClimb, Is.EqualTo(0.5f));
        Assert.That(f.maxEdgeLen, Is.EqualTo(20f));
        Assert.That(f.maxSimplificationError, Is.EqualTo(2f));
        Assert.That(f.minRegionArea, Is.EqualTo(2f));
        Assert.That(f.tiles.Count, Is.EqualTo(1));
        Assert.That(f.tiles[0].cellHeight, Is.EqualTo(0.001f));
        Assert.That(f.tiles[0].width, Is.EqualTo(810));
        Assert.That(f.tiles[0].depth, Is.EqualTo(810));
        Assert.That(f.tiles[0].boundsMin, Is.EqualTo(Vector3f.Of(-101.25f, 0f, -101.25f)));
        Assert.That(f.tiles[0].boundsMax, Is.EqualTo(Vector3f.Of(101.25f, 5.0f, 101.25f)));
    }

    [Test]
    public void shouldReadMultiTileFile()
    {
        byte[] bytes = Loader.ToBytes("test_tiles.voxels");
        using var ms = new MemoryStream(bytes);
        using var bis = new BinaryReader(ms);

        VoxelFileReader reader = new VoxelFileReader();
        VoxelFile f = reader.read(bis);

        Assert.That(f.useTiles, Is.True);
        Assert.That(f.bounds, Is.EqualTo(new float[] { -100.0f, 0f, -100f, 100f, 5f, 100f }));
        Assert.That(f.cellSize, Is.EqualTo(0.25f));
        Assert.That(f.walkableRadius, Is.EqualTo(0.5f));
        Assert.That(f.walkableHeight, Is.EqualTo(2f));
        Assert.That(f.walkableClimb, Is.EqualTo(0.5f));
        Assert.That(f.maxEdgeLen, Is.EqualTo(20f));
        Assert.That(f.maxSimplificationError, Is.EqualTo(2f));
        Assert.That(f.minRegionArea, Is.EqualTo(2f));
        Assert.That(f.tiles.Count, Is.EqualTo(100));
        Assert.That(f.tiles[0].cellHeight, Is.EqualTo(0.001f));
        Assert.That(f.tiles[0].width, Is.EqualTo(90));
        Assert.That(f.tiles[0].depth, Is.EqualTo(90));
        Assert.That(f.tiles[0].boundsMin, Is.EqualTo(Vector3f.Of(-101.25f, 0f, -101.25f)));
        Assert.That(f.tiles[0].boundsMax, Is.EqualTo(Vector3f.Of(-78.75f, 5.0f, -78.75f)));
    }
}