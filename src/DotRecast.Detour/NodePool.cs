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

using System.Collections.Generic;

namespace DotRecast.Detour
{



public class NodePool
{

    private readonly Dictionary<long, List<Node>> m_map = new Dictionary<long, List<Node>>();
    private readonly List<Node> m_nodes = new List<Node>();

    public NodePool() {

    }

    public void clear() {
        m_nodes.Clear();
        m_map.Clear();
    }

    public List<Node> findNodes(long id) {
        var hasNode = m_map.TryGetValue(id, out var nodes);;
        if (nodes == null) {
            nodes = new List<Node>();
        }
        return nodes;
    }

    public Node findNode(long id) {
        var hasNode = m_map.TryGetValue(id, out var nodes);;
        if (nodes != null && 0 != nodes.Count) {
            return nodes[0];
        }
        return null;
    }

    public Node getNode(long id, int state) {
        var hasNode = m_map.TryGetValue(id, out var nodes);;
        if (nodes != null) {
            foreach (Node node in nodes) {
                if (node.state == state) {
                    return node;
                }
            }
        }
        return create(id, state);
    }

    protected Node create(long id, int state) {
        Node node = new Node(m_nodes.Count + 1);
        node.id = id;
        node.state = state;
        m_nodes.Add(node);
        var hasNode = m_map.TryGetValue(id, out var nodes);;
        if (nodes == null) {
            nodes = new List<Node>();
            m_map.Add(id, nodes);
        }
        nodes.Add(node);
        return node;
    }

    public int getNodeIdx(Node node) {
        return node != null ? node.index : 0;
    }

    public Node getNodeAtIdx(int idx) {
        return idx != 0 ? m_nodes[idx - 1] : null;
    }

    public Node getNode(long refs) {
        return getNode(refs, 0);
    }

    public Dictionary<long, List<Node>> getNodeMap() {
        return m_map;
    }

}

}