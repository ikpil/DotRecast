using BenchmarkDotNet.Attributes;
using DotRecast.Core.Numerics;
using DotRecast.Detour;

namespace DotRecast.Tool.Benchmark.DotRecast.Detour;

[MemoryDiagnoser]
public class PolygonByCircleConstraintBenchmarks
{
    private readonly IDtPolygonByCircleConstraint _constraint = DtStrictDtPolygonByCircleConstraint.Shared;

    [Params(100, 10000)]
    public int Count;

    [Benchmark]
    public void ShouldHandlePolygonFullyInsideCircle()
    {
        for (int i = 0; i < Count; ++i)
        {
            float[] polygon = { -2, 0, 2, 2, 0, 2, 2, 0, -2, -2, 0, -2 };
            RcVec3f center = new RcVec3f(1, 0, 1);

            _constraint.Apply(polygon, center, 6, out var constrained);
        }
    }
}