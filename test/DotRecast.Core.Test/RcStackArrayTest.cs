using System;
using System.Collections.Generic;
using DotRecast.Core.Collections;
using NuGet.Frameworks;
using NUnit.Framework;

namespace DotRecast.Core.Test;

[Parallelizable]
public class RcStackArrayTest
{
    public List<int> RandomValues(int size)
    {
        var rand = new RcRand();

        // excepted values
        var list = new List<int>();
        for (int i = 0; i < size; ++i)
        {
            list.Add(rand.NextInt32());
        }

        return list;
    }

    [Test]
    public void TestStackOverflow()
    {
        // normal
        var array_128_512_1 = RcStackArray128<RcStackArray512<float>>.Empty; // 128 * 512 = 65536
        
        // warn
        //var array_128_512_2 = RcStackArray128<RcStackArray512<float>>.Empty; // 128 * 512 = 65536

        // danger
        // var array_32_512_1 = RcStackArray32<RcStackArray512<float>>.Empty; // 32 * 512 = 16384
        // var array_16_512_1 = RcStackArray16<RcStackArray512<float>>.Empty; // 16 * 512 = 8192
        // var array_8_512_1 = RcStackArray8<RcStackArray512<float>>.Empty; // 8 * 512 = 4196
        // var array_4_256_1 = RcStackArray4<RcStackArray256<float>>.Empty; // 4 * 256 = 1024
        // var array_4_64_1 = RcStackArray4<RcStackArray64<float>>.Empty; // 4 * 64 = 256
        // var array_2_8_1 = RcStackArray2<RcStackArray8<float>>.Empty; // 2 * 8 = 16
        // var array_2_4_1 = RcStackArray2<RcStackArray2<float>>.Empty; // 2 * 2 = 4

        float f1 = 0.0f; // 1
        //float f2 = 0.0f; // my system stack overflow!
        Assert.That(f1, Is.EqualTo(0.0f));
    }

    [Test]
    public void TestRcStackArray2()
    {
        var array = RcStackArray2<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(2));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }

    [Test]
    public void TestRcStackArray4()
    {
        var array = RcStackArray4<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(4));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }

    [Test]
    public void TestRcStackArray8()
    {
        var array = RcStackArray8<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(8));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }

    [Test]
    public void TestRcStackArray16()
    {
        var array = RcStackArray16<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(16));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }

    [Test]
    public void TestRcStackArray32()
    {
        var array = RcStackArray32<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(32));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }

    [Test]
    public void TestRcStackArray64()
    {
        var array = RcStackArray64<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(64));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }

    [Test]
    public void TestRcStackArray128()
    {
        var array = RcStackArray128<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(128));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }

    [Test]
    public void TestRcStackArray256()
    {
        var array = RcStackArray256<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(256));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }

    [Test]
    public void TestRcStackArray512()
    {
        var array = RcStackArray512<int>.Empty;
        Assert.That(array.Length, Is.EqualTo(512));

        var values = RandomValues(array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = values[i];
        }

        for (int i = 0; i < array.Length; ++i)
        {
            Assert.That(array[i], Is.EqualTo(values[i]));
        }

        Assert.That(array[^1], Is.EqualTo(values[^1]));

        Assert.Throws<IndexOutOfRangeException>(() => array[-1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length + 1] = 0);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = array[array.Length + 1]);
    }
}