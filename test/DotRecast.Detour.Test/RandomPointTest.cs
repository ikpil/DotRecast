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
using static DotRecast.Core.RecastMath;

namespace DotRecast.Detour.Test;

[Parallelizable]
public class RandomPointTest : AbstractDetourTest
{
    [Test]
    public void testRandom()
    {
        NavMeshQuery.FRand f = new NavMeshQuery.FRand(1);
        QueryFilter filter = new DefaultQueryFilter();
        for (int i = 0; i < 1000; i++)
        {
            Result<FindRandomPointResult> point = query.findRandomPoint(filter, f);
            Assert.That(point.succeeded(), Is.True);
            Tuple<MeshTile, Poly> tileAndPoly = navmesh.getTileAndPolyByRef(point.result.getRandomRef()).result;
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

            Assert.That(point.result.getRandomPt()[0] >= bmin[0], Is.True);
            Assert.That(point.result.getRandomPt()[0] <= bmax[0], Is.True);
            Assert.That(point.result.getRandomPt()[2] >= bmin[1], Is.True);
            Assert.That(point.result.getRandomPt()[2] <= bmax[1], Is.True);
        }
    }

    [Test]
    public void testRandomAroundCircle()
    {
        NavMeshQuery.FRand f = new NavMeshQuery.FRand(1);
        QueryFilter filter = new DefaultQueryFilter();
        FindRandomPointResult point = query.findRandomPoint(filter, f).result;
        for (int i = 0; i < 1000; i++)
        {
            Result<FindRandomPointResult> result = query.findRandomPointAroundCircle(point.getRandomRef(), point.getRandomPt(),
                5f, filter, f);
            Assert.That(result.failed(), Is.False);
            point = result.result;
            Tuple<MeshTile, Poly> tileAndPoly = navmesh.getTileAndPolyByRef(point.getRandomRef()).result;
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

            Assert.That(point.getRandomPt()[0] >= bmin[0], Is.True);
            Assert.That(point.getRandomPt()[0] <= bmax[0], Is.True);
            Assert.That(point.getRandomPt()[2] >= bmin[1], Is.True);
            Assert.That(point.getRandomPt()[2] <= bmax[1], Is.True);
        }
    }

    [Test]
    public void testRandomWithinCircle()
    {
        NavMeshQuery.FRand f = new NavMeshQuery.FRand(1);
        QueryFilter filter = new DefaultQueryFilter();
        FindRandomPointResult point = query.findRandomPoint(filter, f).result;
        float radius = 5f;
        for (int i = 0; i < 1000; i++)
        {
            Result<FindRandomPointResult> result = query.findRandomPointWithinCircle(point.getRandomRef(), point.getRandomPt(),
                radius, filter, f);
            Assert.That(result.failed(), Is.False);
            float distance = vDist2D(point.getRandomPt(), result.result.getRandomPt());
            Assert.That(distance <= radius, Is.True);
            point = result.result;
        }
    }

    [Test]
    public void testPerformance()
    {
        NavMeshQuery.FRand f = new NavMeshQuery.FRand(1);
        QueryFilter filter = new DefaultQueryFilter();
        FindRandomPointResult point = query.findRandomPoint(filter, f).result;
        float radius = 5f;
        // jvm warmup
        for (int i = 0; i < 1000; i++)
        {
            query.findRandomPointAroundCircle(point.getRandomRef(), point.getRandomPt(), radius, filter, f);
        }

        for (int i = 0; i < 1000; i++)
        {
            query.findRandomPointWithinCircle(point.getRandomRef(), point.getRandomPt(), radius, filter, f);
        }

        long t1 = FrequencyWatch.Ticks;
        for (int i = 0; i < 10000; i++)
        {
            query.findRandomPointAroundCircle(point.getRandomRef(), point.getRandomPt(), radius, filter, f);
        }

        long t2 = FrequencyWatch.Ticks;
        for (int i = 0; i < 10000; i++)
        {
            query.findRandomPointWithinCircle(point.getRandomRef(), point.getRandomPt(), radius, filter, f);
        }

        long t3 = FrequencyWatch.Ticks;
        Console.WriteLine("Random point around circle: " + (t2 - t1) / TimeSpan.TicksPerMillisecond + "ms");
        Console.WriteLine("Random point within circle: " + (t3 - t2) / TimeSpan.TicksPerMillisecond + "ms");
    }
}