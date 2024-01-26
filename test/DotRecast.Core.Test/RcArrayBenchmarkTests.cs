using System;
using System.Buffers;
using DotRecast.Core.Buffers;
using DotRecast.Core.Collections;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcArrayBenchmarkTests
{
    private const int StepLength = 256;
    private const int RandomLoop = 5000;
    private readonly RcRand _rand = new RcRand();

    [Test]
    public void BenchmarkArray()
    {
        for (int step = StepLength; step > 0; --step)
        {
            var begin = RcFrequency.Ticks;
            for (int i = 0; i < RandomLoop; ++i)
            {
                RoundForArray(step);
            }

            var end = RcFrequency.Ticks - begin;
            Console.WriteLine($"array - {step}: {end} ticks");
        }
    }

    private void RoundForArray(int len)
    {
        var array = new int[len];
        for (int ii = 0; ii < len; ++ii)
        {
            array[ii] = _rand.NextInt32();
        }
    }

    [Test]
    public void BenchmarkRentArray()
    {
        for (int step = StepLength; step > 0; --step)
        {
            var begin = RcFrequency.Ticks;
            for (int i = 0; i < RandomLoop; ++i)
            {
                RoundForRentArray(step);
            }

            var end = RcFrequency.Ticks - begin;
            Console.WriteLine($"rent array - {step}: {end} ticks");
        }
    }

    private void RoundForRentArray(int len)
    {
        var array = ArrayPool<int>.Shared.Rent(len);
        for (int ii = 0; ii < array.Length; ++ii)
        {
            array[ii] = _rand.NextInt32();
        }
        ArrayPool<int>.Shared.Return(array);
    }

    [Test]
    public void BenchmarkRentedArray()
    {
        for (int step = StepLength; step > 0; --step)
        {
            var begin = RcFrequency.Ticks;
            for (int i = 0; i < RandomLoop; ++i)
            {
                RoundForRentedArray(step);
            }

            var end = RcFrequency.Ticks - begin;
            Console.WriteLine($"rented array - {step}: {end} ticks");
        }
    }

    private void RoundForRentedArray(int len)
    {
        using var array = RcRentedArray.RentDisposableArray<int>(len);
        for (int ii = 0; ii < array.Length; ++ii)
        {
            array.RentedArray[ii] = _rand.NextInt32();
        }
    }


    [Test]
    public void BenchmarkStackArray()
    {
        for (int step = StepLength; step > 0; --step)
        {
            var begin = RcFrequency.Ticks;
            for (int i = 0; i < RandomLoop; ++i)
            {
                RoundForStackArray(step);
            }

            var end = RcFrequency.Ticks - begin;
            Console.WriteLine($"stack array - {step}: {end} ticks");
        }
    }

    private void RoundForStackArray(int len)
    {
        var array = new RcStackArray256<int>();
        for (int ii = 0; ii < len; ++ii)
        {
            array[ii] = _rand.NextInt32();
        }
    }
}