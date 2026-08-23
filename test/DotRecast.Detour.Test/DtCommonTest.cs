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

using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class DtCommonTest
{
    [Test]
    public void TestVAdd()
    {
        RcVec3f v1 = new RcVec3f(1, 2, 3);
        RcVec3f v2 = new RcVec3f(4, 5, 6);

        RcVec3f result = RcVec3f.Add(v1, v2);

        Assert.That(result.X, Is.EqualTo(5f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(7f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(9f).Within(0.001f));
    }

    [Test]
    public void TestVSub()
    {
        RcVec3f v1 = new RcVec3f(5, 6, 7);
        RcVec3f v2 = new RcVec3f(1, 2, 3);

        RcVec3f result = RcVec3f.Subtract(v1, v2);

        Assert.That(result.X, Is.EqualTo(4f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(4f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(4f).Within(0.001f));
    }

    [Test]
    public void TestVCopy()
    {
        RcVec3f v = new RcVec3f(1, 2, 3);
        float[] result = new float[3];

        v.CopyTo(result);

        Assert.That(result[0], Is.EqualTo(1f).Within(0.001f));
        Assert.That(result[1], Is.EqualTo(2f).Within(0.001f));
        Assert.That(result[2], Is.EqualTo(3f).Within(0.001f));
    }

    [Test]
    public void TestVMad()
    {
        RcVec3f v1 = new RcVec3f(1, 2, 3);
        RcVec3f v2 = new RcVec3f(2, 3, 4);

        RcVec3f result = RcVec.Mad(v1, v2, 2.0f);

        Assert.That(result.X, Is.EqualTo(5f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(8f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(11f).Within(0.001f));
    }

    [Test]
    public void TestVDot2D()
    {
        RcVec3f v1 = new RcVec3f(1, 0, 0);
        RcVec3f v2 = new RcVec3f(1, 0, 0);

        float result = RcVec.Dot2(v1, v2);

        Assert.That(result, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void TestVDot2DZeroVector()
    {
        RcVec3f v1 = new RcVec3f(1, 2, 3);

        float result = RcVec.Dot2(v1, RcVec3f.Zero);

        Assert.That(result, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void TestVLen()
    {
        RcVec3f v = new RcVec3f(3, 4, 0);

        float result = v.Length();

        Assert.That(result, Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void TestVLenSqr()
    {
        RcVec3f v = new RcVec3f(3, 4, 0);

        float result = v.LengthSquared();

        Assert.That(result, Is.EqualTo(25f).Within(0.001f));
    }

    [Test]
    public void TestVDist()
    {
        RcVec3f v1 = RcVec3f.Zero;
        RcVec3f v2 = new RcVec3f(3, 4, 0);

        float result = RcVec3f.Distance(v1, v2);

        Assert.That(result, Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void TestVDist2DSqr()
    {
        RcVec3f v1 = RcVec3f.Zero;
        RcVec3f v2 = new RcVec3f(3, 0, 4);

        float result = RcVec.Dist2DSqr(v1, v2);

        Assert.That(result, Is.EqualTo(25f).Within(0.001f));
    }

    [Test]
    public void TestVNormalize()
    {
        RcVec3f v = new RcVec3f(3, 4, 0);

        v = RcVec3f.Normalize(v);

        Assert.That(v.X, Is.EqualTo(0.6f).Within(0.001f));
        Assert.That(v.Y, Is.EqualTo(0.8f).Within(0.001f));
        Assert.That(v.Z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void TestVLerp()
    {
        RcVec3f v1 = RcVec3f.Zero;
        RcVec3f v2 = new RcVec3f(10, 10, 10);

        RcVec3f result = RcVec3f.Lerp(v1, v2, 0.5f);

        Assert.That(result.X, Is.EqualTo(5f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(5f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void TestVLerpStart()
    {
        RcVec3f v1 = RcVec3f.Zero;
        RcVec3f v2 = new RcVec3f(10, 10, 10);

        RcVec3f result = RcVec3f.Lerp(v1, v2, 0.0f);

        Assert.That(result.X, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void TestVLerpEnd()
    {
        RcVec3f v1 = RcVec3f.Zero;
        RcVec3f v2 = new RcVec3f(10, 10, 10);

        RcVec3f result = RcVec3f.Lerp(v1, v2, 1.0f);

        Assert.That(result.X, Is.EqualTo(10f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(10f).Within(0.001f));
        Assert.That(result.Z, Is.EqualTo(10f).Within(0.001f));
    }
}
