using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Detour.QueryResults;
using NUnit.Framework;

namespace DotRecast.Detour.Dynamic.Test;

public class DynamicNavMeshTest
{
    private static readonly Vector3f START_POS = Vector3f.Of(70.87453f, 0.0010070801f, 86.69021f);
    private static readonly Vector3f END_POS = Vector3f.Of(-50.22061f, 0.0010070801f, -70.761444f);
    private static readonly Vector3f EXTENT = Vector3f.Of(0.1f, 0.1f, 0.1f);
    private static readonly Vector3f SPHERE_POS = Vector3f.Of(45.381645f, 0.0010070801f, 52.68981f);


    [Test]
    public void e2eTest()
    {
        byte[] bytes = Loader.ToBytes("test_tiles.voxels");
        using var ms = new MemoryStream(bytes);
        using var bis = new BinaryReader(ms);

        // load voxels from file
        VoxelFileReader reader = new VoxelFileReader();
        VoxelFile f = reader.read(bis);
        // create dynamic navmesh
        DynamicNavMesh mesh = new DynamicNavMesh(f);
        // build navmesh asynchronously using multiple threads
        Task<bool> future = mesh.build(Task.Factory);
        // wait for build to complete
        bool _ = future.Result;
        // create new query
        NavMeshQuery query = new NavMeshQuery(mesh.navMesh());
        QueryFilter filter = new DefaultQueryFilter();
        // find path
        FindNearestPolyResult start = query.findNearestPoly(START_POS, EXTENT, filter).result;
        FindNearestPolyResult end = query.findNearestPoly(END_POS, EXTENT, filter).result;
        List<long> path = query.findPath(start.getNearestRef(), end.getNearestRef(), start.getNearestPos(),
            end.getNearestPos(), filter, NavMeshQuery.DT_FINDPATH_ANY_ANGLE, float.MaxValue).result;
        // check path length without any obstacles
        Assert.That(path.Count, Is.EqualTo(16));
        // place obstacle
        Collider colldier = new SphereCollider(SPHERE_POS, 20, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND, 0.1f);
        long colliderId = mesh.addCollider(colldier);
        // update navmesh asynchronously
        future = mesh.update(Task.Factory);
        // wait for update to complete
        _ = future.Result;
        // create new query
        query = new NavMeshQuery(mesh.navMesh());
        // find path again
        start = query.findNearestPoly(START_POS, EXTENT, filter).result;
        end = query.findNearestPoly(END_POS, EXTENT, filter).result;
        path = query.findPath(start.getNearestRef(), end.getNearestRef(), start.getNearestPos(), end.getNearestPos(), filter,
            NavMeshQuery.DT_FINDPATH_ANY_ANGLE, float.MaxValue).result;
        // check path length with obstacles
        Assert.That(path.Count, Is.EqualTo(19));
        // remove obstacle
        mesh.removeCollider(colliderId);
        // update navmesh asynchronously
        future = mesh.update(Task.Factory);
        // wait for update to complete
        _ = future.Result;
        // create new query
        query = new NavMeshQuery(mesh.navMesh());
        // find path one more time
        start = query.findNearestPoly(START_POS, EXTENT, filter).result;
        end = query.findNearestPoly(END_POS, EXTENT, filter).result;
        path = query.findPath(start.getNearestRef(), end.getNearestRef(), start.getNearestPos(), end.getNearestPos(), filter,
            NavMeshQuery.DT_FINDPATH_ANY_ANGLE, float.MaxValue).result;
        // path length should be back to the initial value
        Assert.That(path.Count, Is.EqualTo(16));
    }
}