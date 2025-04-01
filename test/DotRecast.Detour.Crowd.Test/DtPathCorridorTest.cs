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
using System.Numerics;
using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Detour.Crowd.Test;

public class DtPathCorridorTest
{
    private readonly DtPathCorridor corridor = new DtPathCorridor();
    private readonly IDtQueryFilter filter = new DtQueryDefaultFilter();

    [SetUp]
    public void SetUp()
    {
        corridor.Init(256);
        corridor.Reset(0, new Vector3(10, 20, 30));
    }

    [Test]
    public void ShouldKeepOriginalPathInFindCornersWhenNothingCanBePruned()
    {
        var straightPath = new DtStraightPath[4];
        straightPath[0] = new DtStraightPath(new Vector3(11, 20, 30.00001f), 0, 0);
        straightPath[1] = new DtStraightPath(new Vector3(12, 20, 30.00002f), 0, 0);
        straightPath[2] = new DtStraightPath(new Vector3(11f, 21, 32f), 0, 0);
        straightPath[3] = new DtStraightPath(new Vector3(11f, 21, 32f), 0, 0);
        var query = new DtNavMeshQueryMock(straightPath, DtStatus.DT_SUCCESS);

        Span<DtStraightPath> path = stackalloc DtStraightPath[8];
        var npath = corridor.FindCorners(path, 8, query, filter);
        Assert.That(npath, Is.EqualTo(4));
        Assert.That(path.Slice(0, npath).ToArray(), Is.EqualTo(straightPath));
    }


    [Test]
    public void ShouldPrunePathInFindCorners()
    {
        DtStraightPath[] straightPath = new DtStraightPath[5];
        straightPath[0] = (new DtStraightPath(new Vector3(10, 20, 30.00001f), 0, 0)); // too close
        straightPath[1] = (new DtStraightPath(new Vector3(10, 20, 30.00002f), 0, 0)); // too close
        straightPath[2] = (new DtStraightPath(new Vector3(11f, 21, 32f), 0, 0));
        straightPath[3] = (new DtStraightPath(new Vector3(12f, 22, 33f), DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION, 0)); // offmesh
        straightPath[4] = (new DtStraightPath(new Vector3(11f, 21, 32f), DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION, 0)); // offmesh

        var query = new DtNavMeshQueryMock(straightPath, DtStatus.DT_SUCCESS);

        Span<DtStraightPath> path = stackalloc DtStraightPath[8];
        int npath = corridor.FindCorners(path, 8, query, filter);
        Assert.That(npath, Is.EqualTo(2));
        Assert.That(path.Slice(0, npath).ToArray(), Is.EqualTo(new DtStraightPath[] { straightPath[2], straightPath[3] }));
    }
}