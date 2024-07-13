using BenchmarkDotNet.Running;
using CSharpBencchmark;

BenchmarkRunner.Run([
    //BenchmarkConverter.TypeToBenchmarks(typeof(Bench_PriorityQueue)),
    //BenchmarkConverter.TypeToBenchmarks(typeof(Bench_Math_RcVec3f)),
    BenchmarkConverter.TypeToBenchmarks(typeof(Bench_stackalloc)),
]);
