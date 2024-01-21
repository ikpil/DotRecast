using System;
using System.Collections.Generic;
using DotRecast.Core.Buffers;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcRentedArrayTest
{
    public List<int> RandomValues(int length)
    {
        var rand = new RcRand();

        // excepted values
        var list = new List<int>();
        for (int i = 0; i < length; ++i)
        {
            list.Add(rand.NextInt32());
        }

        return list;
    }

    [Test]
    public void TestRentedArray()
    {
        var rand = new RcRand();
        for (int loop = 0; loop < 1024; ++loop)
        {
            RcRentedArray<int> rentedArray;
            {
                int length = (int)(rand.Next() * 2048);
                var values = RandomValues(length);
                using var array = RcRentedArray.RentDisposableArray<int>(length);

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
                
                // danger
                rentedArray = array;
            }
            Assert.Throws<NullReferenceException>(() => rentedArray[^1] = 0);
        }
    }
}