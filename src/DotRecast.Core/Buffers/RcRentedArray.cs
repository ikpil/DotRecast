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
        private bool _disposed;

        public int Length { get; }
        public bool IsDisposed => _disposed;

        internal RcRentedArray(ArrayPool<T> owner, T[] array, int length)
        {
            _owner = owner;
            _array = array;
            Length = length;
            _disposed = false;
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
            if (_disposed)
                return;

            _disposed = true;
            _owner?.Return(_array, true);
            _array = null;
            _owner = null;
        }
    }
}