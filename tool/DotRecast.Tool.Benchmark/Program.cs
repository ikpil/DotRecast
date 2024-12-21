using System;
using BenchmarkDotNet.Running;
using DotRecast.Tool.Benchmark.Benchmarks;

namespace DotRecast.Tool.Benchmark;

public static class Program
{
    public static int Main(string[] args)
    {
        Type[] benchmarkTypes =
        [
            typeof(VectorBenchmarks),
            typeof(PriorityQueueBenchmarks),
            typeof(ArrayBenchmarks)
        ];

        var switcher = new BenchmarkSwitcher(benchmarkTypes);

        if (args == null || args.Length == 0)
        {
            switcher.RunAll();
        }
        else
        {
            switcher.Run(args);
        }

        return 0;
    }
}