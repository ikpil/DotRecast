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

using System.Collections.Generic;
using System.Linq;

namespace DotRecast.Detour
{
    public class DtNodePool
    {
        private readonly Dictionary<long, DtNode> m_map;

        private int m_nodeCount;
        private readonly List<DtNode> m_nodes;

        public DtNodePool()
        {
            m_map = new Dictionary<long, DtNode>();
            m_nodes = new List<DtNode>();
        }

        public void Clear()
        {
            m_map.Clear();
            m_nodeCount = 0;
        }

        public int GetNodeCount()
        {
            return m_nodeCount;
        }
        

        public DtNode FindNode(long id)
        {
            return m_map.GetValueOrDefault(id);
        }

        public DtNode GetNode(long id, int state)
        {
            if (m_map.TryGetValue(id, out var node))
            {
                if (node.state == state)
                {
                    return node;
                }
            }

            var cr = Create(id, state);
            m_map.Add(id, cr);
            return cr;
        }

        private DtNode Create(long id, int state)
        {
            DtNode node = null;
            int i = m_nodeCount;
            
            if (m_nodes.Count <= m_nodeCount)
            {
                node = new DtNode(m_nodeCount);
                m_nodes.Add(node);
            }
            else
            {
                node = m_nodes[i];
            }
            
            m_nodeCount++;
            
            node.pidx = 0;
            node.cost = 0;
            node.total = 0;
            node.id = id;
            node.state = state;
            node.flags = 0;
            node.shortcut = null;

            return node;
        }

        public int GetNodeIdx(DtNode node)
        {
            return node != null
                ? node.ptr + 1
                : 0;
        }

        public DtNode GetNodeAtIdx(int idx)
        {
            return idx != 0
                ? m_nodes[idx - 1]
                : null;
        }

        public DtNode GetNode(long refs)
        {
            return GetNode(refs, 0);
        }

        public IEnumerable<DtNode> AsEnumerable()
        {
            return m_nodes.Take(m_nodeCount);
        }
    }
}