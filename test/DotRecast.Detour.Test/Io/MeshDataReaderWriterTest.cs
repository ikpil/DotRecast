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

using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;
using NUnit.Framework;

namespace DotRecast.Detour.Test.Io;

[Parallelizable]
public class MeshDataReaderWriterTest
{
    private const int VERTS_PER_POLYGON = 6;
    private DtMeshData meshData;

    [SetUp]
    public void SetUp()
    {
        RecastTestMeshBuilder rcBuilder = new RecastTestMeshBuilder();
        meshData = rcBuilder.GetMeshData();
    }

    [Test]
    public void TestCCompatibility()
    {
        Test(true, RcByteOrder.BIG_ENDIAN);
    }

    [Test]
    public void TestCompact()
    {
        Test(false, RcByteOrder.BIG_ENDIAN);
    }

    [Test]
    public void TestCCompatibilityLE()
    {
        Test(true, RcByteOrder.LITTLE_ENDIAN);
    }

    [Test]
    public void TestCompactLE()
    {
        Test(false, RcByteOrder.LITTLE_ENDIAN);
    }

    public void Test(bool cCompatibility, RcByteOrder order)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        DtMeshDataWriter writer = new DtMeshDataWriter();
        writer.Write(bw, meshData, order, cCompatibility);
        ms.Seek(0, SeekOrigin.Begin);

        using var br = new BinaryReader(ms);
        DtMeshDataReader reader = new DtMeshDataReader();
        DtMeshData readData = reader.Read(br, VERTS_PER_POLYGON);

        Assert.That(readData.header.vertCount, Is.EqualTo(meshData.header.vertCount));
        Assert.That(readData.header.polyCount, Is.EqualTo(meshData.header.polyCount));
        Assert.That(readData.header.detailMeshCount, Is.EqualTo(meshData.header.detailMeshCount));
        Assert.That(readData.header.detailTriCount, Is.EqualTo(meshData.header.detailTriCount));
        Assert.That(readData.header.detailVertCount, Is.EqualTo(meshData.header.detailVertCount));
        Assert.That(readData.header.bvNodeCount, Is.EqualTo(meshData.header.bvNodeCount));
        Assert.That(readData.header.offMeshConCount, Is.EqualTo(meshData.header.offMeshConCount));
        for (int i = 0; i < meshData.header.vertCount; i++)
        {
            Assert.That(readData.verts[i], Is.EqualTo(meshData.verts[i]));
        }

        for (int i = 0; i < meshData.header.polyCount; i++)
        {
            Assert.That(readData.polys[i].vertCount, Is.EqualTo(meshData.polys[i].vertCount));
            Assert.That(readData.polys[i].areaAndtype, Is.EqualTo(meshData.polys[i].areaAndtype));
            for (int j = 0; j < meshData.polys[i].vertCount; j++)
            {
                Assert.That(readData.polys[i].verts[j], Is.EqualTo(meshData.polys[i].verts[j]));
                Assert.That(readData.polys[i].neis[j], Is.EqualTo(meshData.polys[i].neis[j]));
            }
        }

        for (int i = 0; i < meshData.header.detailMeshCount; i++)
        {
            Assert.That(readData.detailMeshes[i].vertBase, Is.EqualTo(meshData.detailMeshes[i].vertBase));
            Assert.That(readData.detailMeshes[i].vertCount, Is.EqualTo(meshData.detailMeshes[i].vertCount));
            Assert.That(readData.detailMeshes[i].triBase, Is.EqualTo(meshData.detailMeshes[i].triBase));
            Assert.That(readData.detailMeshes[i].triCount, Is.EqualTo(meshData.detailMeshes[i].triCount));
        }

        for (int i = 0; i < meshData.header.detailVertCount; i++)
        {
            Assert.That(readData.detailVerts[i], Is.EqualTo(meshData.detailVerts[i]));
        }

        for (int i = 0; i < meshData.header.detailTriCount; i++)
        {
            Assert.That(readData.detailTris[i], Is.EqualTo(meshData.detailTris[i]));
        }

        for (int i = 0; i < meshData.header.bvNodeCount; i++)
        {
            Assert.That(readData.bvTree[i].i, Is.EqualTo(meshData.bvTree[i].i));
            for (int j = 0; j < 3; j++)
            {
                Assert.That(readData.bvTree[i].bmin[j], Is.EqualTo(meshData.bvTree[i].bmin[j]));
                Assert.That(readData.bvTree[i].bmax[j], Is.EqualTo(meshData.bvTree[i].bmax[j]));
            }
        }

        for (int i = 0; i < meshData.header.offMeshConCount; i++)
        {
            Assert.That(readData.offMeshCons[i].flags, Is.EqualTo(meshData.offMeshCons[i].flags));
            Assert.That(readData.offMeshCons[i].rad, Is.EqualTo(meshData.offMeshCons[i].rad));
            Assert.That(readData.offMeshCons[i].poly, Is.EqualTo(meshData.offMeshCons[i].poly));
            Assert.That(readData.offMeshCons[i].side, Is.EqualTo(meshData.offMeshCons[i].side));
            Assert.That(readData.offMeshCons[i].userId, Is.EqualTo(meshData.offMeshCons[i].userId));
            for (int j = 0; j < 6; j++)
            {
                Assert.That(readData.offMeshCons[i].pos[j], Is.EqualTo(meshData.offMeshCons[i].pos[j]));
            }
        }
    }
}