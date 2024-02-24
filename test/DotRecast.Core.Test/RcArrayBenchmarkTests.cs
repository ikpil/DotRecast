using System;
using System.Buffers;
using System.Collections.Generic;
using DotRecast.Core.Buffers;
using DotRecast.Core.Collections;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcArrayBenchmarkTests
{
    private const int StepLength = 512;
    private const int RandomLoop = 1000;
    private readonly RcRand _rand = new RcRand();

    private (string title, long ticks) Bench(string title, Action<int> source)
    {
        var begin = RcFrequency.Ticks;
        for (int step = StepLength; step > 0; --step)
        {
            for (int i = 0; i < RandomLoop; ++i)
            {
                source.Invoke(step);
            }
        }

        var end = RcFrequency.Ticks - begin;
        return (title, end);
    }


    private void RoundForArray(int len)
    {
        var array = new int[len];
        for (int ii = 0; ii < len; ++ii)
        {
            array[ii] = _rand.NextInt32();
        }
    }


    private void RoundForPureRentArray(int len)
    {
        var array = ArrayPool<int>.Shared.Rent(len);
        for (int ii = 0; ii < array.Length; ++ii)
        {
            array[ii] = _rand.NextInt32();
        }

        ArrayPool<int>.Shared.Return(array);
    }


    private void RoundForRcRentedArray(int len)
    {
        using var rentedArray = RcRentedArray.Rent<int>(len);
        var array = rentedArray.AsArray();
        for (int i = 0; i < rentedArray.Length; ++i)
        {
            array[i] = _rand.NextInt32();
        }
    }


    private void RoundForRcStackArray(int len)
    {
        var array = new RcStackArray512<int>();
        for (int ii = 0; ii < len; ++ii)
        {
            array[ii] = _rand.NextInt32();
        }
    }

    private void RoundForStackalloc(int len)
    {
        Span<int> array = stackalloc int[len];
        for (int ii = 0; ii < len; ++ii)
        {
            array[ii] = _rand.NextInt32();
        }
    }


    [Test]
    public void TestBenchmarkArrays()
    {
        var list = new List<(string title, long ticks)>();
        list.Add(Bench("new int[len]", RoundForArray));
        list.Add(Bench("ArrayPool<int>.Shared.Rent(len)", RoundForPureRentArray));
        list.Add(Bench("RcRentedArray.Rent<int>(len)", RoundForRcRentedArray));
        list.Add(Bench("new RcStackArray512<int>()", RoundForRcStackArray));
        list.Add(Bench("stackalloc int[len]", RoundForStackalloc));

        list.Sort((x, y) => x.ticks.CompareTo(y.ticks));

        foreach (var t in list)
        {
            Console.WriteLine($"{t.title} {t.ticks / (double)TimeSpan.TicksPerMillisecond} ms");
        }
    }
}