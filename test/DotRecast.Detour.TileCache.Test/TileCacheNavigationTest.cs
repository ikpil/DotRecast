/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
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

using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using NUnit.Framework;

namespace DotRecast.Detour.TileCache.Test;

[Parallelizable]
public class TileCacheNavigationTest : AbstractTileCacheTest
{
    protected readonly long[] startRefs = { 281475006070787L };
    protected readonly long[] endRefs = { 281474986147841L };
    protected readonly Vector3f[] startPoss = { Vector3f.Of(39.447338f, 9.998177f, -0.784811f) };
    protected readonly Vector3f[] endPoss = { Vector3f.Of(19.292645f, 11.611748f, -57.750366f) };
    private readonly Status[] statuses = { Status.SUCCSESS };

    private readonly long[][] results =
    {
        new[]
        {
            281475006070787L, 281475006070785L, 281475005022208L, 281475005022209L, 281475003973633L,
            281475003973634L, 281475003973632L, 281474996633604L, 281474996633605L, 281474996633603L, 281474995585027L,
            281474995585029L, 281474995585026L, 281474995585028L, 281474995585024L, 281474991390721L, 281474991390722L,
            281474991390725L, 281474991390720L, 281474987196418L, 281474987196417L, 281474988244995L, 281474988245001L,
            281474988244997L, 281474988244998L, 281474988245002L, 281474988245000L, 281474988244999L, 281474988244994L,
            281474985099264L, 281474985099266L, 281474986147841L
        }
    };

    protected NavMesh navmesh;
    protected NavMeshQuery query;

    [SetUp]
    public void SetUp()
    {
        bool cCompatibility = true;
        IInputGeomProvider geom = ObjImporter.Load(Loader.ToBytes("dungeon.obj"));
        TestTileLayerBuilder layerBuilder = new TestTileLayerBuilder(geom);
        List<byte[]> layers = layerBuilder.Build(RcByteOrder.LITTLE_ENDIAN, cCompatibility, 1);
        TileCache tc = GetTileCache(geom, RcByteOrder.LITTLE_ENDIAN, cCompatibility);
        foreach (byte[] data in layers)
        {
            tc.AddTile(data, 0);
        }

        for (int y = 0; y < layerBuilder.GetTh(); ++y)
        {
            for (int x = 0; x < layerBuilder.GetTw(); ++x)
            {
                foreach (long refs in tc.GetTilesAt(x, y))
                {
                    tc.BuildNavMeshTile(refs);
                }
            }
        }

        navmesh = tc.GetNavMesh();
        query = new NavMeshQuery(navmesh);
    }

    [Test]
    public void TestFindPathWithDefaultHeuristic()
    {
        IQueryFilter filter = new DefaultQueryFilter();
        for (int i = 0; i < startRefs.Length; i++)
        {
            long startRef = startRefs[i];
            long endRef = endRefs[i];
            Vector3f startPos = startPoss[i];
            Vector3f endPos = endPoss[i];
            Result<List<long>> path = query.FindPath(startRef, endRef, startPos, endPos, filter);
            Assert.That(path.status, Is.EqualTo(statuses[i]));
            Assert.That(path.result.Count, Is.EqualTo(results[i].Length));
            for (int j = 0; j < results[i].Length; j++)
            {
                Assert.That(path.result[j], Is.EqualTo(results[i][j])); // TODO : 확인 필요
            }
        }
    }

    [Test]
    public void TestFindPathWithNoHeuristic()
    {
        IQueryFilter filter = new DefaultQueryFilter();
        for (int i = 0; i < startRefs.Length; i++)
        {
            long startRef = startRefs[i];
            long endRef = endRefs[i];
            Vector3f startPos = startPoss[i];
            Vector3f endPos = endPoss[i];
            Result<List<long>> path = query.FindPath(startRef, endRef, startPos, endPos, filter, new DefaultQueryHeuristic(0.0f),
                0, 0);
            Assert.That(path.status, Is.EqualTo(statuses[i]));
            Assert.That(path.result.Count, Is.EqualTo(results[i].Length));
            for (int j = 0; j < results[i].Length; j++)
            {
                Assert.That(path.result[j], Is.EqualTo(results[i][j])); // TODO : 확인 필요
            }
        }
    }
}