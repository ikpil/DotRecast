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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour
{
    using static DotRecast.Core.RecastMath;
    using static Node;

    public class NavMeshQuery
    {
        /**
     * Use raycasts during pathfind to "shortcut" (raycast still consider costs) Options for
     * NavMeshQuery::initSlicedFindPath and updateSlicedFindPath
     */
        public const int DT_FINDPATH_ANY_ANGLE = 0x02;

        /** Raycast should calculate movement cost along the ray and fill RaycastHit::cost */
        public const int DT_RAYCAST_USE_COSTS = 0x01;

        /// Vertex flags returned by findStraightPath.
        /** The vertex is the start position in the path. */
        public const int DT_STRAIGHTPATH_START = 0x01;

        /** The vertex is the end position in the path. */
        public const int DT_STRAIGHTPATH_END = 0x02;

        /** The vertex is the start of an off-mesh connection. */
        public const int DT_STRAIGHTPATH_OFFMESH_CONNECTION = 0x04;

        /// Options for findStraightPath.
        public const int DT_STRAIGHTPATH_AREA_CROSSINGS = 0x01;

        /// < Add a vertex at every polygon edge crossing
        /// where area changes.
        public const int DT_STRAIGHTPATH_ALL_CROSSINGS = 0x02;

        /// < Add a vertex at every polygon edge crossing.
        protected readonly NavMesh m_nav;

        protected readonly NodePool m_nodePool;
        protected readonly NodeQueue m_openList;
        protected QueryData m_query;

        /// < Sliced query state.
        public NavMeshQuery(NavMesh nav)
        {
            m_nav = nav;
            m_nodePool = new NodePool();
            m_openList = new NodeQueue();
        }

        public class FRand
        {
            private readonly Random r;

            public FRand()
            {
                r = new Random();
            }

            public FRand(long seed)
            {
                r = new Random((int)seed); // TODO : 랜덤 시드 확인 필요
            }

            public float frand()
            {
                return (float)r.NextDouble();
            }
        }

        /**
     * Returns random location on navmesh. Polygons are chosen weighted by area. The search runs in linear related to
     * number of polygon.
     *
     * @param filter
     *            The polygon filter to apply to the query.
     * @param frand
     *            Function returning a random number [0..1).
     * @return Random location
     */
        public Result<FindRandomPointResult> findRandomPoint(QueryFilter filter, FRand frand)
        {
            // Randomly pick one tile. Assume that all tiles cover roughly the same area.
            if (null == filter || null == frand)
            {
                return Results.invalidParam<FindRandomPointResult>();
            }

            MeshTile tile = null;
            float tsum = 0.0f;
            for (int i = 0; i < m_nav.getMaxTiles(); i++)
            {
                MeshTile mt = m_nav.getTile(i);
                if (mt == null || mt.data == null || mt.data.header == null)
                {
                    continue;
                }

                // Choose random tile using reservoi sampling.
                float area = 1.0f; // Could be tile area too.
                tsum += area;
                float u = frand.frand();
                if (u * tsum <= area)
                {
                    tile = mt;
                }
            }

            if (tile == null)
            {
                return Results.invalidParam<FindRandomPointResult>("Tile not found");
            }

            // Randomly pick one polygon weighted by polygon area.
            Poly poly = null;
            long polyRef = 0;
            long @base = m_nav.getPolyRefBase(tile);

            float areaSum = 0.0f;
            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                Poly p = tile.data.polys[i];
                // Do not return off-mesh connection polygons.
                if (p.getType() != Poly.DT_POLYTYPE_GROUND)
                {
                    continue;
                }

                // Must pass filter
                long refs = @base | i;
                if (!filter.passFilter(refs, tile, p))
                {
                    continue;
                }

                // Calc area of the polygon.
                float polyArea = 0.0f;
                for (int j = 2; j < p.vertCount; ++j)
                {
                    int va = p.verts[0] * 3;
                    int vb = p.verts[j - 1] * 3;
                    int vc = p.verts[j] * 3;
                    polyArea += triArea2D(tile.data.verts, va, vb, vc);
                }

                // Choose random polygon weighted by area, using reservoi sampling.
                areaSum += polyArea;
                float u = frand.frand();
                if (u * areaSum <= polyArea)
                {
                    poly = p;
                    polyRef = refs;
                }
            }

            if (poly == null)
            {
                return Results.invalidParam<FindRandomPointResult>("Poly not found");
            }

            // Randomly pick point on polygon.
            float[] verts = new float[3 * m_nav.getMaxVertsPerPoly()];
            float[] areas = new float[m_nav.getMaxVertsPerPoly()];
            Array.Copy(tile.data.verts, poly.verts[0] * 3, verts, 0, 3);
            for (int j = 1; j < poly.vertCount; ++j)
            {
                Array.Copy(tile.data.verts, poly.verts[j] * 3, verts, j * 3, 3);
            }

            float s = frand.frand();
            float t = frand.frand();

            var pt = randomPointInConvexPoly(verts, poly.vertCount, areas, s, t);
            ClosestPointOnPolyResult closest = closestPointOnPoly(polyRef, pt).result;
            return Results.success(new FindRandomPointResult(polyRef, closest.getClosest()));
        }

        /**
     * Returns random location on navmesh within the reach of specified location. Polygons are chosen weighted by area.
     * The search runs in linear related to number of polygon. The location is not exactly constrained by the circle,
     * but it limits the visited polygons.
     *
     * @param startRef
     *            The reference id of the polygon where the search starts.
     * @param centerPos
     *            The center of the search circle. [(x, y, z)]
     * @param maxRadius
     * @param filter
     *            The polygon filter to apply to the query.
     * @param frand
     *            Function returning a random number [0..1).
     * @return Random location
     */
        public Result<FindRandomPointResult> findRandomPointAroundCircle(long startRef, Vector3f centerPos, float maxRadius,
            QueryFilter filter, FRand frand)
        {
            return findRandomPointAroundCircle(startRef, centerPos, maxRadius, filter, frand, PolygonByCircleConstraint.noop());
        }

        /**
     * Returns random location on navmesh within the reach of specified location. Polygons are chosen weighted by area.
     * The search runs in linear related to number of polygon. The location is strictly constrained by the circle.
     *
     * @param startRef
     *            The reference id of the polygon where the search starts.
     * @param centerPos
     *            The center of the search circle. [(x, y, z)]
     * @param maxRadius
     * @param filter
     *            The polygon filter to apply to the query.
     * @param frand
     *            Function returning a random number [0..1).
     * @return Random location
     */
        public Result<FindRandomPointResult> findRandomPointWithinCircle(long startRef, Vector3f centerPos, float maxRadius,
            QueryFilter filter, FRand frand)
        {
            return findRandomPointAroundCircle(startRef, centerPos, maxRadius, filter, frand, PolygonByCircleConstraint.strict());
        }

        public Result<FindRandomPointResult> findRandomPointAroundCircle(long startRef, Vector3f centerPos, float maxRadius,
            QueryFilter filter, FRand frand, PolygonByCircleConstraint constraint)
        {
            // Validate input
            if (!m_nav.isValidPolyRef(startRef) || !vIsFinite(centerPos) || maxRadius < 0
                || !float.IsFinite(maxRadius) || null == filter || null == frand)
            {
                return Results.invalidParam<FindRandomPointResult>();
            }

            Tuple<MeshTile, Poly> tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(startRef);
            MeshTile startTile = tileAndPoly.Item1;
            Poly startPoly = tileAndPoly.Item2;
            if (!filter.passFilter(startRef, startTile, startPoly))
            {
                return Results.invalidParam<FindRandomPointResult>("Invalid start ref");
            }

            m_nodePool.clear();
            m_openList.clear();

            Node startNode = m_nodePool.getNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = DT_NODE_OPEN;
            m_openList.push(startNode);

            float radiusSqr = maxRadius * maxRadius;
            float areaSum = 0.0f;

            Poly randomPoly = null;
            long randomPolyRef = 0;
            float[] randomPolyVerts = null;

            while (!m_openList.isEmpty())
            {
                Node bestNode = m_openList.pop();
                bestNode.flags &= ~DT_NODE_OPEN;
                bestNode.flags |= DT_NODE_CLOSED;
                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                long bestRef = bestNode.id;
                Tuple<MeshTile, Poly> bestTilePoly = m_nav.getTileAndPolyByRefUnsafe(bestRef);
                MeshTile bestTile = bestTilePoly.Item1;
                Poly bestPoly = bestTilePoly.Item2;

                // Place random locations on on ground.
                if (bestPoly.getType() == Poly.DT_POLYTYPE_GROUND)
                {
                    // Calc area of the polygon.
                    float polyArea = 0.0f;
                    float[] polyVerts = new float[bestPoly.vertCount * 3];
                    for (int j = 0; j < bestPoly.vertCount; ++j)
                    {
                        Array.Copy(bestTile.data.verts, bestPoly.verts[j] * 3, polyVerts, j * 3, 3);
                    }

                    float[] constrainedVerts = constraint.aply(polyVerts, centerPos, maxRadius);
                    if (constrainedVerts != null)
                    {
                        int vertCount = constrainedVerts.Length / 3;
                        for (int j = 2; j < vertCount; ++j)
                        {
                            int va = 0;
                            int vb = (j - 1) * 3;
                            int vc = j * 3;
                            polyArea += triArea2D(constrainedVerts, va, vb, vc);
                        }

                        // Choose random polygon weighted by area, using reservoi sampling.
                        areaSum += polyArea;
                        float u = frand.frand();
                        if (u * areaSum <= polyArea)
                        {
                            randomPoly = bestPoly;
                            randomPolyRef = bestRef;
                            randomPolyVerts = constrainedVerts;
                        }
                    }
                }

                // Get parent poly and tile.
                long parentRef = 0;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.getNodeAtIdx(bestNode.pidx).id;
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

                    // Expand to neighbour
                    Tuple<MeshTile, Poly> neighbourTilePoly = m_nav.getTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = neighbourTilePoly.Item1;
                    Poly neighbourPoly = neighbourTilePoly.Item2;

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.passFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    Result<PortalResult> portalpoints = getPortalPoints(bestRef, bestPoly, bestTile, neighbourRef,
                        neighbourPoly, neighbourTile, 0, 0);
                    if (portalpoints.failed())
                    {
                        continue;
                    }

                    var va = portalpoints.result.left;
                    var vb = portalpoints.result.right;

                    // If the circle is not touching the next polygon, skip it.
                    var distseg = distancePtSegSqr2D(centerPos, va, vb);
                    float distSqr = distseg.Item1;
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    Node neighbourNode = m_nodePool.getNode(neighbourRef);

                    if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        neighbourNode.pos = vLerp(va, vb, 0.5f);
                    }

                    float total = bestNode.total + vDist(bestNode.pos, neighbourNode.pos);

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~Node.DT_NODE_CLOSED);
                    neighbourNode.pidx = m_nodePool.getNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        m_openList.modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags = Node.DT_NODE_OPEN;
                        m_openList.push(neighbourNode);
                    }
                }
            }

            if (randomPoly == null)
            {
                return Results.failure<FindRandomPointResult>();
            }

            // Randomly pick point on polygon.
            float s = frand.frand();
            float t = frand.frand();

            float[] areas = new float[randomPolyVerts.Length / 3];
            Vector3f pt = randomPointInConvexPoly(randomPolyVerts, randomPolyVerts.Length / 3, areas, s, t);
            ClosestPointOnPolyResult closest = closestPointOnPoly(randomPolyRef, pt).result;
            return Results.success(new FindRandomPointResult(randomPolyRef, closest.getClosest()));
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        /// @par
        ///
        /// Uses the detail polygons to find the surface height. (Most accurate.)
        ///
        /// @p pos does not have to be within the bounds of the polygon or navigation mesh.
        ///
        /// See closestPointOnPolyBoundary() for a limited but faster option.
        ///
        /// Finds the closest point on the specified polygon.
        /// @param[in] ref The reference id of the polygon.
        /// @param[in] pos The position to check. [(x, y, z)]
        /// @param[out] closest
        /// @param[out] posOverPoly
        /// @returns The status flags for the query.
        public Result<ClosestPointOnPolyResult> closestPointOnPoly(long refs, Vector3f pos)
        {
            if (!m_nav.isValidPolyRef(refs) || !vIsFinite(pos))
            {
                return Results.invalidParam<ClosestPointOnPolyResult>();
            }

            return Results.success(m_nav.closestPointOnPoly(refs, pos));
        }

        /// @par
        ///
        /// Much faster than closestPointOnPoly().
        ///
        /// If the provided position lies within the polygon's xz-bounds (above or below),
        /// then @p pos and @p closest will be equal.
        ///
        /// The height of @p closest will be the polygon boundary. The height detail is not used.
        ///
        /// @p pos does not have to be within the bounds of the polybon or the navigation mesh.
        ///
        /// Returns a point on the boundary closest to the source point if the source point is outside the
        /// polygon's xz-bounds.
        /// @param[in] ref The reference id to the polygon.
        /// @param[in] pos The position to check. [(x, y, z)]
        /// @param[out] closest The closest point. [(x, y, z)]
        /// @returns The status flags for the query.
        public Result<Vector3f> closestPointOnPolyBoundary(long refs, Vector3f pos)
        {
            Result<Tuple<MeshTile, Poly>> tileAndPoly = m_nav.getTileAndPolyByRef(refs);
            if (tileAndPoly.failed())
            {
                return Results.of<Vector3f>(tileAndPoly.status, tileAndPoly.message);
            }

            MeshTile tile = tileAndPoly.result.Item1;
            Poly poly = tileAndPoly.result.Item2;
            if (tile == null)
            {
                return Results.invalidParam<Vector3f>("Invalid tile");
            }

            if (!vIsFinite(pos))
            {
                return Results.invalidParam<Vector3f>();
            }

            // Collect vertices.
            float[] verts = new float[m_nav.getMaxVertsPerPoly() * 3];
            float[] edged = new float[m_nav.getMaxVertsPerPoly()];
            float[] edget = new float[m_nav.getMaxVertsPerPoly()];
            int nv = poly.vertCount;
            for (int i = 0; i < nv; ++i)
            {
                Array.Copy(tile.data.verts, poly.verts[i] * 3, verts, i * 3, 3);
            }

            Vector3f closest;
            if (distancePtPolyEdgesSqr(pos, verts, nv, edged, edget))
            {
                closest = pos;
            }
            else
            {
                // Point is outside the polygon, dtClamp to nearest edge.
                float dmin = edged[0];
                int imin = 0;
                for (int i = 1; i < nv; ++i)
                {
                    if (edged[i] < dmin)
                    {
                        dmin = edged[i];
                        imin = i;
                    }
                }

                int va = imin * 3;
                int vb = ((imin + 1) % nv) * 3;
                closest = vLerp(verts, va, vb, edget[imin]);
            }

            return Results.success(closest);
        }

        /// @par
        ///
        /// Will return #DT_FAILURE if the provided position is outside the xz-bounds
        /// of the polygon.
        ///
        /// Gets the height of the polygon at the provided position using the height detail. (Most accurate.)
        /// @param[in] ref The reference id of the polygon.
        /// @param[in] pos A position within the xz-bounds of the polygon. [(x, y, z)]
        /// @param[out] height The height at the surface of the polygon.
        /// @returns The status flags for the query.
        public Result<float> getPolyHeight(long refs, Vector3f pos)
        {
            Result<Tuple<MeshTile, Poly>> tileAndPoly = m_nav.getTileAndPolyByRef(refs);
            if (tileAndPoly.failed())
            {
                return Results.of<float>(tileAndPoly.status, tileAndPoly.message);
            }

            MeshTile tile = tileAndPoly.result.Item1;
            Poly poly = tileAndPoly.result.Item2;

            if (!vIsFinite2D(pos))
            {
                return Results.invalidParam<float>();
            }

            // We used to return success for offmesh connections, but the
            // getPolyHeight in DetourNavMesh does not do this, so special
            // case it here.
            if (poly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                int i = poly.verts[0] * 3;
                var v0 = new Vector3f { x = tile.data.verts[i], y = tile.data.verts[i + 1], z = tile.data.verts[i + 2] };
                i = poly.verts[1] * 3;
                var v1 = new Vector3f { x = tile.data.verts[i], y = tile.data.verts[i + 1], z = tile.data.verts[i + 2] };
                var dt = distancePtSegSqr2D(pos, v0, v1);
                return Results.success(v0[1] + (v1[1] - v0[1]) * dt.Item2);
            }

            float? height = m_nav.getPolyHeight(tile, poly, pos);
            return null != height ? Results.success(height.Value) : Results.invalidParam<float>();
        }

        /**
     * Finds the polygon nearest to the specified center point. If center and nearestPt point to an equal position,
     * isOverPoly will be true; however there's also a special case of climb height inside the polygon
     *
     * @param center
     *            The center of the search box. [(x, y, z)]
     * @param halfExtents
     *            The search distance along each axis. [(x, y, z)]
     * @param filter
     *            The polygon filter to apply to the query.
     * @return FindNearestPolyResult containing nearestRef, nearestPt and overPoly
     */
        public Result<FindNearestPolyResult> findNearestPoly(Vector3f center, Vector3f halfExtents, QueryFilter filter)
        {
            // Get nearby polygons from proximity grid.
            FindNearestPolyQuery query = new FindNearestPolyQuery(this, center);
            Status status = queryPolygons(center, halfExtents, filter, query);
            if (status.isFailed())
            {
                return Results.of<FindNearestPolyResult>(status, "");
            }

            return Results.success(query.result());
        }

        // FIXME: (PP) duplicate?
        protected void queryPolygonsInTile(MeshTile tile, Vector3f qmin, Vector3f qmax, QueryFilter filter, PolyQuery query)
        {
            if (tile.data.bvTree != null)
            {
                int nodeIndex = 0;
                var tbmin = tile.data.header.bmin;
                var tbmax = tile.data.header.bmax;
                float qfac = tile.data.header.bvQuantFactor;
                // Calculate quantized box
                int[] bmin = new int[3];
                int[] bmax = new int[3];
                // dtClamp query box to world box.
                float minx = clamp(qmin[0], tbmin[0], tbmax[0]) - tbmin[0];
                float miny = clamp(qmin[1], tbmin[1], tbmax[1]) - tbmin[1];
                float minz = clamp(qmin[2], tbmin[2], tbmax[2]) - tbmin[2];
                float maxx = clamp(qmax[0], tbmin[0], tbmax[0]) - tbmin[0];
                float maxy = clamp(qmax[1], tbmin[1], tbmax[1]) - tbmin[1];
                float maxz = clamp(qmax[2], tbmin[2], tbmax[2]) - tbmin[2];
                // Quantize
                bmin[0] = (int)(qfac * minx) & 0x7ffffffe;
                bmin[1] = (int)(qfac * miny) & 0x7ffffffe;
                bmin[2] = (int)(qfac * minz) & 0x7ffffffe;
                bmax[0] = (int)(qfac * maxx + 1) | 1;
                bmax[1] = (int)(qfac * maxy + 1) | 1;
                bmax[2] = (int)(qfac * maxz + 1) | 1;

                // Traverse tree
                long @base = m_nav.getPolyRefBase(tile);
                int end = tile.data.header.bvNodeCount;
                while (nodeIndex < end)
                {
                    BVNode node = tile.data.bvTree[nodeIndex];
                    bool overlap = overlapQuantBounds(bmin, bmax, node.bmin, node.bmax);
                    bool isLeafNode = node.i >= 0;

                    if (isLeafNode && overlap)
                    {
                        long refs = @base | node.i;
                        if (filter.passFilter(refs, tile, tile.data.polys[node.i]))
                        {
                            query.process(tile, tile.data.polys[node.i], refs);
                        }
                    }

                    if (overlap || isLeafNode)
                    {
                        nodeIndex++;
                    }
                    else
                    {
                        int escapeIndex = -node.i;
                        nodeIndex += escapeIndex;
                    }
                }
            }
            else
            {
                Vector3f bmin = new Vector3f();
                Vector3f bmax = new Vector3f();
                long @base = m_nav.getPolyRefBase(tile);
                for (int i = 0; i < tile.data.header.polyCount; ++i)
                {
                    Poly p = tile.data.polys[i];
                    // Do not return off-mesh connection polygons.
                    if (p.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    long refs = @base | i;
                    if (!filter.passFilter(refs, tile, p))
                    {
                        continue;
                    }

                    // Calc polygon bounds.
                    int v = p.verts[0] * 3;
                    vCopy(ref bmin, tile.data.verts, v);
                    vCopy(ref bmax, tile.data.verts, v);
                    for (int j = 1; j < p.vertCount; ++j)
                    {
                        v = p.verts[j] * 3;
                        vMin(ref bmin, tile.data.verts, v);
                        vMax(ref bmax, tile.data.verts, v);
                    }

                    if (overlapBounds(qmin, qmax, bmin, bmax))
                    {
                        query.process(tile, p, refs);
                    }
                }
            }
        }

        /**
     * Finds polygons that overlap the search box.
     *
     * If no polygons are found, the function will return with a polyCount of zero.
     *
     * @param center
     *            The center of the search box. [(x, y, z)]
     * @param halfExtents
     *            The search distance along each axis. [(x, y, z)]
     * @param filter
     *            The polygon filter to apply to the query.
     * @return The reference ids of the polygons that overlap the query box.
     */
        public Status queryPolygons(Vector3f center, Vector3f halfExtents, QueryFilter filter, PolyQuery query)
        {
            if (!vIsFinite(center) || !vIsFinite(halfExtents) || null == filter)
            {
                return Status.FAILURE_INVALID_PARAM;
            }

            // Find tiles the query touches.
            Vector3f bmin = vSub(center, halfExtents);
            Vector3f bmax = vAdd(center, halfExtents);
            foreach (var t in queryTiles(center, halfExtents))
            {
                queryPolygonsInTile(t, bmin, bmax, filter, query);
            }

            return Status.SUCCSESS;
        }

        /**
     * Finds tiles that overlap the search box.
     */
        public IList<MeshTile> queryTiles(Vector3f center, Vector3f halfExtents)
        {
            if (!vIsFinite(center) || !vIsFinite(halfExtents))
            {
                return ImmutableArray<MeshTile>.Empty;
            }

            Vector3f bmin = vSub(center, halfExtents);
            Vector3f bmax = vAdd(center, halfExtents);
            int[] minxy = m_nav.calcTileLoc(bmin);
            int minx = minxy[0];
            int miny = minxy[1];
            int[] maxxy = m_nav.calcTileLoc(bmax);
            int maxx = maxxy[0];
            int maxy = maxxy[1];
            List<MeshTile> tiles = new List<MeshTile>();
            for (int y = miny; y <= maxy; ++y)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    tiles.AddRange(m_nav.getTilesAt(x, y));
                }
            }

            return tiles;
        }

        /**
     * Finds a path from the start polygon to the end polygon.
     *
     * If the end polygon cannot be reached through the navigation graph, the last polygon in the path will be the
     * nearest the end polygon.
     *
     * The start and end positions are used to calculate traversal costs. (The y-values impact the result.)
     *
     * @param startRef
     *            The refrence id of the start polygon.
     * @param endRef
     *            The reference id of the end polygon.
     * @param startPos
     *            A position within the start polygon. [(x, y, z)]
     * @param endPos
     *            A position within the end polygon. [(x, y, z)]
     * @param filter
     *            The polygon filter to apply to the query.
     * @return Found path
     */
        public virtual Result<List<long>> findPath(long startRef, long endRef, Vector3f startPos, Vector3f endPos, QueryFilter filter)
        {
            return findPath(startRef, endRef, startPos, endPos, filter, new DefaultQueryHeuristic(), 0, 0);
        }

        public virtual Result<List<long>> findPath(long startRef, long endRef, Vector3f startPos, Vector3f endPos, QueryFilter filter,
            int options, float raycastLimit)
        {
            return findPath(startRef, endRef, startPos, endPos, filter, new DefaultQueryHeuristic(), options, raycastLimit);
        }

        public Result<List<long>> findPath(long startRef, long endRef, Vector3f startPos, Vector3f endPos, QueryFilter filter,
            QueryHeuristic heuristic, int options, float raycastLimit)
        {
            // Validate input
            if (!m_nav.isValidPolyRef(startRef) || !m_nav.isValidPolyRef(endRef) || !vIsFinite(startPos) || !vIsFinite(endPos) || null == filter)
            {
                return Results.invalidParam<List<long>>();
            }

            float raycastLimitSqr = sqr(raycastLimit);

            // trade quality with performance?
            if ((options & DT_FINDPATH_ANY_ANGLE) != 0 && raycastLimit < 0f)
            {
                // limiting to several times the character radius yields nice results. It is not sensitive
                // so it is enough to compute it from the first tile.
                MeshTile tile = m_nav.getTileByRef(startRef);
                float agentRadius = tile.data.header.walkableRadius;
                raycastLimitSqr = sqr(agentRadius * NavMesh.DT_RAY_CAST_LIMIT_PROPORTIONS);
            }

            if (startRef == endRef)
            {
                List<long> singlePath = new List<long>(1);
                singlePath.Add(startRef);
                return Results.success(singlePath);
            }

            m_nodePool.clear();
            m_openList.clear();

            Node startNode = m_nodePool.getNode(startRef);
            startNode.pos = startPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = heuristic.getCost(startPos, endPos);
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_OPEN;
            m_openList.push(startNode);

            Node lastBestNode = startNode;
            float lastBestNodeCost = startNode.total;

            Status status = Status.SUCCSESS;

            while (!m_openList.isEmpty())
            {
                // Remove node from open list and put it in closed list.
                Node bestNode = m_openList.pop();
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
                Tuple<MeshTile, Poly> tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(bestRef);
                MeshTile bestTile = tileAndPoly.Item1;
                Poly bestPoly = tileAndPoly.Item2;

                // Get parent poly and tile.
                long parentRef = 0, grandpaRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                Node parentNode = null;
                if (bestNode.pidx != 0)
                {
                    parentNode = m_nodePool.getNodeAtIdx(bestNode.pidx);
                    parentRef = parentNode.id;
                    if (parentNode.pidx != 0)
                    {
                        grandpaRef = m_nodePool.getNodeAtIdx(parentNode.pidx).id;
                    }
                }

                if (parentRef != 0)
                {
                    tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(parentRef);
                    parentTile = tileAndPoly.Item1;
                    parentPoly = tileAndPoly.Item2;
                }

                // decide whether to test raycast to previous nodes
                bool tryLOS = false;
                if ((options & DT_FINDPATH_ANY_ANGLE) != 0)
                {
                    if ((parentRef != 0) && (raycastLimitSqr >= float.MaxValue
                                             || vDistSqr(parentNode.pos, bestNode.pos) < raycastLimitSqr))
                    {
                        tryLOS = true;
                    }
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
                    tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = tileAndPoly.Item1;
                    Poly neighbourPoly = tileAndPoly.Item2;

                    if (!filter.passFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // get the node
                    Node neighbourNode = m_nodePool.getNode(neighbourRef, 0);

                    // do not expand to nodes that were already visited from the
                    // same parent
                    if (neighbourNode.pidx != 0 && neighbourNode.pidx == bestNode.pidx)
                    {
                        continue;
                    }

                    // If the node is visited the first time, calculate node position.
                    var neighbourPos = neighbourNode.pos;
                    var midpod = neighbourRef == endRef
                        ? getEdgeIntersectionPoint(bestNode.pos, bestRef, bestPoly, bestTile, endPos, neighbourRef,
                            neighbourPoly, neighbourTile)
                        : getEdgeMidPoint(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly, neighbourTile);
                    if (!midpod.failed())
                    {
                        neighbourPos = midpod.result;
                    }

                    // Calculate cost and heuristic.
                    float cost = 0;
                    float heuristicCost = 0;

                    // raycast parent
                    bool foundShortCut = false;
                    List<long> shortcut = null;
                    if (tryLOS)
                    {
                        Result<RaycastHit> rayHit = raycast(parentRef, parentNode.pos, neighbourPos, filter,
                            DT_RAYCAST_USE_COSTS, grandpaRef);
                        if (rayHit.succeeded())
                        {
                            foundShortCut = rayHit.result.t >= 1.0f;
                            if (foundShortCut)
                            {
                                shortcut = rayHit.result.path;
                                // shortcut found using raycast. Using shorter cost
                                // instead
                                cost = parentNode.cost + rayHit.result.pathCost;
                            }
                        }
                    }

                    // update move cost
                    if (!foundShortCut)
                    {
                        float curCost = filter.getCost(bestNode.pos, neighbourPos, parentRef, parentTile,
                            parentPoly, bestRef, bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);
                        cost = bestNode.cost + curCost;
                    }

                    // Special case for last node.
                    if (neighbourRef == endRef)
                    {
                        // Cost
                        float endCost = filter.getCost(neighbourPos, endPos, bestRef, bestTile, bestPoly, neighbourRef,
                            neighbourTile, neighbourPoly, 0L, null, null);
                        cost = cost + endCost;
                    }
                    else
                    {
                        // Cost
                        heuristicCost = heuristic.getCost(neighbourPos, endPos);
                    }

                    float total = cost + heuristicCost;

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
                    neighbourNode.pidx = foundShortCut ? bestNode.pidx : m_nodePool.getNodeIdx(bestNode);
                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~Node.DT_NODE_CLOSED);
                    neighbourNode.cost = cost;
                    neighbourNode.total = total;
                    neighbourNode.pos = neighbourPos;
                    neighbourNode.shortcut = shortcut;

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        // Already in open, update node location.
                        m_openList.modify(neighbourNode);
                    }
                    else
                    {
                        // Put the node in open list.
                        neighbourNode.flags |= Node.DT_NODE_OPEN;
                        m_openList.push(neighbourNode);
                    }

                    // Update nearest node to target so far.
                    if (heuristicCost < lastBestNodeCost)
                    {
                        lastBestNodeCost = heuristicCost;
                        lastBestNode = neighbourNode;
                    }
                }
            }

            List<long> path = getPathToNode(lastBestNode);

            if (lastBestNode.id != endRef)
            {
                status = Status.PARTIAL_RESULT;
            }

            return Results.of(status, path);
        }

        /**
     * Intializes a sliced path query.
     *
     * Common use case: -# Call initSlicedFindPath() to initialize the sliced path query. -# Call updateSlicedFindPath()
     * until it returns complete. -# Call finalizeSlicedFindPath() to get the path.
     *
     * @param startRef
     *            The reference id of the start polygon.
     * @param endRef
     *            The reference id of the end polygon.
     * @param startPos
     *            A position within the start polygon. [(x, y, z)]
     * @param endPos
     *            A position within the end polygon. [(x, y, z)]
     * @param filter
     *            The polygon filter to apply to the query.
     * @param options
     *            query options (see: #FindPathOptions)
     * @return
     */
        public Status initSlicedFindPath(long startRef, long endRef, Vector3f startPos, Vector3f endPos, QueryFilter filter,
            int options)
        {
            return initSlicedFindPath(startRef, endRef, startPos, endPos, filter, options, new DefaultQueryHeuristic(), -1.0f);
        }

        public Status initSlicedFindPath(long startRef, long endRef, Vector3f startPos, Vector3f endPos, QueryFilter filter,
            int options, float raycastLimit)
        {
            return initSlicedFindPath(startRef, endRef, startPos, endPos, filter, options, new DefaultQueryHeuristic(), raycastLimit);
        }

        public Status initSlicedFindPath(long startRef, long endRef, Vector3f startPos, Vector3f endPos, QueryFilter filter,
            int options, QueryHeuristic heuristic, float raycastLimit)
        {
            // Init path state.
            m_query = new QueryData();
            m_query.status = Status.FAILURE;
            m_query.startRef = startRef;
            m_query.endRef = endRef;
            m_query.startPos = startPos;
            m_query.endPos = endPos;
            m_query.filter = filter;
            m_query.options = options;
            m_query.heuristic = heuristic;
            m_query.raycastLimitSqr = sqr(raycastLimit);

            // Validate input
            if (!m_nav.isValidPolyRef(startRef) || !m_nav.isValidPolyRef(endRef) || !vIsFinite(startPos) || !vIsFinite(endPos) || null == filter)
            {
                return Status.FAILURE_INVALID_PARAM;
            }

            // trade quality with performance?
            if ((options & DT_FINDPATH_ANY_ANGLE) != 0 && raycastLimit < 0f)
            {
                // limiting to several times the character radius yields nice results. It is not sensitive
                // so it is enough to compute it from the first tile.
                MeshTile tile = m_nav.getTileByRef(startRef);
                float agentRadius = tile.data.header.walkableRadius;
                m_query.raycastLimitSqr = sqr(agentRadius * NavMesh.DT_RAY_CAST_LIMIT_PROPORTIONS);
            }

            if (startRef == endRef)
            {
                m_query.status = Status.SUCCSESS;
                return Status.SUCCSESS;
            }

            m_nodePool.clear();
            m_openList.clear();

            Node startNode = m_nodePool.getNode(startRef);
            startNode.pos = startPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = heuristic.getCost(startPos, endPos);
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_OPEN;
            m_openList.push(startNode);

            m_query.status = Status.IN_PROGRESS;
            m_query.lastBestNode = startNode;
            m_query.lastBestNodeCost = startNode.total;

            return m_query.status;
        }

        /**
     * Updates an in-progress sliced path query.
     *
     * @param maxIter
     *            The maximum number of iterations to perform.
     * @return The status flags for the query.
     */
        public virtual Result<int> updateSlicedFindPath(int maxIter)
        {
            if (!m_query.status.isInProgress())
            {
                return Results.of(m_query.status, 0);
            }

            // Make sure the request is still valid.
            if (!m_nav.isValidPolyRef(m_query.startRef) || !m_nav.isValidPolyRef(m_query.endRef))
            {
                m_query.status = Status.FAILURE;
                return Results.of(m_query.status, 0);
            }

            int iter = 0;
            while (iter < maxIter && !m_openList.isEmpty())
            {
                iter++;

                // Remove node from open list and put it in closed list.
                Node bestNode = m_openList.pop();
                bestNode.flags &= ~Node.DT_NODE_OPEN;
                bestNode.flags |= Node.DT_NODE_CLOSED;

                // Reached the goal, stop searching.
                if (bestNode.id == m_query.endRef)
                {
                    m_query.lastBestNode = bestNode;
                    m_query.status = Status.SUCCSESS;
                    return Results.of(m_query.status, iter);
                }

                // Get current poly and tile.
                // The API input has been cheked already, skip checking internal
                // data.
                long bestRef = bestNode.id;
                Result<Tuple<MeshTile, Poly>> tileAndPoly = m_nav.getTileAndPolyByRef(bestRef);
                if (tileAndPoly.failed())
                {
                    m_query.status = Status.FAILURE;
                    // The polygon has disappeared during the sliced query, fail.
                    return Results.of(m_query.status, iter);
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
                    parentNode = m_nodePool.getNodeAtIdx(bestNode.pidx);
                    parentRef = parentNode.id;
                    if (parentNode.pidx != 0)
                    {
                        grandpaRef = m_nodePool.getNodeAtIdx(parentNode.pidx).id;
                    }
                }

                if (parentRef != 0)
                {
                    bool invalidParent = false;
                    tileAndPoly = m_nav.getTileAndPolyByRef(parentRef);
                    invalidParent = tileAndPoly.failed();
                    if (invalidParent || (grandpaRef != 0 && !m_nav.isValidPolyRef(grandpaRef)))
                    {
                        // The polygon has disappeared during the sliced query,
                        // fail.
                        m_query.status = Status.FAILURE;
                        return Results.of(m_query.status, iter);
                    }

                    parentTile = tileAndPoly.result.Item1;
                    parentPoly = tileAndPoly.result.Item2;
                }

                // decide whether to test raycast to previous nodes
                bool tryLOS = false;
                if ((m_query.options & DT_FINDPATH_ANY_ANGLE) != 0)
                {
                    if ((parentRef != 0) && (m_query.raycastLimitSqr >= float.MaxValue
                                             || vDistSqr(parentNode.pos, bestNode.pos) < m_query.raycastLimitSqr))
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
                    Tuple<MeshTile, Poly> tileAndPolyUns = m_nav.getTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = tileAndPolyUns.Item1;
                    Poly neighbourPoly = tileAndPolyUns.Item2;

                    if (!m_query.filter.passFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // get the neighbor node
                    Node neighbourNode = m_nodePool.getNode(neighbourRef, 0);

                    // do not expand to nodes that were already visited from the
                    // same parent
                    if (neighbourNode.pidx != 0 && neighbourNode.pidx == bestNode.pidx)
                    {
                        continue;
                    }

                    // If the node is visited the first time, calculate node
                    // position.
                    var neighbourPos = neighbourNode.pos;
                    var midpod = neighbourRef == m_query.endRef
                        ? getEdgeIntersectionPoint(bestNode.pos, bestRef, bestPoly, bestTile, m_query.endPos,
                            neighbourRef, neighbourPoly, neighbourTile)
                        : getEdgeMidPoint(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly, neighbourTile);
                    if (!midpod.failed())
                    {
                        neighbourPos = midpod.result;
                    }

                    // Calculate cost and heuristic.
                    float cost = 0;
                    float heuristic = 0;

                    // raycast parent
                    bool foundShortCut = false;
                    List<long> shortcut = null;
                    if (tryLOS)
                    {
                        Result<RaycastHit> rayHit = raycast(parentRef, parentNode.pos, neighbourPos, m_query.filter,
                            DT_RAYCAST_USE_COSTS, grandpaRef);
                        if (rayHit.succeeded())
                        {
                            foundShortCut = rayHit.result.t >= 1.0f;
                            if (foundShortCut)
                            {
                                shortcut = rayHit.result.path;
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
                        float curCost = m_query.filter.getCost(bestNode.pos, neighbourPos, parentRef, parentTile,
                            parentPoly, bestRef, bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);
                        cost = bestNode.cost + curCost;
                    }

                    // Special case for last node.
                    if (neighbourRef == m_query.endRef)
                    {
                        float endCost = m_query.filter.getCost(neighbourPos, m_query.endPos, bestRef, bestTile,
                            bestPoly, neighbourRef, neighbourTile, neighbourPoly, 0, null, null);

                        cost = cost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        heuristic = m_query.heuristic.getCost(neighbourPos, m_query.endPos);
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
                    neighbourNode.pidx = foundShortCut ? bestNode.pidx : m_nodePool.getNodeIdx(bestNode);
                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~Node.DT_NODE_CLOSED);
                    neighbourNode.cost = cost;
                    neighbourNode.total = total;
                    neighbourNode.pos = neighbourPos;
                    neighbourNode.shortcut = shortcut;

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        // Already in open, update node location.
                        m_openList.modify(neighbourNode);
                    }
                    else
                    {
                        // Put the node in open list.
                        neighbourNode.flags |= Node.DT_NODE_OPEN;
                        m_openList.push(neighbourNode);
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
            if (m_openList.isEmpty())
            {
                m_query.status = Status.PARTIAL_RESULT;
            }

            return Results.of(m_query.status, iter);
        }

        /// Finalizes and returns the results of a sliced path query.
        /// @param[out] path An ordered list of polygon references representing the path. (Start to end.)
        /// [(polyRef) * @p pathCount]
        /// @returns The status flags for the query.
        public virtual Result<List<long>> finalizeSlicedFindPath()
        {
            List<long> path = new List<long>(64);
            if (m_query.status.isFailed())
            {
                // Reset query.
                m_query = new QueryData();
                return Results.failure(path);
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

                path = getPathToNode(m_query.lastBestNode);
            }

            Status status = m_query.status;
            // Reset query.
            m_query = new QueryData();

            return Results.of(status, path);
        }

        /// Finalizes and returns the results of an incomplete sliced path query, returning the path to the furthest
        /// polygon on the existing path that was visited during the search.
        /// @param[in] existing An array of polygon references for the existing path.
        /// @param[in] existingSize The number of polygon in the @p existing array.
        /// @param[out] path An ordered list of polygon references representing the path. (Start to end.)
        /// [(polyRef) * @p pathCount]
        /// @returns The status flags for the query.
        public virtual Result<List<long>> finalizeSlicedFindPathPartial(List<long> existing)
        {
            List<long> path = new List<long>(64);
            if (null == existing || existing.Count <= 0)
            {
                return Results.failure(path);
            }

            if (m_query.status.isFailed())
            {
                // Reset query.
                m_query = new QueryData();
                return Results.failure(path);
            }

            if (m_query.startRef == m_query.endRef)
            {
                // Special case: the search starts and ends at same poly.
                path.Add(m_query.startRef);
            }
            else
            {
                // Find furthest existing node that was visited.
                Node node = null;
                for (int i = existing.Count - 1; i >= 0; --i)
                {
                    node = m_nodePool.findNode(existing[i]);
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

                path = getPathToNode(node);
            }

            Status status = m_query.status;
            // Reset query.
            m_query = new QueryData();

            return Results.of(status, path);
        }

        protected Status appendVertex(Vector3f pos, int flags, long refs, List<StraightPathItem> straightPath,
            int maxStraightPath)
        {
            if (straightPath.Count > 0 && vEqual(straightPath[straightPath.Count - 1].pos, pos))
            {
                // The vertices are equal, update flags and poly.
                straightPath[straightPath.Count - 1].flags = flags;
                straightPath[straightPath.Count - 1].refs = refs;
            }
            else
            {
                if (straightPath.Count < maxStraightPath)
                {
                    // Append new vertex.
                    straightPath.Add(new StraightPathItem(pos, flags, refs));
                }

                // If reached end of path or there is no space to append more vertices, return.
                if (flags == DT_STRAIGHTPATH_END || straightPath.Count >= maxStraightPath)
                {
                    return Status.SUCCSESS;
                }
            }

            return Status.IN_PROGRESS;
        }

        protected Status appendPortals(int startIdx, int endIdx, Vector3f endPos, List<long> path,
            List<StraightPathItem> straightPath, int maxStraightPath, int options)
        {
            var startPos = straightPath[straightPath.Count - 1].pos;
            // Append or update last vertex
            Status stat;
            for (int i = startIdx; i < endIdx; i++)
            {
                // Calculate portal
                long from = path[i];
                Result<Tuple<MeshTile, Poly>> tileAndPoly = m_nav.getTileAndPolyByRef(from);
                if (tileAndPoly.failed())
                {
                    return Status.FAILURE;
                }

                MeshTile fromTile = tileAndPoly.result.Item1;
                Poly fromPoly = tileAndPoly.result.Item2;

                long to = path[i + 1];
                tileAndPoly = m_nav.getTileAndPolyByRef(to);
                if (tileAndPoly.failed())
                {
                    return Status.FAILURE;
                }

                MeshTile toTile = tileAndPoly.result.Item1;
                Poly toPoly = tileAndPoly.result.Item2;

                Result<PortalResult> portals = getPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, 0, 0);
                if (portals.failed())
                {
                    break;
                }

                var left = portals.result.left;
                var right = portals.result.right;

                if ((options & DT_STRAIGHTPATH_AREA_CROSSINGS) != 0)
                {
                    // Skip intersection if only area crossings are requested.
                    if (fromPoly.getArea() == toPoly.getArea())
                    {
                        continue;
                    }
                }

                // Append intersection
                Tuple<float, float> interect = intersectSegSeg2D(startPos, endPos, left, right);
                if (null != interect)
                {
                    float t = interect.Item2;
                    var pt = vLerp(left, right, t);
                    stat = appendVertex(pt, 0, path[i + 1], straightPath, maxStraightPath);
                    if (!stat.isInProgress())
                    {
                        return stat;
                    }
                }
            }

            return Status.IN_PROGRESS;
        }

        /// @par
        /// Finds the straight path from the start to the end position within the polygon corridor.
        ///
        /// This method peforms what is often called 'string pulling'.
        ///
        /// The start position is clamped to the first polygon in the path, and the
        /// end position is clamped to the last. So the start and end positions should
        /// normally be within or very near the first and last polygons respectively.
        ///
        /// The returned polygon references represent the reference id of the polygon
        /// that is entered at the associated path position. The reference id associated
        /// with the end point will always be zero. This allows, for example, matching
        /// off-mesh link points to their representative polygons.
        ///
        /// If the provided result buffers are too small for the entire result set,
        /// they will be filled as far as possible from the start toward the end
        /// position.
        ///
        /// @param[in] startPos Path start position. [(x, y, z)]
        /// @param[in] endPos Path end position. [(x, y, z)]
        /// @param[in] path An array of polygon references that represent the path corridor.
        /// @param[out] straightPath Points describing the straight path. [(x, y, z) * @p straightPathCount].
        /// @param[in] maxStraightPath The maximum number of points the straight path arrays can hold. [Limit: > 0]
        /// @param[in] options Query options. (see: #dtStraightPathOptions)
        /// @returns The status flags for the query.
        public virtual Result<List<StraightPathItem>> findStraightPath(Vector3f startPos, Vector3f endPos, List<long> path,
            int maxStraightPath, int options)
        {
            List<StraightPathItem> straightPath = new List<StraightPathItem>();
            if (!vIsFinite(startPos) || !vIsFinite(endPos)
                                     || null == path || 0 == path.Count || path[0] == 0 || maxStraightPath <= 0)
            {
                return Results.invalidParam<List<StraightPathItem>>();
            }

            // TODO: Should this be callers responsibility?
            Result<Vector3f> closestStartPosRes = closestPointOnPolyBoundary(path[0], startPos);
            if (closestStartPosRes.failed())
            {
                return Results.invalidParam<List<StraightPathItem>>("Cannot find start position");
            }

            var closestStartPos = closestStartPosRes.result;
            var closestEndPosRes = closestPointOnPolyBoundary(path[path.Count - 1], endPos);
            if (closestEndPosRes.failed())
            {
                return Results.invalidParam<List<StraightPathItem>>("Cannot find end position");
            }

            var closestEndPos = closestEndPosRes.result;
            // Add start point.
            Status stat = appendVertex(closestStartPos, DT_STRAIGHTPATH_START, path[0], straightPath, maxStraightPath);
            if (!stat.isInProgress())
            {
                return Results.success(straightPath);
            }

            if (path.Count > 1)
            {
                Vector3f portalApex = closestStartPos;
                Vector3f portalLeft = portalApex;
                Vector3f portalRight = portalApex;
                int apexIndex = 0;
                int leftIndex = 0;
                int rightIndex = 0;

                int leftPolyType = 0;
                int rightPolyType = 0;

                long leftPolyRef = path[0];
                long rightPolyRef = path[0];

                for (int i = 0; i < path.Count; ++i)
                {
                    Vector3f left;
                    Vector3f right;
                    int toType;

                    if (i + 1 < path.Count)
                    {
                        // Next portal.
                        Result<PortalResult> portalPoints = getPortalPoints(path[i], path[i + 1]);
                        if (portalPoints.failed())
                        {
                            closestEndPosRes = closestPointOnPolyBoundary(path[i], endPos);
                            if (closestEndPosRes.failed())
                            {
                                return Results.invalidParam<List<StraightPathItem>>();
                            }

                            closestEndPos = closestEndPosRes.result;
                            // Append portals along the current straight path segment.
                            if ((options & (DT_STRAIGHTPATH_AREA_CROSSINGS | DT_STRAIGHTPATH_ALL_CROSSINGS)) != 0)
                            {
                                // Ignore status return value as we're just about to return anyway.
                                appendPortals(apexIndex, i, closestEndPos, path, straightPath, maxStraightPath, options);
                            }

                            // Ignore status return value as we're just about to return anyway.
                            appendVertex(closestEndPos, 0, path[i], straightPath, maxStraightPath);
                            return Results.success(straightPath);
                        }

                        left = portalPoints.result.left;
                        right = portalPoints.result.right;
                        toType = portalPoints.result.toType;

                        // If starting really close the portal, advance.
                        if (i == 0)
                        {
                            Tuple<float, float> dt = distancePtSegSqr2D(portalApex, left, right);
                            if (dt.Item1 < sqr(0.001f))
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        // End of the path.
                        left = closestEndPos;
                        right = closestEndPos;
                        toType = Poly.DT_POLYTYPE_GROUND;
                    }

                    // Right vertex.
                    if (triArea2D(portalApex, portalRight, right) <= 0.0f)
                    {
                        if (vEqual(portalApex, portalRight) || triArea2D(portalApex, portalLeft, right) > 0.0f)
                        {
                            portalRight = right;
                            rightPolyRef = (i + 1 < path.Count) ? path[i + 1] : 0;
                            rightPolyType = toType;
                            rightIndex = i;
                        }
                        else
                        {
                            // Append portals along the current straight path segment.
                            if ((options & (DT_STRAIGHTPATH_AREA_CROSSINGS | DT_STRAIGHTPATH_ALL_CROSSINGS)) != 0)
                            {
                                stat = appendPortals(apexIndex, leftIndex, portalLeft, path, straightPath, maxStraightPath,
                                    options);
                                if (!stat.isInProgress())
                                {
                                    return Results.success(straightPath);
                                }
                            }

                            portalApex = portalLeft;
                            apexIndex = leftIndex;

                            int flags = 0;
                            if (leftPolyRef == 0)
                            {
                                flags = DT_STRAIGHTPATH_END;
                            }
                            else if (leftPolyType == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
                            {
                                flags = DT_STRAIGHTPATH_OFFMESH_CONNECTION;
                            }

                            long refs = leftPolyRef;

                            // Append or update vertex
                            stat = appendVertex(portalApex, flags, refs, straightPath, maxStraightPath);
                            if (!stat.isInProgress())
                            {
                                return Results.success(straightPath);
                            }

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            // Restart
                            i = apexIndex;

                            continue;
                        }
                    }

                    // Left vertex.
                    if (triArea2D(portalApex, portalLeft, left) >= 0.0f)
                    {
                        if (vEqual(portalApex, portalLeft) || triArea2D(portalApex, portalRight, left) < 0.0f)
                        {
                            portalLeft = left;
                            leftPolyRef = (i + 1 < path.Count) ? path[i + 1] : 0;
                            leftPolyType = toType;
                            leftIndex = i;
                        }
                        else
                        {
                            // Append portals along the current straight path segment.
                            if ((options & (DT_STRAIGHTPATH_AREA_CROSSINGS | DT_STRAIGHTPATH_ALL_CROSSINGS)) != 0)
                            {
                                stat = appendPortals(apexIndex, rightIndex, portalRight, path, straightPath,
                                    maxStraightPath, options);
                                if (!stat.isInProgress())
                                {
                                    return Results.success(straightPath);
                                }
                            }

                            portalApex = portalRight;
                            apexIndex = rightIndex;

                            int flags = 0;
                            if (rightPolyRef == 0)
                            {
                                flags = DT_STRAIGHTPATH_END;
                            }
                            else if (rightPolyType == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
                            {
                                flags = DT_STRAIGHTPATH_OFFMESH_CONNECTION;
                            }

                            long refs = rightPolyRef;

                            // Append or update vertex
                            stat = appendVertex(portalApex, flags, refs, straightPath, maxStraightPath);
                            if (!stat.isInProgress())
                            {
                                return Results.success(straightPath);
                            }

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            // Restart
                            i = apexIndex;

                            continue;
                        }
                    }
                }

                // Append portals along the current straight path segment.
                if ((options & (DT_STRAIGHTPATH_AREA_CROSSINGS | DT_STRAIGHTPATH_ALL_CROSSINGS)) != 0)
                {
                    stat = appendPortals(apexIndex, path.Count - 1, closestEndPos, path, straightPath, maxStraightPath,
                        options);
                    if (!stat.isInProgress())
                    {
                        return Results.success(straightPath);
                    }
                }
            }

            // Ignore status return value as we're just about to return anyway.
            appendVertex(closestEndPos, DT_STRAIGHTPATH_END, 0, straightPath, maxStraightPath);
            return Results.success(straightPath);
        }

        /// @par
        ///
        /// This method is optimized for small delta movement and a small number of
        /// polygons. If used for too great a distance, the result set will form an
        /// incomplete path.
        ///
        /// @p resultPos will equal the @p endPos if the end is reached.
        /// Otherwise the closest reachable position will be returned.
        ///
        /// @p resultPos is not projected onto the surface of the navigation
        /// mesh. Use #getPolyHeight if this is needed.
        ///
        /// This method treats the end position in the same manner as
        /// the #raycast method. (As a 2D point.) See that method's documentation
        /// for details.
        ///
        /// If the @p visited array is too small to hold the entire result set, it will
        /// be filled as far as possible from the start position toward the end
        /// position.
        ///
        /// Moves from the start to the end position constrained to the navigation mesh.
        /// @param[in] startRef The reference id of the start polygon.
        /// @param[in] startPos A position of the mover within the start polygon. [(x, y, x)]
        /// @param[in] endPos The desired end position of the mover. [(x, y, z)]
        /// @param[in] filter The polygon filter to apply to the query.
        /// @returns Path
        public Result<MoveAlongSurfaceResult> moveAlongSurface(long startRef, Vector3f startPos, Vector3f endPos, QueryFilter filter)
        {
            // Validate input
            if (!m_nav.isValidPolyRef(startRef) || !vIsFinite(startPos)
                                                || !vIsFinite(endPos) || null == filter)
            {
                return Results.invalidParam<MoveAlongSurfaceResult>();
            }

            NodePool tinyNodePool = new NodePool();

            Node startNode = tinyNodePool.getNode(startRef);
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_CLOSED;
            LinkedList<Node> stack = new LinkedList<Node>();
            stack.AddLast(startNode);

            Vector3f bestPos = new Vector3f();
            float bestDist = float.MaxValue;
            Node bestNode = null;
            bestPos = startPos;

            // Search constraints
            var searchPos = vLerp(startPos, endPos, 0.5f);
            float searchRadSqr = sqr(vDist(startPos, endPos) / 2.0f + 0.001f);

            float[] verts = new float[m_nav.getMaxVertsPerPoly() * 3];

            while (0 < stack.Count)
            {
                // Pop front.
                Node curNode = stack.First?.Value;
                stack.RemoveFirst();

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                long curRef = curNode.id;
                Tuple<MeshTile, Poly> tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(curRef);
                MeshTile curTile = tileAndPoly.Item1;
                Poly curPoly = tileAndPoly.Item2;

                // Collect vertices.
                int nverts = curPoly.vertCount;
                for (int i = 0; i < nverts; ++i)
                {
                    Array.Copy(curTile.data.verts, curPoly.verts[i] * 3, verts, i * 3, 3);
                }

                // If target is inside the poly, stop search.
                if (pointInPolygon(endPos, verts, nverts))
                {
                    bestNode = curNode;
                    bestPos = endPos;
                    break;
                }

                // Find wall edges and find nearest point inside the walls.
                for (int i = 0, j = curPoly.vertCount - 1; i < curPoly.vertCount; j = i++)
                {
                    // Find links to neighbours.
                    int MAX_NEIS = 8;
                    int nneis = 0;
                    long[] neis = new long[MAX_NEIS];

                    if ((curPoly.neis[j] & NavMesh.DT_EXT_LINK) != 0)
                    {
                        // Tile border.
                        for (int k = curTile.polyLinks[curPoly.index]; k != NavMesh.DT_NULL_LINK; k = curTile.links[k].next)
                        {
                            Link link = curTile.links[k];
                            if (link.edge == j)
                            {
                                if (link.refs != 0)
                                {
                                    tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(link.refs);
                                    MeshTile neiTile = tileAndPoly.Item1;
                                    Poly neiPoly = tileAndPoly.Item2;
                                    if (filter.passFilter(link.refs, neiTile, neiPoly))
                                    {
                                        if (nneis < MAX_NEIS)
                                        {
                                            neis[nneis++] = link.refs;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (curPoly.neis[j] != 0)
                    {
                        int idx = curPoly.neis[j] - 1;
                        long refs = m_nav.getPolyRefBase(curTile) | idx;
                        if (filter.passFilter(refs, curTile, curTile.data.polys[idx]))
                        {
                            // Internal edge, encode id.
                            neis[nneis++] = refs;
                        }
                    }

                    if (nneis == 0)
                    {
                        // Wall edge, calc distance.
                        int vj = j * 3;
                        int vi = i * 3;
                        Tuple<float, float> distSeg = distancePtSegSqr2D(endPos, verts, vj, vi);
                        float distSqr = distSeg.Item1;
                        float tseg = distSeg.Item2;
                        if (distSqr < bestDist)
                        {
                            // Update nearest distance.
                            bestPos = vLerp(verts, vj, vi, tseg);
                            bestDist = distSqr;
                            bestNode = curNode;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < nneis; ++k)
                        {
                            Node neighbourNode = tinyNodePool.getNode(neis[k]);
                            // Skip if already visited.
                            if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0)
                            {
                                continue;
                            }

                            // Skip the link if it is too far from search constraint.
                            // TODO: Maybe should use getPortalPoints(), but this one is way faster.
                            int vj = j * 3;
                            int vi = i * 3;
                            Tuple<float, float> distseg = distancePtSegSqr2D(searchPos, verts, vj, vi);
                            float distSqr = distseg.Item1;
                            if (distSqr > searchRadSqr)
                            {
                                continue;
                            }

                            // Mark as the node as visited and push to queue.
                            neighbourNode.pidx = tinyNodePool.getNodeIdx(curNode);
                            neighbourNode.flags |= Node.DT_NODE_CLOSED;
                            stack.AddLast(neighbourNode);
                        }
                    }
                }
            }

            List<long> visited = new List<long>();
            if (bestNode != null)
            {
                // Reverse the path.
                Node prev = null;
                Node node = bestNode;
                do
                {
                    Node next = tinyNodePool.getNodeAtIdx(node.pidx);
                    node.pidx = tinyNodePool.getNodeIdx(prev);
                    prev = node;
                    node = next;
                } while (node != null);

                // Store result
                node = prev;
                do
                {
                    visited.Add(node.id);
                    node = tinyNodePool.getNodeAtIdx(node.pidx);
                } while (node != null);
            }

            return Results.success(new MoveAlongSurfaceResult(bestPos, visited));
        }

        public class PortalResult
        {
            public readonly Vector3f left;
            public readonly Vector3f right;
            public readonly int fromType;
            public readonly int toType;

            public PortalResult(Vector3f left, Vector3f right, int fromType, int toType)
            {
                this.left = left;
                this.right = right;
                this.fromType = fromType;
                this.toType = toType;
            }
        }

        protected Result<PortalResult> getPortalPoints(long from, long to)
        {
            Result<Tuple<MeshTile, Poly>> tileAndPolyResult = m_nav.getTileAndPolyByRef(from);
            if (tileAndPolyResult.failed())
            {
                return Results.of<PortalResult>(tileAndPolyResult.status, tileAndPolyResult.message);
            }

            Tuple<MeshTile, Poly> tileAndPoly = tileAndPolyResult.result;
            MeshTile fromTile = tileAndPoly.Item1;
            Poly fromPoly = tileAndPoly.Item2;
            int fromType = fromPoly.getType();

            tileAndPolyResult = m_nav.getTileAndPolyByRef(to);
            if (tileAndPolyResult.failed())
            {
                return Results.of<PortalResult>(tileAndPolyResult.status, tileAndPolyResult.message);
            }

            tileAndPoly = tileAndPolyResult.result;
            MeshTile toTile = tileAndPoly.Item1;
            Poly toPoly = tileAndPoly.Item2;
            int toType = toPoly.getType();

            return getPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, fromType, toType);
        }

        // Returns portal points between two polygons.
        protected Result<PortalResult> getPortalPoints(long from, Poly fromPoly, MeshTile fromTile, long to, Poly toPoly,
            MeshTile toTile, int fromType, int toType)
        {
            Vector3f left = new Vector3f();
            Vector3f right = new Vector3f();
            // Find the link that points to the 'to' polygon.
            Link link = null;
            for (int i = fromTile.polyLinks[fromPoly.index]; i != NavMesh.DT_NULL_LINK; i = fromTile.links[i].next)
            {
                if (fromTile.links[i].refs == to)
                {
                    link = fromTile.links[i];
                    break;
                }
            }

            if (link == null)
            {
                return Results.invalidParam<PortalResult>("No link found");
            }

            // Handle off-mesh connections.
            if (fromPoly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                // Find link that points to first vertex.
                for (int i = fromTile.polyLinks[fromPoly.index]; i != NavMesh.DT_NULL_LINK; i = fromTile.links[i].next)
                {
                    if (fromTile.links[i].refs == to)
                    {
                        int v = fromTile.links[i].edge;
                        left[0] = fromTile.data.verts[fromPoly.verts[v] * 3];
                        left[1] = fromTile.data.verts[fromPoly.verts[v] * 3 + 1];
                        left[2] = fromTile.data.verts[fromPoly.verts[v] * 3 + 2];

                        right[0] = fromTile.data.verts[fromPoly.verts[v] * 3];
                        right[1] = fromTile.data.verts[fromPoly.verts[v] * 3 + 1];
                        right[2] = fromTile.data.verts[fromPoly.verts[v] * 3 + 2];
                        return Results.success(new PortalResult(left, right, fromType, toType));
                    }
                }

                return Results.invalidParam<PortalResult>("Invalid offmesh from connection");
            }

            if (toPoly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                for (int i = toTile.polyLinks[toPoly.index]; i != NavMesh.DT_NULL_LINK; i = toTile.links[i].next)
                {
                    if (toTile.links[i].refs == from)
                    {
                        int v = toTile.links[i].edge;
                        left[0] = toTile.data.verts[toPoly.verts[v] * 3];
                        left[1] = toTile.data.verts[toPoly.verts[v] * 3 + 1];
                        left[2] = toTile.data.verts[toPoly.verts[v] * 3 + 2];

                        right[0] = toTile.data.verts[toPoly.verts[v] * 3];
                        right[1] = toTile.data.verts[toPoly.verts[v] * 3 + 1];
                        right[2] = toTile.data.verts[toPoly.verts[v] * 3 + 2];

                        return Results.success(new PortalResult(left, right, fromType, toType));
                    }
                }

                return Results.invalidParam<PortalResult>("Invalid offmesh to connection");
            }

            // Find portal vertices.
            int v0 = fromPoly.verts[link.edge];
            int v1 = fromPoly.verts[(link.edge + 1) % fromPoly.vertCount];
            left[0] = fromTile.data.verts[v0 * 3];
            left[1] = fromTile.data.verts[v0 * 3 + 1];
            left[2] = fromTile.data.verts[v0 * 3 + 2];

            right[0] = fromTile.data.verts[v1 * 3];
            right[1] = fromTile.data.verts[v1 * 3 + 1];
            right[2] = fromTile.data.verts[v1 * 3 + 2];

            // If the link is at tile boundary, dtClamp the vertices to
            // the link width.
            if (link.side != 0xff)
            {
                // Unpack portal limits.
                if (link.bmin != 0 || link.bmax != 255)
                {
                    float s = 1.0f / 255.0f;
                    float tmin = link.bmin * s;
                    float tmax = link.bmax * s;
                    left = vLerp(fromTile.data.verts, v0 * 3, v1 * 3, tmin);
                    right = vLerp(fromTile.data.verts, v0 * 3, v1 * 3, tmax);
                }
            }

            return Results.success(new PortalResult(left, right, fromType, toType));
        }

        protected Result<Vector3f> getEdgeMidPoint(long from, Poly fromPoly, MeshTile fromTile, long to,
            Poly toPoly, MeshTile toTile)
        {
            Result<PortalResult> ppoints = getPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, 0, 0);
            if (ppoints.failed())
            {
                return Results.of<Vector3f>(ppoints.status, ppoints.message);
            }

            var left = ppoints.result.left;
            var right = ppoints.result.right;
            Vector3f mid = new Vector3f();
            mid[0] = (left[0] + right[0]) * 0.5f;
            mid[1] = (left[1] + right[1]) * 0.5f;
            mid[2] = (left[2] + right[2]) * 0.5f;
            return Results.success(mid);
        }

        protected Result<Vector3f> getEdgeIntersectionPoint(Vector3f fromPos, long from, Poly fromPoly, MeshTile fromTile,
            Vector3f toPos, long to, Poly toPoly, MeshTile toTile)
        {
            Result<PortalResult> ppoints = getPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, 0, 0);
            if (ppoints.failed())
            {
                return Results.of<Vector3f>(ppoints.status, ppoints.message);
            }

            Vector3f left = ppoints.result.left;
            Vector3f right = ppoints.result.right;
            float t = 0.5f;
            Tuple<float, float> interect = intersectSegSeg2D(fromPos, toPos, left, right);
            if (null != interect)
            {
                t = clamp(interect.Item2, 0.1f, 0.9f);
            }

            Vector3f pt = vLerp(left, right, t);
            return Results.success(pt);
        }

        private static float s = 1.0f / 255.0f;

        /// @par
        ///
        /// This method is meant to be used for quick, short distance checks.
        ///
        /// If the path array is too small to hold the result, it will be filled as
        /// far as possible from the start postion toward the end position.
        ///
        /// <b>Using the Hit Parameter t of RaycastHit</b>
        ///
        /// If the hit parameter is a very high value (FLT_MAX), then the ray has hit
        /// the end position. In this case the path represents a valid corridor to the
        /// end position and the value of @p hitNormal is undefined.
        ///
        /// If the hit parameter is zero, then the start position is on the wall that
        /// was hit and the value of @p hitNormal is undefined.
        ///
        /// If 0 < t < 1.0 then the following applies:
        ///
        /// @code
        /// distanceToHitBorder = distanceToEndPosition * t
        /// hitPoint = startPos + (endPos - startPos) * t
        /// @endcode
        ///
        /// <b>Use Case Restriction</b>
        ///
        /// The raycast ignores the y-value of the end position. (2D check.) This
        /// places significant limits on how it can be used. For example:
        ///
        /// Consider a scene where there is a main floor with a second floor balcony
        /// that hangs over the main floor. So the first floor mesh extends below the
        /// balcony mesh. The start position is somewhere on the first floor. The end
        /// position is on the balcony.
        ///
        /// The raycast will search toward the end position along the first floor mesh.
        /// If it reaches the end position's xz-coordinates it will indicate FLT_MAX
        /// (no wall hit), meaning it reached the end position. This is one example of why
        /// this method is meant for short distance checks.
        ///
        /// Casts a 'walkability' ray along the surface of the navigation mesh from
        /// the start position toward the end position.
        /// @note A wrapper around raycast(..., RaycastHit*). Retained for backward compatibility.
        /// @param[in] startRef The reference id of the start polygon.
        /// @param[in] startPos A position within the start polygon representing
        /// the start of the ray. [(x, y, z)]
        /// @param[in] endPos The position to cast the ray toward. [(x, y, z)]
        /// @param[out] t The hit parameter. (FLT_MAX if no wall hit.)
        /// @param[out] hitNormal The normal of the nearest wall hit. [(x, y, z)]
        /// @param[in] filter The polygon filter to apply to the query.
        /// @param[out] path The reference ids of the visited polygons. [opt]
        /// @param[out] pathCount The number of visited polygons. [opt]
        /// @param[in] maxPath The maximum number of polygons the @p path array can hold.
        /// @returns The status flags for the query.
        public Result<RaycastHit> raycast(long startRef, Vector3f startPos, Vector3f endPos, QueryFilter filter, int options,
            long prevRef)
        {
            // Validate input
            if (!m_nav.isValidPolyRef(startRef) || !vIsFinite(startPos) || !vIsFinite(endPos) || null == filter
                || (prevRef != 0 && !m_nav.isValidPolyRef(prevRef)))
            {
                return Results.invalidParam<RaycastHit>();
            }

            RaycastHit hit = new RaycastHit();

            float[] verts = new float[m_nav.getMaxVertsPerPoly() * 3 + 3];

            Vector3f curPos = Vector3f.Zero;
            Vector3f lastPos = Vector3f.Zero;

            curPos = startPos;
            var dir = vSub(endPos, startPos);

            MeshTile prevTile, tile, nextTile;
            Poly prevPoly, poly, nextPoly;

            // The API input has been checked already, skip checking internal data.
            long curRef = startRef;
            Tuple<MeshTile, Poly> tileAndPolyUns = m_nav.getTileAndPolyByRefUnsafe(curRef);
            tile = tileAndPolyUns.Item1;
            poly = tileAndPolyUns.Item2;
            nextTile = prevTile = tile;
            nextPoly = prevPoly = poly;
            if (prevRef != 0)
            {
                tileAndPolyUns = m_nav.getTileAndPolyByRefUnsafe(prevRef);
                prevTile = tileAndPolyUns.Item1;
                prevPoly = tileAndPolyUns.Item2;
            }

            while (curRef != 0)
            {
                // Cast ray against current polygon.

                // Collect vertices.
                int nv = 0;
                for (int i = 0; i < poly.vertCount; ++i)
                {
                    Array.Copy(tile.data.verts, poly.verts[i] * 3, verts, nv * 3, 3);
                    nv++;
                }

                IntersectResult iresult = intersectSegmentPoly2D(startPos, endPos, verts, nv);
                if (!iresult.intersects)
                {
                    // Could not hit the polygon, keep the old t and report hit.
                    return Results.success(hit);
                }

                hit.hitEdgeIndex = iresult.segMax;

                // Keep track of furthest t so far.
                if (iresult.tmax > hit.t)
                {
                    hit.t = iresult.tmax;
                }

                // Store visited polygons.
                hit.path.Add(curRef);

                // Ray end is completely inside the polygon.
                if (iresult.segMax == -1)
                {
                    hit.t = float.MaxValue;

                    // add the cost
                    if ((options & DT_RAYCAST_USE_COSTS) != 0)
                    {
                        hit.pathCost += filter.getCost(curPos, endPos, prevRef, prevTile, prevPoly, curRef, tile, poly,
                            curRef, tile, poly);
                    }

                    return Results.success(hit);
                }

                // Follow neighbours.
                long nextRef = 0;

                for (int i = tile.polyLinks[poly.index]; i != NavMesh.DT_NULL_LINK; i = tile.links[i].next)
                {
                    Link link = tile.links[i];

                    // Find link which contains this edge.
                    if (link.edge != iresult.segMax)
                    {
                        continue;
                    }

                    // Get pointer to the next polygon.
                    tileAndPolyUns = m_nav.getTileAndPolyByRefUnsafe(link.refs);
                    nextTile = tileAndPolyUns.Item1;
                    nextPoly = tileAndPolyUns.Item2;
                    // Skip off-mesh connections.
                    if (nextPoly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Skip links based on filter.
                    if (!filter.passFilter(link.refs, nextTile, nextPoly))
                    {
                        continue;
                    }

                    // If the link is internal, just return the ref.
                    if (link.side == 0xff)
                    {
                        nextRef = link.refs;
                        break;
                    }

                    // If the link is at tile boundary,

                    // Check if the link spans the whole edge, and accept.
                    if (link.bmin == 0 && link.bmax == 255)
                    {
                        nextRef = link.refs;
                        break;
                    }

                    // Check for partial edge links.
                    int v0 = poly.verts[link.edge];
                    int v1 = poly.verts[(link.edge + 1) % poly.vertCount];
                    int left = v0 * 3;
                    int right = v1 * 3;

                    // Check that the intersection lies inside the link portal.
                    if (link.side == 0 || link.side == 4)
                    {
                        // Calculate link size.
                        float lmin = tile.data.verts[left + 2]
                                     + (tile.data.verts[right + 2] - tile.data.verts[left + 2]) * (link.bmin * s);
                        float lmax = tile.data.verts[left + 2]
                                     + (tile.data.verts[right + 2] - tile.data.verts[left + 2]) * (link.bmax * s);
                        if (lmin > lmax)
                        {
                            (lmin, lmax) = (lmax, lmin);
                        }

                        // Find Z intersection.
                        float z = startPos[2] + (endPos[2] - startPos[2]) * iresult.tmax;
                        if (z >= lmin && z <= lmax)
                        {
                            nextRef = link.refs;
                            break;
                        }
                    }
                    else if (link.side == 2 || link.side == 6)
                    {
                        // Calculate link size.
                        float lmin = tile.data.verts[left]
                                     + (tile.data.verts[right] - tile.data.verts[left]) * (link.bmin * s);
                        float lmax = tile.data.verts[left]
                                     + (tile.data.verts[right] - tile.data.verts[left]) * (link.bmax * s);
                        if (lmin > lmax)
                        {
                            (lmin, lmax) = (lmax, lmin);
                        }

                        // Find X intersection.
                        float x = startPos[0] + (endPos[0] - startPos[0]) * iresult.tmax;
                        if (x >= lmin && x <= lmax)
                        {
                            nextRef = link.refs;
                            break;
                        }
                    }
                }

                // add the cost
                if ((options & DT_RAYCAST_USE_COSTS) != 0)
                {
                    // compute the intersection point at the furthest end of the polygon
                    // and correct the height (since the raycast moves in 2d)
                    lastPos = curPos;
                    curPos = vMad(startPos, dir, hit.t);
                    VectorPtr e1 = new VectorPtr(verts, iresult.segMax * 3);
                    VectorPtr e2 = new VectorPtr(verts, ((iresult.segMax + 1) % nv) * 3);
                    Vector3f eDir = vSub(e2, e1);
                    Vector3f diff = vSub(curPos, e1);
                    float s = sqr(eDir[0]) > sqr(eDir[2]) ? diff[0] / eDir[0] : diff[2] / eDir[2];
                    curPos[1] = e1.get(1) + eDir[1] * s;

                    hit.pathCost += filter.getCost(lastPos, curPos, prevRef, prevTile, prevPoly, curRef, tile, poly,
                        nextRef, nextTile, nextPoly);
                }

                if (nextRef == 0)
                {
                    // No neighbour, we hit a wall.

                    // Calculate hit normal.
                    int a = iresult.segMax;
                    int b = iresult.segMax + 1 < nv ? iresult.segMax + 1 : 0;
                    int va = a * 3;
                    int vb = b * 3;
                    float dx = verts[vb] - verts[va];
                    float dz = verts[vb + 2] - verts[va + 2];
                    hit.hitNormal[0] = dz;
                    hit.hitNormal[1] = 0;
                    hit.hitNormal[2] = -dx;
                    vNormalize(ref hit.hitNormal);
                    return Results.success(hit);
                }

                // No hit, advance to neighbour polygon.
                prevRef = curRef;
                curRef = nextRef;
                prevTile = tile;
                tile = nextTile;
                prevPoly = poly;
                poly = nextPoly;
            }

            return Results.success(hit);
        }

        /// @par
        ///
        /// At least one result array must be provided.
        ///
        /// The order of the result set is from least to highest cost to reach the polygon.
        ///
        /// A common use case for this method is to perform Dijkstra searches.
        /// Candidate polygons are found by searching the graph beginning at the start polygon.
        ///
        /// If a polygon is not found via the graph search, even if it intersects the
        /// search circle, it will not be included in the result set. For example:
        ///
        /// polyA is the start polygon.
        /// polyB shares an edge with polyA. (Is adjacent.)
        /// polyC shares an edge with polyB, but not with polyA
        /// Even if the search circle overlaps polyC, it will not be included in the
        /// result set unless polyB is also in the set.
        ///
        /// The value of the center point is used as the start position for cost
        /// calculations. It is not projected onto the surface of the mesh, so its
        /// y-value will effect the costs.
        ///
        /// Intersection tests occur in 2D. All polygons and the search circle are
        /// projected onto the xz-plane. So the y-value of the center point does not
        /// effect intersection tests.
        ///
        /// If the result arrays are to small to hold the entire result set, they will be
        /// filled to capacity.
        ///
        /// @}
        /// @name Dijkstra Search Functions
        /// @{
        /// Finds the polygons along the navigation graph that touch the specified circle.
        /// @param[in] startRef The reference id of the polygon where the search starts.
        /// @param[in] centerPos The center of the search circle. [(x, y, z)]
        /// @param[in] radius The radius of the search circle.
        /// @param[in] filter The polygon filter to apply to the query.
        /// @param[out] resultRef The reference ids of the polygons touched by the circle. [opt]
        /// @param[out] resultParent The reference ids of the parent polygons for each result.
        /// Zero if a result polygon has no parent. [opt]
        /// @param[out] resultCost The search cost from @p centerPos to the polygon. [opt]
        /// @param[out] resultCount The number of polygons found. [opt]
        /// @param[in] maxResult The maximum number of polygons the result arrays can hold.
        /// @returns The status flags for the query.
        public Result<FindPolysAroundResult> findPolysAroundCircle(long startRef, Vector3f centerPos, float radius, QueryFilter filter)
        {
            // Validate input

            if (!m_nav.isValidPolyRef(startRef) || !vIsFinite(centerPos) || radius < 0
                || !float.IsFinite(radius) || null == filter)
            {
                return Results.invalidParam<FindPolysAroundResult>();
            }

            List<long> resultRef = new List<long>();
            List<long> resultParent = new List<long>();
            List<float> resultCost = new List<float>();

            m_nodePool.clear();
            m_openList.clear();

            Node startNode = m_nodePool.getNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_OPEN;
            m_openList.push(startNode);

            float radiusSqr = sqr(radius);

            while (!m_openList.isEmpty())
            {
                Node bestNode = m_openList.pop();
                bestNode.flags &= ~Node.DT_NODE_OPEN;
                bestNode.flags |= Node.DT_NODE_CLOSED;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                long bestRef = bestNode.id;
                Tuple<MeshTile, Poly> tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(bestRef);
                MeshTile bestTile = tileAndPoly.Item1;
                Poly bestPoly = tileAndPoly.Item2;

                // Get parent poly and tile.
                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.getNodeAtIdx(bestNode.pidx).id;
                }

                if (parentRef != 0)
                {
                    tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(parentRef);
                    parentTile = tileAndPoly.Item1;
                    parentPoly = tileAndPoly.Item2;
                }

                resultRef.Add(bestRef);
                resultParent.Add(parentRef);
                resultCost.Add(bestNode.total);

                for (int i = bestTile.polyLinks[bestPoly.index]; i != NavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    Link link = bestTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = tileAndPoly.Item1;
                    Poly neighbourPoly = tileAndPoly.Item2;

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.passFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    Result<PortalResult> pp = getPortalPoints(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly,
                        neighbourTile, 0, 0);
                    if (pp.failed())
                    {
                        continue;
                    }

                    var va = pp.result.left;
                    var vb = pp.result.right;

                    // If the circle is not touching the next polygon, skip it.
                    Tuple<float, float> distseg = distancePtSegSqr2D(centerPos, va, vb);
                    float distSqr = distseg.Item1;
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    Node neighbourNode = m_nodePool.getNode(neighbourRef);

                    if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        neighbourNode.pos = vLerp(va, vb, 0.5f);
                    }

                    float cost = filter.getCost(bestNode.pos, neighbourNode.pos, parentRef, parentTile, parentPoly, bestRef,
                        bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);

                    float total = bestNode.total + cost;
                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.pidx = m_nodePool.getNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        m_openList.modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags = Node.DT_NODE_OPEN;
                        m_openList.push(neighbourNode);
                    }
                }
            }

            return Results.success(new FindPolysAroundResult(resultRef, resultParent, resultCost));
        }

        /// @par
        ///
        /// The order of the result set is from least to highest cost.
        ///
        /// At least one result array must be provided.
        ///
        /// A common use case for this method is to perform Dijkstra searches.
        /// Candidate polygons are found by searching the graph beginning at the start
        /// polygon.
        ///
        /// The same intersection test restrictions that apply to findPolysAroundCircle()
        /// method apply to this method.
        ///
        /// The 3D centroid of the search polygon is used as the start position for cost
        /// calculations.
        ///
        /// Intersection tests occur in 2D. All polygons are projected onto the
        /// xz-plane. So the y-values of the vertices do not effect intersection tests.
        ///
        /// If the result arrays are is too small to hold the entire result set, they will
        /// be filled to capacity.
        ///
        /// Finds the polygons along the naviation graph that touch the specified convex polygon.
        /// @param[in] startRef The reference id of the polygon where the search starts.
        /// @param[in] verts The vertices describing the convex polygon. (CCW)
        /// [(x, y, z) * @p nverts]
        /// @param[in] nverts The number of vertices in the polygon.
        /// @param[in] filter The polygon filter to apply to the query.
        /// @param[out] resultRef The reference ids of the polygons touched by the search polygon. [opt]
        /// @param[out] resultParent The reference ids of the parent polygons for each result. Zero if a
        /// result polygon has no parent. [opt]
        /// @param[out] resultCost The search cost from the centroid point to the polygon. [opt]
        /// @param[out] resultCount The number of polygons found.
        /// @param[in] maxResult The maximum number of polygons the result arrays can hold.
        /// @returns The status flags for the query.
        public Result<FindPolysAroundResult> findPolysAroundShape(long startRef, float[] verts, QueryFilter filter)
        {
            // Validate input
            int nverts = verts.Length / 3;
            if (!m_nav.isValidPolyRef(startRef) || null == verts || nverts < 3 || null == filter)
            {
                return Results.invalidParam<FindPolysAroundResult>();
            }

            List<long> resultRef = new List<long>();
            List<long> resultParent = new List<long>();
            List<float> resultCost = new List<float>();

            m_nodePool.clear();
            m_openList.clear();

            Vector3f centerPos = Vector3f.Zero;
            for (int i = 0; i < nverts; ++i)
            {
                centerPos[0] += verts[i * 3];
                centerPos[1] += verts[i * 3 + 1];
                centerPos[2] += verts[i * 3 + 2];
            }

            float scale = 1.0f / nverts;
            centerPos[0] *= scale;
            centerPos[1] *= scale;
            centerPos[2] *= scale;

            Node startNode = m_nodePool.getNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_OPEN;
            m_openList.push(startNode);

            while (!m_openList.isEmpty())
            {
                Node bestNode = m_openList.pop();
                bestNode.flags &= ~Node.DT_NODE_OPEN;
                bestNode.flags |= Node.DT_NODE_CLOSED;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                long bestRef = bestNode.id;
                Tuple<MeshTile, Poly> tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(bestRef);
                MeshTile bestTile = tileAndPoly.Item1;
                Poly bestPoly = tileAndPoly.Item2;

                // Get parent poly and tile.
                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.getNodeAtIdx(bestNode.pidx).id;
                }

                if (parentRef != 0)
                {
                    tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(parentRef);
                    parentTile = tileAndPoly.Item1;
                    parentPoly = tileAndPoly.Item2;
                }

                resultRef.Add(bestRef);
                resultParent.Add(parentRef);
                resultCost.Add(bestNode.total);

                for (int i = bestTile.polyLinks[bestPoly.index]; i != NavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    Link link = bestTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = tileAndPoly.Item1;
                    Poly neighbourPoly = tileAndPoly.Item2;

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.passFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    Result<PortalResult> pp = getPortalPoints(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly,
                        neighbourTile, 0, 0);
                    if (pp.failed())
                    {
                        continue;
                    }

                    var va = pp.result.left;
                    var vb = pp.result.right;

                    // If the poly is not touching the edge to the next polygon, skip the connection it.
                    IntersectResult ir = intersectSegmentPoly2D(va, vb, verts, nverts);
                    if (!ir.intersects)
                    {
                        continue;
                    }

                    if (ir.tmin > 1.0f || ir.tmax < 0.0f)
                    {
                        continue;
                    }

                    Node neighbourNode = m_nodePool.getNode(neighbourRef);

                    if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        neighbourNode.pos = vLerp(va, vb, 0.5f);
                    }

                    float cost = filter.getCost(bestNode.pos, neighbourNode.pos, parentRef, parentTile, parentPoly, bestRef,
                        bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);

                    float total = bestNode.total + cost;

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.pidx = m_nodePool.getNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        m_openList.modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags = Node.DT_NODE_OPEN;
                        m_openList.push(neighbourNode);
                    }
                }
            }

            return Results.success(new FindPolysAroundResult(resultRef, resultParent, resultCost));
        }

        /// @par
        ///
        /// This method is optimized for a small search radius and small number of result
        /// polygons.
        ///
        /// Candidate polygons are found by searching the navigation graph beginning at
        /// the start polygon.
        ///
        /// The same intersection test restrictions that apply to the findPolysAroundCircle
        /// mehtod applies to this method.
        ///
        /// The value of the center point is used as the start point for cost calculations.
        /// It is not projected onto the surface of the mesh, so its y-value will effect
        /// the costs.
        ///
        /// Intersection tests occur in 2D. All polygons and the search circle are
        /// projected onto the xz-plane. So the y-value of the center point does not
        /// effect intersection tests.
        ///
        /// If the result arrays are is too small to hold the entire result set, they will
        /// be filled to capacity.
        ///
        /// Finds the non-overlapping navigation polygons in the local neighbourhood around the center position.
        /// @param[in] startRef The reference id of the polygon where the search starts.
        /// @param[in] centerPos The center of the query circle. [(x, y, z)]
        /// @param[in] radius The radius of the query circle.
        /// @param[in] filter The polygon filter to apply to the query.
        /// @param[out] resultRef The reference ids of the polygons touched by the circle.
        /// @param[out] resultParent The reference ids of the parent polygons for each result.
        /// Zero if a result polygon has no parent. [opt]
        /// @param[out] resultCount The number of polygons found.
        /// @param[in] maxResult The maximum number of polygons the result arrays can hold.
        /// @returns The status flags for the query.
        public Result<FindLocalNeighbourhoodResult> findLocalNeighbourhood(long startRef, Vector3f centerPos, float radius,
            QueryFilter filter)
        {
            // Validate input
            if (!m_nav.isValidPolyRef(startRef) || !vIsFinite(centerPos) || radius < 0
                || !float.IsFinite(radius) || null == filter)
            {
                return Results.invalidParam<FindLocalNeighbourhoodResult>();
            }

            List<long> resultRef = new List<long>();
            List<long> resultParent = new List<long>();

            NodePool tinyNodePool = new NodePool();

            Node startNode = tinyNodePool.getNode(startRef);
            startNode.pidx = 0;
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_CLOSED;
            LinkedList<Node> stack = new LinkedList<Node>();
            stack.AddLast(startNode);

            resultRef.Add(startNode.id);
            resultParent.Add(0L);

            float radiusSqr = sqr(radius);

            float[] pa = new float[m_nav.getMaxVertsPerPoly() * 3];
            float[] pb = new float[m_nav.getMaxVertsPerPoly() * 3];

            while (0 < stack.Count)
            {
                // Pop front.
                Node curNode = stack.First?.Value;
                stack.RemoveFirst();

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                long curRef = curNode.id;
                Tuple<MeshTile, Poly> tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(curRef);
                MeshTile curTile = tileAndPoly.Item1;
                Poly curPoly = tileAndPoly.Item2;

                for (int i = curTile.polyLinks[curPoly.index]; i != NavMesh.DT_NULL_LINK; i = curTile.links[i].next)
                {
                    Link link = curTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours.
                    if (neighbourRef == 0)
                    {
                        continue;
                    }

                    Node neighbourNode = tinyNodePool.getNode(neighbourRef);
                    // Skip visited.
                    if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = tileAndPoly.Item1;
                    Poly neighbourPoly = tileAndPoly.Item2;

                    // Skip off-mesh connections.
                    if (neighbourPoly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.passFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    Result<PortalResult> pp = getPortalPoints(curRef, curPoly, curTile, neighbourRef, neighbourPoly,
                        neighbourTile, 0, 0);
                    if (pp.failed())
                    {
                        continue;
                    }

                    var va = pp.result.left;
                    var vb = pp.result.right;

                    // If the circle is not touching the next polygon, skip it.
                    Tuple<float, float> distseg = distancePtSegSqr2D(centerPos, va, vb);
                    float distSqr = distseg.Item1;
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    // Mark node visited, this is done before the overlap test so that
                    // we will not visit the poly again if the test fails.
                    neighbourNode.flags |= Node.DT_NODE_CLOSED;
                    neighbourNode.pidx = tinyNodePool.getNodeIdx(curNode);

                    // Check that the polygon does not collide with existing polygons.

                    // Collect vertices of the neighbour poly.
                    int npa = neighbourPoly.vertCount;
                    for (int k = 0; k < npa; ++k)
                    {
                        Array.Copy(neighbourTile.data.verts, neighbourPoly.verts[k] * 3, pa, k * 3, 3);
                    }

                    bool overlap = false;
                    for (int j = 0; j < resultRef.Count; ++j)
                    {
                        long pastRef = resultRef[j];

                        // Connected polys do not overlap.
                        bool connected = false;
                        for (int k = curTile.polyLinks[curPoly.index]; k != NavMesh.DT_NULL_LINK; k = curTile.links[k].next)
                        {
                            if (curTile.links[k].refs == pastRef)
                            {
                                connected = true;
                                break;
                            }
                        }

                        if (connected)
                        {
                            continue;
                        }

                        // Potentially overlapping.
                        tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(pastRef);
                        MeshTile pastTile = tileAndPoly.Item1;
                        Poly pastPoly = tileAndPoly.Item2;

                        // Get vertices and test overlap
                        int npb = pastPoly.vertCount;
                        for (int k = 0; k < npb; ++k)
                        {
                            Array.Copy(pastTile.data.verts, pastPoly.verts[k] * 3, pb, k * 3, 3);
                        }

                        if (overlapPolyPoly2D(pa, npa, pb, npb))
                        {
                            overlap = true;
                            break;
                        }
                    }

                    if (overlap)
                    {
                        continue;
                    }

                    resultRef.Add(neighbourRef);
                    resultParent.Add(curRef);
                    stack.AddLast(neighbourNode);
                }
            }

            return Results.success(new FindLocalNeighbourhoodResult(resultRef, resultParent));
        }

        public class SegInterval
        {
            public long refs;
            public int tmin, tmax;

            public SegInterval(long refs, int tmin, int tmax)
            {
                this.refs = refs;
                this.tmin = tmin;
                this.tmax = tmax;
            }
        }

        protected void insertInterval(List<SegInterval> ints, int tmin, int tmax, long refs)
        {
            // Find insertion point.
            int idx = 0;
            while (idx < ints.Count)
            {
                if (tmax <= ints[idx].tmin)
                {
                    break;
                }

                idx++;
            }

            // Store
            ints.Insert(idx, new SegInterval(refs, tmin, tmax));
        }

        /// @par
        ///
        /// If the @p segmentRefs parameter is provided, then all polygon segments will be returned.
        /// Otherwise only the wall segments are returned.
        ///
        /// A segment that is normally a portal will be included in the result set as a
        /// wall if the @p filter results in the neighbor polygon becoomming impassable.
        ///
        /// The @p segmentVerts and @p segmentRefs buffers should normally be sized for the
        /// maximum segments per polygon of the source navigation mesh.
        ///
        /// Returns the segments for the specified polygon, optionally including portals.
        /// @param[in] ref The reference id of the polygon.
        /// @param[in] filter The polygon filter to apply to the query.
        /// @param[out] segmentVerts The segments. [(ax, ay, az, bx, by, bz) * segmentCount]
        /// @param[out] segmentRefs The reference ids of each segment's neighbor polygon.
        /// Or zero if the segment is a wall. [opt] [(parentRef) * @p segmentCount]
        /// @param[out] segmentCount The number of segments returned.
        /// @param[in] maxSegments The maximum number of segments the result arrays can hold.
        /// @returns The status flags for the query.
        public Result<GetPolyWallSegmentsResult> getPolyWallSegments(long refs, bool storePortals, QueryFilter filter)
        {
            Result<Tuple<MeshTile, Poly>> tileAndPoly = m_nav.getTileAndPolyByRef(refs);
            if (tileAndPoly.failed())
            {
                return Results.of<GetPolyWallSegmentsResult>(tileAndPoly.status, tileAndPoly.message);
            }

            if (null == filter)
            {
                return Results.invalidParam<GetPolyWallSegmentsResult>();
            }

            MeshTile tile = tileAndPoly.result.Item1;
            Poly poly = tileAndPoly.result.Item2;

            List<long> segmentRefs = new List<long>();
            List<SegmentVert> segmentVerts = new List<SegmentVert>();
            List<SegInterval> ints = new List<SegInterval>(16);

            for (int i = 0, j = poly.vertCount - 1; i < poly.vertCount; j = i++)
            {
                // Skip non-solid edges.
                ints.Clear();
                if ((poly.neis[j] & NavMesh.DT_EXT_LINK) != 0)
                {
                    // Tile border.
                    for (int k = tile.polyLinks[poly.index]; k != NavMesh.DT_NULL_LINK; k = tile.links[k].next)
                    {
                        Link link = tile.links[k];
                        if (link.edge == j)
                        {
                            if (link.refs != 0)
                            {
                                Tuple<MeshTile, Poly> tileAndPolyUnsafe = m_nav.getTileAndPolyByRefUnsafe(link.refs);
                                MeshTile neiTile = tileAndPolyUnsafe.Item1;
                                Poly neiPoly = tileAndPolyUnsafe.Item2;
                                if (filter.passFilter(link.refs, neiTile, neiPoly))
                                {
                                    insertInterval(ints, link.bmin, link.bmax, link.refs);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Internal edge
                    long neiRef = 0;
                    if (poly.neis[j] != 0)
                    {
                        int idx = (poly.neis[j] - 1);
                        neiRef = m_nav.getPolyRefBase(tile) | idx;
                        if (!filter.passFilter(neiRef, tile, tile.data.polys[idx]))
                        {
                            neiRef = 0;
                        }
                    }

                    // If the edge leads to another polygon and portals are not stored, skip.
                    if (neiRef != 0 && !storePortals)
                    {
                        continue;
                    }

                    int ivj = poly.verts[j] * 3;
                    int ivi = poly.verts[i] * 3;
                    var seg = new SegmentVert();
                    vCopy(ref seg.vmin, tile.data.verts, ivj);
                    vCopy(ref seg.vmax, tile.data.verts, ivi);
                    // Array.Copy(tile.data.verts, ivj, seg, 0, 3);
                    // Array.Copy(tile.data.verts, ivi, seg, 3, 3);
                    segmentVerts.Add(seg);
                    segmentRefs.Add(neiRef);
                    continue;
                }

                // Add sentinels
                insertInterval(ints, -1, 0, 0);
                insertInterval(ints, 255, 256, 0);

                // Store segments.
                int vj = poly.verts[j] * 3;
                int vi = poly.verts[i] * 3;
                for (int k = 1; k < ints.Count; ++k)
                {
                    // Portal segment.
                    if (storePortals && ints[k].refs != 0)
                    {
                        float tmin = ints[k].tmin / 255.0f;
                        float tmax = ints[k].tmax / 255.0f;
                        var seg = new SegmentVert();
                        seg.vmin = vLerp(tile.data.verts, vj, vi, tmin);
                        seg.vmax = vLerp(tile.data.verts, vj, vi, tmax);
                        segmentVerts.Add(seg);
                        segmentRefs.Add(ints[k].refs);
                    }

                    // Wall segment.
                    int imin = ints[k - 1].tmax;
                    int imax = ints[k].tmin;
                    if (imin != imax)
                    {
                        float tmin = imin / 255.0f;
                        float tmax = imax / 255.0f;
                        var seg = new SegmentVert();
                        seg.vmin = vLerp(tile.data.verts, vj, vi, tmin);
                        seg.vmax = vLerp(tile.data.verts, vj, vi, tmax);
                        segmentVerts.Add(seg);
                        segmentRefs.Add(0L);
                    }
                }
            }

            return Results.success(new GetPolyWallSegmentsResult(segmentVerts, segmentRefs));
        }

        /// @par
        ///
        /// @p hitPos is not adjusted using the height detail data.
        ///
        /// @p hitDist will equal the search radius if there is no wall within the
        /// radius. In this case the values of @p hitPos and @p hitNormal are
        /// undefined.
        ///
        /// The normal will become unpredicable if @p hitDist is a very small number.
        ///
        /// Finds the distance from the specified position to the nearest polygon wall.
        /// @param[in] startRef The reference id of the polygon containing @p centerPos.
        /// @param[in] centerPos The center of the search circle. [(x, y, z)]
        /// @param[in] maxRadius The radius of the search circle.
        /// @param[in] filter The polygon filter to apply to the query.
        /// @param[out] hitDist The distance to the nearest wall from @p centerPos.
        /// @param[out] hitPos The nearest position on the wall that was hit. [(x, y, z)]
        /// @param[out] hitNormal The normalized ray formed from the wall point to the
        /// source point. [(x, y, z)]
        /// @returns The status flags for the query.
        public virtual Result<FindDistanceToWallResult> findDistanceToWall(long startRef, Vector3f centerPos, float maxRadius,
            QueryFilter filter)
        {
            // Validate input
            if (!m_nav.isValidPolyRef(startRef) || !vIsFinite(centerPos) || maxRadius < 0
                || !float.IsFinite(maxRadius) || null == filter)
            {
                return Results.invalidParam<FindDistanceToWallResult>();
            }

            m_nodePool.clear();
            m_openList.clear();

            Node startNode = m_nodePool.getNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = Node.DT_NODE_OPEN;
            m_openList.push(startNode);

            float radiusSqr = sqr(maxRadius);
            Vector3f hitPos = new Vector3f();
            VectorPtr bestvj = null;
            VectorPtr bestvi = null;
            while (!m_openList.isEmpty())
            {
                Node bestNode = m_openList.pop();
                bestNode.flags &= ~Node.DT_NODE_OPEN;
                bestNode.flags |= Node.DT_NODE_CLOSED;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                long bestRef = bestNode.id;
                Tuple<MeshTile, Poly> tileAndPoly = m_nav.getTileAndPolyByRefUnsafe(bestRef);
                MeshTile bestTile = tileAndPoly.Item1;
                Poly bestPoly = tileAndPoly.Item2;

                // Get parent poly and tile.
                long parentRef = 0;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.getNodeAtIdx(bestNode.pidx).id;
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
                                    Tuple<MeshTile, Poly> linkTileAndPoly = m_nav.getTileAndPolyByRefUnsafe(link.refs);
                                    MeshTile neiTile = linkTileAndPoly.Item1;
                                    Poly neiPoly = linkTileAndPoly.Item2;
                                    if (filter.passFilter(link.refs, neiTile, neiPoly))
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
                        long refs = m_nav.getPolyRefBase(bestTile) | idx;
                        if (filter.passFilter(refs, bestTile, bestTile.data.polys[idx]))
                        {
                            continue;
                        }
                    }

                    // Calc distance to the edge.
                    int vj = bestPoly.verts[j] * 3;
                    int vi = bestPoly.verts[i] * 3;
                    Tuple<float, float> distseg = distancePtSegSqr2D(centerPos, bestTile.data.verts, vj, vi);
                    float distSqr = distseg.Item1;
                    float tseg = distseg.Item2;

                    // Edge is too far, skip.
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    // Hit wall, update radius.
                    radiusSqr = distSqr;
                    // Calculate hit pos.
                    hitPos[0] = bestTile.data.verts[vj] + (bestTile.data.verts[vi] - bestTile.data.verts[vj]) * tseg;
                    hitPos[1] = bestTile.data.verts[vj + 1]
                                + (bestTile.data.verts[vi + 1] - bestTile.data.verts[vj + 1]) * tseg;
                    hitPos[2] = bestTile.data.verts[vj + 2]
                                + (bestTile.data.verts[vi + 2] - bestTile.data.verts[vj + 2]) * tseg;
                    bestvj = new VectorPtr(bestTile.data.verts, vj);
                    bestvi = new VectorPtr(bestTile.data.verts, vi);
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
                    Tuple<MeshTile, Poly> neighbourTileAndPoly = m_nav.getTileAndPolyByRefUnsafe(neighbourRef);
                    MeshTile neighbourTile = neighbourTileAndPoly.Item1;
                    Poly neighbourPoly = neighbourTileAndPoly.Item2;

                    // Skip off-mesh connections.
                    if (neighbourPoly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Calc distance to the edge.
                    int va = bestPoly.verts[link.edge] * 3;
                    int vb = bestPoly.verts[(link.edge + 1) % bestPoly.vertCount] * 3;
                    Tuple<float, float> distseg = distancePtSegSqr2D(centerPos, bestTile.data.verts, va, vb);
                    float distSqr = distseg.Item1;
                    // If the circle is not touching the next polygon, skip it.
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    if (!filter.passFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    Node neighbourNode = m_nodePool.getNode(neighbourRef);

                    if ((neighbourNode.flags & Node.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        var midPoint = getEdgeMidPoint(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly,
                            neighbourTile);
                        if (midPoint.succeeded())
                        {
                            neighbourNode.pos = midPoint.result;
                        }
                    }

                    float total = bestNode.total + vDist(bestNode.pos, neighbourNode.pos);

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~Node.DT_NODE_CLOSED);
                    neighbourNode.pidx = m_nodePool.getNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & Node.DT_NODE_OPEN) != 0)
                    {
                        m_openList.modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags |= Node.DT_NODE_OPEN;
                        m_openList.push(neighbourNode);
                    }
                }
            }

            // Calc hit normal.
            Vector3f hitNormal = new Vector3f();
            if (bestvi != null && bestvj != null)
            {
                var tangent = vSub(bestvi, bestvj);
                hitNormal[0] = tangent[2];
                hitNormal[1] = 0;
                hitNormal[2] = -tangent[0];
                vNormalize(ref hitNormal);
            }

            return Results.success(new FindDistanceToWallResult((float)Math.Sqrt(radiusSqr), hitPos, hitNormal));
        }

        /// Returns true if the polygon reference is valid and passes the filter restrictions.
        /// @param[in] ref The polygon reference to check.
        /// @param[in] filter The filter to apply.
        public bool isValidPolyRef(long refs, QueryFilter filter)
        {
            Result<Tuple<MeshTile, Poly>> tileAndPolyResult = m_nav.getTileAndPolyByRef(refs);
            if (tileAndPolyResult.failed())
            {
                return false;
            }

            Tuple<MeshTile, Poly> tileAndPoly = tileAndPolyResult.result;
            // If cannot pass filter, assume flags has changed and boundary is invalid.
            if (!filter.passFilter(refs, tileAndPoly.Item1, tileAndPoly.Item2))
            {
                return false;
            }

            return true;
        }

        /// Gets the navigation mesh the query object is using.
        /// @return The navigation mesh the query object is using.
        public NavMesh getAttachedNavMesh()
        {
            return m_nav;
        }

        /**
     * Gets a path from the explored nodes in the previous search.
     *
     * @param endRef
     *            The reference id of the end polygon.
     * @returns An ordered list of polygon references representing the path. (Start to end.)
     * @remarks The result of this function depends on the state of the query object. For that reason it should only be
     *          used immediately after one of the two Dijkstra searches, findPolysAroundCircle or findPolysAroundShape.
     */
        public Result<List<long>> getPathFromDijkstraSearch(long endRef)
        {
            if (!m_nav.isValidPolyRef(endRef))
            {
                return Results.invalidParam<List<long>>("Invalid end ref");
            }

            List<Node> nodes = m_nodePool.findNodes(endRef);
            if (nodes.Count != 1)
            {
                return Results.invalidParam<List<long>>("Invalid end ref");
            }

            Node endNode = nodes[0];
            if ((endNode.flags & DT_NODE_CLOSED) == 0)
            {
                return Results.invalidParam<List<long>>("Invalid end ref");
            }

            return Results.success(getPathToNode(endNode));
        }

        /**
     * Gets the path leading to the specified end node.
     */
        protected List<long> getPathToNode(Node endNode)
        {
            List<long> path = new List<long>();
            // Reverse the path.
            Node curNode = endNode;
            do
            {
                path.Insert(0, curNode.id);
                Node nextNode = m_nodePool.getNodeAtIdx(curNode.pidx);
                if (curNode.shortcut != null)
                {
                    // remove potential duplicates from shortcut path
                    for (int i = curNode.shortcut.Count - 1; i >= 0; i--)
                    {
                        long id = curNode.shortcut[i];
                        if (id != curNode.id && id != nextNode.id)
                        {
                            path.Insert(0, id);
                        }
                    }
                }

                curNode = nextNode;
            } while (curNode != null);

            return path;
        }

        /**
     * The closed list is the list of polygons that were fully evaluated during the last navigation graph search. (A* or
     * Dijkstra)
     */
        public bool isInClosedList(long refs)
        {
            if (m_nodePool == null)
            {
                return false;
            }

            foreach (Node n in m_nodePool.findNodes(refs))
            {
                if ((n.flags & DT_NODE_CLOSED) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public NodePool getNodePool()
        {
            return m_nodePool;
        }
    }
}