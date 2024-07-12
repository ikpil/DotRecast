using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotRecast.Core;
using System.Numerics;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Detour.Dynamic.Test.Io;
using NUnit.Framework;

namespace DotRecast.Detour.Dynamic.Test;

public class DynamicNavMeshTest
{
    private static readonly Vector3 START_POS = new Vector3(70.87453f, 0.0010070801f, 86.69021f);
    private static readonly Vector3 END_POS = new Vector3(-50.22061f, 0.0010070801f, -70.761444f);
    private static readonly Vector3 EXTENT = new Vector3(0.1f, 0.1f, 0.1f);
    private static readonly Vector3 SPHERE_POS = new Vector3(45.381645f, 0.0010070801f, 52.68981f);


    [Test]
    public void E2eTest()
    {
        byte[] bytes = RcIO.ReadFileIfFound("test_tiles.voxels");
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        // load voxels from file
        DtVoxelFileReader reader = new DtVoxelFileReader(DtVoxelTileLZ4ForTestCompressor.Shared);
        DtVoxelFile f = reader.Read(br);
        // create dynamic navmesh
        DtDynamicNavMesh mesh = new DtDynamicNavMesh(f);
        // build navmesh asynchronously using multiple threads
        mesh.Build(Task.Factory);

        // create new query
        DtNavMeshQuery query = new DtNavMeshQuery(mesh.NavMesh(), 512);
        IDtQueryFilter filter = new DtQueryDefaultFilter();

        // find path
        query.FindNearestPoly(START_POS, EXTENT, filter, out var startRef, out var startPt, out var _);
        query.FindNearestPoly(END_POS, EXTENT, filter, out var endRef, out var endPt, out var _);

        var path = new long[32];
        query.FindPath(startRef, endRef, startPt, endPt, filter, path, out var pathCount, DtFindPathOption.AnyAngle);
        // check path length without any obstacles
        Assert.That(pathCount, Is.EqualTo(16));

        // place obstacle
        IDtCollider colldier = new DtSphereCollider(SPHERE_POS, 20, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND, 0.1f);
        long colliderId = mesh.AddCollider(colldier);

        // update navmesh asynchronously
        mesh.Update(Task.Factory);

        // create new query
        query = new DtNavMeshQuery(mesh.NavMesh(), 512);

        // find path again
        query.FindNearestPoly(START_POS, EXTENT, filter, out startRef, out startPt, out var _);
        query.FindNearestPoly(END_POS, EXTENT, filter, out endRef, out endPt, out var _);
        query.FindPath(startRef, endRef, startPt, endPt, filter, path, out pathCount, DtFindPathOption.AnyAngle);

        // check path length with obstacles
        Assert.That(pathCount, Is.EqualTo(19));
        // remove obstacle
        mesh.RemoveCollider(colliderId);
        // update navmesh asynchronously
        mesh.Update(Task.Factory);
        // create new query
        query = new DtNavMeshQuery(mesh.NavMesh(), 512);

        // find path one more time
        query.FindNearestPoly(START_POS, EXTENT, filter, out startRef, out startPt, out var _);
        query.FindNearestPoly(END_POS, EXTENT, filter, out endRef, out endPt, out var _);
        query.FindPath(startRef, endRef, startPt, endPt, filter, path, out pathCount, DtFindPathOption.AnyAngle);

        // path length should be back to the initial value
        Assert.That(pathCount, Is.EqualTo(16));
    }
}