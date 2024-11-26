﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotRecast.Core.Buffers
{
    public static class RcRentedArray
    {
        private sealed class RentIdPool
        {
            private int[] _generations;
            private readonly Queue<int> _freeIds;
            private int _maxId;

            public RentIdPool(int capacity)
            {
                _generations = new int[capacity];
                _freeIds = new Queue<int>(capacity);
            }

            internal RentIdGen AcquireId()
            {
                if (!_freeIds.TryDequeue(out int id))
                {
                    id = _maxId++;
                    if(_generations.Length <= id)
                        Array.Resize(ref _generations, _generations.Length << 1);    
                }

                return new RentIdGen(id, _generations[id]);
            }

            internal void ReturnId(int id)
            {
                _generations[id]++;
                _freeIds.Enqueue(id);
            }

            internal int GetGeneration(int id)
            {
                return _generations.Length <= id ? 0 : _generations[id];
            }
        }
        
        public const int START_RENT_ID_POOL_CAPACITY = 16;
        private static readonly ThreadLocal<RentIdPool> _rentPool = new ThreadLocal<RentIdPool>(() => new RentIdPool(START_RENT_ID_POOL_CAPACITY));
        
        public static RcRentedArray<T> Rent<T>(int minimumLength)
        {
            var array = ArrayPool<T>.Shared.Rent(minimumLength);
            return new RcRentedArray<T>(ArrayPool<T>.Shared, _rentPool.Value.AcquireId(), array, minimumLength);
        }

        internal static bool IsDisposed(RentIdGen rentIdGen)
        {
            return _rentPool.Value.GetGeneration(rentIdGen.Id) != rentIdGen.Gen;
        }

        internal static void ReturnId(RentIdGen rentIdGen)
        {
            _rentPool.Value.ReturnId(rentIdGen.Id);
        }
    }

    public readonly struct RentIdGen
    {
        public readonly int Id;
        public readonly int Gen;

        public RentIdGen(int id, int gen)
        {
            Id = id;
            Gen = gen;
        }
    }

    public struct RcRentedArray<T> : IDisposable
    {
        private ArrayPool<T> _owner;
        private T[] _array;
        private readonly RentIdGen _rentIdGen;

        public int Length { get; }
        public bool IsDisposed => null == _owner || null == _array || RcRentedArray.IsDisposed(_rentIdGen);

        internal RcRentedArray(ArrayPool<T> owner, RentIdGen rentIdGen, T[] array, int length)
        {
            _owner = owner;
            _array = array;
            Length = length;
            _rentIdGen = rentIdGen;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                RcThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);
                if (IsDisposed)
                    throw new NullReferenceException();
                return ref _array[index];
            }
        }

        public T[] AsArray()
        {
            return _array;
        }

        public Span<T> AsSpan()
        {
            return new Span<T>(_array, 0, Length);
        }


        public void Dispose()
        {
            if (null != _owner && null != _array && !RcRentedArray.IsDisposed(_rentIdGen))
            {
                RcRentedArray.ReturnId(_rentIdGen);
                _owner.Return(_array, true);
            }
            
            _owner = null;
            _array = null;
        }
    }
}