using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace DotRecast.Benchmark.Benchmarks;

/*

| Method                         | TestArraySize | Mean         | Error      | StdDev      | Median       |
|------------------------------- |-------------- |-------------:|-----------:|------------:|-------------:|
| Stackalloc_Long                | 16            |     3.016 ns |  0.0179 ns |   0.0149 ns |     3.017 ns |
| Stackalloc_Long_SkipLocalsInit | 16            |     2.265 ns |  0.0197 ns |   0.0184 ns |     2.258 ns |
| New_Long                       | 16            |     5.917 ns |  0.1964 ns |   0.5634 ns |     5.761 ns |
| New_Long_SkipLocalsInit        | 16            |     5.703 ns |  0.1371 ns |   0.3935 ns |     5.661 ns |
| Stackalloc_Long                | 256           |    39.418 ns |  0.2737 ns |   0.2285 ns |    39.410 ns |
| Stackalloc_Long_SkipLocalsInit | 256           |     2.274 ns |  0.0147 ns |   0.0131 ns |     2.274 ns |
| New_Long                       | 256           |    53.901 ns |  2.9999 ns |   8.4614 ns |    51.449 ns |
| New_Long_SkipLocalsInit        | 256           |    53.480 ns |  1.8716 ns |   5.4298 ns |    51.858 ns |
| Stackalloc_Long                | 1024          |   137.037 ns |  0.3652 ns |   0.3416 ns |   137.031 ns |
| Stackalloc_Long_SkipLocalsInit | 1024          |     3.669 ns |  0.0254 ns |   0.0226 ns |     3.668 ns |
| New_Long                       | 1024          |   197.324 ns |  9.2795 ns |  27.0687 ns |   186.588 ns |
| New_Long_SkipLocalsInit        | 1024          |   210.996 ns | 10.0255 ns |  27.9471 ns |   206.110 ns |
| Stackalloc_Long                | 8192          | 1,897.989 ns |  7.1814 ns |   5.9968 ns | 1,897.814 ns |
| Stackalloc_Long_SkipLocalsInit | 8192          |    20.598 ns |  0.2645 ns |   0.2344 ns |    20.572 ns |
| New_Long                       | 8192          | 1,324.061 ns | 39.8447 ns | 116.2288 ns | 1,298.794 ns |
| New_Long_SkipLocalsInit        | 8192          | 1,305.211 ns | 35.1855 ns | 102.0796 ns | 1,295.539 ns |
*/

public class StackallocBenchmarks
{
    private readonly Consumer _consumer = new();

    [Params(1 << 4, 1 << 8, 1 << 10, 1 << 13)]
    public int HashTableSize;

    [Benchmark]
    public void Stackalloc_Long()
    {
        Span<long> hashTable = stackalloc long[HashTableSize];

        _consumer.Consume(hashTable[0]);
    }

    [Benchmark]
    [SkipLocalsInit]
    public void Stackalloc_Long_SkipLocalsInit()
    {
        Span<long> hashTable = stackalloc long[HashTableSize];

        _consumer.Consume(hashTable[0]);
    }

    [Benchmark]
    public void New_Long()
    {
        Span<long> hashTable = new long[HashTableSize];

        _consumer.Consume(hashTable[0]);
    }


    [Benchmark]
    [SkipLocalsInit]
    public void New_Long_SkipLocalsInit()
    {
        Span<long> hashTable = new long[HashTableSize];

        _consumer.Consume(hashTable[0]);
    }
}