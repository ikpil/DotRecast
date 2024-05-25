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

using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

using static DtDetour;

public class NavMeshBuilderTest
{
    private DtMeshData nmd;

    [SetUp]
    public void SetUp()
    {
        nmd = RecastTestMeshBuilder.Create();
    }

    [Test]
    public void TestBVTree()
    {
        Assert.That(nmd.verts.Length / 3, Is.EqualTo(225));
        Assert.That(nmd.polys.Length, Is.EqualTo(119));
        Assert.That(nmd.header.maxLinkCount, Is.EqualTo(457));
        Assert.That(nmd.detailMeshes.Length, Is.EqualTo(118));
        Assert.That(nmd.detailTris.Length / 4, Is.EqualTo(291));
        Assert.That(nmd.detailVerts.Length / 3, Is.EqualTo(60));
        Assert.That(nmd.offMeshCons.Length, Is.EqualTo(1));
        Assert.That(nmd.header.offMeshBase, Is.EqualTo(118));
        Assert.That(nmd.bvTree.Length, Is.EqualTo(236));
        Assert.That(nmd.bvTree.Length, Is.GreaterThanOrEqualTo(nmd.header.bvNodeCount));
        for (int i = 0; i < nmd.header.bvNodeCount; i++)
        {
            Assert.That(nmd.bvTree[i], Is.Not.Null);
        }

        for (int i = 0; i < 2; i++)
        {
            Assert.That(RcVecUtils.Create(nmd.verts, 223 * 3 + (i * 3)), Is.EqualTo(nmd.offMeshCons[0].pos[i]));
        }

        Assert.That(nmd.offMeshCons[0].rad, Is.EqualTo(0.1f));
        Assert.That(nmd.offMeshCons[0].poly, Is.EqualTo(118));
        Assert.That(nmd.offMeshCons[0].flags, Is.EqualTo(DT_OFFMESH_CON_BIDIR));
        Assert.That(nmd.offMeshCons[0].side, Is.EqualTo(0xFF));
        Assert.That(nmd.offMeshCons[0].userId, Is.EqualTo(0x4567));
        Assert.That(nmd.polys[118].vertCount, Is.EqualTo(2));
        Assert.That(nmd.polys[118].verts[0], Is.EqualTo(223));
        Assert.That(nmd.polys[118].verts[1], Is.EqualTo(224));
        Assert.That(nmd.polys[118].flags, Is.EqualTo(12));
        Assert.That(nmd.polys[118].GetArea(), Is.EqualTo(2));
        Assert.That(nmd.polys[118].GetPolyType(), Is.EqualTo(DtPolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION));
    }
}