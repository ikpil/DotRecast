using System.Collections.Generic;
using System.IO;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using DotRecast.Recast.Toolset.Tools;
using NUnit.Framework;

namespace DotRecast.Recast.Toolset.Test.Tools;

public class RcTestNavMeshToolTest
{
    private DtNavMeshQuery _query;
    private long _startRef;
    private readonly RcVec3f _center = new RcVec3f(39.587433f, -0.13924503f, -29.736252f);
    private readonly DtQueryDefaultFilter _filter = new DtQueryDefaultFilter();

    [SetUp]
    public void SetUp()
    {
        using var stream = new MemoryStream(RcIO.ReadFileIfFound("all_tiles_navmesh.bin"));
        using var reader = new BinaryReader(stream);
        var mesh = new DtMeshSetReader().Read(reader, 6);
        _query = new DtNavMeshQuery(mesh);
        var status = _query.FindNearestPoly(_center, new RcVec3f(2, 4, 2), _filter,
            out _startRef, out var nearest, out var overPoly);
        Assert.That(status.Succeeded(), Is.True);
        Assert.That(_startRef, Is.Not.Zero);
        Assert.That(overPoly, Is.False);
        Assert.That(RcVec.Dist2D(_center, nearest), Is.EqualTo(0.14755693f).Within(1e-6f));
    }

    [TestCase(0.120082453f)]
    [TestCase(0f)]
    public void ReturnsFailureWhenCircleDoesNotOverlapNavMesh(float radius)
    {
        var points = new List<RcVec3f>();
        var tool = new RcTestNavMeshTool();
        var end = new RcVec3f(_center.X + radius, _center.Y, _center.Z);

        var status = tool.FindRandomPointAroundCircle(_query, _startRef, _startRef,
            _center, end, _filter, true, 300, ref points);

        Assert.That(status.Value, Is.EqualTo(DtStatus.DT_FAILURE.Value));
        Assert.That(points, Is.Empty);
    }

    [Test]
    public void ReturnsInvalidParameterInsteadOfRetryingInvalidReference()
    {
        var points = new List<RcVec3f>();
        var tool = new RcTestNavMeshTool();
        var end = new RcVec3f(_center.X + 1, _center.Y, _center.Z);

        var status = tool.FindRandomPointAroundCircle(_query, -1, _startRef,
            _center, end, _filter, true, 300, ref points);

        Assert.That(status.Value, Is.EqualTo((DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM).Value));
        Assert.That(points, Is.Empty);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ReplacesPreviousPointsWithRequestedCount(bool constrainByCircle)
    {
        var points = new List<RcVec3f> { RcVec3f.Zero, RcVec3f.Zero };
        var tool = new RcTestNavMeshTool();
        var end = new RcVec3f(_center.X + 0.3f, _center.Y, _center.Z);

        var status = tool.FindRandomPointAroundCircle(_query, _startRef, _startRef,
            _center, end, _filter, constrainByCircle, 30, ref points);

        Assert.That(status.Succeeded(), Is.True);
        Assert.That(points, Has.Count.EqualTo(30));
        foreach (var point in points)
        {
            Assert.That(point.IsFinite(), Is.True);
            _query.ClosestPointOnPoly(_startRef, point, out var closest, out var overPoly);
            Assert.That(overPoly, Is.True);
            Assert.That(RcVec.Dist2D(point, closest), Is.LessThan(1e-5f));
            if (constrainByCircle)
            {
                Assert.That(RcVec.Dist2D(_center, point), Is.LessThanOrEqualTo(0.30001f));
            }
        }
    }
}
