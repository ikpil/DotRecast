using System;
using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcVec3iTest
{
    [Test]
    [Repeat(100000)]
    public void TestEquals()
    {
        var x = Random.Shared.Next();
        var y = Random.Shared.Next();
        var z = Random.Shared.Next();

        var v1 = new RcVec3i(x, y, z);
        var v2 = new RcVec3i(x, y, z);
        var v3 = new RcVec3i(x + 1, y, z);

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
        var v1 = new RcVec3i(Random.Shared.Next(1000), Random.Shared.Next(1000), Random.Shared.Next(1000));
        var v2 = new RcVec3i(Random.Shared.Next(1000), Random.Shared.Next(1000), Random.Shared.Next(1000));
        var scalar = Random.Shared.Next(100);

        // Add
        var vAdd = v1 + v2;
        Assert.That(vAdd.X, Is.EqualTo(v1.X + v2.X));
        Assert.That(vAdd.Y, Is.EqualTo(v1.Y + v2.Y));
        Assert.That(vAdd.Z, Is.EqualTo(v1.Z + v2.Z));

        // Subtract
        var vSub = v1 - v2;
        Assert.That(vSub.X, Is.EqualTo(v1.X - v2.X));
        Assert.That(vSub.Y, Is.EqualTo(v1.Y - v2.Y));
        Assert.That(vSub.Z, Is.EqualTo(v1.Z - v2.Z));

        // Multiply
        var vMul = v1 * scalar;
        Assert.That(vMul.X, Is.EqualTo(v1.X * scalar));
        Assert.That(vMul.Y, Is.EqualTo(v1.Y * scalar));
        Assert.That(vMul.Z, Is.EqualTo(v1.Z * scalar));
    }

    [Test]
    public void TestIndexer()
    {
        var v = new RcVec3i(1, 2, 3);
        Assert.That(v[0], Is.EqualTo(1));
        Assert.That(v[1], Is.EqualTo(2));
        Assert.That(v[2], Is.EqualTo(3));
        Assert.Throws<IndexOutOfRangeException>(() => { var _ = v[3]; });
    }

    [Test]
    public void TestToString()
    {
        var v = new RcVec3i(1, 2, 3);
        Assert.That(v.ToString(), Is.EqualTo("(1, 2, 3)"));
    }

    [Test]
    public void TestStaticProperties()
    {
        Assert.That(RcVec3i.Zero, Is.EqualTo(new RcVec3i(0, 0, 0)));
        Assert.That(RcVec3i.UnitX, Is.EqualTo(new RcVec3i(1, 0, 0)));
        Assert.That(RcVec3i.UnitY, Is.EqualTo(new RcVec3i(0, 1, 0)));
        Assert.That(RcVec3i.UnitZ, Is.EqualTo(new RcVec3i(0, 1, 1)));
    }
}