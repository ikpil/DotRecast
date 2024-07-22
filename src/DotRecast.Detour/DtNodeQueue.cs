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

using System.Runtime.CompilerServices;
using DotRecast.Core.Collections;

namespace DotRecast.Detour
{
    public class DtNodeQueue
    {
        private readonly RcSortedQueue<DtNode> m_heap;

        public DtNodeQueue() : this(512)
        { }

        public DtNodeQueue(int capacity)
        {
            m_heap = new RcSortedQueue<DtNode>(capacity, DtNode.ComparisonNodeTotal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => m_heap.Count();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => m_heap.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DtNode Peek() => m_heap.Peek();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DtNode Pop() => m_heap.Dequeue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(DtNode node) => m_heap.Enqueue(node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Modify(DtNode node)
        {
            m_heap.Remove(node);
            Push(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() => m_heap.IsEmpty();
    }
}