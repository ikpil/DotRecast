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

using DotRecast.Core;
using DotRecast.Detour.QueryResults;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

[Parallelizable]
public class FindNearestPolyTest : AbstractDetourTest
{
    private static readonly long[] POLY_REFS = { 281474976710696L, 281474976710773L, 281474976710680L, 281474976710753L, 281474976710733L };

    private static readonly float[][] POLY_POS =
    {
        new[] { 22.606520f, 10.197294f, -45.918674f }, new[] { 22.331268f, 10.197294f, -1.040187f },
        new[] { 18.694363f, 15.803535f, -73.090416f }, new[] { 0.745335f, 10.197294f, -5.940050f },
        new[] { -20.651257f, 5.904126f, -13.712508f }
    };

    [Test]
    public void TestFindNearestPoly()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        RcVec3f extents = RcVec3f.Of(2, 4, 2);
        for (int i = 0; i < startRefs.Length; i++)
        {
            RcVec3f startPos = startPoss[i];
            var status = query.FindNearestPoly(startPos, extents, filter, out var nearestRef, out var nearestPt, out var _);
            Assert.That(status.Succeeded(), Is.True);
            Assert.That(nearestRef, Is.EqualTo(POLY_REFS[i]));
            for (int v = 0; v < POLY_POS[i].Length; v++)
            {
                Assert.That(nearestPt[v], Is.EqualTo(POLY_POS[i][v]).Within(0.001f));
            }
        }
    }


    [Test]
    public void ShouldReturnStartPosWhenNoPolyIsValid()
    {
        var filter = new DtQueryEmptyFilter();
        RcVec3f extents = RcVec3f.Of(2, 4, 2);
        for (int i = 0; i < startRefs.Length; i++)
        {
            RcVec3f startPos = startPoss[i];
            var status = query.FindNearestPoly(startPos, extents, filter, out var nearestRef, out var nearestPt, out var _);
            Assert.That(status.Succeeded(), Is.True);
            Assert.That(nearestRef, Is.EqualTo(0L));
            for (int v = 0; v < POLY_POS[i].Length; v++)
            {
                Assert.That(nearestPt[v], Is.EqualTo(startPos[v]).Within(0.001f));
            }
        }
    }
}