using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotRecast.Core
{
    public static class FCollectionsMarshal
    {
        /// <summary>
        /// similar as AsSpan but modify size to create fixed-size span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> CreateSpan<T>(List<T> list, int count)
        {
#if NET8_0_OR_GREATER
            CollectionsMarshal.SetCount(list, count);
            return CollectionsMarshal.AsSpan(list);
#else
            // TODO 有一些差异，CollectionsMarshal.SetCount 会清掉引用类型的对象
            if (list.Capacity < count)
                list.Capacity = count;

            ref var view = ref Unsafe.As<List<T>, ListView<T>>(ref list); // 0 gc
            view._size = count;
            return view._items.AsSpan(0, count);
#endif
        }

        /// <summary>
        /// similar as AsSpan but modify size to create fixed-size span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(List<T> list)
        {
#if NET6_0_OR_GREATER
            return CollectionsMarshal.AsSpan(list);
#else
            ref var view = ref Unsafe.As<List<T>, ListView<T>>(ref list);
            return view._items.AsSpan(0, view._size);
#endif
        }

#if !NET8_0_OR_GREATER
        // NOTE: These structure depndent on .NET 7, if changed, require to keep same structure.
        internal class ListView<T>
        {
            public T[] _items;
            public int _size;
            public int _version;
        }
#endif
    }
}