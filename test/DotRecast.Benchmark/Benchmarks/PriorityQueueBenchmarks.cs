using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DotRecast.Core.Collections;

namespace DotRecast.Benchmark.Benchmarks;

/*

// * Summary *
BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3958/23H2/2023Update/SunValley3)
AMD Ryzen 5 3600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2

| Method                         | Count | Mean                | Error            | StdDev           |
|------------------------------- |------ |--------------------:|-----------------:|-----------------:|
| Enqueue_RcSortedQueue          | 10    |            87.49 ns |         0.774 ns |         0.724 ns |
| Enqueue_RcBinaryMinHeap        | 10    |           185.23 ns |         1.730 ns |         1.533 ns |
| Enqueue_PriorityQueue          | 10    |           202.95 ns |         1.611 ns |         1.428 ns |
| DequeueAll_RcSortedQueue       | 10    |           460.97 ns |         2.169 ns |         2.029 ns |
| DequeueAll_RcBinaryMinHeap     | 10    |           573.17 ns |         2.542 ns |         2.378 ns |
| DequeueAll_PriorityQueue       | 10    |           500.68 ns |         2.364 ns |         2.212 ns |
| EnqueueDequeue_RcSortedQueue   | 10    |           525.43 ns |         1.842 ns |         1.632 ns |
| EnqueueDequeue_RcBinaryMinHeap | 10    |           455.65 ns |         2.410 ns |         2.254 ns |
| EnqueueDequeue_PriorityQueue   | 10    |           381.82 ns |         6.036 ns |         5.646 ns |
| Enqueue_RcSortedQueue          | 100   |           730.57 ns |         5.229 ns |         4.635 ns |
| Enqueue_RcBinaryMinHeap        | 100   |         3,012.15 ns |        10.875 ns |         9.640 ns |
| Enqueue_PriorityQueue          | 100   |         2,306.80 ns |        26.694 ns |        23.663 ns |
| DequeueAll_RcSortedQueue       | 100   |         6,241.67 ns |        31.856 ns |        29.798 ns |
| DequeueAll_RcBinaryMinHeap     | 100   |        13,692.29 ns |        38.829 ns |        34.421 ns |
| DequeueAll_PriorityQueue       | 100   |        12,482.93 ns |        93.955 ns |        87.886 ns |
| EnqueueDequeue_RcSortedQueue   | 100   |        64,002.79 ns |       316.081 ns |       280.197 ns |
| EnqueueDequeue_RcBinaryMinHeap | 100   |         8,655.79 ns |        23.703 ns |        22.172 ns |
| EnqueueDequeue_PriorityQueue   | 100   |         7,806.20 ns |       105.801 ns |        98.967 ns |
| Enqueue_RcSortedQueue          | 1000  |         7,566.23 ns |       149.010 ns |       218.418 ns |
| Enqueue_RcBinaryMinHeap        | 1000  |        36,277.43 ns |        96.710 ns |        90.462 ns |
| Enqueue_PriorityQueue          | 1000  |        28,564.19 ns |       186.866 ns |       174.795 ns |
| DequeueAll_RcSortedQueue       | 1000  |       108,574.26 ns |       745.459 ns |       697.303 ns |
| DequeueAll_RcBinaryMinHeap     | 1000  |       210,346.25 ns |       332.478 ns |       311.000 ns |
| DequeueAll_PriorityQueue       | 1000  |       189,536.32 ns |     1,180.045 ns |     1,046.079 ns |
| EnqueueDequeue_RcSortedQueue   | 1000  |     8,957,965.42 ns |    45,715.567 ns |    42,762.370 ns |
| EnqueueDequeue_RcBinaryMinHeap | 1000  |       131,615.02 ns |       394.216 ns |       368.750 ns |
| EnqueueDequeue_PriorityQueue   | 1000  |       114,799.89 ns |     1,269.621 ns |     1,060.191 ns |
| Enqueue_RcSortedQueue          | 10000 |        77,143.76 ns |       996.372 ns |       932.007 ns |
| Enqueue_RcBinaryMinHeap        | 10000 |       417,620.57 ns |       853.343 ns |       756.466 ns |
| Enqueue_PriorityQueue          | 10000 |       278,791.68 ns |     1,566.093 ns |     1,464.924 ns |
| DequeueAll_RcSortedQueue       | 10000 |     1,435,539.99 ns |     9,329.910 ns |     8,727.204 ns |
| DequeueAll_RcBinaryMinHeap     | 10000 |     2,956,366.90 ns |     6,344.030 ns |     5,934.210 ns |
| DequeueAll_PriorityQueue       | 10000 |     2,642,186.54 ns |     9,482.374 ns |     8,869.819 ns |
| EnqueueDequeue_RcSortedQueue   | 10000 | 1,318,277,320.00 ns | 6,725,701.525 ns | 6,291,225.379 ns |
| EnqueueDequeue_RcBinaryMinHeap | 10000 |     1,712,170.68 ns |     5,674.513 ns |     5,307.943 ns |
| EnqueueDequeue_PriorityQueue   | 10000 |     1,466,910.77 ns |     4,394.686 ns |     4,110.792 ns |

*/

public class PriorityQueueBenchmarks
{
    [Params(10, 100, 1000, 10000)] public int Count;

    private RcSortedQueue<Node> _sq;
    private RcBinaryMinHeap<Node> _bmHeap;
    private PriorityQueue<Node, Node> _pq;

    private float[] _priority;

    class Node
    {
        public int id;
        public float total;
    }

    [GlobalSetup]
    public void Setup()
    {
        static int Comp(Node x, Node y)
        {
            var v = x.total.CompareTo(y.total);
            if (v != 0)
                return v;
            return x.id.CompareTo(y.id);
        }

        _sq = new(Comp);
        _bmHeap = new(Count, Comp);
        _pq = new(Count, Comparer<Node>.Create(Comp));

        _priority = new float[Count];
        for (int i = 0; i < Count; i++)
        {
            _priority[i] = (float)Random.Shared.NextDouble() * 100f;
        }
    }

    [Benchmark]
    public void Enqueue_RcSortedQueue()
    {
        _sq.Clear();
        for (int i = 0; i < Count; i++)
        {
            _sq.Enqueue(new Node
            {
                id = i,
                total = _priority[i],
            });
        }
    }

    [Benchmark]
    public void Enqueue_RcBinaryMinHeap()
    {
        _bmHeap.Clear();
        for (int i = 0; i < Count; i++)
        {
            _bmHeap.Push(new Node
            {
                id = i,
                total = _priority[i],
            });
        }
    }

    [Benchmark]
    public void Enqueue_PriorityQueue()
    {
        _pq.Clear();
        for (int i = 0; i < Count; i++)
        {
            var node = new Node
            {
                id = i,
                total = _priority[i],
            };
            _pq.Enqueue(node, node);
        }
    }

    [Benchmark]
    public void DequeueAll_RcSortedQueue()
    {
        _sq.Clear();
        for (int i = 0; i < Count; i++)
        {
            _sq.Enqueue(new Node
            {
                id = i,
                total = _priority[i],
            });
        }

        while (_sq.Count() > 0)
        {
            _sq.Dequeue();
        }
    }

    [Benchmark]
    public void DequeueAll_RcBinaryMinHeap()
    {
        _bmHeap.Clear();
        for (int i = 0; i < Count; i++)
        {
            _bmHeap.Push(new Node
            {
                id = i,
                total = _priority[i],
            });
        }

        while (_bmHeap.Count > 0)
        {
            _bmHeap.Pop();
        }
    }

    [Benchmark]
    public void DequeueAll_PriorityQueue()
    {
        _pq.Clear();
        for (int i = 0; i < Count; i++)
        {
            var node = new Node
            {
                id = i,
                total = _priority[i],
            };
            _pq.Enqueue(node, node);
        }

        while (_pq.Count > 0)
        {
            _pq.Dequeue();
        }
    }


    [Benchmark]
    public void EnqueueDequeue_RcSortedQueue()
    {
        _sq.Clear();
        int half = Count / 2;
        for (int i = 0; i < half; i++)
        {
            _sq.Enqueue(new Node
            {
                id = i,
                total = _priority[i],
            });
        }
        
        for (int i = half; i < Count; i++)
        {
            _sq.Enqueue(new Node
            {
                id = i,
                total = _priority[i],
            });

            _sq.Dequeue();
        }

    }

    [Benchmark]
    public void EnqueueDequeue_RcBinaryMinHeap()
    {
        _bmHeap.Clear();
        int half = Count / 2;
        for (int i = 0; i < half; i++)
        {
            _bmHeap.Push(new Node
            {
                id = i,
                total = _priority[i],
            });
        }
        
        for (int i = half; i < Count; i++)
        {
            _bmHeap.Push(new Node
            {
                id = i,
                total = _priority[i],
            });

            _bmHeap.Pop();
        }

    }

    [Benchmark]
    public void EnqueueDequeue_PriorityQueue()
    {
        _pq.Clear();
        int half = Count / 2;
        for (int i = 0; i < half; i++)
        {
            var node = new Node
            {
                id = i,
                total = _priority[i],
            };
            _pq.Enqueue(node, node);
        }

        for (int i = half; i < Count; i++)
        {
            var node = new Node
            {
                id = i,
                total = _priority[i],
            };
            _pq.Enqueue(node, node);
            _pq.Dequeue();
        }
    }
}