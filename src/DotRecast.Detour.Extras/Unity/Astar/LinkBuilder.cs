/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

namespace DotRecast.Detour.Extras.Unity.Astar
{
    public class LinkBuilder
    {
        // Process connections and transform them into recast neighbour flags
        public void Build(int nodeOffset, GraphMeshData graphData, List<int[]> connections)
        {
            for (int n = 0; n < connections.Count; n++)
            {
                int[] nodeConnections = connections[n];
                MeshData tile = graphData.GetTile(n);
                Poly node = graphData.GetNode(n);
                foreach (int connection in nodeConnections)
                {
                    MeshData neighbourTile = graphData.GetTile(connection - nodeOffset);
                    if (neighbourTile != tile)
                    {
                        BuildExternalLink(tile, node, neighbourTile);
                    }
                    else
                    {
                        Poly neighbour = graphData.GetNode(connection - nodeOffset);
                        BuildInternalLink(tile, node, neighbourTile, neighbour);
                    }
                }
            }
        }

        private void BuildInternalLink(MeshData tile, Poly node, MeshData neighbourTile, Poly neighbour)
        {
            int edge = PolyUtils.FindEdge(node, neighbour, tile, neighbourTile);
            if (edge >= 0)
            {
                node.neis[edge] = neighbour.index + 1;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        // In case of external link to other tiles we must find the direction
        private void BuildExternalLink(MeshData tile, Poly node, MeshData neighbourTile)
        {
            if (neighbourTile.header.bmin.x > tile.header.bmin.x)
            {
                node.neis[PolyUtils.FindEdge(node, tile, neighbourTile.header.bmin.x, 0)] = NavMesh.DT_EXT_LINK;
            }
            else if (neighbourTile.header.bmin.x < tile.header.bmin.x)
            {
                node.neis[PolyUtils.FindEdge(node, tile, tile.header.bmin.x, 0)] = NavMesh.DT_EXT_LINK | 4;
            }
            else if (neighbourTile.header.bmin.z > tile.header.bmin.z)
            {
                node.neis[PolyUtils.FindEdge(node, tile, neighbourTile.header.bmin.z, 2)] = NavMesh.DT_EXT_LINK | 2;
            }
            else
            {
                node.neis[PolyUtils.FindEdge(node, tile, tile.header.bmin.z, 2)] = NavMesh.DT_EXT_LINK | 6;
            }
        }
    }
}