using System;
using System.Collections.Generic;
using DotRecast.Core.Collections;
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
    public void TestRcStackArray4()
    {
        var values = RandomValues(4);
        RcStackArray4<int> array = RcStackArray4<int>.Empty;
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