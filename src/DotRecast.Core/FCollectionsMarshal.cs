/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

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