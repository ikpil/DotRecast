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
using System.IO;
using DotRecast.Core;
using DotRecast.Detour.TileCache.Io;
using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Detour.TileCache.Test.Io;
using NUnit.Framework;

namespace DotRecast.Detour.TileCache.Test;

[Parallelizable]
public class TileCacheFindPathTest : AbstractTileCacheTest
{
    private readonly RcVec3f start = new RcVec3f(39.44734f, 9.998177f, -0.784811f);
    private readonly RcVec3f end = new RcVec3f(19.292645f, 11.611748f, -57.750366f);
    private readonly DtNavMesh navmesh;
    private readonly DtNavMeshQuery query;

    public TileCacheFindPathTest()
    {
        using var msr = new MemoryStream(RcResources.Load("dungeon_all_tiles_tilecache.bin"));
        using var br = new BinaryReader(msr);
        DtTileCache tcC = new DtTileCacheReader(DtTileCacheCompressorFactory.Shared).Read(br, 6, new TestTileCacheMeshProcess());
        navmesh = tcC.GetNavMesh();
        query = new DtNavMeshQuery(navmesh);
    }

    [Test]
    public void TestFindPath()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        RcVec3f extents = new RcVec3f(2f, 4f, 2f);
        query.FindNearestPoly(start, extents, filter, out var startRef, out var startPos, out var _);
        query.FindNearestPoly(end, extents, filter, out var endRef, out var endPos, out var _);

        var path = new List<long>();
        var status = query.FindPath(startRef, endRef, startPos, endPos, filter, ref path, DtFindPathOption.NoOption);
        int maxStraightPath = 256;
        int options = 0;

        var pathStr = new List<DtStraightPath>();
        query.FindStraightPath(startPos, endPos, path, ref pathStr, maxStraightPath, options);
        Assert.That(pathStr.Count, Is.EqualTo(8));
    }
}