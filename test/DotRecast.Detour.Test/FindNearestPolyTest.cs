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

using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class FindNearestPolyTest : AbstractDetourTest
{
    private static readonly long[] POLY_REFS =
    {
        281474976710696L, 281474976710773L, 281474976710680L, 281474976710753L, 281474976710733L
    };

    private static readonly RcVec3f[] POLY_POS =
    {
        new RcVec3f(22.606520f, 10.197294f, -45.918674f),
        new RcVec3f(22.331268f, 10.197294f, -1.040187f),
        new RcVec3f(18.694363f, 15.803535f, -73.090416f),
        new RcVec3f(0.745335f, 10.197294f, -5.940050f),
        new RcVec3f(-20.651257f, 5.904126f, -13.712508f)
    };

    [Test]
    public void TestFindNearestPoly()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        RcVec3f extents = new RcVec3f(2, 4, 2);
        for (int i = 0; i < startRefs.Length; i++)
        {
            RcVec3f startPos = startPoss[i];
            var status = query.FindNearestPoly(startPos, extents, filter, out var nearestRef, out var nearestPt, out var _);
            Assert.That(status.Succeeded(), Is.True, $"index({i})");
            Assert.That(nearestRef, Is.EqualTo(POLY_REFS[i]), $"index({i})");
            Assert.That(nearestPt.X, Is.EqualTo(POLY_POS[i].X).Within(0.001f), $"index({i})");
            Assert.That(nearestPt.Y, Is.EqualTo(POLY_POS[i].Y).Within(0.001f), $"index({i})");
            Assert.That(nearestPt.Z, Is.EqualTo(POLY_POS[i].Z).Within(0.001f), $"index({i})");
        }
    }


    [Test]
    public void ShouldReturnStartPosWhenNoPolyIsValid()
    {
        RcVec3f extents = new RcVec3f(2, 4, 2);
        for (int i = 0; i < startRefs.Length; i++)
        {
            RcVec3f startPos = startPoss[i];
            var status = query.FindNearestPoly(startPos, extents, DtQueryEmptyFilter.Shared, out var nearestRef, out var nearestPt, out var _);
            Assert.That(status.Succeeded(), Is.True);
            Assert.That(nearestRef, Is.EqualTo(0L));
            Assert.That(nearestPt.X, Is.EqualTo(startPos.X).Within(0.001f));
            Assert.That(nearestPt.Y, Is.EqualTo(startPos.Y).Within(0.001f));
            Assert.That(nearestPt.Z, Is.EqualTo(startPos.Z).Within(0.001f));
        }
    }
}