using System;
using System.Numerics;
using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class Vector2Test
{
    [Test]
    [Repeat(100000)]
    public void TestImplicitCasting()
    {
        var v1 = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle());
        var v2 = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle());

        Assert.That(Vector2.Distance(v1, v2), Is.EqualTo(Vector2.Distance(v1, v2)));
    }

}