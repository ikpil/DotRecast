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
        using var rentedArray = RcRentedArray.Shared.Rent<int>(len);
        var array = rentedArray.AsSpan();
        for (int i = 0; i < rentedArray.Length; ++i)
        {
            array[i] = _rand.NextInt32();
        }
    }


    private void RoundForRcFixedArray(int len)
    {
        var array = new RcFixedArray512<int>();
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
        var results = new List<(string title, long ticks)>();
        results.Add(Bench("new int[len]", RoundForArray));
        results.Add(Bench("ArrayPool<int>.Shared.Rent(len)", RoundForPureRentArray));
        results.Add(Bench("RcRentedArray.Shared.Rent<int>(len)", RoundForRcRentedArray));
        results.Add(Bench("new RcFixedArray512<int>()", RoundForRcFixedArray));
        results.Add(Bench("stackalloc int[len]", RoundForStackalloc));

        results.Sort((x, y) => x.ticks.CompareTo(y.ticks));

        foreach (var t in results)
        {
            Console.WriteLine($"{t.title} {t.ticks / (double)TimeSpan.TicksPerMillisecond} ms");
        }
    }

    [Test]
    public void TestSpanVsArray()
    {
        var r = new RcRand();
        var list = new List<(long[] src, long[] dest)>();
        for (int i = 0; i < 14; ++i)
        {
            var s = new long[(int)Math.Pow(2, i + 1)];
            var d = new long[(int)Math.Pow(2, i + 1)];
            for (int ii = 0; ii < s.Length; ++ii)
            {
                s[ii] = r.NextInt32();
            }

            list.Add((s, d));
        }

        var results = new List<(string title, long ticks)>();
        for (int i = 0; i < list.Count; ++i)
        {
            var seq = i;

            Array.Fill(list[seq].dest, 0);
            var resultLong = Bench($"long[{list[seq].src.Length}]", _ =>
            {
                var v = list[seq];
                RcArrays.Copy(v.src, 0, v.dest, 0, v.src.Length);
            });


            Array.Fill(list[seq].dest, 0);
            var resultSpan = Bench($"Span<long[], {list[seq].src.Length}>", _ =>
            {
                var v = list[seq];
                RcSpans.Copy<long>(v.src, 0, v.dest, 0, v.src.Length);
            });


            results.Add(resultLong);
            results.Add(resultSpan);
        }


        int newLine = 0;
        foreach (var t in results)
        {
            Console.WriteLine($"{t.title}: {t.ticks / (double)TimeSpan.TicksPerMillisecond} ms");
            newLine += 1;
            if (0 == (newLine % 2))
            {
                Console.WriteLine("");
            }
        }
    }
}