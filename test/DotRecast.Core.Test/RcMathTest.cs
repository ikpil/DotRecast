using System;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcMathTest
{
    [Test]
    public void TestSqr()
    {
        Assert.That(RcMath.Sqr(0), Is.EqualTo(0));
        Assert.That(RcMath.Sqr(5), Is.EqualTo(25));
        Assert.That(RcMath.Sqr(-5), Is.EqualTo(25));
        Assert.That(RcMath.Sqr(float.PositiveInfinity), Is.EqualTo(float.PositiveInfinity));
        Assert.That(RcMath.Sqr(float.NegativeInfinity), Is.EqualTo(float.PositiveInfinity));
        Assert.That(RcMath.Sqr(float.NaN), Is.EqualTo(float.NaN));
    }

    [Test]
    public void TestLerp()
    {
        //
        Assert.That(RcMath.Lerp(-10, 10, 2f), Is.EqualTo(30));
        Assert.That(RcMath.Lerp(-10, 10, 1f), Is.EqualTo(10));
        Assert.That(RcMath.Lerp(-10, 10, 0.5f), Is.EqualTo(0));
        Assert.That(RcMath.Lerp(-10, 10, 0.25f), Is.EqualTo(-5));
        Assert.That(RcMath.Lerp(-10, 10, 0), Is.EqualTo(-10));
        Assert.That(RcMath.Lerp(-10, 10, -0.5f), Is.EqualTo(-20));
        Assert.That(RcMath.Lerp(-10, 10, -1f), Is.EqualTo(-30));

        //
        Assert.That(RcMath.Lerp(10, 10, 0.5f), Is.EqualTo(10));
        Assert.That(RcMath.Lerp(10, 10, 0.8f), Is.EqualTo(10));

        //
        Assert.That(RcMath.Lerp(10, -10, 0.75f), Is.EqualTo(-5));
    }
}