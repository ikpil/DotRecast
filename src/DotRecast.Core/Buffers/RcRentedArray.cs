using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Buffers
{
    public static class RcRentedArray
    {
        public static RcRentedArray<T> Rent<T>(int minimumLength)
        {
            var array = ArrayPool<T>.Shared.Rent(minimumLength);
            return new RcRentedArray<T>(ArrayPool<T>.Shared, array, minimumLength);
        }
    }

    public class RcRentedArray<T> : IDisposable
    {
        private ArrayPool<T> _owner;
        private T[] _array;

        public int Length { get; }

        internal RcRentedArray(ArrayPool<T> owner, T[] array, int length)
        {
            _owner = owner;
            _array = array;
            Length = length;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                RcThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);
                return ref _array[index];
            }
        }


        public void Dispose()
        {
            if (null != _owner && null != _array)
            {
                _owner.Return(_array, true);
                _owner = null;
                _array = null;
            }
        }
    }
}