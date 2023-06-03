/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

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
using DotRecast.Core;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour
{
    using static DotRecast.Core.RcMath;


    public class LegacyNavMeshQuery : NavMeshQuery
    {
        private static float H_SCALE = 0.999f; // Search heuristic scale.

        public LegacyNavMeshQuery(NavMesh nav) : base(nav)
        {
        }

        public override Result<List<long>> FindPath(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IQueryFilter filter,
            int options, float raycastLimit)
        {
            return FindPath(startRef, endRef, startPos, endPos, filter);
        }

        public override Result<List<long>> FindPath(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IQueryFilter filter)
        {
            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !m_nav.IsValidPolyRef(endRef) || !RcVec3f.IsFinite(startPos) || !RcVec3f.IsFinite(endPos) || null == filter)
            {
                return Results.InvalidParam<List<long>>();
            }

            if (startRef == endRef)
            {
                List<long> singlePath = new List<long>(1);
                singlePath.Add(startRef);
                return Results.Success(singlePath);
            }

            m_nodePool.Clear();
            m_openList.Clear();

            Node startNode = m_nodePool.GetNode(startRef);
            startNode.pos = startPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = RcVec3f.Distance(startPos, endPos) * H_SCALE;
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_OPEN;
            m_openList.Push(startNode);

            Node lastBestNode = startNode;
            float lastBestNodeCost = startNode.total;

            Status status = Status.SUCCSESS;

            while (!m_openList.IsEmpty())
            {
                // Remove node from open list and put it in closed list.
                Node bestNode = m_openList.Pop();
                bestNode.flags &= ~Node.DT_NODE_OPEN;
                bestNode.flags |= Node.DT_NODE_CLOSED;

                // Reached the goal, stop searching.
                if (bestNode.id == endRef)
                {
                    lastBestNode = bestNode;
                    break;
                }

                // Get current poly and tile.
                // The API input has been cheked already, skip checking internal data.
                long bestRef = bestNode.id;
                Tuple<MeshTile, Poly> tileAndPoly = m_nav.GetTileAndPolyByRefUnsafe(bestRef);
                MeshTile bestTile = tileAndPoly.Item1;
                Poly bestPoly = tileAndPoly.Item2;

                // Get parent poly and tile.
                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.pidx).id;
                }

                if (parentRef != 0)
                {
                    tileAndPoly = m_nav.GetTileAndPolyByRefUnsafe(parentRef);
                    parentTile = tileAndPoly.Item1;
                    parentPoly = tileAndPoly.Item2;
                }

                for (int i = bestTile.polyLinks[bestPoly.index]; i != NavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    long neighbourRef = bestTile.links[i].refs;

                    // Skip invalid ids and do not expand back to where we came from.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Get neighbour poly and tile.
                    // The API input has been cheked already, skip checking internal data.
                    tileAndPoly = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = tileAndPoly.Item1;
                    Poly neighbourPoly = tileAndPoly.Item2;

                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // deal explicitly with crossing tile boundaries
                    int crossSide = 0;
                    if (bestTile.links[i].side != 0xff)
                    {
                        crossSide = bestTile.links[i].side >> 1;
                    }

                    // get the node
                    Node neighbourNode = m_nodePool.GetNode(neighbourRef, crossSide);

                    // If the node is visited the first time, calculate node position.
                    if (neighbourNode.flags == 0)
                    {
                        var midpod = GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly,
                            neighbourTile);
                        if (!midpod.Failed())
                        {
                            neighbourNode.pos = midpod.result;
                        }
                    }

                    // Calculate cost and heuristic.
                    float cost = 0;
                    float heuristic = 0;

                    // Special case for last node.
                    if (neighbourRef == endRef)
                    {
                        // Cost
                        float curCost = filter.GetCost(bestNode.pos, neighbourNode.pos, parentRef, parentTile, parentPoly,
                            bestRef, bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);
                        float endCost = filter.GetCost(neighbourNode.pos, endPos, bestRef, bestTile, bestPoly, neighbourRef,
                            neighbourTile, neighbourPoly, 0L, null, null);

                        cost = bestNode.cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        // Cost
                        float curCost = filter.GetCost(bestNode.pos, neighbourNode.pos, parentRef, parentTile, parentPoly,
                            bestRef, bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);
                        cost = bestNode.cost + curCost;
                        heuristic = RcVec3f.Distance(neighbourNode.pos, endPos) * H_SCALE;
                    }

                    float total = cost + heuristic;

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    // The node is already visited and process, and the new result is worse, skip.
                    if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    // Add or update the node.
                    neighbourNode.pidx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~Node.DT_NODE_CLOSED);
                    neighbourNode.cost = cost;
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        // Already in open, update node location.
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        // Put the node in open list.
                        neighbourNode.flags |= Node.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }

                    // Update nearest node to target so far.
                    if (heuristic < lastBestNodeCost)
                    {
                        lastBestNodeCost = heuristic;
                        lastBestNode = neighbourNode;
                    }
                }
            }

            List<long> path = GetPathToNode(lastBestNode);

            if (lastBestNode.id != endRef)
            {
                status = Status.PARTIAL_RESULT;
            }

            return Results.Of(status, path);
        }

        /**
     * Updates an in-progress sliced path query.
     *
     * @param maxIter
     *            The maximum number of iterations to perform.
     * @return The status flags for the query.
     */
        public override Result<int> UpdateSlicedFindPath(int maxIter)
        {
            if (!m_query.status.IsInProgress())
            {
                return Results.Of(m_query.status, 0);
            }

            // Make sure the request is still valid.
            if (!m_nav.IsValidPolyRef(m_query.startRef) || !m_nav.IsValidPolyRef(m_query.endRef))
            {
                m_query.status = Status.FAILURE;
                return Results.Of(m_query.status, 0);
            }

            int iter = 0;
            while (iter < maxIter && !m_openList.IsEmpty())
            {
                iter++;

                // Remove node from open list and put it in closed list.
                Node bestNode = m_openList.Pop();
                bestNode.flags &= ~Node.DT_NODE_OPEN;
                bestNode.flags |= Node.DT_NODE_CLOSED;

                // Reached the goal, stop searching.
                if (bestNode.id == m_query.endRef)
                {
                    m_query.lastBestNode = bestNode;
                    m_query.status = Status.SUCCSESS;
                    return Results.Of(m_query.status, iter);
                }

                // Get current poly and tile.
                // The API input has been cheked already, skip checking internal
                // data.
                long bestRef = bestNode.id;
                Result<Tuple<MeshTile, Poly>> tileAndPoly = m_nav.GetTileAndPolyByRef(bestRef);
                if (tileAndPoly.Failed())
                {
                    m_query.status = Status.FAILURE;
                    // The polygon has disappeared during the sliced query, fail.
                    return Results.Of(m_query.status, iter);
                }

                MeshTile bestTile = tileAndPoly.result.Item1;
                Poly bestPoly = tileAndPoly.result.Item2;
                // Get parent and grand parent poly and tile.
                long parentRef = 0, grandpaRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                Node parentNode = null;
                if (bestNode.pidx != 0)
                {
                    parentNode = m_nodePool.GetNodeAtIdx(bestNode.pidx);
                    parentRef = parentNode.id;
                    if (parentNode.pidx != 0)
                    {
                        grandpaRef = m_nodePool.GetNodeAtIdx(parentNode.pidx).id;
                    }
                }

                if (parentRef != 0)
                {
                    bool invalidParent = false;
                    tileAndPoly = m_nav.GetTileAndPolyByRef(parentRef);
                    invalidParent = tileAndPoly.Failed();
                    if (invalidParent || (grandpaRef != 0 && !m_nav.IsValidPolyRef(grandpaRef)))
                    {
                        // The polygon has disappeared during the sliced query,
                        // fail.
                        m_query.status = Status.FAILURE;
                        return Results.Of(m_query.status, iter);
                    }

                    parentTile = tileAndPoly.result.Item1;
                    parentPoly = tileAndPoly.result.Item2;
                }

                // decide whether to test raycast to previous nodes
                bool tryLOS = false;
                if ((m_query.options & DT_FINDPATH_ANY_ANGLE) != 0)
                {
                    if ((parentRef != 0) && (RcVec3f.DistSqr(parentNode.pos, bestNode.pos) < m_query.raycastLimitSqr))
                    {
                        tryLOS = true;
                    }
                }

                for (int i = bestTile.polyLinks[bestPoly.index]; i != NavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    long neighbourRef = bestTile.links[i].refs;

                    // Skip invalid ids and do not expand back to where we came
                    // from.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Get neighbour poly and tile.
                    // The API input has been cheked already, skip checking internal
                    // data.
                    Tuple<MeshTile, Poly> tileAndPolyUns = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = tileAndPolyUns.Item1;
                    Poly neighbourPoly = tileAndPolyUns.Item2;

                    if (!m_query.filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // get the neighbor node
                    Node neighbourNode = m_nodePool.GetNode(neighbourRef, 0);

                    // do not expand to nodes that were already visited from the
                    // same parent
                    if (neighbourNode.pidx != 0 && neighbourNode.pidx == bestNode.pidx)
                    {
                        continue;
                    }

                    // If the node is visited the first time, calculate node
                    // position.
                    if (neighbourNode.flags == 0)
                    {
                        var midpod = GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly,
                            neighbourTile);
                        if (!midpod.Failed())
                        {
                            neighbourNode.pos = midpod.result;
                        }
                    }

                    // Calculate cost and heuristic.
                    float cost = 0;
                    float heuristic = 0;

                    // raycast parent
                    bool foundShortCut = false;
                    if (tryLOS)
                    {
                        Result<RaycastHit> rayHit = Raycast(parentRef, parentNode.pos, neighbourNode.pos, m_query.filter,
                            DT_RAYCAST_USE_COSTS, grandpaRef);
                        if (rayHit.Succeeded())
                        {
                            foundShortCut = rayHit.result.t >= 1.0f;
                            if (foundShortCut)
                            {
                                // shortcut found using raycast. Using shorter cost
                                // instead
                                cost = parentNode.cost + rayHit.result.pathCost;
                            }
                        }
                    }

                    // update move cost
                    if (!foundShortCut)
                    {
                        // No shortcut found.
                        float curCost = m_query.filter.GetCost(bestNode.pos, neighbourNode.pos, parentRef, parentTile,
                            parentPoly, bestRef, bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);
                        cost = bestNode.cost + curCost;
                    }

                    // Special case for last node.
                    if (neighbourRef == m_query.endRef)
                    {
                        float endCost = m_query.filter.GetCost(neighbourNode.pos, m_query.endPos, bestRef, bestTile,
                            bestPoly, neighbourRef, neighbourTile, neighbourPoly, 0, null, null);

                        cost = cost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        heuristic = RcVec3f.Distance(neighbourNode.pos, m_query.endPos) * H_SCALE;
                    }

                    float total = cost + heuristic;

                    // The node is already in open list and the new result is worse,
                    // skip.
                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    // The node is already visited and process, and the new result
                    // is worse, skip.
                    if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    // Add or update the node.
                    neighbourNode.pidx = foundShortCut ? bestNode.pidx : m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~(Node.DT_NODE_CLOSED | Node.DT_NODE_PARENT_DETACHED));
                    neighbourNode.cost = cost;
                    neighbourNode.total = total;
                    if (foundShortCut)
                    {
                        neighbourNode.flags = (neighbourNode.flags | Node.DT_NODE_PARENT_DETACHED);
                    }

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        // Already in open, update node location.
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        // Put the node in open list.
                        neighbourNode.flags |= Node.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }

                    // Update nearest node to target so far.
                    if (heuristic < m_query.lastBestNodeCost)
                    {
                        m_query.lastBestNodeCost = heuristic;
                        m_query.lastBestNode = neighbourNode;
                    }
                }
            }

            // Exhausted all nodes, but could not find path.
            if (m_openList.IsEmpty())
            {
                m_query.status = Status.PARTIAL_RESULT;
            }

            return Results.Of(m_query.status, iter);
        }

        /// Finalizes and returns the results of a sliced path query.
        /// @param[out] path An ordered list of polygon references representing the path. (Start to end.)
        /// [(polyRef) * @p pathCount]
        /// @returns The status flags for the query.
        public override Result<List<long>> FinalizeSlicedFindPath()
        {
            List<long> path = new List<long>(64);
            if (m_query.status.IsFailed())
            {
                // Reset query.
                m_query = new QueryData();
                return Results.Failure(path);
            }

            if (m_query.startRef == m_query.endRef)
            {
                // Special case: the search starts and ends at same poly.
                path.Add(m_query.startRef);
            }
            else
            {
                // Reverse the path.
                if (m_query.lastBestNode.id != m_query.endRef)
                {
                    m_query.status = Status.PARTIAL_RESULT;
                }

                Node prev = null;
                Node node = m_query.lastBestNode;
                int prevRay = 0;
                do
                {
                    Node next = m_nodePool.GetNodeAtIdx(node.pidx);
                    node.pidx = m_nodePool.GetNodeIdx(prev);
                    prev = node;
                    int nextRay = node.flags & Node.DT_NODE_PARENT_DETACHED; // keep track of whether parent is not adjacent
                    // (i.e. due to raycast shortcut)
                    node.flags = (node.flags & ~Node.DT_NODE_PARENT_DETACHED) | prevRay; // and store it in the reversed
                    // path's node
                    prevRay = nextRay;
                    node = next;
                } while (node != null);

                // Store path
                node = prev;
                do
                {
                    Node next = m_nodePool.GetNodeAtIdx(node.pidx);
                    if ((node.flags & Node.DT_NODE_PARENT_DETACHED) != 0)
                    {
                        Result<RaycastHit> iresult = Raycast(node.id, node.pos, next.pos, m_query.filter, 0, 0);
                        if (iresult.Succeeded())
                        {
                            path.AddRange(iresult.result.path);
                        }

                        // raycast ends on poly boundary and the path might include the next poly boundary.
                        if (path[path.Count - 1] == next.id)
                        {
                            path.RemoveAt(path.Count - 1); // remove to avoid duplicates
                        }
                    }
                    else
                    {
                        path.Add(node.id);
                    }

                    node = next;
                } while (node != null);
            }

            Status status = m_query.status;
            // Reset query.
            m_query = new QueryData();

            return Results.Of(status, path);
        }

        /// Finalizes and returns the results of an incomplete sliced path query, returning the path to the furthest
        /// polygon on the existing path that was visited during the search.
        /// @param[in] existing An array of polygon references for the existing path.
        /// @param[in] existingSize The number of polygon in the @p existing array.
        /// @param[out] path An ordered list of polygon references representing the path. (Start to end.)
        /// [(polyRef) * @p pathCount]
        /// @returns The status flags for the query.
        public override Result<List<long>> FinalizeSlicedFindPathPartial(List<long> existing)
        {
            List<long> path = new List<long>(64);
            if (null == existing || existing.Count <= 0)
            {
                return Results.Failure(path);
            }

            if (m_query.status.IsFailed())
            {
                // Reset query.
                m_query = new QueryData();
                return Results.Failure(path);
            }

            if (m_query.startRef == m_query.endRef)
            {
                // Special case: the search starts and ends at same poly.
                path.Add(m_query.startRef);
            }
            else
            {
                // Find furthest existing node that was visited.
                Node prev = null;
                Node node = null;
                for (int i = existing.Count - 1; i >= 0; --i)
                {
                    node = m_nodePool.FindNode(existing[i]);
                    if (node != null)
                    {
                        break;
                    }
                }

                if (node == null)
                {
                    m_query.status = Status.PARTIAL_RESULT;
                    node = m_query.lastBestNode;
                }

                // Reverse the path.
                int prevRay = 0;
                do
                {
                    Node next = m_nodePool.GetNodeAtIdx(node.pidx);
                    node.pidx = m_nodePool.GetNodeIdx(prev);
                    prev = node;
                    int nextRay = node.flags & Node.DT_NODE_PARENT_DETACHED; // keep track of whether parent is not adjacent
                    // (i.e. due to raycast shortcut)
                    node.flags = (node.flags & ~Node.DT_NODE_PARENT_DETACHED) | prevRay; // and store it in the reversed
                    // path's node
                    prevRay = nextRay;
                    node = next;
                } while (node != null);

                // Store path
                node = prev;
                do
                {
                    Node next = m_nodePool.GetNodeAtIdx(node.pidx);
                    if ((node.flags & Node.DT_NODE_PARENT_DETACHED) != 0)
                    {
                        Result<RaycastHit> iresult = Raycast(node.id, node.pos, next.pos, m_query.filter, 0, 0);
                        if (iresult.Succeeded())
                        {
                            path.AddRange(iresult.result.path);
                        }

                        // raycast ends on poly boundary and the path might include the next poly boundary.
                        if (path[path.Count - 1] == next.id)
                        {
                            path.RemoveAt(path.Count - 1); // remove to avoid duplicates
                        }
                    }
                    else
                    {
                        path.Add(node.id);
                    }

                    node = next;
                } while (node != null);
            }

            Status status = m_query.status;
            // Reset query.
            m_query = new QueryData();

            return Results.Of(status, path);
        }

        public override Result<FindDistanceToWallResult> FindDistanceToWall(long startRef, RcVec3f centerPos, float maxRadius, IQueryFilter filter)
        {
            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !RcVec3f.IsFinite(centerPos) || maxRadius < 0
                || !float.IsFinite(maxRadius) || null == filter)
            {
                return Results.InvalidParam<FindDistanceToWallResult>();
            }

            m_nodePool.Clear();
            m_openList.Clear();

            Node startNode = m_nodePool.GetNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_OPEN;
            m_openList.Push(startNode);

            float radiusSqr = Sqr(maxRadius);
            RcVec3f hitPos = RcVec3f.Zero;
            RcVec3f? bestvj = null;
            RcVec3f? bestvi = null;

            while (!m_openList.IsEmpty())
            {
                Node bestNode = m_openList.Pop();
                bestNode.flags &= ~Node.DT_NODE_OPEN;
                bestNode.flags |= Node.DT_NODE_CLOSED;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                long bestRef = bestNode.id;
                Tuple<MeshTile, Poly> tileAndPoly = m_nav.GetTileAndPolyByRefUnsafe(bestRef);
                MeshTile bestTile = tileAndPoly.Item1;
                Poly bestPoly = tileAndPoly.Item2;

                // Get parent poly and tile.
                long parentRef = 0;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.pidx).id;
                }

                // Hit test walls.
                for (int i = 0, j = bestPoly.vertCount - 1; i < bestPoly.vertCount; j = i++)
                {
                    // Skip non-solid edges.
                    if ((bestPoly.neis[j] & NavMesh.DT_EXT_LINK) != 0)
                    {
                        // Tile border.
                        bool solid = true;
                        for (int k = bestTile.polyLinks[bestPoly.index]; k != NavMesh.DT_NULL_LINK; k = bestTile.links[k].next)
                        {
                            Link link = bestTile.links[k];
                            if (link.edge == j)
                            {
                                if (link.refs != 0)
                                {
                                    Tuple<MeshTile, Poly> linkTileAndPoly = m_nav.GetTileAndPolyByRefUnsafe(link.refs);
                                    MeshTile neiTile = linkTileAndPoly.Item1;
                                    Poly neiPoly = linkTileAndPoly.Item2;
                                    if (filter.PassFilter(link.refs, neiTile, neiPoly))
                                    {
                                        solid = false;
                                    }
                                }

                                break;
                            }
                        }

                        if (!solid)
                        {
                            continue;
                        }
                    }
                    else if (bestPoly.neis[j] != 0)
                    {
                        // Internal edge
                        int idx = (bestPoly.neis[j] - 1);
                        long refs = m_nav.GetPolyRefBase(bestTile) | (long)idx;
                        if (filter.PassFilter(refs, bestTile, bestTile.data.polys[idx]))
                        {
                            continue;
                        }
                    }

                    // Calc distance to the edge.
                    int vj = bestPoly.verts[j] * 3;
                    int vi = bestPoly.verts[i] * 3;
                    var distSqr =  DetourCommon.DistancePtSegSqr2D(centerPos, bestTile.data.verts, vj, vi, out var tseg);
                    // Edge is too far, skip.
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    // Hit wall, update radius.
                    radiusSqr = distSqr;
                    // Calculate hit pos.
                    hitPos.x = bestTile.data.verts[vj] + (bestTile.data.verts[vi] - bestTile.data.verts[vj]) * tseg;
                    hitPos.y = bestTile.data.verts[vj + 1]
                               + (bestTile.data.verts[vi + 1] - bestTile.data.verts[vj + 1]) * tseg;
                    hitPos.z = bestTile.data.verts[vj + 2]
                               + (bestTile.data.verts[vi + 2] - bestTile.data.verts[vj + 2]) * tseg;
                    bestvj = RcVec3f.Of(bestTile.data.verts, vj);
                    bestvi = RcVec3f.Of(bestTile.data.verts, vi);
                }

                for (int i = bestTile.polyLinks[bestPoly.index]; i != NavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    Link link = bestTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour.
                    Tuple<MeshTile, Poly> neighbourTileAndPoly = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = neighbourTileAndPoly.Item1;
                    Poly neighbourPoly = neighbourTileAndPoly.Item2;

                    // Skip off-mesh connections.
                    if (neighbourPoly.GetPolyType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Calc distance to the edge.
                    int va = bestPoly.verts[link.edge] * 3;
                    int vb = bestPoly.verts[(link.edge + 1) % bestPoly.vertCount] * 3;
                    var distSqr = DetourCommon.DistancePtSegSqr2D(centerPos, bestTile.data.verts, va, vb, out var tseg);
                    // If the circle is not touching the next polygon, skip it.
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    Node neighbourNode = m_nodePool.GetNode(neighbourRef);

                    if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        var midPoint = GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly,
                            neighbourTile);
                        if (midPoint.Succeeded())
                        {
                            neighbourNode.pos = midPoint.result;
                        }
                    }

                    float total = bestNode.total + RcVec3f.Distance(bestNode.pos, neighbourNode.pos);

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~Node.DT_NODE_CLOSED);
                    neighbourNode.pidx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags |= Node.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }
                }
            }

            // Calc hit normal.
            RcVec3f hitNormal = new RcVec3f();
            if (bestvi != null && bestvj != null)
            {
                var tangent = bestvi.Value.Subtract(bestvj.Value);
                hitNormal.x = tangent.z;
                hitNormal.y = 0;
                hitNormal.z = -tangent.x;
                hitNormal.Normalize();
            }

            return Results.Success(new FindDistanceToWallResult((float)Math.Sqrt(radiusSqr), hitPos, hitNormal));
        }
    }
}