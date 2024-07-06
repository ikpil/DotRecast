using BenchmarkDotNet.Attributes;
using DotRecast.Core.Collections;

namespace CSharpBencchmark
{
    /*

| Method          | Count | Mean          | Error        | StdDev       |
|---------------- |------ |--------------:|-------------:|-------------:|
| Enqueue_rcQueue | 10    |      84.19 ns |     1.328 ns |     1.242 ns |
| Enqueue_heap    | 10    |     208.44 ns |     3.522 ns |     5.981 ns |
| Enqueue_pqueue  | 10    |     202.59 ns |     2.320 ns |     2.170 ns |
| Enqueue_rcQueue | 100   |     791.99 ns |    15.733 ns |    43.333 ns |
| Enqueue_heap    | 100   |   3,136.11 ns |    57.433 ns |    50.912 ns |
| Enqueue_pqueue  | 100   |   2,256.86 ns |    19.259 ns |    17.073 ns |
| Enqueue_rcQueue | 1000  |   7,258.35 ns |    55.554 ns |    49.247 ns |
| Enqueue_heap    | 1000  |  31,613.03 ns |   602.311 ns |   591.550 ns |
| Enqueue_pqueue  | 1000  |  24,313.61 ns |   463.713 ns |   455.429 ns |
| Enqueue_rcQueue | 10000 |  98,246.69 ns | 1,824.495 ns | 1,706.634 ns |
| Enqueue_heap    | 10000 | 356,910.42 ns | 3,376.793 ns | 2,993.439 ns |
| Enqueue_pqueue  | 10000 | 278,814.15 ns | 3,733.262 ns | 3,309.439 ns |

    */

    public class Bench_PriorityQueue
    {
        [Params(10, 100, 1000, 10000)]
        public int Count;

        RcSortedQueue<Node> _rcQueue;
        //TBinaryHeap<Node> _heap;
        PriorityQueue<Node, Node> _pqueue;

        float[] _priority;

        class Node
        {
            public int id;
            public float total;
        }

        [GlobalSetup]
        public void Setup()
        {
            Comparison<Node> _comparison = (x, y) =>
            {
                var v = x.total.CompareTo(y.total);
                if (v != 0)
                    return v;
                return x.id.CompareTo(y.id);
            };

            _rcQueue = new(Count, _comparison);
            //_heap = new(Count, _comparison);
            _pqueue = new(Count, Comparer<Node>.Create(_comparison));

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

        //[Benchmark]
        //public void Enqueue_heap()
        //{
        //    _heap.Clear();
        //    for (int i = 0; i < Count; i++)
        //    {
        //        _heap.Push(new Node
        //        {
        //            id = i,
        //            total = _priority[i],
        //        });
        //    }
        //}

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
        public void EnqueueDequeue_rcQueue()
        {
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

        //[Benchmark]
        //public void EnqueueDequeue_heap()
        //{
        //    for (int i = 0; i < Count; i++)
        //    {
        //        _heap.Push(new Node
        //        {
        //            id = i,
        //            total = _priority[i],
        //        });
        //    }

        //    while (_heap.Count > 0)
        //    {
        //        _heap.Pop();
        //    }
        //}

        [Benchmark]
        public void EnqueueDequeue_pqueue()
        {
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
    }
}
