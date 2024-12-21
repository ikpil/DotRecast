using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Running;
using DotRecast.Tool.Benchmark.Benchmarks;

namespace DotRecast.Tool.Benchmark;

public static class BenchmarkProgram
{
    public static int Main(string[] args)
    {
        var runs = ImmutableArray.Create(
            // BenchmarkConverter.TypeToBenchmarks(typeof(VectorBenchmarks)),
            // BenchmarkConverter.TypeToBenchmarks(typeof(PriorityQueueBenchmarks)),
            BenchmarkConverter.TypeToBenchmarks(typeof(ArrayBenchmarks))
        );

        var summary = BenchmarkRunner.Run(runs.ToArray());

        return 0;
    }
}