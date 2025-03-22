using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DotRecast.Core.Buffers;
using DotRecast.Core.Collections;

namespace DotRecast.Tool.Benchmark.DotRecast.Core;

/*

// * Summary *

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3476)
AMD Ryzen 5 3600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.101
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2


| Method         | count | Mean       | Error     | StdDev     | Median     | Gen0   | Allocated |
|--------------- |------ |-----------:|----------:|-----------:|-----------:|-------:|----------:|
| New            | 16    |   5.736 ns | 0.0189 ns |  0.0147 ns |   5.739 ns | 0.0182 |     152 B |
| Stackalloc     | 16    |   4.089 ns | 0.0569 ns |  0.0532 ns |   4.069 ns |      - |         - |
| Rent           | 16    |  16.687 ns | 0.0949 ns |  0.0887 ns |  16.674 ns |      - |         - |
| FixedArray16   | 16    |   1.221 ns | 0.0051 ns |  0.0048 ns |   1.220 ns |      - |         - |
| New            | 256   |  49.760 ns | 0.2309 ns |  0.5707 ns |  49.697 ns | 0.2477 |    2072 B |
| Stackalloc     | 256   |  66.572 ns | 0.1490 ns |  0.1321 ns |  66.589 ns |      - |         - |
| Rent           | 256   |  42.986 ns | 0.2254 ns |  0.2109 ns |  42.985 ns |      - |         - |
| FixedArray256  | 256   |  32.173 ns | 0.0352 ns |  0.0312 ns |  32.180 ns |      - |         - |
| New            | 512   |  95.287 ns | 0.2699 ns |  0.6414 ns |  95.150 ns | 0.4923 |    4120 B |
| Stackalloc     | 512   | 131.808 ns | 0.2255 ns |  0.2109 ns | 131.848 ns |      - |         - |
| Rent           | 512   |  58.828 ns | 0.0965 ns |  0.0855 ns |  58.826 ns |      - |         - |
| FixedArray512  | 512   |  65.193 ns | 0.2332 ns |  0.2182 ns |  65.256 ns |      - |         - |
| New            | 1024  | 189.911 ns | 3.8249 ns | 10.2753 ns | 184.212 ns | 0.9813 |    8216 B |
| Stackalloc     | 1024  | 264.237 ns | 0.6615 ns |  0.5864 ns | 264.234 ns |      - |         - |
| Rent           | 1024  |  91.853 ns | 0.1526 ns |  0.1352 ns |  91.847 ns |      - |         - |
| FixedArray1024 | 1024  | 132.748 ns | 0.6387 ns |  0.5974 ns | 132.429 ns |      - |         - |



*/

[MemoryDiagnoser]
public class ArrayBenchmarks
{
    private readonly Consumer _consumer = new();

    [Benchmark]
    [Arguments(16)]
    [Arguments(256)]
    [Arguments(512)]
    [Arguments(1024)]
    public void New(int count)
    {
        Span<long> hashTable = new long[count];
        _consumer.Consume(hashTable[0]);
    }

    [Benchmark]
    [Arguments(16)]
    [Arguments(256)]
    [Arguments(512)]
    [Arguments(1024)]
    public void Stackalloc(int count)
    {
        Span<long> hashTable = stackalloc long[count];
        _consumer.Consume(hashTable[0]);
    }

    [Benchmark]
    [Arguments(16)]
    [Arguments(256)]
    [Arguments(512)]
    [Arguments(1024)]
    public void Rent(int count)
    {
        using var hashTable = RcRentedArray.Shared.Rent<long>(count);
        _consumer.Consume(hashTable[0]);
    }

    [Benchmark]
    [Arguments(16)]
    public void FixedArray16(int count)
    {
        RcFixedArray16<long> hashTable = new RcFixedArray16<long>();
        var tableSpan = hashTable.AsSpan();
        _consumer.Consume(tableSpan[0]);
    }

    [Benchmark]
    [Arguments(256)]
    public void FixedArray256(int count)
    {
        RcFixedArray256<long> hashTable = new RcFixedArray256<long>();
        var tableSpan = hashTable.AsSpan();
        _consumer.Consume(tableSpan[0]);
    }

    [Benchmark]
    [Arguments(512)]
    public void FixedArray512(int count)
    {
        RcFixedArray512<long> hashTable = new RcFixedArray512<long>();
        var tableSpan = hashTable.AsSpan();
        _consumer.Consume(tableSpan[0]);
    }

    [Benchmark]
    [Arguments(1024)]
    public void FixedArray1024(int count)
    {
        RcFixedArray1024<long> hashTable = new RcFixedArray1024<long>();
        var tableSpan = hashTable.AsSpan();
        _consumer.Consume(tableSpan[0]);
    }
}