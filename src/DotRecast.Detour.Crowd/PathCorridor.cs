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

namespace DotRecast.Detour.Crowd
{
    using static DotRecast.Core.RecastMath;

    /**
 * Represents a dynamic polygon corridor used to plan agent movement.
 *
 * The corridor is loaded with a path, usually obtained from a #NavMeshQuery::findPath() query. The corridor is then
 * used to plan local movement, with the corridor automatically updating as needed to deal with inaccurate agent
 * locomotion.
 *
 * Example of a common use case:
 *
 * -# Construct the corridor object and call -# Obtain a path from a #dtNavMeshQuery object. -# Use #reset() to set the
 * agent's current position. (At the beginning of the path.) -# Use #setCorridor() to load the path and target. -# Use
 * #findCorners() to plan movement. (This handles dynamic path straightening.) -# Use #movePosition() to feed agent
 * movement back into the corridor. (The corridor will automatically adjust as needed.) -# If the target is moving, use
 * #moveTargetPosition() to update the end of the corridor. (The corridor will automatically adjust as needed.) -#
 * Repeat the previous 3 steps to continue to move the agent.
 *
 * The corridor position and target are always constrained to the navigation mesh.
 *
 * One of the difficulties in maintaining a path is that floating point errors, locomotion inaccuracies, and/or local
 * steering can result in the agent crossing the boundary of the path corridor, temporarily invalidating the path. This
 * class uses local mesh queries to detect and update the corridor as needed to handle these types of issues.
 *
 * The fact that local mesh queries are used to move the position and target locations results in two beahviors that
 * need to be considered:
 *
 * Every time a move function is used there is a chance that the path will become non-optimial. Basically, the further
 * the target is moved from its original location, and the further the position is moved outside the original corridor,
 * the more likely the path will become non-optimal. This issue can be addressed by periodically running the
 * #optimizePathTopology() and #optimizePathVisibility() methods.
 *
 * All local mesh queries have distance limitations. (Review the #dtNavMeshQuery methods for details.) So the most
 * accurate use case is to move the position and target in small increments. If a large increment is used, then the
 * corridor may not be able to accurately find the new location. Because of this limiation, if a position is moved in a
 * large increment, then compare the desired and resulting polygon references. If the two do not match, then path
 * replanning may be needed. E.g. If you move the target, check #getLastPoly() to see if it is the expected polygon.
 *
 */
    public class PathCorridor
    {
        private readonly Vector3f m_pos = new Vector3f();
        private readonly Vector3f m_target = new Vector3f();
        private List<long> m_path;

        protected List<long> mergeCorridorStartMoved(List<long> path, List<long> visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = path.Count - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; --j)
                {
                    if (path[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path.
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return path;
            }

            // Concatenate paths.

            // Adjust beginning of the buffer to include the visited.
            List<long> result = new List<long>();
            // Store visited
            for (int i = visited.Count - 1; i > furthestVisited; --i)
            {
                result.Add(visited[i]);
            }

            result.AddRange(path.GetRange(furthestPath, path.Count - furthestPath));
            return result;
        }

        protected List<long> mergeCorridorEndMoved(List<long> path, List<long> visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = 0; i < path.Count; ++i)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; --j)
                {
                    if (path[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path.
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return path;
            }

            // Concatenate paths.
            List<long> result = path.GetRange(0, furthestPath);
            result.AddRange(visited.GetRange(furthestVisited, visited.Count - furthestVisited));
            return result;
        }

        protected List<long> mergeCorridorStartShortcut(List<long> path, List<long> visited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = path.Count - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = visited.Count - 1; j >= 0; --j)
                {
                    if (path[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path.
            if (furthestPath == -1 || furthestVisited <= 0)
            {
                return path;
            }

            // Concatenate paths.

            // Adjust beginning of the buffer to include the visited.
            List<long> result = visited.GetRange(0, furthestVisited);
            result.AddRange(path.GetRange(furthestPath, path.Count - furthestPath));
            return result;
        }

        /**
     * Allocates the corridor's path buffer.
     */
        public PathCorridor()
        {
            m_path = new List<long>();
        }

        /**
     * Resets the path corridor to the specified position.
     *
     * @param ref
     *            The polygon reference containing the position.
     * @param pos
     *            The new position in the corridor. [(x, y, z)]
     */
        public void reset(long refs, float[] pos)
        {
            m_path.Clear();
            m_path.Add(refs);
            vCopy(m_pos, pos);
            vCopy(m_target, pos);
        }

        private static readonly float MIN_TARGET_DIST = sqr(0.01f);

        /**
     * Finds the corners in the corridor from the position toward the target. (The straightened path.)
     *
     * This is the function used to plan local movement within the corridor. One or more corners can be detected in
     * order to plan movement. It performs essentially the same function as #dtNavMeshQuery::findStraightPath.
     *
     * Due to internal optimizations, the maximum number of corners returned will be (@p maxCorners - 1) For example: If
     * the buffers are sized to hold 10 corners, the function will never return more than 9 corners. So if 10 corners
     * are needed, the buffers should be sized for 11 corners.
     *
     * If the target is within range, it will be the last corner and have a polygon reference id of zero.
     *
     * @param filter
     *
     * @param[in] navquery The query object used to build the corridor.
     * @return Corners
     */
        public List<StraightPathItem> findCorners(int maxCorners, NavMeshQuery navquery, QueryFilter filter)
        {
            List<StraightPathItem> path = new List<StraightPathItem>();
            Result<List<StraightPathItem>> result = navquery.findStraightPath(m_pos, m_target, m_path, maxCorners, 0);
            if (result.succeeded())
            {
                path = result.result;
                // Prune points in the beginning of the path which are too close.
                int start = 0;
                foreach (StraightPathItem spi in path)
                {
                    if ((spi.getFlags() & NavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0
                        || vDist2DSqr(spi.getPos(), m_pos) > MIN_TARGET_DIST)
                    {
                        break;
                    }

                    start++;
                }

                int end = path.Count;
                // Prune points after an off-mesh connection.
                for (int i = start; i < path.Count; i++)
                {
                    StraightPathItem spi = path[i];
                    if ((spi.getFlags() & NavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        end = i + 1;
                        break;
                    }
                }

                path = path.GetRange(start, end - start);
            }

            return path;
        }

        /**
     * Attempts to optimize the path if the specified point is visible from the current position.
     *
     * Inaccurate locomotion or dynamic obstacle avoidance can force the agent position significantly outside the
     * original corridor. Over time this can result in the formation of a non-optimal corridor. Non-optimal paths can
     * also form near the corners of tiles.
     *
     * This function uses an efficient local visibility search to try to optimize the corridor between the current
     * position and @p next.
     *
     * The corridor will change only if @p next is visible from the current position and moving directly toward the
     * point is better than following the existing path.
     *
     * The more inaccurate the agent movement, the more beneficial this function becomes. Simply adjust the frequency of
     * the call to match the needs to the agent.
     *
     * This function is not suitable for long distance searches.
     *
     * @param next
     *            The point to search toward. [(x, y, z])
     * @param pathOptimizationRange
     *            The maximum range to search. [Limit: > 0]
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     */
        public void optimizePathVisibility(float[] next, float pathOptimizationRange, NavMeshQuery navquery,
            QueryFilter filter)
        {
            // Clamp the ray to max distance.
            float dist = vDist2D(m_pos, next);

            // If too close to the goal, do not try to optimize.
            if (dist < 0.01f)
            {
                return;
            }

            // Overshoot a little. This helps to optimize open fields in tiled
            // meshes.
            dist = Math.Min(dist + 0.01f, pathOptimizationRange);

            // Adjust ray length.
            float[] delta = vSub(next, m_pos);
            float[] goal = vMad(m_pos, delta, pathOptimizationRange / dist);

            Result<RaycastHit> rc = navquery.raycast(m_path[0], m_pos, goal, filter, 0, 0);
            if (rc.succeeded())
            {
                if (rc.result.path.Count > 1 && rc.result.t > 0.99f)
                {
                    m_path = mergeCorridorStartShortcut(m_path, rc.result.path);
                }
            }
        }

        /**
     * Attempts to optimize the path using a local area search. (Partial replanning.)
     *
     * Inaccurate locomotion or dynamic obstacle avoidance can force the agent position significantly outside the
     * original corridor. Over time this can result in the formation of a non-optimal corridor. This function will use a
     * local area path search to try to re-optimize the corridor.
     *
     * The more inaccurate the agent movement, the more beneficial this function becomes. Simply adjust the frequency of
     * the call to match the needs to the agent.
     *
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     *
     */
        public bool optimizePathTopology(NavMeshQuery navquery, QueryFilter filter, int maxIterations)
        {
            if (m_path.Count < 3)
            {
                return false;
            }

            navquery.initSlicedFindPath(m_path[0], m_path[m_path.Count - 1], m_pos, m_target, filter, 0);
            navquery.updateSlicedFindPath(maxIterations);
            Result<List<long>> fpr = navquery.finalizeSlicedFindPathPartial(m_path);

            if (fpr.succeeded() && fpr.result.Count > 0)
            {
                m_path = mergeCorridorStartShortcut(m_path, fpr.result);
                return true;
            }

            return false;
        }

        public bool moveOverOffmeshConnection(long offMeshConRef, long[] refs, float[] start, float[] end,
            NavMeshQuery navquery)
        {
            // Advance the path up to and over the off-mesh connection.
            long prevRef = 0, polyRef = m_path[0];
            int npos = 0;
            while (npos < m_path.Count && polyRef != offMeshConRef)
            {
                prevRef = polyRef;
                polyRef = m_path[npos];
                npos++;
            }

            if (npos == m_path.Count)
            {
                // Could not find offMeshConRef
                return false;
            }

            // Prune path
            m_path = m_path.GetRange(npos, m_path.Count - npos);
            refs[0] = prevRef;
            refs[1] = polyRef;

            NavMesh nav = navquery.getAttachedNavMesh();
            Result<Tuple<float[], float[]>> startEnd = nav.getOffMeshConnectionPolyEndPoints(refs[0], refs[1]);
            if (startEnd.succeeded())
            {
                vCopy(m_pos, startEnd.result.Item2);
                vCopy(start, startEnd.result.Item1);
                vCopy(end, startEnd.result.Item2);
                return true;
            }

            return false;
        }

        /**
     * Moves the position from the current location to the desired location, adjusting the corridor as needed to reflect
     * the change.
     *
     * Behavior:
     *
     * - The movement is constrained to the surface of the navigation mesh. - The corridor is automatically adjusted
     * (shorted or lengthened) in order to remain valid. - The new position will be located in the adjusted corridor's
     * first polygon.
     *
     * The expected use case is that the desired position will be 'near' the current corridor. What is considered 'near'
     * depends on local polygon density, query search extents, etc.
     *
     * The resulting position will differ from the desired position if the desired position is not on the navigation
     * mesh, or it can't be reached using a local search.
     *
     * @param npos
     *            The desired new position. [(x, y, z)]
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     */
        public bool movePosition(float[] npos, NavMeshQuery navquery, QueryFilter filter)
        {
            // Move along navmesh and update new position.
            Result<MoveAlongSurfaceResult> masResult = navquery.moveAlongSurface(m_path[0], m_pos, npos, filter);
            if (masResult.succeeded())
            {
                m_path = mergeCorridorStartMoved(m_path, masResult.result.getVisited());
                // Adjust the position to stay on top of the navmesh.
                vCopy(m_pos, masResult.result.getResultPos());
                Result<float> hr = navquery.getPolyHeight(m_path[0], masResult.result.getResultPos());
                if (hr.succeeded())
                {
                    m_pos[1] = hr.result;
                }

                return true;
            }

            return false;
        }

        /**
     * Moves the target from the curent location to the desired location, adjusting the corridor as needed to reflect
     * the change. Behavior: - The movement is constrained to the surface of the navigation mesh. - The corridor is
     * automatically adjusted (shorted or lengthened) in order to remain valid. - The new target will be located in the
     * adjusted corridor's last polygon.
     *
     * The expected use case is that the desired target will be 'near' the current corridor. What is considered 'near'
     * depends on local polygon density, query search extents, etc. The resulting target will differ from the desired
     * target if the desired target is not on the navigation mesh, or it can't be reached using a local search.
     *
     * @param npos
     *            The desired new target position. [(x, y, z)]
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     */
        public bool moveTargetPosition(float[] npos, NavMeshQuery navquery, QueryFilter filter)
        {
            // Move along navmesh and update new position.
            Result<MoveAlongSurfaceResult> masResult = navquery.moveAlongSurface(m_path[m_path.Count - 1], m_target,
                npos, filter);
            if (masResult.succeeded())
            {
                m_path = mergeCorridorEndMoved(m_path, masResult.result.getVisited());
                // TODO: should we do that?
                // Adjust the position to stay on top of the navmesh.
                /*
                 * float h = m_target[1]; navquery->getPolyHeight(m_path[m_npath-1],
                 * result, &h); result[1] = h;
                 */
                vCopy(m_target, masResult.result.getResultPos());
                return true;
            }

            return false;
        }

        /**
     * Loads a new path and target into the corridor. The current corridor position is expected to be within the first
     * polygon in the path. The target is expected to be in the last polygon.
     *
     * @warning The size of the path must not exceed the size of corridor's path buffer set during #init().
     * @param target
     *            The target location within the last polygon of the path. [(x, y, z)]
     * @param path
     *            The path corridor.
     */
        public void setCorridor(float[] target, List<long> path)
        {
            vCopy(m_target, target);
            m_path = new List<long>(path);
        }

        public void fixPathStart(long safeRef, float[] safePos)
        {
            vCopy(m_pos, safePos);
            if (m_path.Count < 3 && m_path.Count > 0)
            {
                long p = m_path[m_path.Count - 1];
                m_path.Clear();
                m_path.Add(safeRef);
                m_path.Add(0L);
                m_path.Add(p);
            }
            else
            {
                m_path.Clear();
                m_path.Add(safeRef);
                m_path.Add(0L);
            }
        }

        public void trimInvalidPath(long safeRef, float[] safePos, NavMeshQuery navquery, QueryFilter filter)
        {
            // Keep valid path as far as possible.
            int n = 0;
            while (n < m_path.Count && navquery.isValidPolyRef(m_path[n], filter))
            {
                n++;
            }

            if (n == 0)
            {
                // The first polyref is bad, use current safe values.
                vCopy(m_pos, safePos);
                m_path.Clear();
                m_path.Add(safeRef);
            }
            else if (n < m_path.Count)
            {
                m_path = m_path.GetRange(0, n);
                // The path is partially usable.
            }

            // Clamp target pos to last poly
            Result<float[]> result = navquery.closestPointOnPolyBoundary(m_path[m_path.Count - 1], m_target);
            if (result.succeeded())
            {
                vCopy(m_target, result.result);
            }
        }

        /**
     * Checks the current corridor path to see if its polygon references remain valid. The path can be invalidated if
     * there are structural changes to the underlying navigation mesh, or the state of a polygon within the path changes
     * resulting in it being filtered out. (E.g. An exclusion or inclusion flag changes.)
     *
     * @param maxLookAhead
     *            The number of polygons from the beginning of the corridor to search.
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     * @return
     */
        public bool isValid(int maxLookAhead, NavMeshQuery navquery, QueryFilter filter)
        {
            // Check that all polygons still pass query filter.
            int n = Math.Min(m_path.Count, maxLookAhead);
            for (int i = 0; i < n; ++i)
            {
                if (!navquery.isValidPolyRef(m_path[i], filter))
                {
                    return false;
                }
            }

            return true;
        }

        /**
     * Gets the current position within the corridor. (In the first polygon.)
     *
     * @return The current position within the corridor.
     */
        public float[] getPos()
        {
            return m_pos;
        }

        /**
     * Gets the current target within the corridor. (In the last polygon.)
     *
     * @return The current target within the corridor.
     */
        public float[] getTarget()
        {
            return m_target;
        }

        /**
     * The polygon reference id of the first polygon in the corridor, the polygon containing the position.
     *
     * @return The polygon reference id of the first polygon in the corridor. (Or zero if there is no path.)
     */
        public long getFirstPoly()
        {
            return 0 == m_path.Count ? 0 : m_path[0];
        }

        /**
     * The polygon reference id of the last polygon in the corridor, the polygon containing the target.
     *
     * @return The polygon reference id of the last polygon in the corridor. (Or zero if there is no path.)
     */
        public long getLastPoly()
        {
            return 0 == m_path.Count ? 0 : m_path[m_path.Count - 1];
        }

        /**
     * The corridor's path.
     */
        public List<long> getPath()
        {
            return m_path;
        }

        /**
     * The number of polygons in the current corridor path.
     *
     * @return The number of polygons in the current corridor path.
     */
        public int getPathCount()
        {
            return m_path.Count;
        }
    }
}