using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Tool.Benchmark.DotRecast.Core;

/*
 
// * Summary *
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2605)
AMD Ryzen 5 5625U with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2


| Method            | Mean      | Error     | StdDev    | Allocated |
|------------------ |----------:|----------:|----------:|----------:|
| Dot_Vector3       | 0.4205 ns | 0.0023 ns | 0.0020 ns |         - |
| Dot_RcVec3f       | 0.0560 ns | 0.0164 ns | 0.0145 ns |         - |
| Cross_Vector3     | 1.3648 ns | 0.0406 ns | 0.0380 ns |         - |
| Cross_RcVec3f     | 1.2007 ns | 0.0279 ns | 0.0233 ns |         - |
| Normalize_Vector3 | 1.4201 ns | 0.0210 ns | 0.0186 ns |         - |
| Normalize_RcVec3f | 1.1737 ns | 0.0090 ns | 0.0084 ns |         - |

*/

[MemoryDiagnoser]
public class VectorBenchmarks
{
    private readonly Consumer _consumer = new();

    [Benchmark]
    public void Dot_Vector3()
    {
        var v1 = new System.Numerics.Vector3(1, 2, 3);
        var v2 = new System.Numerics.Vector3(1, 2, 3);
        var v = System.Numerics.Vector3.Dot(v1, v2);
        _consumer.Consume(v);
    }

    [Benchmark]
    public void Dot_RcVec3f()
    {
        var v1 = new Vector3(1, 2, 3);
        var v2 = new Vector3(1, 2, 3);
        var v = Vector3.Dot(v1, v2);
        _consumer.Consume(v);
    }

    [Benchmark]
    public void Cross_Vector3()
    {
        var v1 = new System.Numerics.Vector3(1, 2, 3);
        var v2 = new System.Numerics.Vector3(1, 2, 3);
        var v = System.Numerics.Vector3.Cross(v1, v2);
        _consumer.Consume(v);
    }

    [Benchmark]
    public void Cross_RcVec3f()
    {
        var v1 = new Vector3(1, 2, 3);
        var v2 = new Vector3(1, 2, 3);
        var v = Vector3.Cross(v1, v2);
        _consumer.Consume(v);
    }

    [Benchmark]
    public void Normalize_Vector3()
    {
        var v1 = new System.Numerics.Vector3(1, 2, 3);
        var v = System.Numerics.Vector3.Normalize(v1);
        _consumer.Consume(v);
    }

    [Benchmark]
    public void Normalize_RcVec3f()
    {
        var v1 = new Vector3(1, 2, 3);
        var v = Vector3.Normalize(v1);
        _consumer.Consume(v);
    }
}