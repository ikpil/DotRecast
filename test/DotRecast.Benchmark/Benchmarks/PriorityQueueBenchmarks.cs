using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DotRecast.Core.Collections;

namespace DotRecast.Benchmark.Benchmarks;

/*

| Method                 | Count     |             Mean |            Error |           StdDev |           Median |
| ---------------------- | --------- | ---------------: | ---------------: | ---------------: | ---------------: |
| **Enqueue_rcQueue**    | **10**    |     **83.81 ns** |     **1.722 ns** |     **2.925 ns** |     **83.00 ns** |
| Enqueue_heap           | 10        |        173.27 ns |         3.431 ns |         3.813 ns |        172.40 ns |
| Enqueue_pqueue         | 10        |        151.13 ns |         3.045 ns |         3.625 ns |        151.81 ns |
| DequeueAll_rcQueue     | 10        |        293.28 ns |         5.368 ns |         5.021 ns |        293.56 ns |
| DequeueAll_heap        | 10        |        409.89 ns |         4.982 ns |         4.416 ns |        410.04 ns |
| DequeueAll_pqueue      | 10        |        448.56 ns |         4.490 ns |         3.980 ns |        448.17 ns |
| EnqueueDequeue_rcQueue | 10        |        116.73 ns |         0.126 ns |         0.105 ns |        116.72 ns |
| EnqueueDequeue_heap    | 10        |        130.94 ns |         0.936 ns |         0.781 ns |        130.80 ns |
| EnqueueDequeue_pqueue  | 10        |        101.39 ns |         0.589 ns |         0.551 ns |        101.14 ns |
| **Enqueue_rcQueue**    | **100**   |    **690.10 ns** |     **1.463 ns** |     **1.297 ns** |    **689.74 ns** |
| Enqueue_heap           | 100       |      2,517.08 ns |         8.466 ns |         7.070 ns |      2,515.99 ns |
| Enqueue_pqueue         | 100       |      2,188.55 ns |        26.386 ns |        24.682 ns |      2,193.53 ns |
| DequeueAll_rcQueue     | 100       |      4,862.85 ns |        71.216 ns |        59.469 ns |      4,849.71 ns |
| DequeueAll_heap        | 100       |      8,791.09 ns |       145.019 ns |       183.403 ns |      8,731.62 ns |
| DequeueAll_pqueue      | 100       |     10,819.65 ns |        97.138 ns |        90.863 ns |     10,837.05 ns |
| EnqueueDequeue_rcQueue | 100       |      1,123.50 ns |        10.281 ns |         9.114 ns |      1,119.03 ns |
| EnqueueDequeue_heap    | 100       |      1,228.70 ns |         4.664 ns |         3.894 ns |      1,227.22 ns |
| EnqueueDequeue_pqueue  | 100       |        968.43 ns |        19.095 ns |        30.834 ns |        963.56 ns |
| **Enqueue_rcQueue**    | **1000**  |  **7,416.73 ns** |   **147.213 ns** |   **229.193 ns** |  **7,377.16 ns** |
| Enqueue_heap           | 1000      |     35,362.30 ns |       478.398 ns |       447.494 ns |     35,391.69 ns |
| Enqueue_pqueue         | 1000      |     24,861.28 ns |       438.919 ns |       389.091 ns |     24,737.34 ns |
| DequeueAll_rcQueue     | 1000      |     81,520.39 ns |       299.823 ns |       250.366 ns |     81,538.01 ns |
| DequeueAll_heap        | 1000      |    150,237.95 ns |       475.349 ns |       371.121 ns |    150,241.44 ns |
| DequeueAll_pqueue      | 1000      |    166,375.18 ns |     1,105.089 ns |     1,033.701 ns |    166,338.45 ns |
| EnqueueDequeue_rcQueue | 1000      |     10,984.87 ns |        44.043 ns |        41.198 ns |     10,985.13 ns |
| EnqueueDequeue_heap    | 1000      |     14,047.62 ns |       174.581 ns |       163.303 ns |     14,061.52 ns |
| EnqueueDequeue_pqueue  | 1000      |      9,105.53 ns |        90.691 ns |        80.395 ns |      9,102.35 ns |
| **Enqueue_rcQueue**    | **10000** | **90,623.51 ns** | **1,526.788 ns** | **1,353.458 ns** | **90,429.58 ns** |
| Enqueue_heap           | 10000     |    347,060.71 ns |     4,511.258 ns |     3,767.105 ns |    347,319.53 ns |
| Enqueue_pqueue         | 10000     |    287,118.46 ns |     3,091.524 ns |     2,581.562 ns |    286,254.88 ns |
| DequeueAll_rcQueue     | 10000     |  1,245,536.36 ns |     7,701.471 ns |     6,827.153 ns |  1,245,206.25 ns |
| DequeueAll_heap        | 10000     |  1,935,679.51 ns |     2,327.083 ns |     1,816.833 ns |  1,935,649.51 ns |
| DequeueAll_pqueue      | 10000     |  2,541,652.37 ns |     7,807.705 ns |     7,303.332 ns |  2,543,812.50 ns |
| EnqueueDequeue_rcQueue | 10000     |    121,456.00 ns |     2,210.749 ns |     5,079.562 ns |    119,552.48 ns |
| EnqueueDequeue_heap    | 10000     |    144,426.41 ns |     2,700.978 ns |     2,526.496 ns |    143,537.16 ns |
| EnqueueDequeue_pqueue  | 10000     |    102,263.93 ns |       984.973 ns |       873.153 ns |    102,031.01 ns |

*/

public class PriorityQueueBenchmarks
{
    [Params(10, 100, 1000, 10000)] public int Count;

    private RcSortedQueue<Node> _rcQueue;
    private RcBinaryHeap<Node> _heap;
    private PriorityQueue<Node, Node> _pqueue;

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

        _rcQueue = new(Comp);
        _heap = new(Count, Comp);
        _pqueue = new(Count, Comparer<Node>.Create(Comp));

        _priority = new float[Count];
        for (int i = 0; i < Count; i++)
        {
            _priority[i] = (float)Random.Shared.NextDouble() * 100f;
        }

        Console.WriteLine("111");
    }

    [Benchmark]
    public void Enqueue_rcQueue()
    {
        _rcQueue.Clear();
        for (int i = 0; i < Count; i++)
        {
            _rcQueue.Enqueue(new Node
            {
                id = i,
                total = _priority[i],
            });
        }
    }

    [Benchmark]
    public void Enqueue_heap()
    {
        _heap.Clear();
        for (int i = 0; i < Count; i++)
        {
            _heap.Push(new Node
            {
                id = i,
                total = _priority[i],
            });
        }
    }

    [Benchmark]
    public void Enqueue_pqueue()
    {
        _pqueue.Clear();
        for (int i = 0; i < Count; i++)
        {
            var node = new Node
            {
                id = i,
                total = _priority[i],
            };
            _pqueue.Enqueue(node, node);
        }
    }

    [Benchmark]
    public void DequeueAll_rcQueue()
    {
        _rcQueue.Clear();
        for (int i = 0; i < Count; i++)
        {
            _rcQueue.Enqueue(new Node
            {
                id = i,
                total = _priority[i],
            });
        }

        while (_rcQueue.Count() > 0)
        {
            _rcQueue.Dequeue();
        }
    }

    [Benchmark]
    public void DequeueAll_heap()
    {
        _heap.Clear();
        for (int i = 0; i < Count; i++)
        {
            _heap.Push(new Node
            {
                id = i,
                total = _priority[i],
            });
        }

        while (_heap.Count > 0)
        {
            _heap.Pop();
        }
    }

    [Benchmark]
    public void DequeueAll_pqueue()
    {
        _pqueue.Clear();
        for (int i = 0; i < Count; i++)
        {
            var node = new Node
            {
                id = i,
                total = _priority[i],
            };
            _pqueue.Enqueue(node, node);
        }

        while (_pqueue.Count > 0)
        {
            _pqueue.Dequeue();
        }
    }


    [Benchmark]
    public void EnqueueDequeue_rcQueue()
    {
        _rcQueue.Clear();
        for (int i = 0; i < Count; i++)
        {
            _rcQueue.Enqueue(new Node
            {
                id = i,
                total = _priority[i],
            });

            _rcQueue.Dequeue();
        }
    }

    [Benchmark]
    public void EnqueueDequeue_heap()
    {
        _heap.Clear();
        for (int i = 0; i < Count; i++)
        {
            _heap.Push(new Node
            {
                id = i,
                total = _priority[i],
            });

            _heap.Pop();
        }
    }

    [Benchmark]
    public void EnqueueDequeue_pqueue()
    {
        _pqueue.Clear();
        for (int i = 0; i < Count; i++)
        {
            var node = new Node
            {
                id = i,
                total = _priority[i],
            };
            _pqueue.Enqueue(node, node);

            _pqueue.Dequeue();
        }
    }
}