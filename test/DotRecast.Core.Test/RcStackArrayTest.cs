using System.Collections.Generic;
using DotRecast.Core.Collections;
using NUnit.Framework;

namespace DotRecast.Core.Test;

[Parallelizable]
public class RcStackArrayTest
{
    [Test]
    public void TestRcStackArray()
    {
        var rand = new RcRand();

        // excepted values
        var list = new List<int>();
        for (int i = 0; i < 1024; ++i)
        {
            list.Add(rand.NextInt32());
        }

        {
            RcStackArray4<int> array = RcStackArray4<int>.Empty;
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = list[i];
            }

            for (int i = 0; i < array.Length; ++i)
            {
                Assert.That(array[i], Is.EqualTo(list[i]));
            }
        }

        {
            RcStackArray8<int> array = RcStackArray8<int>.Empty;
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = list[i];
            }

            for (int i = 0; i < array.Length; ++i)
            {
                Assert.That(array[i], Is.EqualTo(list[i]));
            }
        }
    }

    public void Test<T>(T a)
    {
        T s = a;
    }
}