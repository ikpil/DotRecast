using System;
using System.Linq;
using System.Numerics;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class FindCollectPolyTest : AbstractDetourTest
{
    private static readonly long[][] POLY_REFS =
    [
        [
            281474976710697L,
            281474976710695L,
            281474976710696L,
            281474976710691L,
        ],
        [
            281474976710769L,
            281474976710773L,
        ],
        [
            281474976710676L,
            281474976710678L,
            281474976710679L,
            281474976710674L,
            281474976710677L,
            281474976710683L,
            281474976710680L,
            281474976710684L,
        ],

        [
            281474976710748L,
            281474976710753L,
            281474976710752L,
            281474976710750L,
        ],

        [
            281474976710736L,
            281474976710733L,
            281474976710735L,
        ]
    ];

    [Test]
    public void TestFindNearestPoly()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        Vector3 extents = new Vector3(2, 4, 2);
        var polys = new long[32];
        for (int i = 0; i < startRefs.Length; i++)
        {
            Array.Fill(polys, 0);
            Vector3 startPos = startPoss[i];
            var status = query.QueryPolygons(startPos, extents, filter, polys, out var polyCount, 32);
            Assert.That(status.Succeeded(), Is.True, $"index({i})");
            Assert.That(polyCount, Is.EqualTo(POLY_REFS[i].Length), $"index({i})");
            Assert.That(polys.AsSpan(0, polyCount).ToArray(), Is.EqualTo(POLY_REFS[i]), $"index({i})");
        }
    }
}