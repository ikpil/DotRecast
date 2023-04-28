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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotRecast.Core;
using DotRecast.Detour.Extras.Unity.Astar;
using DotRecast.Detour.Io;
using DotRecast.Detour.QueryResults;
using NUnit.Framework;

namespace DotRecast.Detour.Extras.Test.Unity.Astar;

[Parallelizable]
public class UnityAStarPathfindingImporterTest
{
    [Test]
    public void test_v4_0_6()
    {
        NavMesh mesh = loadNavMesh("graph.zip");
        Vector3f startPos = Vector3f.Of(8.200293f, 2.155071f, -26.176147f);
        Vector3f endPos = Vector3f.Of(11.971109f, 0.000000f, 8.663261f);
        Result<List<long>> path = findPath(mesh, startPos, endPos);
        Assert.That(path.status, Is.EqualTo(Status.SUCCSESS));
        Assert.That(path.result.Count, Is.EqualTo(57));
        saveMesh(mesh, "v4_0_6");
    }

    [Test]
    public void test_v4_1_16()
    {
        NavMesh mesh = loadNavMesh("graph_v4_1_16.zip");
        Vector3f startPos = Vector3f.Of(22.93f, -2.37f, -5.11f);
        Vector3f endPos = Vector3f.Of(16.81f, -2.37f, 25.52f);
        Result<List<long>> path = findPath(mesh, startPos, endPos);
        Assert.That(path.status.isSuccess(), Is.True);
        Assert.That(path.result.Count, Is.EqualTo(15));
        saveMesh(mesh, "v4_1_16");
    }

    [Test]
    public void testBoundsTree()
    {
        NavMesh mesh = loadNavMesh("test_boundstree.zip");
        Vector3f position = Vector3f.Of(387.52988f, 19.997f, 368.86282f);

        int[] tilePos = mesh.calcTileLoc(position);
        long tileRef = mesh.getTileRefAt(tilePos[0], tilePos[1], 0);
        MeshTile tile = mesh.getTileByRef(tileRef);
        MeshData data = tile.data;
        BVNode[] bvNodes = data.bvTree;
        data.bvTree = null; // set BV-Tree empty to get 'clear' search poly without BV
        FindNearestPolyResult clearResult = getNearestPolys(mesh, position)[0]; // check poly to exists

        // restore BV-Tree and try search again
        // important aspect in that test: BV result must equals result without BV
        // if poly not found or found other poly - tile bounds is wrong!
        data.bvTree = bvNodes;
        FindNearestPolyResult bvResult = getNearestPolys(mesh, position)[0];

        Assert.That(bvResult.getNearestRef(), Is.EqualTo(clearResult.getNearestRef()));
    }

    private NavMesh loadNavMesh(string filename)
    {
        var filepath = Loader.ToRPath(filename);
        using var fs = new FileStream(filepath, FileMode.Open);

        // Import the graphs
        UnityAStarPathfindingImporter importer = new UnityAStarPathfindingImporter();

        NavMesh[] meshes = importer.load(fs);
        return meshes[0];
    }

    private Result<List<long>> findPath(NavMesh mesh, Vector3f startPos, Vector3f endPos)
    {
        // Perform a simple pathfinding
        NavMeshQuery query = new NavMeshQuery(mesh);
        QueryFilter filter = new DefaultQueryFilter();

        FindNearestPolyResult[] polys = getNearestPolys(mesh, startPos, endPos);
        return query.findPath(polys[0].getNearestRef(), polys[1].getNearestRef(), startPos, endPos, filter);
    }

    private FindNearestPolyResult[] getNearestPolys(NavMesh mesh, params Vector3f[] positions)
    {
        NavMeshQuery query = new NavMeshQuery(mesh);
        QueryFilter filter = new DefaultQueryFilter();
        Vector3f extents = Vector3f.Of(0.1f, 0.1f, 0.1f);

        FindNearestPolyResult[] results = new FindNearestPolyResult[positions.Length];
        for (int i = 0; i < results.Length; i++)
        {
            Vector3f position = positions[i];
            Result<FindNearestPolyResult> result = query.findNearestPoly(position, extents, filter);
            Assert.That(result.Succeeded(), Is.True);
            Assert.That(result.result.getNearestPos(), Is.Not.EqualTo(Vector3f.Zero), "Nearest start position is null!");
            results[i] = result.result;
        }

        return results;
    }

    private void saveMesh(NavMesh mesh, string filePostfix)
    {
        // Set the flag to RecastDemo work properly
        for (int i = 0; i < mesh.getTileCount(); i++)
        {
            foreach (Poly p in mesh.getTile(i).data.polys)
            {
                p.flags = 1;
            }
        }

        // Save the mesh as recast file,
        MeshSetWriter writer = new MeshSetWriter();
        string filename = $"all_tiles_navmesh_{filePostfix}.bin";
        string filepath = Path.Combine("test-output", filename);
        using var fs = new FileStream(filename, FileMode.Create);
        using var os = new BinaryWriter(fs);
        writer.write(os, mesh, ByteOrder.LITTLE_ENDIAN, true);
    }
}