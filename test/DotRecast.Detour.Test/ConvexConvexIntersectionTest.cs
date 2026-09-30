/*
recast4j copyright (c) 2021-2026 Piotr Piastucki piotr@recast4j.org
DotRecast Copyright (c) 2023-2026 Choi Ikpil ikpil@naver.com

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
using System.Diagnostics;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class ConvexConvexIntersectionTest
{
    [Test]
    public void ShouldHandleSamePolygonIntersection()
    {
        float[] p = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        float[] q = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        float[] expected = { -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4, -4, 0, 0 };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandleIntersection()
    {
        float[] p = { -5, 0, -5, -5, 0, 4, 1, 0, 4, 1, 0, -5 };
        float[] q = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        float[] expected = { 1, 0, -3.4f, -2, 0, -4, -4, 0, 0, -3, 0, 3, 1, 0, 3 };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandlePartialOverlap()
    {
        float[] p = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        float[] q = { 1, 0, 3, 3, 0, 3, 3, 0, 1, 1, 0, 1 };
        float[] expected = { 1, 0, 2, 2, 0, 2, 2, 0, 1, 1, 0, 1 };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandleOneInsideAnother()
    {
        float[] p = { 0, 0, 10, 10, 0, 10, 10, 0, 0, 0, 0, 0 };
        float[] q = { 2, 0, 3, 3, 0, 3, 3, 0, 2, 2, 0, 2 };
        float[] expected = { 2, 0, 3, 3, 0, 3, 3, 0, 2, 2, 0, 2 };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandleTouchingAtCorner()
    {
        float[] p = { 0, 0, 0, 2, 0, 0, 2, 0, 2, 0, 0, 2 };
        float[] q = { 2, 0, 2, 4, 0, 2, 4, 0, 4, 2, 0, 4 };
        float[] expected = null;
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandleCollinearEdgeOverlap()
    {
        float[] p = { 0, 0, 3, 3, 0, 3, 3, 0, 0, 0, 0, 0 };
        float[] q = { 2, 0, 5, 5, 0, 5, 5, 0, 0, 2, 0, 0 };
        float[] expected = { 2, 0, 3, 3, 0, 3, 3, 0, 0, 2, 0, 0 };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandleRotatedSquareIntersection()
    {
        float[] p = { 0, 0, 4, 4, 0, 4, 4, 0, 0, 0, 0, 0 };
        float[] q = { -2, 0, -2, -2, 0, 6, 2, 0, 6, 2, 0, -2 };
        float[] expected = { 0, 0, 4, 2, 0, 4, 2, 0, 0, 0, 0, 0 };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandleNoIntersection()
    {
        float[] p = { 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1 };
        float[] q = { 5, 0, 5, 6, 0, 5, 6, 0, 6, 5, 0, 6 };
        float[] expected = null;
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandleSamePolygonIntersectionWithDifferentStartVertex()
    {
        float[] p = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        float[] q = { 3, 0, -3, -2, 0, -4, -4, 0, 0, -3, 0, 3, 2, 0, 3 };
        float[] expected = { -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4, -4, 0, 0 };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void PerformanceTest()
    {
        // Regular 12-vertex polygon (large, radius 10)
        float[] p = { 10, 0, 0, 8.66f, 0, 5, 5, 0, 8.66f, 0, 0, 10, -5, 0, 8.66f, -8.66f, 0, 5, -10, 0, 0, -8.66f, 0, -5, -5, 0,
            -8.66f, 0, 0, -10, 5, 0, -8.66f, 8.66f, 0, -5 };

        // 6-vertex polygon (small, with 4 vertices inside p)
        float[] q = { -3, 0, -2, 2, 0, -3, 4, 0, 1, 3, 0, 5, -1, 0, 6, -5, 0, 2 };

        const int WARMUP_ITERATIONS = 50000;
        const int MEASURE_ITERATIONS = 500000;

        int maxIntersection = DtConvexPolygonIntersector.CalculateIntersectionBufferSize(p.Length / 3, q.Length / 3);
        Span<float> intersection = stackalloc float[maxIntersection];

        TestContext.Out.WriteLine("\n=== ConvexPolygonIntersector ===");
        for (int i = 0; i < WARMUP_ITERATIONS; i++)
        {
            DtConvexPolygonIntersector.Intersect(p, q, intersection, out _);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < MEASURE_ITERATIONS; i++)
        {
            DtConvexPolygonIntersector.Intersect(p, q, intersection, out _);
        }

        stopwatch.Stop();
        double elapsedMillis = stopwatch.Elapsed.TotalMilliseconds;
        TestContext.Out.WriteLine("Time for {0} iterations: {1:F2} ms ({2:F4} ms per iteration)", MEASURE_ITERATIONS, elapsedMillis,
            elapsedMillis / MEASURE_ITERATIONS);
    }

    [Test]
    public void ShouldInterpolateHeightFromSubjectPolygon()
    {
        float[] p = { 0, 12, 4, 4, 20, 4, 4, 8, 0, 0, 0, 0 };
        float[] q = { 2, 100, 5, 5, 100, 5, 5, 100, 1, 2, 100, 1 };
        float[] expected = { 2, 16, 4, 4, 20, 4, 4, 11, 1, 2, 7, 1 };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldRejectSharedEdgeWithoutArea()
    {
        float[] p = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        float[] q = { 2, 0, 2, 4, 0, 2, 4, 0, 0, 2, 0, 0 };
        AssertResultsMatch(p, q, null);
    }

    [Test]
    public void ShouldRejectClockwisePolygonsTouchingAtCorner()
    {
        float[] p = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        float[] q = { 2, 0, 4, 4, 0, 4, 4, 0, 2, 2, 0, 2 };
        AssertResultsMatch(p, q, null);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void ShouldRejectFewerThanThreeVertices(int vertexCount)
    {
        float[] p = new float[vertexCount * 3];
        float[] q = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        AssertResultsMatch(p, q, null);
        AssertResultsMatch(q, p, null);
    }

    private static void AssertResultsMatch(float[] p, float[] q, float[] expected)
    {
        int maxIntersection = DtConvexPolygonIntersector.CalculateIntersectionBufferSize(p.Length / 3, q.Length / 3);
        Span<float> intersection = stackalloc float[maxIntersection];
        bool result = DtConvexPolygonIntersector.Intersect(p, q, intersection, out int nin);

        if (expected == null)
        {
            Assert.That(result, Is.False);
            Assert.That(nin, Is.Zero);
            return;
        }

        Assert.That(result, Is.True);
        Assert.That(Normalize(intersection.Slice(0, nin).ToArray()), Is.EqualTo(Normalize(expected)).Within(0.0001f));
    }

    private static float[] Normalize(float[] polygon)
    {
        if (polygon == null)
        {
            return null;
        }

        int vertexCount = polygon.Length / 3;
        if (vertexCount <= 1)
        {
            return (float[])polygon.Clone();
        }

        int startIndex = 0;
        for (int i = 1; i < vertexCount; i++)
        {
            if (CompareVertices(polygon, i, startIndex) < 0)
            {
                startIndex = i;
            }
        }

        float[] normalized = new float[polygon.Length];
        for (int i = 0; i < vertexCount; i++)
        {
            int sourceIndex = (startIndex + i) % vertexCount;
            normalized[3 * i] = polygon[3 * sourceIndex];
            normalized[3 * i + 1] = polygon[3 * sourceIndex + 1];
            normalized[3 * i + 2] = polygon[3 * sourceIndex + 2];
        }

        return normalized;
    }

    private static int CompareVertices(float[] polygon, int firstIndex, int secondIndex)
    {
        int firstOffset = 3 * firstIndex;
        int secondOffset = 3 * secondIndex;
        int xComparison = polygon[firstOffset].CompareTo(polygon[secondOffset]);
        if (xComparison != 0)
        {
            return xComparison;
        }

        int zComparison = polygon[firstOffset + 2].CompareTo(polygon[secondOffset + 2]);
        if (zComparison != 0)
        {
            return zComparison;
        }

        return polygon[firstOffset + 1].CompareTo(polygon[secondOffset + 1]);
    }
}
