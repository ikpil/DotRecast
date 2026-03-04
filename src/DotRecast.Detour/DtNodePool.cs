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
        private readonly Dictionary<long, NodeBucket> m_map;
        private readonly Stack<NodeBucket> m_bucketPool;
        private readonly List<DtNode> m_findNodesBuffer;

        private int m_nodeCount;
        private readonly List<DtNode> m_nodes;

        public DtNodePool()
        {
            m_map = new Dictionary<long, NodeBucket>();
            m_bucketPool = new Stack<NodeBucket>();
            m_findNodesBuffer = new List<DtNode>(4);
            m_nodes = new List<DtNode>();
        }

        public void Clear()
        {
            foreach (NodeBucket bucket in m_map.Values)
            {
                bucket.Reset();
                m_bucketPool.Push(bucket);
            }

            m_map.Clear();
            m_findNodesBuffer.Clear();
            m_nodeCount = 0;
        }

        public int GetNodeCount()
        {
            return m_nodeCount;
        }

        public int FindNodes(long id, out List<DtNode> nodes)
        {
            if (m_map.TryGetValue(id, out var bucket))
            {
                m_findNodesBuffer.Clear();
                bucket.CopyTo(m_findNodesBuffer);
                nodes = m_findNodesBuffer;
                return m_findNodesBuffer.Count;
            }

            nodes = null;
            return 0;
        }

        public DtNode FindNode(long id)
        {
            if (m_map.TryGetValue(id, out var bucket))
            {
                return bucket.FindAny();
            }

            return null;
        }

        public DtNode GetNode(long id, int state)
        {
            m_map.TryGetValue(id, out var bucket);
            if (bucket == null)
            {
                bucket = RentBucket();
                m_map.Add(id, bucket);
            }

            DtNode node = bucket.Find(state);
            if (node != null)
            {
                return node;
            }

            node = Create(id, state);
            bucket.Add(state, node);
            return node;
        }

        private DtNode Create(long id, int state)
        {
            if (m_nodes.Count <= m_nodeCount)
            {
                var newNode = new DtNode(m_nodeCount);
                m_nodes.Add(newNode);
            }

            int i = m_nodeCount;
            m_nodeCount++;
            var node = m_nodes[i];
            node.pidx = 0;
            node.cost = 0;
            node.total = 0;
            node.id = id;
            node.state = state;
            node.flags = 0;
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

        private NodeBucket RentBucket()
        {
            if (0 < m_bucketPool.Count)
            {
                return m_bucketPool.Pop();
            }

            return new NodeBucket();
        }

        private sealed class NodeBucket
        {
            private DtNode _node0;
            private DtNode _node1;
            private DtNode _node2;
            private DtNode _node3;
            private List<DtNode> _overflow;
            private int _count;

            public void Reset()
            {
                _node0 = null;
                _node1 = null;
                _node2 = null;
                _node3 = null;
                _overflow?.Clear();
                _count = 0;
            }

            public DtNode Find(int state)
            {
                if (null != _node0 && _node0.state == state) return _node0;
                if (null != _node1 && _node1.state == state) return _node1;
                if (null != _node2 && _node2.state == state) return _node2;
                if (null != _node3 && _node3.state == state) return _node3;

                if (_overflow == null)
                {
                    return null;
                }

                for (int i = 0; i < _overflow.Count; ++i)
                {
                    DtNode node = _overflow[i];
                    if (node.state == state)
                    {
                        return node;
                    }
                }

                return null;
            }

            public void Add(int state, DtNode node)
            {
                if (null == node)
                    return;

                if (_node0 == null)
                {
                    _node0 = node;
                }
                else if (_node1 == null)
                {
                    _node1 = node;
                }
                else if (_node2 == null)
                {
                    _node2 = node;
                }
                else if (_node3 == null)
                {
                    _node3 = node;
                }
                else
                {
                    _overflow ??= new List<DtNode>(4);
                    _overflow.Add(node);
                }

                _count++;
            }

            public DtNode FindAny()
            {
                if (_node0 != null) return _node0;
                if (_node1 != null) return _node1;
                if (_node2 != null) return _node2;
                if (_node3 != null) return _node3;
                if (_overflow != null && 0 < _overflow.Count) return _overflow[0];
                return null;
            }

            public void CopyTo(List<DtNode> dst)
            {
                if (_count == 0)
                    return;

                if (_node0 != null) dst.Add(_node0);
                if (_node1 != null) dst.Add(_node1);
                if (_node2 != null) dst.Add(_node2);
                if (_node3 != null) dst.Add(_node3);
                if (_overflow != null)
                {
                    dst.AddRange(_overflow);
                }
            }
        }
    }
}
