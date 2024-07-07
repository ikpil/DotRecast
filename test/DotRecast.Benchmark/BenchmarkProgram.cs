using BenchmarkDotNet.Running;
using DotRecast.Benchmark.Benchmarks;

namespace DotRecast.Benchmark;

public static class BenchmarkProgram
{
    public static int Main(string[] args)
    {
        var summary = BenchmarkRunner.Run([
            //BenchmarkConverter.TypeToBenchmarks(typeof(VectorBenchmarks)),
            //BenchmarkConverter.TypeToBenchmarks(typeof(PriorityQueueBenchmarks)),
            BenchmarkConverter.TypeToBenchmarks(typeof(StackallocBenchmarks)),
        ]);

        return 0;
    }
}