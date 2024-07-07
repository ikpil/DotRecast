using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotRecast.Core.Collections
{
    public sealed class RcBinaryHeap<T>
    {
        public int Count => _count;
        public int Capacity => _values.Length;

        public T this[int index] => _values[index];

        private T[] _values;
        private int _count;

        private Comparison<T> _comparision;

        public RcBinaryHeap(Comparison<T> comparison) : this(8, comparison)
        {
        }

        public RcBinaryHeap(int capacity, Comparison<T> comparison)
        {
            if (capacity <= 0)
                throw new ArgumentException("capacity must greater than zero");

            _values = new T[capacity];
            _comparision = comparison;
            _count = 0;
        }

        public void Push(T val)
        {
            EnsureCapacity();

            _values[_count++] = val;

            UpHeap(_count - 1);
        }

        public T Pop()
        {
            if (_count == 0)
            {
                Throw();

                static void Throw() =>
                    throw new InvalidOperationException("no element to pop");
            }

            Swap(0, --_count);
            DownHeap(1);

            return _values[_count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Top()
        {
            return _values[0];
        }

        public void Modify(T node)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_values[i].Equals(node))
                {
                    UpHeap(i);
                    return;
                }
            }
        }

        public void Clear()
        {
            Array.Clear(_values, 0, _count);
            _count = 0;
        }

        public void FastClear()
        {
            _count = 0;
        }

        public T[] ToArray()
        {
            var copy = new T[_count];
            Array.Copy(_values, copy, _count);
            return copy;
        }

        public void ReBuild()
        {
            for (int i = _count / 2; i >= 1; i--)
            {
                DownHeap(i);
            }
        }

        private void EnsureCapacity()
        {
            if (_values.Length <= _count)
            {
                var newValues = new T[Capacity * 2];
                Array.Copy(_values, newValues, _count);
                _values = newValues;
            }
        }

        private void UpHeap(int i)
        {
            int p = (i - 1) / 2;
            while (p >= 0)
            {
                if (_comparision(_values[p], _values[i]) <= 0)
                    break;

                Swap(p, i);

                i = p;
                p = (i - 1) / 2;
            }
        }

        private void DownHeap(int i)
        {
            T d = _values[i - 1];
            int child;
            while (i <= _count / 2)
            {
                child = i * 2;
                if (child < _count && _comparision(_values[child - 1], _values[child]) > 0)
                    child++;

                if (_comparision(d, _values[child - 1]) <= 0)
                    break;

                _values[i - 1] = _values[child - 1];
                i = child;
            }
            _values[i - 1] = d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int x, int y)
        {
            if (x == y)
                return;
            (_values[y], _values[x]) = (_values[x], _values[y]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            return _count == 0;
        }
    }
}
