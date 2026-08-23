/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j Copyright (c) 2015-2026 Piotr Piastucki piotr@jtilia.org
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
using DotRecast.Core;
using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Recast.Test;

public class RecastUtilTest
{
    [Test]
    public void TestVCross()
    {
        float[] v1 = { 3, -3, 1 };
        float[] v2 = { 4, 9, 2 };
        float[] result = new float[3];

        RcVec.Cross(result, v1, v2);

        Assert.That(result[0], Is.EqualTo(-15f).Within(0.001f));
        Assert.That(result[1], Is.EqualTo(-2f).Within(0.001f));
        Assert.That(result[2], Is.EqualTo(39f).Within(0.001f));
    }

    [Test]
    public void TestVCrossSelfIsZero()
    {
        float[] v = { 3, -3, 1 };
        float[] result = new float[3];

        RcVec.Cross(result, v, v);

        Assert.That(result[0], Is.EqualTo(0f).Within(0.001f));
        Assert.That(result[1], Is.EqualTo(0f).Within(0.001f));
        Assert.That(result[2], Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void TestVDot()
    {
        RcVec3f v = new RcVec3f(1, 0, 0);

        float result = RcVec3f.Dot(v, v);

        Assert.That(result, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void TestVDotZeroVectorIsZero()
    {
        RcVec3f v = new RcVec3f(1, 2, 3);

        float result = RcVec3f.Dot(v, RcVec3f.Zero);

        Assert.That(result, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void TestVAdd()
    {
        RcVec3f v1 = new RcVec3f(1, 2, 3);
        RcVec3f v2 = new RcVec3f(5, 6, 7);

        RcVec3f result = RcVec3f.Add(v1, v2);

        Assert.That(result.X, Is.EqualTo(6f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(8f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(10f).Within(0.001f));
    }

    [Test]
    public void TestVSub()
    {
        RcVec3f v1 = new RcVec3f(5, 4, 3);
        RcVec3f v2 = new RcVec3f(1, 2, 3);

        RcVec3f result = RcVec3f.Subtract(v1, v2);

        Assert.That(result.X, Is.EqualTo(4f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(2f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void TestVMin()
    {
        RcVec3f v1 = new RcVec3f(5, 4, 0);
        RcVec3f v2 = new RcVec3f(1, 2, 9);

        RcVec3f result = RcVec3f.Min(v1, v2);

        Assert.That(result.X, Is.EqualTo(1f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(2f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void TestVMinV1IsMin()
    {
        RcVec3f v1 = new RcVec3f(1, 2, 3);
        RcVec3f v2 = new RcVec3f(4, 5, 6);

        RcVec3f result = RcVec3f.Min(v1, v2);

        Assert.That(result, Is.EqualTo(v1));
    }

    [Test]
    public void TestVMinV2IsMin()
    {
        RcVec3f v1 = new RcVec3f(4, 5, 6);
        RcVec3f v2 = new RcVec3f(1, 2, 3);

        RcVec3f result = RcVec3f.Min(v1, v2);

        Assert.That(result, Is.EqualTo(v2));
    }

    [Test]
    public void TestVMax()
    {
        RcVec3f v1 = new RcVec3f(5, 4, 0);
        RcVec3f v2 = new RcVec3f(1, 2, 9);

        RcVec3f result = RcVec3f.Max(v1, v2);

        Assert.That(result.X, Is.EqualTo(5f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(4f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(9f).Within(0.001f));
    }

    [Test]
    public void TestVMaxV2IsMax()
    {
        RcVec3f v1 = new RcVec3f(1, 2, 3);
        RcVec3f v2 = new RcVec3f(4, 5, 6);

        RcVec3f result = RcVec3f.Max(v1, v2);

        Assert.That(result, Is.EqualTo(v2));
    }

    [Test]
    public void TestVMaxV1IsMax()
    {
        RcVec3f v1 = new RcVec3f(4, 5, 6);
        RcVec3f v2 = new RcVec3f(1, 2, 3);

        RcVec3f result = RcVec3f.Max(v1, v2);

        Assert.That(result, Is.EqualTo(v1));
    }

    [Test]
    public void TestVCopy()
    {
        float[] source = { 5, 4, 0 };
        float[] result = { 1, 2, 9 };

        RcVec.Copy(result, 0, source, 0);

        Assert.That(result, Is.EqualTo(new float[] { 5, 4, 0 }));
        Assert.That(source, Is.EqualTo(new float[] { 5, 4, 0 }));
    }

    [Test]
    public void TestVNormalize()
    {
        RcVec3f v = new RcVec3f(3, 3, 3);

        v = RcVec3f.Normalize(v);

        float expected = MathF.Sqrt(1.0f / 3.0f);
        Assert.That(v.X, Is.EqualTo(expected).Within(0.001f));
        Assert.That(v.Y, Is.EqualTo(expected).Within(0.001f));
        Assert.That(v.Z, Is.EqualTo(expected).Within(0.001f));
        Assert.That(v.Length(), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void TestCalcGridSize()
    {
        float[] verts = { 1, 2, 3, 0, 2, 6 };
        float[] bmin = new float[3];
        float[] bmax = new float[3];
        RcRecast.CalcBounds(verts, 2, bmin, bmax);

        RcRecast.CalcGridSize(bmin.ToVec3(), bmax.ToVec3(), 1.5f, out int width, out int height);

        Assert.That(width, Is.EqualTo(1));
        Assert.That(height, Is.EqualTo(2));
    }

    [Test]
    public void TestMarkWalkableTriangles()
    {
        RcContext context = new RcContext();
        float[] verts = { 0, 0, 0, 1, 0, 0, 0, 0, -1 };
        int[] walkableTriangle = { 0, 1, 2 };
        int[] unwalkableTriangle = { 0, 2, 1 };

        int[] areas = RcRecast.MarkWalkableTriangles(
            context, 45, verts, walkableTriangle, 1, new RcAreaModification(1, 1));
        Assert.That(areas[0], Is.EqualTo(1), "One walkable triangle");

        areas = RcRecast.MarkWalkableTriangles(
            context, 45, verts, unwalkableTriangle, 1, new RcAreaModification(0, 0));
        Assert.That(areas[0], Is.EqualTo(0), "One non-walkable triangle");

        areas = RcRecast.MarkWalkableTriangles(
            context, 0, verts, walkableTriangle, 1, new RcAreaModification(1, 1));
        Assert.That(areas[0], Is.EqualTo(0), "Slopes equal to the max slope are considered unwalkable.");
    }
}
