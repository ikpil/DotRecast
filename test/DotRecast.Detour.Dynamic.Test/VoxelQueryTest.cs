/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotRecast.Core;
using System.Numerics;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Detour.Dynamic.Test.Io;
using DotRecast.Recast;
using Moq;
using NUnit.Framework;

namespace DotRecast.Detour.Dynamic.Test;

public class VoxelQueryTest
{
    private const int TILE_WIDTH = 100;
    private const int TILE_DEPTH = 90;
    private static readonly Vector3 ORIGIN = new Vector3(50, 10, 40);


    [Test]
    public void ShouldTraverseTiles()
    {
        var hfProvider = new Mock<Func<int, int, RcHeightfield>>();

        // Given
        List<int> captorX = new();
        List<int> captorZ = new();

        hfProvider
            .Setup(e => e.Invoke(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((RcHeightfield)null)
            .Callback<int, int>((x, z) =>
            {
                captorX.Add(x);
                captorZ.Add(z);
            });

        DtVoxelQuery query = new DtVoxelQuery(ORIGIN, TILE_WIDTH, TILE_DEPTH, hfProvider.Object);
        Vector3 start = new Vector3(120, 10, 365);
        Vector3 end = new Vector3(320, 10, 57);

        // When
        query.Raycast(start, end, out var hit);
        // Then
        hfProvider.Verify(mock => mock.Invoke(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(6));
        Assert.That(captorX, Is.EqualTo(new[] { 0, 1, 1, 1, 2, 2 }));
        Assert.That(captorZ, Is.EqualTo(new[] { 3, 3, 2, 1, 1, 0 }));
    }

    [Test]
    public void ShouldHandleRaycastWithoutObstacles()
    {
        DtDynamicNavMesh mesh = CreateDynaMesh();
        DtVoxelQuery query = mesh.VoxelQuery();
        Vector3 start = new Vector3(7.4f, 0.5f, -64.8f);
        Vector3 end = new Vector3(31.2f, 0.5f, -75.3f);
        bool isHit = query.Raycast(start, end, out var hit);
        Assert.That(isHit, Is.EqualTo(false));
    }

    [Test]
    public void ShouldHandleRaycastWithObstacles()
    {
        DtDynamicNavMesh mesh = CreateDynaMesh();
        DtVoxelQuery query = mesh.VoxelQuery();
        Vector3 start = new Vector3(32.3f, 0.5f, 47.9f);
        Vector3 end = new Vector3(-31.2f, 0.5f, -29.8f);
        bool isHit = query.Raycast(start, end, out var hit);
        Assert.That(isHit, Is.EqualTo(true));
        Assert.That(hit, Is.EqualTo(0.5263836f).Within(1e-7f));
    }

    private DtDynamicNavMesh CreateDynaMesh()
    {
        var bytes = RcIO.ReadFileIfFound("test_tiles.voxels");
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        // load voxels from file
        DtVoxelFileReader reader = new DtVoxelFileReader(DtVoxelTileLZ4ForTestCompressor.Shared);
        DtVoxelFile f = reader.Read(br);
        
        // create dynamic navmesh
        var mesh = new DtDynamicNavMesh(f);
        
        // build navmesh asynchronously using multiple threads
        mesh.Build(Task.Factory);
        return mesh;
    }
}