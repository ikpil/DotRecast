/*
recast4j Copyright (c) 2015-2021 Piotr Piastucki piotr@jtilia.org

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
using System.Diagnostics;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;
using NUnit.Framework;
using static DotRecast.Core.RcMath;

namespace DotRecast.Detour.Test;

[Parallelizable]
public class RandomPointTest : AbstractDetourTest
{
    [Test]
    public void TestRandom()
    {
        FRand f = new FRand(1);
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        for (int i = 0; i < 1000; i++)
        {
            Result<FindRandomPointResult> point = query.FindRandomPoint(filter, f);
            Assert.That(point.Succeeded(), Is.True);
            Tuple<DtMeshTile, DtPoly> tileAndPoly = navmesh.GetTileAndPolyByRef(point.result.GetRandomRef()).result;
            float[] bmin = new float[2];
            float[] bmax = new float[2];
            for (int j = 0; j < tileAndPoly.Item2.vertCount; j++)
            {
                int v = tileAndPoly.Item2.verts[j] * 3;
                bmin[0] = j == 0 ? tileAndPoly.Item1.data.verts[v] : Math.Min(bmin[0], tileAndPoly.Item1.data.verts[v]);
                bmax[0] = j == 0 ? tileAndPoly.Item1.data.verts[v] : Math.Max(bmax[0], tileAndPoly.Item1.data.verts[v]);
                bmin[1] = j == 0 ? tileAndPoly.Item1.data.verts[v + 2] : Math.Min(bmin[1], tileAndPoly.Item1.data.verts[v + 2]);
                bmax[1] = j == 0 ? tileAndPoly.Item1.data.verts[v + 2] : Math.Max(bmax[1], tileAndPoly.Item1.data.verts[v + 2]);
            }

            Assert.That(point.result.GetRandomPt().x >= bmin[0], Is.True);
            Assert.That(point.result.GetRandomPt().x <= bmax[0], Is.True);
            Assert.That(point.result.GetRandomPt().z >= bmin[1], Is.True);
            Assert.That(point.result.GetRandomPt().z <= bmax[1], Is.True);
        }
    }

    [Test]
    public void TestRandomAroundCircle()
    {
        FRand f = new FRand(1);
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        FindRandomPointResult point = query.FindRandomPoint(filter, f).result;
        for (int i = 0; i < 1000; i++)
        {
            Result<FindRandomPointResult> result = query.FindRandomPointAroundCircle(point.GetRandomRef(), point.GetRandomPt(),
                5f, filter, f);
            Assert.That(result.Failed(), Is.False);
            point = result.result;
            Tuple<DtMeshTile, DtPoly> tileAndPoly = navmesh.GetTileAndPolyByRef(point.GetRandomRef()).result;
            float[] bmin = new float[2];
            float[] bmax = new float[2];
            for (int j = 0; j < tileAndPoly.Item2.vertCount; j++)
            {
                int v = tileAndPoly.Item2.verts[j] * 3;
                bmin[0] = j == 0 ? tileAndPoly.Item1.data.verts[v] : Math.Min(bmin[0], tileAndPoly.Item1.data.verts[v]);
                bmax[0] = j == 0 ? tileAndPoly.Item1.data.verts[v] : Math.Max(bmax[0], tileAndPoly.Item1.data.verts[v]);
                bmin[1] = j == 0 ? tileAndPoly.Item1.data.verts[v + 2] : Math.Min(bmin[1], tileAndPoly.Item1.data.verts[v + 2]);
                bmax[1] = j == 0 ? tileAndPoly.Item1.data.verts[v + 2] : Math.Max(bmax[1], tileAndPoly.Item1.data.verts[v + 2]);
            }

            Assert.That(point.GetRandomPt().x >= bmin[0], Is.True);
            Assert.That(point.GetRandomPt().x <= bmax[0], Is.True);
            Assert.That(point.GetRandomPt().z >= bmin[1], Is.True);
            Assert.That(point.GetRandomPt().z <= bmax[1], Is.True);
        }
    }

    [Test]
    public void TestRandomWithinCircle()
    {
        FRand f = new FRand(1);
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        FindRandomPointResult point = query.FindRandomPoint(filter, f).result;
        float radius = 5f;
        for (int i = 0; i < 1000; i++)
        {
            Result<FindRandomPointResult> result = query.FindRandomPointWithinCircle(point.GetRandomRef(), point.GetRandomPt(),
                radius, filter, f);
            Assert.That(result.Failed(), Is.False);
            float distance = RcVec3f.Dist2D(point.GetRandomPt(), result.result.GetRandomPt());
            Assert.That(distance <= radius, Is.True);
            point = result.result;
        }
    }

    [Test]
    public void TestPerformance()
    {
        FRand f = new FRand(1);
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        FindRandomPointResult point = query.FindRandomPoint(filter, f).result;
        float radius = 5f;
        // jvm warmup
        for (int i = 0; i < 1000; i++)
        {
            query.FindRandomPointAroundCircle(point.GetRandomRef(), point.GetRandomPt(), radius, filter, f);
        }

        for (int i = 0; i < 1000; i++)
        {
            query.FindRandomPointWithinCircle(point.GetRandomRef(), point.GetRandomPt(), radius, filter, f);
        }

        long t1 = RcFrequency.Ticks;
        for (int i = 0; i < 10000; i++)
        {
            query.FindRandomPointAroundCircle(point.GetRandomRef(), point.GetRandomPt(), radius, filter, f);
        }

        long t2 = RcFrequency.Ticks;
        for (int i = 0; i < 10000; i++)
        {
            query.FindRandomPointWithinCircle(point.GetRandomRef(), point.GetRandomPt(), radius, filter, f);
        }

        long t3 = RcFrequency.Ticks;
        Console.WriteLine("Random point around circle: " + (t2 - t1) / TimeSpan.TicksPerMillisecond + "ms");
        Console.WriteLine("Random point within circle: " + (t3 - t2) / TimeSpan.TicksPerMillisecond + "ms");
    }
}
