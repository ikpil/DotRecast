using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DotRecast.Core.Collections;

namespace DotRecast.Tool.Benchmark.DotRecast.Core;

/*
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3624)
AMD Ryzen 5 5625U, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.200
  [Host]     : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2


| Method                                    | Count | Mean             | Error           | StdDev          | Median           | Gen0    | Gen1    | Gen2    | Allocated |
|------------------------------------------ |------ |-----------------:|----------------:|----------------:|-----------------:|--------:|--------:|--------:|----------:|
| Enqueue_RcSortedQueue                     | 100   |         873.2 ns |        17.43 ns |        29.60 ns |         872.6 ns |  0.5589 |  0.0076 |       - |   4.57 KB |
| Enqueue_RcBinaryMinHeap                   | 100   |       2,172.9 ns |        43.31 ns |       101.24 ns |       2,176.6 ns |  0.3929 |  0.0038 |       - |   3.21 KB |
| Enqueue_PriorityQueue                     | 100   |       1,220.7 ns |        23.36 ns |        59.89 ns |       1,217.6 ns |  0.4883 |  0.0057 |       - |      4 KB |
| EnqueueAll_DequeueAll_RcSortedQueue       | 100   |       5,493.5 ns |       504.63 ns |     1,487.91 ns |       4,680.8 ns |  0.5569 |       - |       - |   4.57 KB |
| EnqueueAll_DequeueAll_RcBinaryMinHeap     | 100   |       6,625.9 ns |       122.18 ns |       120.00 ns |       6,636.7 ns |  0.3967 |       - |       - |   3.24 KB |
| EnqueueAll_DequeueAll_PriorityQueue       | 100   |       6,329.1 ns |       121.57 ns |       124.84 ns |       6,344.6 ns |  0.4883 |       - |       - |      4 KB |
| EnqueueDequeue_RcSortedQueue              | 100   |      38,617.2 ns |       765.68 ns |       678.75 ns |      38,637.2 ns |  0.4272 |       - |       - |   3.55 KB |
| EnqueueDequeue_RcBinaryMinHeap            | 100   |       4,646.3 ns |        46.78 ns |        39.06 ns |       4,649.7 ns |  0.3967 |       - |       - |   3.24 KB |
| EnqueueDequeue_PriorityQueue              | 100   |       3,891.3 ns |        77.75 ns |        95.48 ns |       3,886.7 ns |  0.4883 |       - |       - |      4 KB |
| Enqueue_RcSortedQueue                     | 10000 |     247,050.4 ns |     4,529.12 ns |     3,782.02 ns |     246,733.4 ns | 83.0078 | 82.5195 | 41.5039 | 490.78 KB |
| Enqueue_RcBinaryMinHeap                   | 10000 |     281,605.3 ns |     5,517.52 ns |     5,161.09 ns |     282,650.0 ns | 38.0859 | 24.9023 |       - | 312.59 KB |
| Enqueue_PriorityQueue                     | 10000 |     279,167.6 ns |     5,559.77 ns |    11,848.32 ns |     282,628.6 ns | 49.8047 | 49.8047 | 49.8047 | 390.74 KB |
| EnqueueAll_DequeueAll_RcSortedQueue       | 10000 |   1,145,484.0 ns |    13,429.18 ns |    12,561.66 ns |   1,145,563.3 ns | 82.0313 | 80.0781 | 41.0156 | 490.78 KB |
| EnqueueAll_DequeueAll_RcBinaryMinHeap     | 10000 |   2,011,314.3 ns |    14,635.49 ns |    12,221.30 ns |   2,012,173.4 ns | 35.1563 | 11.7188 |       - | 312.62 KB |
| EnqueueAll_DequeueAll_PriorityQueue       | 10000 |   2,049,687.6 ns |    13,863.42 ns |    12,967.85 ns |   2,052,782.4 ns | 46.8750 | 46.8750 | 46.8750 | 390.74 KB |
| EnqueueDequeue_RcSortedQueue              | 10000 | 889,071,578.6 ns | 4,148,761.86 ns | 3,677,769.33 ns | 888,254,250.0 ns |       - |       - |       - | 363.17 KB |
| EnqueueDequeue_RcBinaryMinHeap            | 10000 |   1,140,019.5 ns |    10,311.67 ns |     9,141.02 ns |   1,140,033.8 ns | 37.1094 | 17.5781 |       - | 312.62 KB |
| EnqueueDequeue_PriorityQueue              | 10000 |   1,183,681.4 ns |     6,984.25 ns |     6,191.35 ns |   1,181,323.3 ns | 48.8281 | 48.8281 | 48.8281 | 390.74 KB |

*/

public class TotalNode
{
    public int Id;
    public float Total;
}

[MemoryDiagnoser]
public class PriorityQueueBenchmarks
{
    [Params(100, 10000)] public int Count;

    private float[] _priority;

    private static int Comp(TotalNode x, TotalNode y)
    {
        var v = x.Total.CompareTo(y.Total);
        if (v != 0)
            return v;
        return x.Id.CompareTo(y.Id);
    }


    [GlobalSetup]
    public void Setup()
    {
        _priority = new float[Count];
        for (int i = 0; i < Count; i++)
        {
            _priority[i] = (float)Random.Shared.NextDouble() * 100f;
        }
    }

    [Benchmark]
    public void Enqueue_RcSortedQueue()
    {
        RcSortedQueue<TotalNode> sq = new RcSortedQueue<TotalNode>(Comp);
        for (int i = 0; i < Count; i++)
        {
            sq.Enqueue(new TotalNode
            {
                Id = i,
                Total = _priority[i],
            });
        }
    }

    [Benchmark]
    public void Enqueue_RcBinaryMinHeap()
    {
        RcBinaryMinHeap<TotalNode> bmHeap = new RcBinaryMinHeap<TotalNode>(Count, Comp);
        for (int i = 0; i < Count; i++)
        {
            bmHeap.Push(new TotalNode
            {
                Id = i,
                Total = _priority[i],
            });
        }
    }

    [Benchmark]
    public void Enqueue_PriorityQueue()
    {
        PriorityQueue<TotalNode, TotalNode> pg = new PriorityQueue<TotalNode, TotalNode>(Count, Comparer<TotalNode>.Create(Comp));
        for (int i = 0; i < Count; i++)
        {
            var node = new TotalNode
            {
                Id = i,
                Total = _priority[i],
            };
            pg.Enqueue(node, node);
        }
    }

    [Benchmark]
    public void EnqueueAll_DequeueAll_RcSortedQueue()
    {
        RcSortedQueue<TotalNode> sq = new RcSortedQueue<TotalNode>(Comp);
        for (int i = 0; i < Count; i++)
        {
            sq.Enqueue(new TotalNode
            {
                Id = i,
                Total = _priority[i],
            });
        }

        while (sq.Count() > 0)
        {
            sq.Dequeue();
        }
    }

    [Benchmark]
    public void EnqueueAll_DequeueAll_RcBinaryMinHeap()
    {
        RcBinaryMinHeap<TotalNode> bmHeap = new RcBinaryMinHeap<TotalNode>(Count, Comp);
        for (int i = 0; i < Count; i++)
        {
            bmHeap.Push(new TotalNode
            {
                Id = i,
                Total = _priority[i],
            });
        }

        while (bmHeap.Count > 0)
        {
            bmHeap.Pop();
        }
    }

    [Benchmark]
    public void EnqueueAll_DequeueAll_PriorityQueue()
    {
        PriorityQueue<TotalNode, TotalNode> pq = new PriorityQueue<TotalNode, TotalNode>(Count, Comparer<TotalNode>.Create(Comp));
        for (int i = 0; i < Count; i++)
        {
            var node = new TotalNode
            {
                Id = i,
                Total = _priority[i],
            };
            pq.Enqueue(node, node);
        }

        while (pq.Count > 0)
        {
            pq.Dequeue();
        }
    }


    [Benchmark]
    public void EnqueueDequeue_RcSortedQueue()
    {
        RcSortedQueue<TotalNode> sq = new RcSortedQueue<TotalNode>(Comp);
        int half = Count / 2;
        for (int i = 0; i < half; i++)
        {
            sq.Enqueue(new TotalNode
            {
                Id = i,
                Total = _priority[i],
            });
        }

        for (int i = half; i < Count; i++)
        {
            sq.Enqueue(new TotalNode
            {
                Id = i,
                Total = _priority[i],
            });

            sq.Dequeue();
        }
    }

    [Benchmark]
    public void EnqueueDequeue_RcBinaryMinHeap()
    {
        RcBinaryMinHeap<TotalNode> bmHeap = new RcBinaryMinHeap<TotalNode>(Count, Comp);
        int half = Count / 2;
        for (int i = 0; i < half; i++)
        {
            bmHeap.Push(new TotalNode
            {
                Id = i,
                Total = _priority[i],
            });
        }

        for (int i = half; i < Count; i++)
        {
            bmHeap.Push(new TotalNode
            {
                Id = i,
                Total = _priority[i],
            });

            bmHeap.Pop();
        }
    }

    [Benchmark]
    public void EnqueueDequeue_PriorityQueue()
    {
        PriorityQueue<TotalNode, TotalNode> pq = new PriorityQueue<TotalNode, TotalNode>(Count, Comparer<TotalNode>.Create(Comp));
        int half = Count / 2;
        for (int i = 0; i < half; i++)
        {
            var node = new TotalNode
            {
                Id = i,
                Total = _priority[i],
            };
            pq.Enqueue(node, node);
        }

        for (int i = half; i < Count; i++)
        {
            var node = new TotalNode
            {
                Id = i,
                Total = _priority[i],
            };
            pq.Enqueue(node, node);
            pq.Dequeue();
        }
    }
}