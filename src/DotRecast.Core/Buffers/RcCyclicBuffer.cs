using System;

namespace DotRecast.Core.Buffers
{
    public class RcCyclicBuffer<T>
    {
        public int MinIndex { get; private set; }
        public int MaxIndex { get; private set; }
        public int Count => MaxIndex - MinIndex + 1;
        public readonly int Size;
        
        public T this[int index] => Get(index);

        private readonly T[] _buffer;

        public RcCyclicBuffer(in int size)
        {
            _buffer = new T[size];
            Size = size;
            MinIndex = 0;
            MaxIndex = -1;
        }

        public void Add(in T item)
        {
            MaxIndex++;
            var index = MaxIndex % Size;

            if (MaxIndex >= Size)
                MinIndex = MaxIndex - Size + 1;

            _buffer[index] = item;
        }

        public T Get(in int index)
        {
            if (index < MinIndex || index > MaxIndex)
                throw new ArgumentOutOfRangeException();

            return _buffer[index % Size];
        }

        public Span<T> AsSpan()
        {
            return _buffer.AsSpan(0, Count);
        }
    }
}