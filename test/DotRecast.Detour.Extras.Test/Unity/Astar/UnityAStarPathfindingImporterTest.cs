/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

using System.IO;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour.Extras.Unity.Astar;
using DotRecast.Detour.Io;
using NUnit.Framework;

namespace DotRecast.Detour.Extras.Test.Unity.Astar;

[Parallelizable]
public class UnityAStarPathfindingImporterTest
{
    [Test]
    public void Test_v4_0_6()
    {
        DtNavMesh mesh = LoadNavMesh("graph.zip");
        RcVec3f startPos = new RcVec3f(8.200293f, 2.155071f, -26.176147f);
        RcVec3f endPos = new RcVec3f(11.971109f, 0.000000f, 8.663261f);
        var path = new List<long>();
        var status = FindPath(mesh, startPos, endPos, ref path);
        Assert.That(status, Is.EqualTo(DtStatus.DT_SUCCESS));
        Assert.That(path.Count, Is.EqualTo(57));
        SaveMesh(mesh, "v4_0_6");
    }

    [Test]
    public void Test_v4_1_16()
    {
        DtNavMesh mesh = LoadNavMesh("graph_v4_1_16.zip");
        RcVec3f startPos = new RcVec3f(22.93f, -2.37f, -5.11f);
        RcVec3f endPos = new RcVec3f(16.81f, -2.37f, 25.52f);
        var path = new List<long>();
        var status = FindPath(mesh, startPos, endPos, ref path);
        Assert.That(status.Succeeded(), Is.True);
        Assert.That(path.Count, Is.EqualTo(15));
        SaveMesh(mesh, "v4_1_16");
    }

    [Test]
    public void TestBoundsTree()
    {
        DtNavMesh mesh = LoadNavMesh("test_boundstree.zip");
        RcVec3f position = new RcVec3f(387.52988f, 19.997f, 368.86282f);

        mesh.CalcTileLoc(position, out var tileX, out var tileY);
        long tileRef = mesh.GetTileRefAt(tileX, tileY, 0);
        DtMeshTile tile = mesh.GetTileByRef(tileRef);
        DtMeshData data = tile.data;
        DtBVNode[] bvNodes = data.bvTree;
        data.bvTree = null; // set BV-Tree empty to get 'clear' search poly without BV
        var clearResult = GetNearestPolys(mesh, position)[0]; // check poly to exists

        // restore BV-Tree and try search again
        // important aspect in that test: BV result must equals result without BV
        // if poly not found or found other poly - tile bounds is wrong!
        data.bvTree = bvNodes;
        var bvResult = GetNearestPolys(mesh, position)[0];

        Assert.That(bvResult.refs, Is.EqualTo(clearResult.refs));
    }

    private DtNavMesh LoadNavMesh(string filename)
    {
        var filepath = RcDirectory.SearchFile($"resources/{filename}");
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);

        // Import the graphs
        UnityAStarPathfindingImporter importer = new UnityAStarPathfindingImporter();

        DtNavMesh[] meshes = importer.Load(fs);
        return meshes[0];
    }

    private DtStatus FindPath(DtNavMesh mesh, RcVec3f startPos, RcVec3f endPos, ref List<long> path)
    {
        // Perform a simple pathfinding
        DtNavMeshQuery query = new DtNavMeshQuery(mesh);
        IDtQueryFilter filter = new DtQueryDefaultFilter();

        var polys = GetNearestPolys(mesh, startPos, endPos);
        return query.FindPath(polys[0].refs, polys[1].refs, startPos, endPos, filter, ref path, DtFindPathOption.NoOption);
    }

    private DtPolyPoint[] GetNearestPolys(DtNavMesh mesh, params RcVec3f[] positions)
    {
        DtNavMeshQuery query = new DtNavMeshQuery(mesh);
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        RcVec3f extents = new RcVec3f(0.1f, 0.1f, 0.1f);

        var results = new DtPolyPoint[positions.Length];
        for (int i = 0; i < results.Length; i++)
        {
            RcVec3f position = positions[i];
            var status = query.FindNearestPoly(position, extents, filter, out var nearestRef, out var nearestPt, out var _);
            Assert.That(status.Succeeded(), Is.True);
            Assert.That(nearestPt, Is.Not.EqualTo(RcVec3f.Zero), "Nearest start position is null!");

            results[i] = new DtPolyPoint(nearestRef, nearestPt);
        }

        return results;
    }

    private void SaveMesh(DtNavMesh mesh, string filePostfix)
    {
        // Set the flag to RecastDemo work properly
        for (int i = 0; i < mesh.GetTileCount(); i++)
        {
            foreach (DtPoly p in mesh.GetTile(i).data.polys)
            {
                p.flags = 1;
            }
        }

        // Save the mesh as recast file,
        DtMeshSetWriter writer = new DtMeshSetWriter();
        string filename = $"all_tiles_navmesh_{filePostfix}.bin";
        string filepath = Path.Combine("test-output", filename);
        using var fs = new FileStream(filename, FileMode.Create);
        using var bw = new BinaryWriter(fs);
        writer.Write(bw, mesh, RcByteOrder.LITTLE_ENDIAN, true);
    }
}