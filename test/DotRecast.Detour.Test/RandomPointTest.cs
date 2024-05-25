/*
recast4j Copyright (c) 2015-2021 Piotr Piastucki piotr@jtilia.org
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
using DotRecast.Core;
using DotRecast.Core.Numerics;
using NUnit.Framework;


namespace DotRecast.Detour.Test;

public class RandomPointTest : AbstractDetourTest
{
    [Test]
    [Repeat(10)]
    public void TestRandom()
    {
        RcRand f = new RcRand(1);
        IDtQueryFilter filter = new DtQueryDefaultFilter();

        var begin = RcFrequency.Ticks;
        for (int i = 0; i < 10000; i++)
        {
            var status = query.FindRandomPoint(filter, f, out var randomRef, out var randomPt);
            Assert.That(status.Succeeded(), Is.True);

            status = navmesh.GetTileAndPolyByRef(randomRef, out var tile, out var poly);
            float[] bmin = new float[2];
            float[] bmax = new float[2];
            for (int j = 0; j < poly.vertCount; j++)
            {
                int v = poly.verts[j] * 3;
                bmin[0] = j == 0 ? tile.data.verts[v] : Math.Min(bmin[0], tile.data.verts[v]);
                bmax[0] = j == 0 ? tile.data.verts[v] : Math.Max(bmax[0], tile.data.verts[v]);
                bmin[1] = j == 0 ? tile.data.verts[v + 2] : Math.Min(bmin[1], tile.data.verts[v + 2]);
                bmax[1] = j == 0 ? tile.data.verts[v + 2] : Math.Max(bmax[1], tile.data.verts[v + 2]);
            }

            Assert.That(randomPt.X >= bmin[0], Is.True);
            Assert.That(randomPt.X <= bmax[0], Is.True);
            Assert.That(randomPt.Z >= bmin[1], Is.True);
            Assert.That(randomPt.Z <= bmax[1], Is.True);
        }

        var ticks = RcFrequency.Ticks - begin;
        Console.WriteLine($"RandomPointTest::TestRandom() - {(double)ticks / TimeSpan.TicksPerMillisecond} ms");
    }

    [Test]
    public void TestRandomAroundCircle()
    {
        RcRand f = new RcRand(1);
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        query.FindRandomPoint(filter, f, out var randomRef, out var randomPt);
        for (int i = 0; i < 1000; i++)
        {
            var status = query.FindRandomPointAroundCircle(randomRef, randomPt, 5f, filter, f, out var nextRandomRef, out var nextRandomPt);
            Assert.That(status.Failed(), Is.False);

            randomRef = nextRandomRef;
            randomPt = nextRandomPt;

            status = navmesh.GetTileAndPolyByRef(randomRef, out var tile, out var poly);

            float[] bmin = new float[2];
            float[] bmax = new float[2];
            for (int j = 0; j < poly.vertCount; j++)
            {
                int v = poly.verts[j] * 3;
                bmin[0] = j == 0 ? tile.data.verts[v] : Math.Min(bmin[0], tile.data.verts[v]);
                bmax[0] = j == 0 ? tile.data.verts[v] : Math.Max(bmax[0], tile.data.verts[v]);
                bmin[1] = j == 0 ? tile.data.verts[v + 2] : Math.Min(bmin[1], tile.data.verts[v + 2]);
                bmax[1] = j == 0 ? tile.data.verts[v + 2] : Math.Max(bmax[1], tile.data.verts[v + 2]);
            }

            Assert.That(randomPt.X >= bmin[0], Is.True);
            Assert.That(randomPt.X <= bmax[0], Is.True);
            Assert.That(randomPt.Z >= bmin[1], Is.True);
            Assert.That(randomPt.Z <= bmax[1], Is.True);
        }
    }

    [Test]
    public void TestRandomWithinCircle()
    {
        RcRand f = new RcRand(1);
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        query.FindRandomPoint(filter, f, out var randomRef, out var randomPt);
        float radius = 5f;
        for (int i = 0; i < 1000; i++)
        {
            var status = query.FindRandomPointWithinCircle(randomRef, randomPt, radius, filter, f, out var nextRandomRef, out var nextRandomPt);
            Assert.That(status.Failed(), Is.False);

            float distance = RcVecUtils.Dist2D(randomPt, nextRandomPt);
            Assert.That(distance <= radius, Is.True);

            randomRef = nextRandomRef;
            randomPt = nextRandomPt;
        }
    }

    [Test]
    public void TestPerformance()
    {
        RcRand f = new RcRand(1);
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        query.FindRandomPoint(filter, f, out var randomRef, out var randomPt);

        float radius = 5f;
        // jvm warmup
        for (int i = 0; i < 1000; i++)
        {
            query.FindRandomPointAroundCircle(randomRef, randomPt, radius, filter, f, out var _, out var _);
        }

        for (int i = 0; i < 1000; i++)
        {
            query.FindRandomPointWithinCircle(randomRef, randomPt, radius, filter, f, out var _, out var _);
        }

        long t1 = RcFrequency.Ticks;
        for (int i = 0; i < 10000; i++)
        {
            query.FindRandomPointAroundCircle(randomRef, randomPt, radius, filter, f, out var _, out var _);
        }

        long t2 = RcFrequency.Ticks;
        for (int i = 0; i < 10000; i++)
        {
            query.FindRandomPointWithinCircle(randomRef, randomPt, radius, filter, f, out var _, out var _);
        }

        long t3 = RcFrequency.Ticks;
        Console.WriteLine("Random point around circle: " + (t2 - t1) / TimeSpan.TicksPerMillisecond + "ms");
        Console.WriteLine("Random point within circle: " + (t3 - t2) / TimeSpan.TicksPerMillisecond + "ms");
    }
}