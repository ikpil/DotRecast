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

using System;
using System.Collections.Generic;
using DotRecast.Core.Numerics;
using Moq;
using NUnit.Framework;

namespace DotRecast.Detour.Crowd.Test;

public class DtPathCorridorTest
{
    private DtPathCorridor corridor;
    private IDtQueryFilter filter;

    [SetUp]
    public void SetUp()
    {
        corridor = new DtPathCorridor();
        corridor.Init(DtCrowdConst.MAX_PATH_RESULT);
        corridor.Reset(0, new RcVec3f(10, 20, 30));

        filter = new DtQueryDefaultFilter();
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldHandleEmptyInput()
    {
        var path = new List<long>();
        const int npath = 0;
        const int maxPath = 0;
        Span<long> visited = stackalloc long[0];
        const int nvisited = 0;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldHandleEmptyVisited()
    {
        var path = new List<long> { 1 };
        const int npath = 1;
        const int maxPath = 1;
        Span<long> visited = stackalloc long[0];
        const int nvisited = 0;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(1));

        var expectedPath = new List<long> { 1 };
        Assert.That(path, Is.EqualTo(expectedPath));
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldHandleEmptyPath()
    {
        var path = new List<long>();
        const int npath = 0;
        const int maxPath = 0;
        Span<long> visited = stackalloc long[] { 1 };
        const int nvisited = 1;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldStripVisitedPointsFromPathExceptLast()
    {
        var path = new List<long> { 1, 2 };
        const int npath = 2;
        const int maxPath = 2;
        Span<long> visited = stackalloc long[] { 1, 2 };
        const int nvisited = 2;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(1));

        var expectedPath = new List<long> { 2, 2 };
        Assert.That(path, Is.EqualTo(expectedPath));
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldAddVisitedPointsNotPresentInPathInReverseOrder()
    {
        var path = new List<long> { 1, 2, 0 };
        const int npath = 2;
        const int maxPath = 3;
        Span<long> visited = stackalloc long[] { 1, 2, 3, 4 };
        const int nvisited = 3;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(3));

        var expectedPath = new List<long> { 4, 3, 2 };
        Assert.That(path, Is.EqualTo(expectedPath));
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldAddVisitedPointsNotPresentInPathUpToThePathCapacity()
    {
        var path = new List<long>() { 1, 2, 0 };
        const int npath = 2;
        const int maxPath = 3;
        Span<long> visited = stackalloc long[] { 1, 2, 3, 4, 5 };
        const int nvisited = 5;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(3));

        var expectedPath = new List<long> { 5, 4, 3 };
        Assert.That(path, Is.EqualTo(expectedPath));
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldNotChangePathIfThereIsNoIntersectionWithVisited()
    {
        var path = new List<long>() { 1, 2 };
        const int npath = 2;
        const int maxPath = 2;
        Span<long> visited = stackalloc long[] { 3, 4 };
        const int nvisited = 2;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(2));

        var expectedPath = new List<long> { 1, 2 };
        Assert.That(path, Is.EqualTo(expectedPath));
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldSaveUnvisitedPathPoints()
    {
        var path = new List<long>() { 1, 2, 0 };
        const int npath = 2;
        const int maxPath = 3;
        Span<long> visited = stackalloc long[] { 1, 3 };
        const int nvisited = 2;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(3));
        var expectedPath = new List<long> { 3, 1, 2 };
        Assert.That(path, Is.EqualTo(expectedPath));
    }

    [Test(Description = "dtMergeCorridorStartMoved")]
    public void ShouldSaveUnvisitedPathPointsUpToThePathCapacity()
    {
        var path = new List<long>() { 1, 2 };
        const int npath = 2;
        const int maxPath = 2;
        Span<long> visited = stackalloc long[] { 1, 3 };
        const int nvisited = 2;
        var result = DtPathUtils.MergeCorridorStartMoved(ref path, npath, maxPath, visited, nvisited);
        Assert.That(result, Is.EqualTo(2));

        var expectedPath = new List<long> { 3, 1 };
        Assert.That(path, Is.EqualTo(expectedPath));
    }

    [Test]
    public void ShouldKeepOriginalPathInFindCornersWhenNothingCanBePruned()
    {
        List<DtStraightPath> straightPath = new();
        straightPath.Add(new DtStraightPath(new RcVec3f(11, 20, 30.00001f), 0, 0));
        straightPath.Add(new DtStraightPath(new RcVec3f(12, 20, 30.00002f), 0, 0));
        straightPath.Add(new DtStraightPath(new RcVec3f(11f, 21, 32f), 0, 0));
        straightPath.Add(new DtStraightPath(new RcVec3f(11f, 21, 32f), 0, 0));
        var mockQuery = new Mock<DtNavMeshQuery>(It.IsAny<DtNavMesh>());
        mockQuery.Setup(q => q.FindStraightPath(
                It.IsAny<RcVec3f>(),
                It.IsAny<RcVec3f>(),
                It.IsAny<List<long>>(),
                It.IsAny<int>(),
                ref It.Ref<List<DtStraightPath>>.IsAny,
                It.IsAny<int>(),
                It.IsAny<int>())
            )
            .Callback((RcVec3f startPos, RcVec3f endPos, List<long> path, int npath,
                ref List<DtStraightPath> refStraightPath, int maxStraightPath, int options) =>
            {
                refStraightPath = straightPath;
            })
            .Returns(() => DtStatus.DT_SUCCESS);

        var path = new List<DtStraightPath>();
        corridor.FindCorners(ref path, int.MaxValue, mockQuery.Object, filter);
        Assert.That(path.Count, Is.EqualTo(4));
        Assert.That(path, Is.EqualTo(straightPath));
    }

    [Test]
    public void ShouldPrunePathInFindCorners()
    {
        List<DtStraightPath> straightPath = new();
        straightPath.Add(new DtStraightPath(new RcVec3f(10, 20, 30.00001f), 0, 0)); // too close
        straightPath.Add(new DtStraightPath(new RcVec3f(10, 20, 30.00002f), 0, 0)); // too close
        straightPath.Add(new DtStraightPath(new RcVec3f(11f, 21, 32f), 0, 0));
        straightPath.Add(new DtStraightPath(new RcVec3f(12f, 22, 33f), DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION, 0)); // offmesh
        straightPath.Add(new DtStraightPath(new RcVec3f(11f, 21, 32f), DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION, 0)); // offmesh

        var mockQuery = new Mock<DtNavMeshQuery>(It.IsAny<DtNavMesh>());
        mockQuery.Setup(q => q.FindStraightPath(
                It.IsAny<RcVec3f>(),
                It.IsAny<RcVec3f>(),
                It.IsAny<List<long>>(),
                It.IsAny<int>(),
                ref It.Ref<List<DtStraightPath>>.IsAny,
                It.IsAny<int>(),
                It.IsAny<int>())
            ).Callback((RcVec3f startPos, RcVec3f endPos, List<long> path, int npath,
                ref List<DtStraightPath> refStraightPath, int maxStraightPath, int options) =>
            {
                refStraightPath = straightPath;
            })
            .Returns(() => DtStatus.DT_SUCCESS);

        var path = new List<DtStraightPath>();
        corridor.FindCorners(ref path, int.MaxValue, mockQuery.Object, filter);
        Assert.That(path.Count, Is.EqualTo(2));
        Assert.That(path, Is.EqualTo(new List<DtStraightPath> { straightPath[2], straightPath[3] }));
    }
}