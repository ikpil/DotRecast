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

namespace DotRecast.Detour
{
    using System.Collections.Generic;

    public class NodeQueue
    {
        private readonly List<Node> m_heap = new List<Node>();

        public int count()
        {
            return m_heap.Count;
        }

        public void clear()
        {
            m_heap.Clear();
        }

        public Node top()
        {
            return m_heap[0];
        }

        public Node pop()
        {
            var node = top();
            m_heap.Remove(node);
            return node;
        }

        public void push(Node node)
        {
            m_heap.Add(node);
            m_heap.Sort((x, y) => x.total.CompareTo(y.total));
        }

        public void modify(Node node)
        {
            m_heap.Remove(node);
            push(node);
        }

        public bool isEmpty()
        {
            return 0 == m_heap.Count;
        }
    }
}