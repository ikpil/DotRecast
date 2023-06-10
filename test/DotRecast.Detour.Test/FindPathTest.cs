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
using DotRecast.Core;
using DotRecast.Detour.QueryResults;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

[Parallelizable]
public class FindPathTest : AbstractDetourTest
{
    private static readonly DtStatus[] STATUSES =
    {
        DtStatus.DT_SUCCSESS, DtStatus.DT_PARTIAL_RESULT, DtStatus.DT_SUCCSESS, DtStatus.DT_SUCCSESS,
        DtStatus.DT_SUCCSESS
    };

    private static readonly long[][] RESULTS =
    {
        new[]
        {
            281474976710696L, 281474976710695L, 281474976710694L, 281474976710703L, 281474976710706L,
            281474976710705L, 281474976710702L, 281474976710701L, 281474976710714L, 281474976710713L,
            281474976710712L, 281474976710727L, 281474976710730L, 281474976710717L, 281474976710721L
        },
        new[]
        {
            281474976710773L, 281474976710772L, 281474976710768L, 281474976710754L, 281474976710755L,
            281474976710753L, 281474976710748L, 281474976710752L, 281474976710731L, 281474976710729L,
            281474976710717L, 281474976710724L, 281474976710728L, 281474976710737L, 281474976710738L,
            281474976710736L, 281474976710733L, 281474976710735L, 281474976710742L, 281474976710740L,
            281474976710746L, 281474976710745L, 281474976710744L
        },
        new[]
        {
            281474976710680L, 281474976710684L, 281474976710688L, 281474976710687L, 281474976710686L,
            281474976710697L, 281474976710695L, 281474976710694L, 281474976710703L, 281474976710706L,
            281474976710705L, 281474976710702L, 281474976710701L, 281474976710714L, 281474976710713L,
            281474976710712L, 281474976710727L, 281474976710730L, 281474976710717L, 281474976710729L,
            281474976710731L, 281474976710752L, 281474976710748L, 281474976710753L, 281474976710755L,
            281474976710754L, 281474976710768L, 281474976710772L, 281474976710773L, 281474976710770L,
            281474976710757L, 281474976710761L, 281474976710758L
        },
        new[] { 281474976710753L, 281474976710748L, 281474976710752L, 281474976710731L },
        new[]
        {
            281474976710733L, 281474976710736L, 281474976710738L, 281474976710737L, 281474976710728L,
            281474976710724L, 281474976710717L, 281474976710729L, 281474976710731L, 281474976710752L,
            281474976710748L, 281474976710753L, 281474976710755L, 281474976710754L, 281474976710768L,
            281474976710772L
        }
    };

    private static readonly StraightPathItem[][] STRAIGHT_PATHS =
    {
        new[]
        {
            new StraightPathItem(RcVec3f.Of(22.606520f, 10.197294f, -45.918674f), 1, 281474976710696L),
            new StraightPathItem(RcVec3f.Of(3.484785f, 10.197294f, -34.241272f), 0, 281474976710713L),
            new StraightPathItem(RcVec3f.Of(1.984785f, 10.197294f, -31.241272f), 0, 281474976710712L),
            new StraightPathItem(RcVec3f.Of(1.984785f, 10.197294f, -29.741272f), 0, 281474976710727L),
            new StraightPathItem(RcVec3f.Of(2.584784f, 10.197294f, -27.941273f), 0, 281474976710730L),
            new StraightPathItem(RcVec3f.Of(6.457663f, 10.197294f, -18.334061f), 2, 0L)
        },

        new[]
        {
            new StraightPathItem(RcVec3f.Of(22.331268f, 10.197294f, -1.040187f), 1, 281474976710773L),
            new StraightPathItem(RcVec3f.Of(9.784786f, 10.197294f, -2.141273f), 0, 281474976710755L),
            new StraightPathItem(RcVec3f.Of(7.984783f, 10.197294f, -2.441269f), 0, 281474976710753L),
            new StraightPathItem(RcVec3f.Of(1.984785f, 10.197294f, -8.441269f), 0, 281474976710752L),
            new StraightPathItem(RcVec3f.Of(-4.315216f, 10.197294f, -15.341270f), 0, 281474976710724L),
            new StraightPathItem(RcVec3f.Of(-8.215216f, 10.197294f, -17.441269f), 0, 281474976710728L),
            new StraightPathItem(RcVec3f.Of(-10.015216f, 10.197294f, -17.741272f), 0, 281474976710738L),
            new StraightPathItem(RcVec3f.Of(-11.815216f, 9.997294f, -17.441269f), 0, 281474976710736L),
            new StraightPathItem(RcVec3f.Of(-17.815216f, 5.197294f, -11.441269f), 0, 281474976710735L),
            new StraightPathItem(RcVec3f.Of(-17.815216f, 5.197294f, -8.441269f), 0, 281474976710746L),
            new StraightPathItem(RcVec3f.Of(-11.815216f, 0.197294f, 3.008419f), 2, 0L)
        },

        new[]
        {
            new StraightPathItem(RcVec3f.Of(18.694363f, 15.803535f, -73.090416f), 1, 281474976710680L),
            new StraightPathItem(RcVec3f.Of(17.584785f, 10.197294f, -49.841274f), 0, 281474976710697L),
            new StraightPathItem(RcVec3f.Of(17.284786f, 10.197294f, -48.041275f), 0, 281474976710695L),
            new StraightPathItem(RcVec3f.Of(16.084785f, 10.197294f, -45.341274f), 0, 281474976710694L),
            new StraightPathItem(RcVec3f.Of(3.484785f, 10.197294f, -34.241272f), 0, 281474976710713L),
            new StraightPathItem(RcVec3f.Of(1.984785f, 10.197294f, -31.241272f), 0, 281474976710712L),
            new StraightPathItem(RcVec3f.Of(1.984785f, 10.197294f, -8.441269f), 0, 281474976710753L),
            new StraightPathItem(RcVec3f.Of(7.984783f, 10.197294f, -2.441269f), 0, 281474976710755L),
            new StraightPathItem(RcVec3f.Of(9.784786f, 10.197294f, -2.141273f), 0, 281474976710768L),
            new StraightPathItem(RcVec3f.Of(38.423977f, 10.197294f, -0.116067f), 2, 0L)
        },

        new[]
        {
            new StraightPathItem(RcVec3f.Of(0.745335f, 10.197294f, -5.940050f), 1, 281474976710753L),
            new StraightPathItem(RcVec3f.Of(0.863553f, 10.197294f, -10.310320f), 2, 0L)
        },

        new[]
        {
            new StraightPathItem(RcVec3f.Of(-20.651257f, 5.904126f, -13.712508f), 1, 281474976710733L),
            new StraightPathItem(RcVec3f.Of(-11.815216f, 9.997294f, -17.441269f), 0, 281474976710738L),
            new StraightPathItem(RcVec3f.Of(-10.015216f, 10.197294f, -17.741272f), 0, 281474976710728L),
            new StraightPathItem(RcVec3f.Of(-8.215216f, 10.197294f, -17.441269f), 0, 281474976710724L),
            new StraightPathItem(RcVec3f.Of(-4.315216f, 10.197294f, -15.341270f), 0, 281474976710729L),
            new StraightPathItem(RcVec3f.Of(1.984785f, 10.197294f, -8.441269f), 0, 281474976710753L),
            new StraightPathItem(RcVec3f.Of(7.984783f, 10.197294f, -2.441269f), 0, 281474976710755L),
            new StraightPathItem(RcVec3f.Of(18.784092f, 10.197294f, 3.054368f), 2, 0L)
        }
    };

    [Test]
    public void TestFindPath()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        for (int i = 0; i < startRefs.Length; i++)
        {
            long startRef = startRefs[i];
            long endRef = endRefs[i];
            RcVec3f startPos = startPoss[i];
            RcVec3f endPos = endPoss[i];
            Result<List<long>> path = query.FindPath(startRef, endRef, startPos, endPos, filter);
            Assert.That(path.status, Is.EqualTo(STATUSES[i]));
            Assert.That(path.result.Count, Is.EqualTo(RESULTS[i].Length));
            for (int j = 0; j < RESULTS[i].Length; j++)
            {
                Assert.That(path.result[j], Is.EqualTo(RESULTS[i][j]));
            }
        }
    }

    [Test]
    public void TestFindPathSliced()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        for (int i = 0; i < startRefs.Length; i++)
        {
            long startRef = startRefs[i];
            long endRef = endRefs[i];
            var startPos = startPoss[i];
            var endPos = endPoss[i];
            query.InitSlicedFindPath(startRef, endRef, startPos, endPos, filter, DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE);
            DtStatus status = DtStatus.DT_IN_PROGRESS;
            while (status == DtStatus.DT_IN_PROGRESS)
            {
                Result<int> res = query.UpdateSlicedFindPath(10);
                status = res.status;
            }

            Result<List<long>> path = query.FinalizeSlicedFindPath();
            Assert.That(path.status, Is.EqualTo(STATUSES[i]));
            Assert.That(path.result.Count, Is.EqualTo(RESULTS[i].Length));
            for (int j = 0; j < RESULTS[i].Length; j++)
            {
                Assert.That(path.result[j], Is.EqualTo(RESULTS[i][j]));
            }
        }
    }

    [Test]
    public void TestFindPathStraight()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        for (int i = 0; i < STRAIGHT_PATHS.Length; i++)
        {
            // startRefs.Length; i++) {
            long startRef = startRefs[i];
            long endRef = endRefs[i];
            var startPos = startPoss[i];
            var endPos = endPoss[i];
            Result<List<long>> path = query.FindPath(startRef, endRef, startPos, endPos, filter);
            Result<List<StraightPathItem>> result = query.FindStraightPath(startPos, endPos, path.result,
                int.MaxValue, 0);
            List<StraightPathItem> straightPath = result.result;
            Assert.That(straightPath.Count, Is.EqualTo(STRAIGHT_PATHS[i].Length));
            for (int j = 0; j < STRAIGHT_PATHS[i].Length; j++)
            {
                Assert.That(straightPath[j].refs, Is.EqualTo(STRAIGHT_PATHS[i][j].refs));
                for (int v = 0; v < 3; v++)
                {
                    Assert.That(straightPath[j].pos[v], Is.EqualTo(STRAIGHT_PATHS[i][j].pos[v]).Within(0.01f));
                }

                Assert.That(straightPath[j].flags, Is.EqualTo(STRAIGHT_PATHS[i][j].flags));
            }
        }
    }
}