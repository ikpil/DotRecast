using BenchmarkDotNet.Running;
using CSharpBencchmark;

BenchmarkRunner.Run([
    //BenchmarkConverter.TypeToBenchmarks(typeof(Bench_PriorityQueue)),
    BenchmarkConverter.TypeToBenchmarks(typeof(Bench_Math_RcVec3f)),
]);
