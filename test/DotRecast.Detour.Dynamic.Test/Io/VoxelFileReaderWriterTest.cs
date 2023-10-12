/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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

using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Dynamic.Io;
using NUnit.Framework;

namespace DotRecast.Detour.Dynamic.Test.Io;

[Parallelizable]
public class VoxelFileReaderWriterTest
{
    [TestCase(false)]
    [TestCase(true)]
    public void ShouldReadSingleTileFile(bool compression)
    {
        byte[] bytes = RcResources.Load("test.voxels");
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        DtVoxelFile f = ReadWriteRead(br, compression);
        Assert.That(f.useTiles, Is.False);
        Assert.That(f.bounds, Is.EqualTo(new[] { -100.0f, 0f, -100f, 100f, 5f, 100f }));
        Assert.That(f.cellSize, Is.EqualTo(0.25f));
        Assert.That(f.walkableRadius, Is.EqualTo(0.5f));
        Assert.That(f.walkableHeight, Is.EqualTo(2f));
        Assert.That(f.walkableClimb, Is.EqualTo(0.5f));
        Assert.That(f.maxEdgeLen, Is.EqualTo(20f));
        Assert.That(f.maxSimplificationError, Is.EqualTo(2f));
        Assert.That(f.minRegionArea, Is.EqualTo(2f));
        Assert.That(f.regionMergeArea, Is.EqualTo(12f));
        Assert.That(f.tiles.Count, Is.EqualTo(1));
        Assert.That(f.tiles[0].cellHeight, Is.EqualTo(0.001f));
        Assert.That(f.tiles[0].width, Is.EqualTo(810));
        Assert.That(f.tiles[0].depth, Is.EqualTo(810));
        Assert.That(f.tiles[0].spanData.Length, Is.EqualTo(9021024));
        Assert.That(f.tiles[0].boundsMin, Is.EqualTo(new RcVec3f(-101.25f, 0f, -101.25f)));
        Assert.That(f.tiles[0].boundsMax, Is.EqualTo(new RcVec3f(101.25f, 5.0f, 101.25f)));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ShouldReadMultiTileFile(bool compression)
    {
        byte[] bytes = RcResources.Load("test_tiles.voxels");
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        DtVoxelFile f = ReadWriteRead(br, compression);

        Assert.That(f.useTiles, Is.True);
        Assert.That(f.bounds, Is.EqualTo(new[] { -100.0f, 0f, -100f, 100f, 5f, 100f }));
        Assert.That(f.cellSize, Is.EqualTo(0.25f));
        Assert.That(f.walkableRadius, Is.EqualTo(0.5f));
        Assert.That(f.walkableHeight, Is.EqualTo(2f));
        Assert.That(f.walkableClimb, Is.EqualTo(0.5f));
        Assert.That(f.maxEdgeLen, Is.EqualTo(20f));
        Assert.That(f.maxSimplificationError, Is.EqualTo(2f));
        Assert.That(f.minRegionArea, Is.EqualTo(2f));
        Assert.That(f.regionMergeArea, Is.EqualTo(12f));
        Assert.That(f.tiles.Count, Is.EqualTo(100));
        Assert.That(f.tiles[0].cellHeight, Is.EqualTo(0.001f));
        Assert.That(f.tiles[0].width, Is.EqualTo(90));
        Assert.That(f.tiles[0].depth, Is.EqualTo(90));
        Assert.That(f.tiles[0].spanData.Length, Is.EqualTo(104952));
        Assert.That(f.tiles[5].spanData.Length, Is.EqualTo(109080));
        Assert.That(f.tiles[18].spanData.Length, Is.EqualTo(113400));
        Assert.That(f.tiles[0].boundsMin, Is.EqualTo(new RcVec3f(-101.25f, 0f, -101.25f)));
        Assert.That(f.tiles[0].boundsMax, Is.EqualTo(new RcVec3f(-78.75f, 5.0f, -78.75f)));
    }

    private DtVoxelFile ReadWriteRead(BinaryReader bis, bool compression)
    {
        DtVoxelFileReader reader = new DtVoxelFileReader(DtVoxelTileLZ4ForTestCompressor.Shared);
        DtVoxelFile f = reader.Read(bis);

        using var msw = new MemoryStream();
        using var bw = new BinaryWriter(msw);
        DtVoxelFileWriter writer = new DtVoxelFileWriter(DtVoxelTileLZ4ForTestCompressor.Shared);
        writer.Write(bw, f, compression);

        using var msr = new MemoryStream(msw.ToArray());
        using var br = new BinaryReader(msr);
        return reader.Read(br);
    }
}