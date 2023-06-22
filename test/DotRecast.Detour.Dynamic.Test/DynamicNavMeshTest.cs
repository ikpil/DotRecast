using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Detour.QueryResults;
using NUnit.Framework;

namespace DotRecast.Detour.Dynamic.Test;

[Parallelizable]
public class DynamicNavMeshTest
{
    private static readonly RcVec3f START_POS = RcVec3f.Of(70.87453f, 0.0010070801f, 86.69021f);
    private static readonly RcVec3f END_POS = RcVec3f.Of(-50.22061f, 0.0010070801f, -70.761444f);
    private static readonly RcVec3f EXTENT = RcVec3f.Of(0.1f, 0.1f, 0.1f);
    private static readonly RcVec3f SPHERE_POS = RcVec3f.Of(45.381645f, 0.0010070801f, 52.68981f);


    [Test]
    public void E2eTest()
    {
        byte[] bytes = Loader.ToBytes("test_tiles.voxels");
        using var ms = new MemoryStream(bytes);
        using var bis = new BinaryReader(ms);

        // load voxels from file
        VoxelFileReader reader = new VoxelFileReader();
        VoxelFile f = reader.Read(bis);
        // create dynamic navmesh
        DynamicNavMesh mesh = new DynamicNavMesh(f);
        // build navmesh asynchronously using multiple threads
        Task<bool> future = mesh.Build(Task.Factory);
        // wait for build to complete
        bool _ = future.Result;

        // create new query
        DtNavMeshQuery query = new DtNavMeshQuery(mesh.NavMesh());
        IDtQueryFilter filter = new DtQueryDefaultFilter();

        // find path
        query.FindNearestPoly(START_POS, EXTENT, filter, out var startRef, out var startPt, out var _);
        query.FindNearestPoly(END_POS, EXTENT, filter, out var endRef, out var endPt, out var _);

        var path = new List<long>();
        query.FindPath(startRef, endRef, startPt, endPt, filter, ref path, DtFindPathOption.AnyAngle);
        // check path length without any obstacles
        Assert.That(path.Count, Is.EqualTo(16));

        // place obstacle
        ICollider colldier = new SphereCollider(SPHERE_POS, 20, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND, 0.1f);
        long colliderId = mesh.AddCollider(colldier);

        // update navmesh asynchronously
        future = mesh.Update(Task.Factory);
        // wait for update to complete
        _ = future.Result;
        // create new query
        query = new DtNavMeshQuery(mesh.NavMesh());

        // find path again
        query.FindNearestPoly(START_POS, EXTENT, filter, out startRef, out startPt, out var _);
        query.FindNearestPoly(END_POS, EXTENT, filter, out endRef, out endPt, out var _);
        query.FindPath(startRef, endRef, startPt, endPt, filter, ref path, DtFindPathOption.AnyAngle);

        // check path length with obstacles
        Assert.That(path.Count, Is.EqualTo(19));
        // remove obstacle
        mesh.RemoveCollider(colliderId);
        // update navmesh asynchronously
        future = mesh.Update(Task.Factory);
        // wait for update to complete
        _ = future.Result;
        // create new query
        query = new DtNavMeshQuery(mesh.NavMesh());

        // find path one more time
        query.FindNearestPoly(START_POS, EXTENT, filter, out startRef, out startPt, out var _);
        query.FindNearestPoly(END_POS, EXTENT, filter, out endRef, out endPt, out var _);
        query.FindPath(startRef, endRef, startPt, endPt, filter, ref path, DtFindPathOption.AnyAngle);

        // path length should be back to the initial value
        Assert.That(path.Count, Is.EqualTo(16));
    }
}