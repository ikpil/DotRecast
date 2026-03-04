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

namespace DotRecast.Core.Collections
{
    public class RcSortedQueue<T>
    {
        private readonly List<T> _items;
        private readonly Comparison<T> _comparison;

        public RcSortedQueue(Comparison<T> comp)
        {
            _items = new List<T>();
            _comparison = comp;
        }

        public int Count()
        {
            return _items.Count;
        }

        public bool IsEmpty()
        {
            return 0 == _items.Count;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public T Peek()
        {
            return _items[0];
        }

        public T Dequeue()
        {
            var node = Peek();
            int lastIndex = _items.Count - 1;
            T last = _items[lastIndex];
            _items.RemoveAt(lastIndex);

            if (0 < _items.Count)
            {
                _items[0] = last;
                SiftDown(0);
            }

            return node;
        }

        public void Enqueue(T item)
        {
            if (null == item)
                return;

            _items.Add(item);
            SiftUp(_items.Count - 1);
        }

        public bool Remove(T item)
        {
            if (null == item)
                return false;

            int idx = _items.LastIndexOf(item);
            if (0 > idx)
                return false;

            int lastIndex = _items.Count - 1;
            if (idx == lastIndex)
            {
                _items.RemoveAt(lastIndex);
                return true;
            }

            T last = _items[lastIndex];
            _items.RemoveAt(lastIndex);
            _items[idx] = last;

            int parent = (idx - 1) / 2;
            if (0 < idx && IsHigherPriority(idx, parent))
            {
                SiftUp(idx);
            }
            else
            {
                SiftDown(idx);
            }

            return true;
        }


        public List<T> ToList()
        {
            var temp = new List<T>(_items);
            temp.Sort(_comparison);
            return temp;
        }

        private bool IsHigherPriority(int index, int parentIndex)
        {
            return 0 > _comparison(_items[index], _items[parentIndex]);
        }

        private void SiftUp(int index)
        {
            while (0 < index)
            {
                int parent = (index - 1) / 2;
                if (!IsHigherPriority(index, parent))
                {
                    break;
                }

                (_items[index], _items[parent]) = (_items[parent], _items[index]);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            int count = _items.Count;
            while (true)
            {
                int left = (index * 2) + 1;
                if (left >= count)
                {
                    break;
                }

                int right = left + 1;
                int best = left;
                if (right < count && 0 > _comparison(_items[right], _items[left]))
                {
                    best = right;
                }

                if (0 >= _comparison(_items[index], _items[best]))
                {
                    break;
                }

                (_items[index], _items[best]) = (_items[best], _items[index]);
                index = best;
            }
        }
    }
}
