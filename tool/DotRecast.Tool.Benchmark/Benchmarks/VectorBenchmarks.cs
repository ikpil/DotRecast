using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DotRecast.Core.Numerics;

namespace DotRecast.Tool.Benchmark.Benchmarks;

/*
| Method            | Mean      | Error     | StdDev    |
|------------------ |----------:|----------:|----------:|
| Dot_Vector3       | 0.6395 ns | 0.0125 ns | 0.0104 ns |
| Dot_RcVec3f       | 0.2275 ns | 0.0281 ns | 0.0375 ns |
| Cross_Vector3     | 1.1652 ns | 0.0102 ns | 0.0085 ns |
| Cross_RcVec3f     | 1.1687 ns | 0.0140 ns | 0.0124 ns |
| Normalize_Vector3 | 1.7964 ns | 0.0173 ns | 0.0162 ns |
| Normalize_RcVec3f | 1.2806 ns | 0.0088 ns | 0.0078 ns |
*/

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
        var v1 = new RcVec3f(1, 2, 3);
        var v2 = new RcVec3f(1, 2, 3);
        var v = RcVec3f.Dot(v1, v2);
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
        var v1 = new RcVec3f(1, 2, 3);
        var v2 = new RcVec3f(1, 2, 3);
        var v = RcVec3f.Cross(v1, v2);
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
        var v1 = new RcVec3f(1, 2, 3);
        var v = RcVec3f.Normalize(v1);
        _consumer.Consume(v);
    }
}