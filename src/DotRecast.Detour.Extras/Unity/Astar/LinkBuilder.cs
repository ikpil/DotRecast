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


public class LinkBuilder {

    // Process connections and transform them into recast neighbour flags
    public void build(int nodeOffset, GraphMeshData graphData, List<int[]> connections) {
        for (int n = 0; n < connections.Count; n++) {
            int[] nodeConnections = connections[n];
            MeshData tile = graphData.getTile(n);
            Poly node = graphData.getNode(n);
            foreach (int connection in nodeConnections) {
                MeshData neighbourTile = graphData.getTile(connection - nodeOffset);
                if (neighbourTile != tile) {
                    buildExternalLink(tile, node, neighbourTile);
                } else {
                    Poly neighbour = graphData.getNode(connection - nodeOffset);
                    buildInternalLink(tile, node, neighbourTile, neighbour);
                }
            }
        }
    }

    private void buildInternalLink(MeshData tile, Poly node, MeshData neighbourTile, Poly neighbour) {
        int edge = PolyUtils.findEdge(node, neighbour, tile, neighbourTile);
        if (edge >= 0) {
            node.neis[edge] = neighbour.index + 1;
        } else {
            throw new ArgumentException();
        }
    }

    // In case of external link to other tiles we must find the direction
    private void buildExternalLink(MeshData tile, Poly node, MeshData neighbourTile) {
        if (neighbourTile.header.bmin[0] > tile.header.bmin[0]) {
            node.neis[PolyUtils.findEdge(node, tile, neighbourTile.header.bmin[0], 0)] = NavMesh.DT_EXT_LINK;
        } else if (neighbourTile.header.bmin[0] < tile.header.bmin[0]) {
            node.neis[PolyUtils.findEdge(node, tile, tile.header.bmin[0], 0)] = NavMesh.DT_EXT_LINK | 4;
        } else if (neighbourTile.header.bmin[2] > tile.header.bmin[2]) {
            node.neis[PolyUtils.findEdge(node, tile, neighbourTile.header.bmin[2], 2)] = NavMesh.DT_EXT_LINK | 2;
        } else {
            node.neis[PolyUtils.findEdge(node, tile, tile.header.bmin[2], 2)] = NavMesh.DT_EXT_LINK | 6;
        }
    }

}

}