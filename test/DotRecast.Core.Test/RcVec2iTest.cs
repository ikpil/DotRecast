using System;
using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcVec2iTest
{
    [Test]
    [Repeat(100000)]
    public void TestEquals()
    {
        var x = Random.Shared.Next();
        var y = Random.Shared.Next();

        var v1 = new RcVec2i(x, y);
        var v2 = new RcVec2i(x, y);
        var v3 = new RcVec2i(x + 1, y);

        Assert.That(v1, Is.EqualTo(v2));
        Assert.That(v1 == v2, Is.True);
        Assert.That(v1 != v3, Is.True);
        Assert.That(v1.Equals(v2), Is.True);
        Assert.That(v1.Equals((object)v2), Is.True);
        Assert.That(v1.GetHashCode(), Is.EqualTo(v2.GetHashCode()));
    }

    [Test]
    [Repeat(100000)]
    public void TestArithmetic()
    {
        var v1 = new RcVec2i(Random.Shared.Next(1000), Random.Shared.Next(1000));
        var v2 = new RcVec2i(Random.Shared.Next(1000), Random.Shared.Next(1000));
        var scalar = Random.Shared.Next(100);

        // Add
        var vAdd = v1 + v2;
        Assert.That(vAdd.X, Is.EqualTo(v1.X + v2.X));
        Assert.That(vAdd.Y, Is.EqualTo(v1.Y + v2.Y));

        // Subtract
        var vSub = v1 - v2;
        Assert.That(vSub.X, Is.EqualTo(v1.X - v2.X));
        Assert.That(vSub.Y, Is.EqualTo(v1.Y - v2.Y));

        // Multiply
        var vMul = v1 * scalar;
        Assert.That(vMul.X, Is.EqualTo(v1.X * scalar));
        Assert.That(vMul.Y, Is.EqualTo(v1.Y * scalar));
    }

    [Test]
    public void TestIndexer()
    {
        var v = new RcVec2i(1, 2);
        Assert.That(v[0], Is.EqualTo(1));
        Assert.That(v[1], Is.EqualTo(2));
        Assert.Throws<IndexOutOfRangeException>(() => { var _ = v[2]; });
    }

    [Test]
    public void TestToString()
    {
        var v = new RcVec2i(1, 2);
        Assert.That(v.ToString(), Is.EqualTo("(1, 2)"));
    }

    [Test]
    public void TestStaticProperties()
    {
        Assert.That(RcVec2i.Zero, Is.EqualTo(new RcVec2i(0, 0)));
        Assert.That(RcVec2i.UnitX, Is.EqualTo(new RcVec2i(1, 0)));
        Assert.That(RcVec2i.UnitY, Is.EqualTo(new RcVec2i(0, 1)));
    }
}