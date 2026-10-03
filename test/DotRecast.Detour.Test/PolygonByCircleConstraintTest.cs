/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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

using System;
using DotRecast.Core.Collections;
using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class PolygonByCircleConstraintTest
{
    private readonly IDtPolygonByCircleConstraint _constraint = DtStrictDtPolygonByCircleConstraint.Shared;

    [Test]
    public void ShouldHandlePolygonFullyInsideCircle()
    {
        float[] polygon = { -2, 0, 2, 2, 0, 2, 2, 0, -2, -2, 0, -2 };
        RcVec3f center = new RcVec3f(1, 0, 1);
        RcFixedArray256<float> constrained = new RcFixedArray256<float>();

        _constraint.Apply(polygon, center, 6, constrained.AsSpan(), out var ncverts);
        Assert.That(constrained.AsSpan().Slice(0, ncverts).ToArray(), Is.EqualTo(polygon));
    }

    [Test]
    public void ShouldHandleVerticalSegment()
    {
        int expectedSize = 21;
        float[] polygon = { -2, 0, 2, 2, 0, 2, 2, 0, -2, -2, 0, -2 };
        RcVec3f center = new RcVec3f(2, 0, 0);
        RcFixedArray256<float> constrained = new RcFixedArray256<float>();

        _constraint.Apply(polygon, center, 3, constrained.AsSpan(), out var ncverts);
        Assert.That(ncverts, Is.EqualTo(expectedSize));
        Assert.That(constrained.AsSpan().Slice(0, ncverts).ToArray(), Is.SupersetOf(new[] { 2f, 0f, 2f, 2f, 0f, -2f }));
    }

    [Test]
    public void ShouldHandleCircleFullyInsidePolygon()
    {
        int expectedSize = 12 * 3;
        float[] polygon = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        RcVec3f center = new RcVec3f(-1, 0, -1);
        RcFixedArray256<float> constrained = new RcFixedArray256<float>();

        _constraint.Apply(polygon, center, 2, constrained.AsSpan(), out var ncverts);

        Assert.That(ncverts, Is.EqualTo(expectedSize));

        for (int i = 0; i < expectedSize; i += 3)
        {
            float x = constrained[i] + 1;
            float z = constrained[i + 2] + 1;
            Assert.That(x * x + z * z, Is.EqualTo(4).Within(1e-4f));
        }
    }

    [Test]
    public void ShouldHandleCircleInsidePolygon()
    {
        int expectedSize = 9 * 3;
        float[] polygon = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        RcVec3f center = new RcVec3f(-2, 0, -1);
        RcFixedArray256<float> constrained = new RcFixedArray256<float>();

        _constraint.Apply(polygon, center, 3, constrained.AsSpan(), out var ncverts);

        Assert.That(ncverts, Is.EqualTo(expectedSize));
        AssertCyclicVerticesMatch(constrained.AsSpan().Slice(0, ncverts),
            new float[] { -4.0f, 0.0f, 0.0f, -3.4641f, 0.0f, 1.6076f, -2.0f, 0.0f, 2.0f, -0.5f, 0.0f, 1.5980f, 0.5980f, 0.0f,
                0.4999f, 1.0f, 0.0f, -1f, 0.5980f, 0.0f, -2.5f, -0.5f, 0.0f, -3.5980f, -2.0f, 0.0f, -4.0f });
    }

    [Test]
    public void ShouldHandleCircleOutsidePolygon()
    {
        int expectedSize = 7 * 3;
        float[] polygon = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        RcVec3f center = new RcVec3f(4, 0, 0);
        RcFixedArray256<float> constrained = new RcFixedArray256<float>();

        _constraint.Apply(polygon, center, 4, constrained.AsSpan(), out var ncverts);

        Assert.That(ncverts, Is.EqualTo(expectedSize));
        Assert.That(constrained.AsSpan().Slice(0, ncverts).ToArray(), Is.EqualTo(
            new float[] { 0.5358f, 0.0f, -1.9999f, 0f, 0.0f, 0f, 0.5358f, 0.0f, 2.0f, 1.5358f,
                0.0f, 3.0f, 2.0f, 0.0f, 3.0f, 3.0f, 0.0f, -3.0f, 1.7799f, 0.0f, -3.2440f }).Within(0.0001f));
    }

    [Test]
    public void ShouldHandleZeroRadius()
    {
        float[] polygon = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        RcVec3f center = new RcVec3f(-1, 0, -1);
        RcFixedArray256<float> constrained = new RcFixedArray256<float>();
        Assert.That(_constraint.Apply(polygon, center, 0, constrained.AsSpan(), out int count), Is.True);
        Assert.That(count, Is.EqualTo(36));
        for (int i = 0; i < count; i += 3)
        {
            Assert.That(constrained[i], Is.EqualTo(center.X));
            Assert.That(constrained[i + 1], Is.EqualTo(center.Y));
            Assert.That(constrained[i + 2], Is.EqualTo(center.Z));
        }
    }

    [Test]
    public void ShouldNotReturnWholeCircleWhenThinIntersectionIsRejected()
    {
        // Relative deduplication rejects this thin overlap. The circle's center is
        // inside the polygon, but most circle vertices lie far outside its narrow X bounds.
        float[] polygon = { -5e-6f, 0, 20, 5e-6f, 0, 20, 5e-6f, 0, -20, -5e-6f, 0, -20 };
        RcFixedArray256<float> constrained = new RcFixedArray256<float>();
        _constraint.Apply(polygon, new RcVec3f(0, 0, 0), 10, constrained.AsSpan(), out int count);
        Assert.That(count, Is.Zero);
    }

    private static void AssertCyclicVerticesMatch(ReadOnlySpan<float> actual, ReadOnlySpan<float> expected)
    {
        Assert.That(actual.Length, Is.EqualTo(expected.Length));
        for (int start = 0; start < actual.Length; start += 3)
        {
            bool match = true;
            for (int i = 0; i < actual.Length; i++)
            {
                if (Math.Abs(actual[(start + i) % actual.Length] - expected[i]) > 0.0001f)
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                return;
            }
        }
        Assert.Fail("Polygon coordinates or winding differ, including all cyclic starting vertices.");
    }
}
