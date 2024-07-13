using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System.Runtime.CompilerServices;

namespace CSharpBencchmark
{
    /*

| Method                        | HASH_SIZE | Mean         | Error      | StdDev      | Median       |
|------------------------------ |---------- |-------------:|-----------:|------------:|-------------:|
| test_stackalloc               | 16        |     3.768 ns |  0.0939 ns |   0.0832 ns |     3.738 ns |
| test_stackalloc_skiplocalinit | 16        |     1.740 ns |  0.0575 ns |   0.0538 ns |     1.722 ns |
| test_new                      | 16        |     7.733 ns |  0.1771 ns |   0.1657 ns |     7.688 ns |
| test_new_skiplocalinit        | 16        |     7.811 ns |  0.1857 ns |   0.4340 ns |     7.636 ns |
| test_stackalloc               | 256       |    62.107 ns |  0.5821 ns |   0.5445 ns |    61.972 ns |
| test_stackalloc_skiplocalinit | 256       |     1.706 ns |  0.0457 ns |   0.0405 ns |     1.695 ns |
| test_new                      | 256       |    64.330 ns |  0.5373 ns |   0.5026 ns |    64.103 ns |
| test_new_skiplocalinit        | 256       |    66.952 ns |  1.3670 ns |   2.5675 ns |    66.428 ns |
| test_stackalloc               | 1024      |   245.055 ns |  0.5535 ns |   0.4906 ns |   245.139 ns |
| test_stackalloc_skiplocalinit | 1024      |     2.565 ns |  0.0387 ns |   0.0323 ns |     2.559 ns |
| test_new                      | 1024      |   250.567 ns |  3.6778 ns |   3.4402 ns |   249.411 ns |
| test_new_skiplocalinit        | 1024      |   249.414 ns |  3.7567 ns |   3.3302 ns |   248.382 ns |
| test_stackalloc               | 8192      | 2,022.378 ns |  5.2830 ns |   4.6832 ns | 2,022.577 ns |
| test_stackalloc_skiplocalinit | 8192      |    23.011 ns |  0.6715 ns |   1.9798 ns |    23.447 ns |
| test_new                      | 8192      | 1,913.625 ns | 56.4862 ns | 158.3933 ns | 1,963.575 ns |
| test_new_skiplocalinit        | 8192      | 1,748.605 ns | 45.8971 ns | 127.9429 ns | 1,728.251 ns |

    */
    public class Bench_stackalloc
    {
        Consumer _consumer = new();

        [Params(1 << 4, 1 << 8, 1 << 10, 1 << 13)]
        public int HASH_SIZE;

        [Benchmark]
        public void test_stackalloc()
        {
            Span<long> htab = stackalloc long[HASH_SIZE];

            _consumer.Consume(htab[0]);
        }

        [Benchmark]
        [SkipLocalsInit]
        public void test_stackalloc_skiplocalinit()
        {
            Span<long> htab = stackalloc long[HASH_SIZE];

            _consumer.Consume(htab[0]);
        }

        [Benchmark]
        public void test_new()
        {
            Span<long> htab = new long[HASH_SIZE];

            _consumer.Consume(htab[0]);
        }


        [Benchmark]
        [SkipLocalsInit]
        public void test_new_skiplocalinit()
        {
            Span<long> htab = new long[HASH_SIZE];

            _consumer.Consume(htab[0]);
        }
    }
}
