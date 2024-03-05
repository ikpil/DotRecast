/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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

namespace DotRecast.Recast
{
    /** Represents a span in a heightfield. */
    public struct RcSpan
    {
        /** The lower limit of the span. [Limit: &lt; smax] */
        public int smin;

        /** The upper limit of the span. [Limit: &lt;= SPAN_MAX_HEIGHT] */
        public int smax;

        /** The area id assigned to the span. */
        public int area;

        /** The index next span higher up in column. For span in the free list, this is the index of the next free span. */
        public uint next;
    }

    /** A pool of spans. Index 0 is reserved to mean 'null'. */
    public class RcSpanPool
    {
        private RcSpan[] storage = new RcSpan[64 * 1024];
        private uint firstUnalloc = 1; // storage element with index >= this is not initialized

        public RcSpanPool()
        {
            storage[0].next = firstUnalloc;
        }

        public ref RcSpan Span(uint index) => ref storage[index];

        public uint Alloc()
        {
            var index = storage[0].next;
            if (index < firstUnalloc)
            {
                // get span from the free list
                storage[0].next = storage[index].next;
                storage[index].next = 0;
                return index;
            }

            // free list is empty
            if (storage.Length == firstUnalloc)
            {
                // and storage is full => grow
                var oldStorage = storage;
                storage = new RcSpan[oldStorage.Length * 2];
                Array.Copy(oldStorage, storage, oldStorage.Length);
            }

            // index == firstUnalloc => storage[index] == 0 (never initialized)
            storage[0].next = ++firstUnalloc;
            return index;
        }

        public void Free(uint index)
        {
            // push span to the head of the free list
            storage[index].next = storage[0].next;
            storage[0].next = index;
        }
    }
}
