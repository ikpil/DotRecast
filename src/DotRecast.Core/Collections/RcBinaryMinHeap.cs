using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    // Binary min-heap mirroring recastnavigation's dtNodeQueue (DetourNode.h/cpp),
    // generalized over T and Comparison<T>. BubbleUp/TrickleDown move a hole
    // instead of swapping - one write per level, no recursion, same as upstream.
    public sealed class RcBinaryMinHeap<T>
    {
        private readonly List<T> _items;
        private readonly Comparison<T> _comparison;

        public int Count => _items.Count;
        public int Capacity => _items.Capacity;

        public RcBinaryMinHeap(Comparison<T> comparison)
        {
            _items = new List<T>();
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        public RcBinaryMinHeap(int capacity, Comparison<T> comparison)
        {
            if (capacity <= 0)
                throw new ArgumentException("capacity must be greater than zero", nameof(capacity));

            _items = new List<T>(capacity);
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        public void Push(T val)
        {
            _items.Add(val);
            BubbleUp(_items.Count - 1, val);
        }

        public T Pop()
        {
            var min = Peek();

            int last = _items.Count - 1;
            var node = _items[last];
            _items.RemoveAt(last);
            if (0 < last)
                TrickleDown(0, node);

            return min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("Heap is empty.");
            }

            return _items[0];
        }

        public bool Modify(T node)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Equals(node))
                {
                    int parent = (i - 1) / 2;
                    if (0 < i && _comparison.Invoke(node, _items[parent]) < 0)
                        BubbleUp(i, node);
                    else
                        TrickleDown(i, node);

                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _items.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            return 0 == _items.Count;
        }

        private void BubbleUp(int i, T node)
        {
            int parent = (i - 1) / 2;
            // note: (i > 0) means there is a parent
            while (0 < i && _comparison.Invoke(node, _items[parent]) < 0)
            {
                _items[i] = _items[parent];
                i = parent;
                parent = (i - 1) / 2;
            }

            _items[i] = node;
        }

        private void TrickleDown(int i, T node)
        {
            int count = _items.Count;
            int child = (i * 2) + 1;
            while (child < count)
            {
                if (child + 1 < count && _comparison.Invoke(_items[child + 1], _items[child]) < 0)
                {
                    child++;
                }

                _items[i] = _items[child];
                i = child;
                child = (i * 2) + 1;
            }

            BubbleUp(i, node);
        }

        public T[] ToArray()
        {
            return _items.ToArray();
        }

        public List<T> ToList()
        {
            return new List<T>(_items);
        }
    }
}
