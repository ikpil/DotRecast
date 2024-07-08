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
using System.Linq;
using System.Runtime.CompilerServices;

namespace DotRecast.Detour
{
    public class DtNodePool
    {
        DtNode[] m_nodes;
        ushort[] m_first;
        ushort[] m_next;
        int m_maxNodes;
        int m_hashSize;
        int m_nodeCount;

        const int DT_NULL_IDX = ushort.MaxValue;

        public DtNodePool(int maxNodes) : this(maxNodes, (int)dtNextPow2((uint)(maxNodes / 4)))
        { }

        public DtNodePool(int maxNodes, int hashSize)
        {
            m_maxNodes = maxNodes;
            m_hashSize = hashSize; // dtNextPow2

            m_nodes = new DtNode[m_maxNodes];
            m_next = new ushort[m_maxNodes];
            m_first = new ushort[m_hashSize];
            m_nodeCount = 0;

            m_next.AsSpan().Fill(DT_NULL_IDX);
            m_first.AsSpan().Fill(DT_NULL_IDX);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_first.AsSpan().Fill(DT_NULL_IDX);
            m_nodeCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNodeCount()
        {
            return m_nodeCount;
        }

        public int FindNodes(long id, Span<DtNode> nodes)
        {
            int n = 0;
            uint bucket = (uint)(dtHashRef(id) & (m_hashSize - 1));
            ushort i = m_first[bucket];
            while (i != DT_NULL_IDX)
            {
                if (m_nodes[i].id == id)
                {
                    if (n >= nodes.Length)
                        return n;
                    nodes[n++] = m_nodes[i];
                }
                i = m_next[i];
            }

            return n;
        }

        public DtNode FindNode(long id)
        {
            long bucket = dtHashRef(id) & (m_hashSize - 1);
            ushort i = m_first[bucket];
            while (i != DT_NULL_IDX)
            {
                if (m_nodes[i].id == id /*&& m_nodes[i].state == state*/) // TODO test
                    return m_nodes[i];
                i = m_next[i];
            }
            return null;
        }

        public DtNode GetNode(long id, int state)
        {
            uint bucket = (uint)(dtHashRef(id) & (m_hashSize - 1));
            ushort i = m_first[bucket];
            while (i != DT_NULL_IDX)
            {
                if (m_nodes[i].id == id && m_nodes[i].state == state)
                    return m_nodes[i];
                i = m_next[i];
            }

            if (m_nodeCount >= m_maxNodes)
                return null;

            i = (ushort)m_nodeCount;
            m_nodeCount++;

            // Init node
            var node = m_nodes[i] ?? (m_nodes[i] = new DtNode(i));
            node.pidx = 0;
            node.cost = 0;
            node.total = 0;
            node.id = id;
            node.state = state;
            node.flags = 0;

            m_next[i] = m_first[bucket];
            m_first[bucket] = i;

            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNodeIdx(DtNode node)
        {
            return node != null
                ? node.ptr + 1
                : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DtNode GetNodeAtIdx(int idx)
        {
            return idx != 0
                ? m_nodes[idx - 1]
                : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DtNode GetNode(long refs)
        {
            return GetNode(refs, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<DtNode> AsSpan()
        {
            return m_nodes.AsSpan(0, m_nodeCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint dtNextPow2(uint v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint dtHashRef(long a)
        {
            a += ~(a << 15);
            a ^= (a >> 10);
            a += (a << 3);
            a ^= (a >> 6);
            a += ~(a << 11);
            a ^= (a >> 16);
            return (uint)a;
        }
    }
}