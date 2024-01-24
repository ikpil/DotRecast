using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DotRecast.Core.Buffers;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
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
                int length = Math.Max(2, (int)(rand.Next() * 2048));
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

    [Test]
    public void TestSame()
    {
        // not same
        {
            using var r1 = RcRentedArray.RentDisposableArray<float>(1024);
            using var r2 = RcRentedArray.RentDisposableArray<float>(1024);

            Assert.That(r2.AsRentedArray() != r1.AsRentedArray(), Is.EqualTo(true));
        }

        // same
        {
            // error case 
            float[] r1Array;
            using (var r1 = RcRentedArray.RentDisposableArray<float>(1024))
            {
                r1Array = r1.AsRentedArray();
                for (int i = 0; i < r1.Length; ++i)
                {
                    r1[i] = 123;
                }
            }

            using var r2 = RcRentedArray.RentDisposableArray<float>(1024);

            Assert.That(r2.AsRentedArray() == r1Array, Is.EqualTo(true));
            Assert.That(r2.AsRentedArray().Sum(), Is.EqualTo(0));
        }
    }

    [Test]
    public void TestDispose()
    {
        var r1 = RcRentedArray.RentDisposableArray<float>(1024);
        for (int i = 0; i < r1.Length; ++i)
        {
            r1[i] = 123;
        }

        Assert.That(r1.IsDisposed, Is.EqualTo(false));
        r1.Dispose();
        Assert.That(r1.IsDisposed, Is.EqualTo(true));
        Assert.That(r1.AsRentedArray(), Is.Null);
    }
}