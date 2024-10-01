using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Detour.Dynamic.Io;
using DotRecast.Detour.Dynamic.Test.Io;
using DotRecast.Detour.Io;
using NUnit.Framework;

namespace DotRecast.Detour.Dynamic.Test;

public class DynamicNavMeshTest
{
    private static readonly RcVec3f START_POS = new RcVec3f(70.87453f, 0.0010070801f, 86.69021f);
    private static readonly RcVec3f END_POS = new RcVec3f(-50.22061f, 0.0010070801f, -70.761444f);
    private static readonly RcVec3f EXTENT = new RcVec3f(0.1f, 0.1f, 0.1f);
    private static readonly RcVec3f SPHERE_POS = new RcVec3f(45.381645f, 0.0010070801f, 52.68981f);


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
        IDtCollider colldier = new DtSphereCollider(SPHERE_POS, 20, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND, 0.1f);
        long colliderId = mesh.AddCollider(colldier);

        // update navmesh asynchronously
        mesh.Update(Task.Factory);

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
        mesh.Update(Task.Factory);
        // create new query
        query = new DtNavMeshQuery(mesh.NavMesh());

        // find path one more time
        query.FindNearestPoly(START_POS, EXTENT, filter, out startRef, out startPt, out var _);
        query.FindNearestPoly(END_POS, EXTENT, filter, out endRef, out endPt, out var _);
        query.FindPath(startRef, endRef, startPt, endPt, filter, ref path, DtFindPathOption.AnyAngle);

        // path length should be back to the initial value
        Assert.That(path.Count, Is.EqualTo(16));
    }


    [Test]
    public void ShouldSaveAndLoadDynamicNavMesh()
    {
        using var writerMs = new MemoryStream();
        using var bw = new BinaryWriter(writerMs);


        int maxVertsPerPoly = 6;
        // load voxels from file

        {
            byte[] bytes = RcIO.ReadFileIfFound("test_tiles.voxels");
            using var readMs = new MemoryStream(bytes);
            using var br = new BinaryReader(readMs);

            DtVoxelFileReader reader = new DtVoxelFileReader(DtVoxelTileLZ4ForTestCompressor.Shared);
            DtVoxelFile f = reader.Read(br);

            // create dynamic navmesh
            DtDynamicNavMesh mesh = new DtDynamicNavMesh(f);

            // build navmesh asynchronously using multiple threads
            mesh.Build(Task.Factory);

            // Save the resulting nav mesh and re-use it
            new DtMeshSetWriter().Write(bw, mesh.NavMesh(), RcByteOrder.LITTLE_ENDIAN, true);
            maxVertsPerPoly = mesh.NavMesh().GetMaxVertsPerPoly();
        }

        {
            byte[] bytes = RcIO.ReadFileIfFound("test_tiles.voxels");
            using var readMs = new MemoryStream(bytes);
            using var br = new BinaryReader(readMs);

            // load voxels from file
            DtVoxelFileReader reader = new DtVoxelFileReader(DtVoxelTileLZ4ForTestCompressor.Shared);
            DtVoxelFile f = reader.Read(br);

            // create dynamic navmesh
            DtDynamicNavMesh mesh = new DtDynamicNavMesh(f);
            // use the saved nav mesh instead of building from scratch
            DtNavMesh navMesh = new DtMeshSetReader().Read(new RcByteBuffer(writerMs.ToArray()), maxVertsPerPoly);
            mesh.NavMesh(navMesh);

            DtNavMeshQuery query = new DtNavMeshQuery(mesh.NavMesh());
            IDtQueryFilter filter = new DtQueryDefaultFilter();

            // find path
            _ = query.FindNearestPoly(START_POS, EXTENT, filter, out var startNearestRef, out var startNearestPos, out var _);
            _ = query.FindNearestPoly(END_POS, EXTENT, filter, out var endNearestRef, out var endNearestPos, out var _);

            List<long> path = new List<long>();
            query.FindPath(startNearestRef, endNearestRef, startNearestPos, endNearestPos, filter, ref path, DtFindPathOption.AnyAngle);

            // check path length without any obstacles
            Assert.That(path.Count, Is.EqualTo(16));

            // place obstacle
            DtCollider colldier = new DtSphereCollider(SPHERE_POS, 20, SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND, 0.1f);
            long colliderId = mesh.AddCollider(colldier);

            // update navmesh asynchronously
            mesh.Update(Task.Factory);

            // create new query
            query = new DtNavMeshQuery(mesh.NavMesh());

            // find path again
            _ = query.FindNearestPoly(START_POS, EXTENT, filter, out startNearestRef, out startNearestPos, out var _);
            _ = query.FindNearestPoly(END_POS, EXTENT, filter, out endNearestRef, out endNearestPos, out var _);

            path = new List<long>();
            query.FindPath(startNearestRef, endNearestRef, startNearestPos, endNearestPos, filter, ref path, DtFindPathOption.AnyAngle);

            // check path length with obstacles
            Assert.That(path.Count, Is.EqualTo(19));

            // remove obstacle
            mesh.RemoveCollider(colliderId);
            // update navmesh asynchronously
            mesh.Update(Task.Factory);

            // create new query
            query = new DtNavMeshQuery(mesh.NavMesh());
            // find path one more time
            _ = query.FindNearestPoly(START_POS, EXTENT, filter, out startNearestRef, out startNearestPos, out var _);
            _ = query.FindNearestPoly(END_POS, EXTENT, filter, out endNearestRef, out endNearestPos, out var _);

            path = new List<long>();
            query.FindPath(startNearestRef, endNearestRef, startNearestPos, endNearestPos, filter, ref path, DtFindPathOption.AnyAngle);

            // path length should be back to the initial value
            Assert.That(path.Count, Is.EqualTo(16));
        }
    }
}