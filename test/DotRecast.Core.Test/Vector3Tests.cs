using System;
using System.Numerics;
using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class Vector3Tests
{
    [Test]
    [Repeat(100000)]
    public void TestVectorLength()
    {
        var v1 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v11 = new RcVec3f(v1.X, v1.Y, v1.Z);

        Assert.That(v1.Length(), Is.EqualTo(v11.Length()));
        Assert.That(v1.LengthSquared(), Is.EqualTo(v11.LengthSquared()));
    }

    [Test]
    [Repeat(100000)]
    public void TestVectorSubtract()
    {
        var v1 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v2 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v3 = Vector3.Subtract(v1, v2);
        var v4 = v1 - v2;
        Assert.That(v3, Is.EqualTo(v4));

        var v11 = new RcVec3f(v1.X, v1.Y, v1.Z);
        var v22 = new RcVec3f(v2.X, v2.Y, v2.Z);
        var v33 = RcVec3f.Subtract(v11, v22);
        var v44 = v11 - v22;
        Assert.That(v33, Is.EqualTo(v44));

        Assert.That(v3.X, Is.EqualTo(v33.X));
        Assert.That(v3.Y, Is.EqualTo(v33.Y));
        Assert.That(v3.Z, Is.EqualTo(v33.Z));
    }


    [Test]
    [Repeat(100000)]
    public void TestVectorAdd()
    {
        var v1 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v2 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v3 = Vector3.Add(v1, v2);
        var v4 = v1 + v2;
        Assert.That(v3, Is.EqualTo(v4));

        var v11 = new RcVec3f(v1.X, v1.Y, v1.Z);
        var v22 = new RcVec3f(v2.X, v2.Y, v2.Z);
        var v33 = RcVec3f.Add(v11, v22);
        var v44 = v11 + v22;
        Assert.That(v33, Is.EqualTo(v44));

        Assert.That(v3.X, Is.EqualTo(v33.X));
        Assert.That(v3.Y, Is.EqualTo(v33.Y));
        Assert.That(v3.Z, Is.EqualTo(v33.Z));
    }

    [Test]
    [Repeat(100000)]
    public void TestVectorNormalize()
    {
        var v1 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v2 = Vector3.Normalize(v1);

        var v11 = new RcVec3f(v1.X, v1.Y, v1.Z);
        var v22 = RcVec3f.Normalize(v11);

        Assert.That(v2.X, Is.EqualTo(v22.X).Within(0.000001d));
        Assert.That(v2.Y, Is.EqualTo(v22.Y).Within(0.000001d));
        Assert.That(v2.Z, Is.EqualTo(v22.Z).Within(0.000001d));
    }

    [Test]
    [Repeat(100000)]
    public void TestVectorCross()
    {
        var v1 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v2 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v3 = Vector3.Cross(v1, v2);

        var v11 = new RcVec3f(v1.X, v1.Y, v1.Z);
        var v22 = new RcVec3f(v2.X, v2.Y, v2.Z);
        var v33 = RcVec3f.Cross(v11, v22);

        Assert.That(v3.X, Is.EqualTo(v33.X));
        Assert.That(v3.Y, Is.EqualTo(v33.Y));
        Assert.That(v3.Z, Is.EqualTo(v33.Z));
    }

    [Test]
    [Repeat(100000)]
    public void TestVectorCopyTo()
    {
        var v1 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var array1 = new float[3];
        v1.CopyTo(array1);

        var v11 = new RcVec3f(v1.X, v1.Y, v1.Z);
        var array11 = new float[3];
        v11.CopyTo(array11);

        Assert.That(array1, Is.EqualTo(array11));
    }

    [Test]
    [Repeat(100000)]
    public void TestVectorDistance()
    {
        var v1 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v2 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var d3 = Vector3.Distance(v1, v2);
        var d4 = Vector3.DistanceSquared(v1, v2);

        var v11 = new RcVec3f(v1.X, v1.Y, v1.Z);
        var v22 = new RcVec3f(v2.X, v2.Y, v2.Z);
        var d33 = RcVec3f.Distance(v11, v22);
        var d44 = RcVec3f.DistanceSquared(v11, v22);

        Assert.That(d3, Is.EqualTo(d33));
        Assert.That(d4, Is.EqualTo(d44));
    }
    
    // [Test]
    // [Repeat(100000)]
    // public void TestVectorLerp()
    // {
    //     var amt = Random.Shared.NextSingle();
    //     var v1 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
    //     var v2 = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
    //     var v3 = Vector3.Lerp(v1, v2, amt);
    //
    //     var v11 = new RcVec3f(v1.X, v1.Y, v1.Z);
    //     var v22 = new RcVec3f(v2.X, v2.Y, v2.Z);
    //     var v33 = RcVec3f.Lerp(v11, v22, amt);
    //
    //     Assert.That(v3.X, Is.EqualTo(v33.X));
    //     Assert.That(v3.Y, Is.EqualTo(v33.Y));
    //     Assert.That(v3.Z, Is.EqualTo(v33.Z));
    // }
}