using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DotRecast.Core.Buffers;

namespace DotRecast.Tool.Benchmark.DotRecast.Core;

/*

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2605)
AMD Ryzen 5 3600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2


| Method     | HashTableSize | Mean         | Error      | StdDev     | Median       | Gen0   | Allocated |
|----------- |-------------- |-------------:|-----------:|-----------:|-------------:|-------:|----------:|
| New        | 16            |     5.842 ns |  0.0500 ns |  0.0443 ns |     5.849 ns | 0.0182 |     152 B |
| Stackalloc | 16            |     4.142 ns |  0.0831 ns |  0.0777 ns |     4.112 ns |      - |         - |
| Rent       | 16            |    16.409 ns |  0.0244 ns |  0.0204 ns |    16.399 ns |      - |         - |
| New        | 256           |    50.550 ns |  1.0255 ns |  2.8245 ns |    49.036 ns | 0.2477 |    2072 B |
| Stackalloc | 256           |    65.315 ns |  0.2746 ns |  0.2293 ns |    65.259 ns |      - |         - |
| Rent       | 256           |    39.722 ns |  0.0638 ns |  0.0597 ns |    39.734 ns |      - |         - |
| New        | 1024          |   285.897 ns | 22.9065 ns | 67.5402 ns |   303.147 ns | 0.9813 |    8216 B |
| Stackalloc | 1024          |   261.509 ns |  0.3847 ns |  0.3410 ns |   261.528 ns |      - |         - |
| Rent       | 1024          |    87.780 ns |  0.1627 ns |  0.1359 ns |    87.752 ns |      - |         - |
| New        | 8192          | 1,156.367 ns |  9.6633 ns |  9.0390 ns | 1,156.284 ns | 7.8125 |   65560 B |
| Stackalloc | 8192          | 2,134.754 ns |  5.4929 ns |  4.8693 ns | 2,134.541 ns |      - |         - |
| Rent       | 8192          |   582.443 ns |  1.0532 ns |  0.9336 ns |   582.631 ns |      - |         - |

*/

[MemoryDiagnoser]
public class ArrayBenchmarks
{
    private readonly Consumer _consumer = new();

    [Params(1 << 4, 1 << 8, 1 << 10, 1 << 13)]
    public int HashTableSize;

    [Benchmark]
    public void New()
    {
        Span<long> hashTable = new long[HashTableSize];
        _consumer.Consume(hashTable[0]);
    }

    [Benchmark]
    public void Stackalloc()
    {
        Span<long> hashTable = stackalloc long[HashTableSize];
        _consumer.Consume(hashTable[0]);
    }

    [Benchmark]
    public void Rent()
    {
        using var hashTable = RcRentedArray.Shared.Rent<long>(HashTableSize);
        _consumer.Consume(hashTable[0]);
    }


    // [Benchmark]
    // [SkipLocalsInit]
    // public void Stackalloc_Long_SkipLocalsInit()
    // {
    //     Span<long> hashTable = stackalloc long[HashTableSize];
    //
    //     _consumer.Consume(hashTable[0]);
    // }


    // [Benchmark]
    // [SkipLocalsInit]
    // public void New_Long_SkipLocalsInit()
    // {
    //     Span<long> hashTable = new long[HashTableSize];
    //
    //     _consumer.Consume(hashTable[0]);
    // }
}