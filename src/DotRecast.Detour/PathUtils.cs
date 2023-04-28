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
using DotRecast.Core;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour
{
    public static class PathUtils
    {
        private const int MAX_STEER_POINTS = 3;


        public static SteerTarget getSteerTarget(NavMeshQuery navQuery, Vector3f startPos, Vector3f endPos,
            float minTargetDist, List<long> path)
        {
            // Find steer target.
            Result<List<StraightPathItem>> result = navQuery.findStraightPath(startPos, endPos, path, MAX_STEER_POINTS, 0);
            if (result.Failed())
            {
                return null;
            }

            List<StraightPathItem> straightPath = result.result;
            float[] steerPoints = new float[straightPath.Count * 3];
            for (int i = 0; i < straightPath.Count; i++)
            {
                steerPoints[i * 3] = straightPath[i].getPos()[0];
                steerPoints[i * 3 + 1] = straightPath[i].getPos()[1];
                steerPoints[i * 3 + 2] = straightPath[i].getPos()[2];
            }

            // Find vertex far enough to steer to.
            int ns = 0;
            while (ns < straightPath.Count)
            {
                // Stop at Off-Mesh link or when point is further than slop away.
                if (((straightPath[ns].getFlags() & NavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    || !inRange(straightPath[ns].getPos(), startPos, minTargetDist, 1000.0f))
                    break;
                ns++;
            }

            // Failed to find good point to steer to.
            if (ns >= straightPath.Count)
                return null;

            Vector3f steerPos = Vector3f.Of(
                straightPath[ns].getPos()[0],
                startPos[1],
                straightPath[ns].getPos()[2]
            );
            int steerPosFlag = straightPath[ns].getFlags();
            long steerPosRef = straightPath[ns].getRef();

            SteerTarget target = new SteerTarget(steerPos, steerPosFlag, steerPosRef, steerPoints);
            return target;
        }

        public static bool inRange(Vector3f v1, Vector3f v2, float r, float h)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return (dx * dx + dz * dz) < r * r && Math.Abs(dy) < h;
        }

        public static List<long> fixupCorridor(List<long> path, List<long> visited)
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
                    break;
            }

            // If no intersection found just return current path.
            if (furthestPath == -1 || furthestVisited == -1)
                return path;

            // Concatenate paths.

            // Adjust beginning of the buffer to include the visited.
            int req = visited.Count - furthestVisited;
            int orig = Math.Min(furthestPath + 1, path.Count);
            int size = Math.Max(0, path.Count - orig);
            List<long> fixupPath = new List<long>();
            // Store visited
            for (int i = 0; i < req; ++i)
            {
                fixupPath.Add(visited[(visited.Count - 1) - i]);
            }

            for (int i = 0; i < size; i++)
            {
                fixupPath.Add(path[orig + i]);
            }

            return fixupPath;
        }

        // This function checks if the path has a small U-turn, that is,
        // a polygon further in the path is adjacent to the first polygon
        // in the path. If that happens, a shortcut is taken.
        // This can happen if the target (T) location is at tile boundary,
        // and we're (S) approaching it parallel to the tile edge.
        // The choice at the vertex can be arbitrary,
        // +---+---+
        // |:::|:::|
        // +-S-+-T-+
        // |:::| | <-- the step can end up in here, resulting U-turn path.
        // +---+---+
        public static List<long> fixupShortcuts(List<long> path, NavMeshQuery navQuery)
        {
            if (path.Count < 3)
            {
                return path;
            }

            // Get connected polygons
            List<long> neis = new List<long>();

            Result<Tuple<MeshTile, Poly>> tileAndPoly = navQuery.getAttachedNavMesh().getTileAndPolyByRef(path[0]);
            if (tileAndPoly.Failed())
            {
                return path;
            }

            MeshTile tile = tileAndPoly.result.Item1;
            Poly poly = tileAndPoly.result.Item2;

            for (int k = tile.polyLinks[poly.index]; k != NavMesh.DT_NULL_LINK; k = tile.links[k].next)
            {
                Link link = tile.links[k];
                if (link.refs != 0)
                {
                    neis.Add(link.refs);
                }
            }

            // If any of the neighbour polygons is within the next few polygons
            // in the path, short cut to that polygon directly.
            int maxLookAhead = 6;
            int cut = 0;
            for (int i = Math.Min(maxLookAhead, path.Count) - 1; i > 1 && cut == 0; i--)
            {
                for (int j = 0; j < neis.Count; j++)
                {
                    if (path[i] == neis[j])
                    {
                        cut = i;
                        break;
                    }
                }
            }

            if (cut > 1)
            {
                List<long> shortcut = new List<long>();
                shortcut.Add(path[0]);
                shortcut.AddRange(path.GetRange(cut, path.Count - cut));
                return shortcut;
            }

            return path;
        }
    }
}