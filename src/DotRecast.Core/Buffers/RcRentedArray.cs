using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Buffers
{
    public static class RcRentedArray
    {
        public static RcRentedArray<T> RentDisposableArray<T>(int minimumLength)
        {
            var array = ArrayPool<T>.Shared.Rent(minimumLength);
            return new RcRentedArray<T>(ArrayPool<T>.Shared, array, minimumLength);
        }
    }

    public class RcRentedArray<T> : IDisposable
    {
        private ArrayPool<T> _owner;
        private T[] _array;
        private readonly RcAtomicInteger _disposed;

        public int Length { get; }

        internal RcRentedArray(ArrayPool<T> owner, T[] array, int length)
        {
            _owner = owner;
            _array = array;
            Length = length;
            _disposed = new RcAtomicInteger(0);
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);
                return _array[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);
                _array[index] = value;
            }
        }

        public T[] AsRentedArray()
        {
            return _array;
        }

        public void Dispose()
        {
            if (1 != _disposed.IncrementAndGet())
                return;

            _owner?.Return(_array, true);
            _array = null;
            _owner = null;
        }
    }
}