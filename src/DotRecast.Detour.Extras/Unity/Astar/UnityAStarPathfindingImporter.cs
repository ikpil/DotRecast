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
using System.IO;

namespace DotRecast.Detour.Extras.Unity.Astar
{
    /**
 * Import navmeshes created with A* Pathfinding Project Unity plugin (https://arongranberg.com/astar/). Graph data is
 * loaded from a zip archive and converted to Recast navmesh objects.
 */
    public class UnityAStarPathfindingImporter
    {
        private readonly UnityAStarPathfindingReader reader = new UnityAStarPathfindingReader();
        private readonly BVTreeCreator bvTreeCreator = new BVTreeCreator();
        private readonly LinkBuilder linkCreator = new LinkBuilder();
        private readonly OffMeshLinkCreator offMeshLinkCreator = new OffMeshLinkCreator();

        public NavMesh[] load(FileStream zipFile)
        {
            GraphData graphData = reader.read(zipFile);
            Meta meta = graphData.meta;
            NodeLink2[] nodeLinks2 = graphData.nodeLinks2;
            NavMesh[] meshes = new NavMesh[meta.graphs];
            int nodeOffset = 0;
            for (int graphIndex = 0; graphIndex < meta.graphs; graphIndex++)
            {
                GraphMeta graphMeta = graphData.graphMeta[graphIndex];
                GraphMeshData graphMeshData = graphData.graphMeshData[graphIndex];
                List<int[]> connections = graphData.graphConnections[graphIndex];
                int nodeCount = graphMeshData.countNodes();
                if (connections.Count != nodeCount)
                {
                    throw new ArgumentException("Inconsistent number of nodes in data file: " + nodeCount
                                                                                              + " and connecton files: " + connections.Count);
                }

                // Build BV tree
                bvTreeCreator.build(graphMeshData);
                // Create links between nodes (both internal and portals between tiles)
                linkCreator.build(nodeOffset, graphMeshData, connections);
                // Finally, process all the off-mesh links that can be actually converted to detour data
                offMeshLinkCreator.build(graphMeshData, nodeLinks2, nodeOffset);
                NavMeshParams option = new NavMeshParams();
                option.maxTiles = graphMeshData.tiles.Length;
                option.maxPolys = 32768;
                option.tileWidth = graphMeta.tileSizeX * graphMeta.cellSize;
                option.tileHeight = graphMeta.tileSizeZ * graphMeta.cellSize;
                option.orig.x = -0.5f * graphMeta.forcedBoundsSize.x + graphMeta.forcedBoundsCenter.x;
                option.orig.y = -0.5f * graphMeta.forcedBoundsSize.y + graphMeta.forcedBoundsCenter.y;
                option.orig.z = -0.5f * graphMeta.forcedBoundsSize.z + graphMeta.forcedBoundsCenter.z;
                NavMesh mesh = new NavMesh(option, 3);
                foreach (MeshData t in graphMeshData.tiles)
                {
                    mesh.addTile(t, 0, 0);
                }

                meshes[graphIndex] = mesh;
                nodeOffset += graphMeshData.countNodes();
            }

            return meshes;
        }
    }
}