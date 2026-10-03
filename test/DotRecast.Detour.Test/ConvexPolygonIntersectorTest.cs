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
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class ConvexPolygonIntersectorTest
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
        float[] p = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        float[] q = { 2, 0, 4, 4, 0, 4, 4, 0, 2, 2, 0, 2 };
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
        float[] p = { 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0 };
        float[] q = { 5, 0, 6, 6, 0, 6, 6, 0, 5, 5, 0, 5 };
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
        float[] p = { 8.66f, 0, -5, 5, 0, -8.66f, 0, 0, -10, -5, 0, -8.66f, -8.66f, 0, -5, -10, 0, 0, -8.66f, 0, 5, -5, 0, 8.66f,
            0, 0, 10, 5, 0, 8.66f, 8.66f, 0, 5, 10, 0, 0 };

        // 6-vertex polygon (small, with 4 vertices inside p)
        float[] q = { -5, 0, 2, -1, 0, 6, 3, 0, 5, 4, 0, 1, 2, 0, -3, -3, 0, -2 };

        const int WARMUP_ITERATIONS = 50000;
        const int MEASURE_ITERATIONS = 500000;

        int maxIntersection = DtConvexPolygonIntersector.CalculateIntersectionBufferSize(p.Length / 3, q.Length / 3);
        Span<float> intersection = stackalloc float[maxIntersection];

        Assert.That(DtConvexPolygonIntersector.Intersect(p, q, intersection, out int nin), Is.True);
        Assert.That(nin, Is.GreaterThanOrEqualTo(9));
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

    [Test]
    public void ShouldCalculateGeometricBufferWithoutExponentialGrowth()
    {
        Assert.That(DtConvexPolygonIntersector.CalculateIntersectionBufferSize(4, 12), Is.EqualTo(48));
        Assert.That(DtConvexPolygonIntersector.CalculateIntersectionBufferSize(128, 128), Is.EqualTo(768));
    }

    [Test]
    public void ShouldNotAllocateWhenTriangleIntersectionFillsScratchBuffer()
    {
        float[] p = { 0, 0, 2, 2, 0, -1, -2, 0, -1 };
        float[] q = { -2, 0, 1, 2, 0, 1, 0, 0, -2 };
        float[] intersection = new float[18];
        for (int i = 0; i < 1000; i++)
        {
            DtConvexPolygonIntersector.Intersect(p, q, intersection, out _);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        bool result = DtConvexPolygonIntersector.Intersect(p, q, intersection, out int nin);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.That(result, Is.True);
        Assert.That(nin, Is.EqualTo(18));
        Assert.That(allocated, Is.Zero);
    }

    [Test]
    public void ShouldHandleTwoIntersectingTriangles()
    {
        float[] p = { 0.015540654f, 0, 0.01743388f, 0.017864477f, 0, 0.02010114f, 0.010200679f, 0, 0.005310603f };
        float[] q = { 0.013191576f, 0, 0.020906897f, 0.015401214f, 0, 0.01738648f, 0.011963837f, 0, 0.0072015855f };
        float[] expected = { 0.012201321f, 0, 0.009852636f, 0.015012633f, 0, 0.016235121f, 0.013427231f, 0, 0.011537598f,
            0.012127571f, 0, 0.009029355f };
        AssertResultsMatch(p, q, expected);
    }

    [Test]
    public void ShouldHandleNearlyCollinearVerticesOnClipEdge()
    {
        // Both CW. Nine of p's ten vertices lie (up to float rounding) on q's first edge.
        float[] p = { -49.487995f, 0, 541.00555f, -33.95485f, 0, 558.6862f, -32.96344f, 0, 556.24884f, -31.97203f, 0, 553.8115f,
            -30.980623f, 0, 551.37415f, -29.989214f, 0, 548.9368f, -28.997805f, 0, 546.49945f, -28.006397f, 0, 544.06213f,
            -27.014988f, 0, 541.62476f, -26.023579f, 0, 539.18744f };
        float[] q = { -2.2297719f, 0, 480.69107f, -60.726116f, 0, 456.89728f, -116.245f, 0, 593.38873f, -57.748657f, 0, 617.18256f };
        // This float-rounded input exceeds the geometric output bound too. Unlike
        // Java's returned array, the caller's output span cannot grow inside Intersect.
        float[] geometricBuffer = new float[DtConvexPolygonIntersector.CalculateIntersectionBufferSize(p.Length / 3, q.Length / 3)];
        Assert.Throws<ArgumentException>(() => DtConvexPolygonIntersector.Intersect(p, q, geometricBuffer, out _));
        float[] intersection = new float[geometricBuffer.Length * 2];
        Assert.That(DtConvexPolygonIntersector.Intersect(p, q, intersection, out int nin), Is.True);
        Assert.That(nin, Is.GreaterThanOrEqualTo(9));
        Assert.That(nin, Is.LessThanOrEqualTo(intersection.Length));
        Assert.That(nin, Is.GreaterThan(p.Length + q.Length));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ShouldRejectCounterClockwisePolygons(bool reverseSubject)
    {
        float[] cw = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        float[] ccw = { 1, 0, 1, 3, 0, 1, 3, 0, 3, 1, 0, 3 };
        float[] intersection = new float[24];
        var exception = Assert.Throws<ArgumentException>(() => DtConvexPolygonIntersector.Intersect(
            reverseSubject ? ccw : cw, reverseSubject ? cw : ccw, intersection, out _));
        Assert.That(exception.Message, Is.EqualTo("Input polygons must be convex and clockwise."));
    }

    [Test]
    public void ShouldHandlePolygonsSharingOnlyEdges()
    {
        // Two CW triangles sharing the edge (10.18211, 0.3781242)-(10.11662, 0.32780614); the overlap has zero area.
        float[] p = { 10.18211f, 0, 0.3781242f, 10.11662f, 0, 0.32780614f, -6.559058f, 0, 1.817404f };
        float[] q = { 10.11662f, 0, 0.32780614f, 10.18211f, 0, 0.3781242f, 10.163076f, 0, 0.33511952f };
        AssertResultsMatch(p, q, null);
    }

    [TestCase(1e-7f)]
    [TestCase(1e-3f)]
    [TestCase(1f)]
    [TestCase(1e7f)]
    public void ShouldPreserveOverlapAcrossScales(float scale)
    {
        float[] p = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        float[] q = { 1, 0, 3, 3, 0, 3, 3, 0, 1, 1, 0, 1 };
        for (int i = 0; i < p.Length; i++)
        {
            p[i] *= scale;
            q[i] *= scale;
        }

        float[] intersection = new float[24];
        Assert.That(DtConvexPolygonIntersector.Intersect(p, q, intersection, out int nin), Is.True);
        Assert.That(nin, Is.EqualTo(12));
        for (int i = 0; i < nin; i++)
        {
            intersection[i] /= scale;
        }
        float[] expected = { 1, 0, 2, 2, 0, 2, 2, 0, 1, 1, 0, 1 };
        Assert.That(Normalize(intersection.AsSpan(0, nin).ToArray()), Is.EqualTo(Normalize(expected)).Within(1e-5f));
    }

    [Test]
    public void ShouldRejectZeroAreaPolygons()
    {
        float[] p = { 0, 0, 0, 1, 0, 1, 2, 0, 2 };
        float[] q = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        AssertResultsMatch(p, q, null);
        AssertResultsMatch(q, p, null);
    }

    [Test]
    public void ShouldComputeSignedArea()
    {
        float[] cw = { 0, 0, 0, 0, 0, 2, 2, 0, 2, 2, 0, 0 };
        float[] ccw = { 0, 0, 0, 2, 0, 0, 2, 0, 2, 0, 0, 2 };
        float[] fractional = { 1.3456f, 0, 2.7891f, 3.4567f, 0, 4.1234f, 5.6789f, 0, 1.2345f, 2.3456f, 0, 0.5678f };
        Assert.That(DtConvexPolygonIntersector.AreaXZ(cw, 4), Is.EqualTo(4.0));
        Assert.That(DtConvexPolygonIntersector.AreaXZ(ccw, 4), Is.EqualTo(-4.0));
        Assert.That(DtConvexPolygonIntersector.AreaXZ(fractional, 4), Is.EqualTo(8.5674).Within(0.0001));
    }

    [Test]
    public void ShouldInterpolateBoundaryWithoutExtrapolatingHeight()
    {
        // The old EPS classified the first two vertices as inside the x <= 0 edge,
        // although both are outside. Interpolating that artificial crossing extrapolated Y.
        float[] p = { 2e-7f, 10, 0, 8e-7f, 20, 1, 1, 30, 1, 1, 40, 0 };
        float[] q = { -1, 0, 2, 0, 0, 2, 0, 0, -1, -1, 0, -1 };
        AssertResultsMatch(p, q, null);
    }

    [TestCase(1e-3f)]
    [TestCase(1f)]
    [TestCase(1e3f)]
    public void ShouldRejectSliversRelativeToInputArea(float scale)
    {
        float[] p = { 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0 };
        float[] q = { 0.999999f, 0, 1, 2, 0, 1, 2, 0, 0, 0.999999f, 0, 0 };
        for (int i = 0; i < p.Length; i++)
        {
            p[i] *= scale;
            q[i] *= scale;
        }
        AssertResultsMatch(p, q, null);
    }

    [Test]
    public void ShouldHandleLargePolygonsUsingArrayScratch()
    {
        float[] p = RegularPolygon(128, 0, 0, 1, 1);
        float[] q = { -2, 0, 2, 2, 0, 2, 2, 0, -2, -2, 0, -2 };
        AssertResultsMatch(p, q, p);
    }

    [Test]
    public void ShouldNotAllocateForSmallPolygons()
    {
        float[] p = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        float[] q = { 1, 0, 3, 3, 0, 3, 3, 0, 1, 1, 0, 1 };
        float[] intersection = new float[DtConvexPolygonIntersector.CalculateIntersectionBufferSize(4, 4)];
        for (int i = 0; i < 1000; i++)
        {
            DtConvexPolygonIntersector.Intersect(p, q, intersection, out _);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
        {
            DtConvexPolygonIntersector.Intersect(p, q, intersection, out _);
        }
        Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
    }

    [Test]
    public void ShouldLeaveCountZeroWhenOutputIsTooSmall()
    {
        float[] p = { 0, 0, 2, 2, 0, 2, 2, 0, 0, 0, 0, 0 };
        int nin = -1;
        Assert.Throws<ArgumentException>(() => DtConvexPolygonIntersector.Intersect(p, p, new float[3], out nin));
        Assert.That(nin, Is.Zero);
    }

    [Test]
    public void ShouldMatchIndependentIntersectionArea()
    {
        Random random = new Random(327);
        for (int sample = 0; sample < 200; sample++)
        {
            float scale = sample % 3 == 0 ? 1e-4f : sample % 3 == 1 ? 1f : 1e4f;
            float[] p = RegularPolygon(random.Next(3, 13), 0, 0, scale, scale * 0.7f);
            float[] q = RegularPolygon(random.Next(3, 13), (float)random.NextDouble() * 2 * scale,
                (float)random.NextDouble() * scale, scale * 0.8f, scale);
            float[] intersection = new float[DtConvexPolygonIntersector.CalculateIntersectionBufferSize(p.Length / 3, q.Length / 3)];
            bool result = DtConvexPolygonIntersector.Intersect(p, q, intersection, out int nin);
            double expected = ReferenceIntersectionArea(p, q);
            double actual = result ? DtConvexPolygonIntersector.AreaXZ(intersection, nin / 3) : 0;
            double tolerance = scale * (double)scale * 2e-6;
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance), $"Sample {sample}");
        }
    }

    private static float[] RegularPolygon(int count, float x, float z, float radiusX, float radiusZ)
    {
        float[] polygon = new float[count * 3];
        for (int i = 0; i < count; i++)
        {
            double angle = -i * Math.PI * 2 / count;
            polygon[i * 3] = x + (float)Math.Cos(angle) * radiusX;
            polygon[i * 3 + 2] = z + (float)Math.Sin(angle) * radiusZ;
        }
        return polygon;
    }

    // Independent construction: collect contained vertices and edge crossings, then
    // order the convex intersection around its centroid. No half-plane clipping passes.
    private static double ReferenceIntersectionArea(float[] p, float[] q)
    {
        var points = new List<(double x, double z)>();
        AddContainedVertices(p, q, points);
        AddContainedVertices(q, p, points);
        for (int i = 0; i < p.Length; i += 3)
        {
            int inext = (i + 3) % p.Length;
            double rx = (double)p[inext] - p[i], rz = (double)p[inext + 2] - p[i + 2];
            for (int j = 0; j < q.Length; j += 3)
            {
                int jnext = (j + 3) % q.Length;
                double sx = (double)q[jnext] - q[j], sz = (double)q[jnext + 2] - q[j + 2];
                double denominator = rx * sz - rz * sx;
                if (denominator == 0)
                {
                    continue;
                }
                double dx = (double)q[j] - p[i], dz = (double)q[j + 2] - p[i + 2];
                double t = (dx * sz - dz * sx) / denominator;
                double u = (dx * rz - dz * rx) / denominator;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    points.Add((p[i] + t * rx, p[i + 2] + t * rz));
                }
            }
        }
        if (points.Count < 3)
        {
            return 0;
        }
        double centerX = 0, centerZ = 0;
        foreach (var point in points)
        {
            centerX += point.x / points.Count;
            centerZ += point.z / points.Count;
        }
        points.Sort((a, b) => Math.Atan2(a.z - centerZ, a.x - centerX).CompareTo(Math.Atan2(b.z - centerZ, b.x - centerX)));
        double area = 0;
        for (int i = 0; i < points.Count; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % points.Count];
            area += a.x * b.z - a.z * b.x;
        }
        return Math.Abs(area) / 2;
    }

    private static void AddContainedVertices(float[] p, float[] q, List<(double x, double z)> points)
    {
        for (int i = 0; i < p.Length; i += 3)
        {
            bool inside = true;
            for (int j = 0; j < q.Length; j += 3)
            {
                int next = (j + 3) % q.Length;
                double ax = (double)q[next] - q[j], az = (double)q[next + 2] - q[j + 2];
                double bx = (double)p[i] - q[j], bz = (double)p[i + 2] - q[j + 2];
                if (ax * bz - az * bx > 0)
                {
                    inside = false;
                    break;
                }
            }
            if (inside)
            {
                points.Add((p[i], p[i + 2]));
            }
        }
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
