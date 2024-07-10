using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour;

namespace DotRecast.Recast.Toolset.Tools
{
    public class RcTestNavMeshTool : IRcToolable
    {
        public const int MAX_POLYS = 256;
        public const int MAX_SMOOTH = 2048;

        public RcTestNavMeshTool()
        {
        }

        public string GetName()
        {
            return "Test Navmesh";
        }

        public DtStatus FindFollowPath(DtNavMesh navMesh, DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f startPt, RcVec3f endPt, IDtQueryFilter filter, bool enableRaycast,
             Span<long> pathIterPolys, int pathIterPolyCount, ref List<RcVec3f> smoothPath)
        {
            if (startRef == 0 || endRef == 0)
            {
                //pathIterPolys?.Clear();
                smoothPath?.Clear();

                return DtStatus.DT_FAILURE;
            }

            //pathIterPolys ??= new List<long>();
            smoothPath ??= new List<RcVec3f>();

            pathIterPolys.Clear();
            pathIterPolyCount = 0;

            smoothPath.Clear();

            var opt = new DtFindPathOption(enableRaycast ? DtFindPathOptions.DT_FINDPATH_ANY_ANGLE : 0, float.MaxValue);
            navQuery.FindPath(startRef, endRef, startPt, endPt, filter, pathIterPolys, out pathIterPolyCount, opt);
            if (0 >= pathIterPolyCount)
                return DtStatus.DT_FAILURE;

            //pathIterPolyCount = pathIterPolys.Count;

            // Iterate over the path to find smooth path on the detail mesh surface.
            navQuery.ClosestPointOnPoly(startRef, startPt, out var iterPos, out var _);
            navQuery.ClosestPointOnPoly(pathIterPolys[pathIterPolyCount - 1], endPt, out var targetPos, out var _);

            const float STEP_SIZE = 0.5f;
            const float SLOP = 0.01f;

            smoothPath.Clear();
            smoothPath.Add(iterPos);

            Span<long> visited = stackalloc long[16];
            int nvisited = 0;


            // Move towards target a small advancement at a time until target reached or
            // when ran out of memory to store the path.
            while (0 < pathIterPolyCount && smoothPath.Count < MAX_SMOOTH)
            {
                // Find location to steer towards.
                if (!DtPathUtils.GetSteerTarget(navQuery, iterPos, targetPos, SLOP,
                        pathIterPolys, pathIterPolyCount, out var steerPos, out var steerPosFlag, out var steerPosRef))
                {
                    break;
                }

                bool endOfPath = (steerPosFlag & DtStraightPathFlags.DT_STRAIGHTPATH_END) != 0
                    ? true
                    : false;
                bool offMeshConnection = (steerPosFlag & DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0
                    ? true
                    : false;

                // Find movement delta.
                RcVec3f delta = RcVec3f.Subtract(steerPos, iterPos);
                float len = MathF.Sqrt(RcVec3f.Dot(delta, delta));
                // If the steer target is end of path or off-mesh link, do not move past the location.
                if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                {
                    len = 1;
                }
                else
                {
                    len = STEP_SIZE / len;
                }

                RcVec3f moveTgt = RcVec.Mad(iterPos, delta, len);

                // Move
                navQuery.MoveAlongSurface(pathIterPolys[0], iterPos, moveTgt, filter, out var result, visited, out nvisited, 16);

                iterPos = result;

                pathIterPolyCount = DtPathUtils.MergeCorridorStartMoved(pathIterPolys, pathIterPolyCount, MAX_POLYS, visited, nvisited);
                pathIterPolyCount = DtPathUtils.FixupShortcuts(pathIterPolys, pathIterPolyCount, navQuery);

                var status = navQuery.GetPolyHeight(pathIterPolys[0], result, out var h);
                if (status.Succeeded())
                {
                    iterPos.Y = h;
                }

                // Handle end of path and off-mesh links when close enough.
                if (endOfPath && DtPathUtils.InRange(iterPos, steerPos, SLOP, 1.0f))
                {
                    // Reached end of path.
                    iterPos = targetPos;
                    if (smoothPath.Count < MAX_SMOOTH)
                    {
                        smoothPath.Add(iterPos);
                    }

                    break;
                }
                else if (offMeshConnection && DtPathUtils.InRange(iterPos, steerPos, SLOP, 1.0f))
                {
                    // Reached off-mesh connection.
                    RcVec3f startPos = RcVec3f.Zero;
                    RcVec3f endPos = RcVec3f.Zero;

                    // Advance the path up to and over the off-mesh connection.
                    long prevRef = 0;
                    long polyRef = pathIterPolys[0];
                    int npos = 0;
                    while (npos < pathIterPolyCount && polyRef != steerPosRef)
                    {
                        prevRef = polyRef;
                        polyRef = pathIterPolys[npos];
                        npos++;
                    }

                    //pathIterPolys = pathIterPolys.GetRange(npos, pathIterPolys.Count - npos);
                    for (int i = npos; i < pathIterPolyCount; ++i)
                        pathIterPolys[i - npos] = pathIterPolys[i];
                    pathIterPolyCount -= npos;

                    // Handle the connection.
                    var status2 = navMesh.GetOffMeshConnectionPolyEndPoints(prevRef, polyRef, ref startPos, ref endPos);
                    if (status2.Succeeded())
                    {
                        if (smoothPath.Count < MAX_SMOOTH)
                        {
                            smoothPath.Add(startPos);
                            // Hack to make the dotted path not visible during off-mesh connection.
                            if ((smoothPath.Count & 1) != 0)
                            {
                                smoothPath.Add(startPos);
                            }
                        }

                        // Move position at the other side of the off-mesh link.
                        iterPos = endPos;
                        navQuery.GetPolyHeight(pathIterPolys[0], iterPos, out var eh);
                        iterPos.Y = eh;
                    }
                }

                // Store results.
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }
            }

            return DtStatus.DT_SUCCESS;
        }

        public DtStatus FindStraightPath(DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f startPt, RcVec3f endPt, IDtQueryFilter filter, bool enableRaycast,
            Span<long> polys, Span<DtStraightPath> straightPath, out int straightPathCount, int maxStraightPath, int straightPathOptions)
        {
            straightPathCount = 0;
            if (startRef == 0 || endRef == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            //polys.Clear();
            //straightPath.Clear();

            var opt = new DtFindPathOption(enableRaycast ? DtFindPathOptions.DT_FINDPATH_ANY_ANGLE : 0, float.MaxValue);
            navQuery.FindPath(startRef, endRef, startPt, endPt, filter, polys, out var polysCount, opt);

            if (0 >= polysCount)
                return DtStatus.DT_FAILURE;

            // In case of partial path, make sure the end point is clamped to the last polygon.
            var epos = new RcVec3f(endPt.X, endPt.Y, endPt.Z);
            if (polys[polysCount - 1] != endRef)
            {
                var result = navQuery.ClosestPointOnPoly(polys[polysCount - 1], endPt, out var closest, out var _);
                if (result.Succeeded())
                {
                    epos = closest;
                }
            }

            navQuery.FindStraightPath(startPt, epos, polys, polysCount, straightPath, out straightPathCount, maxStraightPath, straightPathOptions);

            return DtStatus.DT_SUCCESS;
        }

        public DtStatus InitSlicedFindPath(DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter, bool enableRaycast)
        {
            if (startRef == 0 || endRef == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            return navQuery.InitSlicedFindPath(startRef, endRef, startPos, endPos, filter,
                enableRaycast ? DtFindPathOptions.DT_FINDPATH_ANY_ANGLE : 0,
                float.MaxValue
            );
        }

        public DtStatus UpdateSlicedFindPath(DtNavMeshQuery navQuery, int maxIter, long endRef, RcVec3f startPos, RcVec3f endPos,
            Span<long> path, Span<DtStraightPath> straightPath, out int straightPathCount, int maxStraightPath)
        {
            straightPathCount = 0;
            var status = navQuery.UpdateSlicedFindPath(maxIter, out _);

            if (!status.Succeeded())
            {
                return status;
            }

            navQuery.FinalizeSlicedFindPath(path, out var pathCount);

            if (path != null)
            {
                // In case of partial path, make sure the end point is clamped to the last polygon.
                RcVec3f epos = endPos;
                if (path[pathCount - 1] != endRef)
                {
                    var result = navQuery.ClosestPointOnPoly(path[pathCount - 1], endPos, out var closest, out var _);
                    if (result.Succeeded())
                    {
                        epos = closest;
                    }
                }

                navQuery.FindStraightPath(startPos, epos, path, pathCount, straightPath, out straightPathCount, maxStraightPath, DtStraightPathOptions.DT_STRAIGHTPATH_ALL_CROSSINGS);
            }

            return DtStatus.DT_SUCCESS;
        }


        public DtStatus Raycast(DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter,
            Span<long> polys, Span<DtStraightPath> straightPath, out int straightPathCount, int maxStraightPath, ref RcVec3f hitPos, ref RcVec3f hitNormal, ref bool hitResult)
        {
            straightPathCount = 0;
            if (startRef == 0 || endRef == 0)
            {
                //polys?.Clear();
                return DtStatus.DT_FAILURE;
            }

            //var path = new List<long>();
            //polys ??= new List<long>();
            var status = navQuery.Raycast(startRef, startPos, endPos, filter, out var t, out var hitNormal2, polys, out var polysCount);
            if (!status.Succeeded())
            {
                return status;
            }

            // results ...
            //polys = path;

            if (t > 1)
            {
                // No hit
                hitPos = endPos;
                hitResult = false;
            }
            else
            {
                // Hit
                hitPos = RcVec3f.Lerp(startPos, endPos, t);
                hitNormal = hitNormal2;
                hitResult = true;
            }

            // Adjust height.
            if (polysCount > 0)
            {
                var result = navQuery.GetPolyHeight(polys[polysCount - 1], hitPos, out var h);
                if (result.Succeeded())
                {
                    hitPos.Y = h;
                }
            }

            straightPath[straightPathCount++] = new DtStraightPath(startPos, 0, 0);
            straightPath[straightPathCount++] = new DtStraightPath(hitPos, 0, 0);

            return status;
        }

        public DtStatus FindDistanceToWall(DtNavMeshQuery navQuery, long startRef, RcVec3f spos, float maxRadius, IDtQueryFilter filter,
            ref float hitDist, ref RcVec3f hitPos, ref RcVec3f hitNormal)
        {
            if (0 == startRef)
            {
                return DtStatus.DT_FAILURE;
            }

            var status = navQuery.FindDistanceToWall(startRef, spos, maxRadius, filter,
                out var tempHitDist, out var tempHitPos, out var tempHitNormal);

            if (status.Succeeded())
            {
                hitDist = tempHitDist;
                hitPos = tempHitPos;
                hitNormal = tempHitNormal;
            }

            return status;
        }


        public DtStatus FindPolysAroundCircle(DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f spos, RcVec3f epos, IDtQueryFilter filter, Span<long> resultRef, Span<long> resultParent)
        {
            if (startRef == 0 || endRef == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            float dx = epos.X - spos.X;
            float dz = epos.Z - spos.Z;
            float dist = MathF.Sqrt(dx * dx + dz * dz);

            //List<long> tempResultRefs = new List<long>();
            //List<long> tempParentRefs = new List<long>();
            //List<float> tempCosts = new List<float>();
            var status = navQuery.FindPolysAroundCircle(startRef, spos, dist, filter, resultRef, resultParent, null, out var resultCount, resultRef.Length);
            if (status.Succeeded())
            {

            }

            return status;
        }

        public DtStatus FindLocalNeighbourhood(DtNavMeshQuery navQuery, long startRef, RcVec3f spos, float radius, IDtQueryFilter filter,
            Span<long> resultRef, Span<long> resultParent, out int resultCount, int maxResult)
        {
            resultCount = 0;

            if (startRef == 0)
            {
                //resultRef?.Clear();
                //resultParent?.Clear();
                return DtStatus.DT_FAILURE;
            }

            //resultRef ??= new List<long>();
            //resultParent ??= new List<long>();

            //resultRef.Clear();
            //resultParent.Clear();

            var status = navQuery.FindLocalNeighbourhood(startRef, spos, radius, filter, resultRef, resultParent, out resultCount, MAX_POLYS);
            return status;
        }


        public DtStatus FindPolysAroundShape(DtNavMeshQuery navQuery, float agentHeight, long startRef, long endRef, RcVec3f spos, RcVec3f epos, IDtQueryFilter filter,
            Span<long> resultRefs, Span<long> resultParents, ref RcVec3f[] queryPoly)
        {
            if (startRef == 0 || endRef == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            float nx = (epos.Z - spos.Z) * 0.25f;
            float nz = -(epos.X - spos.X) * 0.25f;

            var tempQueryPoly = queryPoly ??= new RcVec3f[4];
            tempQueryPoly[0].X = spos.X + nx * 1.2f;
            tempQueryPoly[0].Y = spos.Y + agentHeight / 2;
            tempQueryPoly[0].Z = spos.Z + nz * 1.2f;

            tempQueryPoly[1].X = spos.X - nx * 1.3f;
            tempQueryPoly[1].Y = spos.Y + agentHeight / 2;
            tempQueryPoly[1].Z = spos.Z - nz * 1.3f;

            tempQueryPoly[2].X = epos.X - nx * 0.8f;
            tempQueryPoly[2].Y = epos.Y + agentHeight / 2;
            tempQueryPoly[2].Z = epos.Z - nz * 0.8f;

            tempQueryPoly[3].X = epos.X + nx;
            tempQueryPoly[3].Y = epos.Y + agentHeight / 2;
            tempQueryPoly[3].Z = epos.Z + nz;

            var status = navQuery.FindPolysAroundShape(startRef, tempQueryPoly, filter, resultRefs, resultParents, null, out var resultCount, resultRefs.Length);
            if (status.Succeeded())
            {
                //resultRefs = tempResultRefs;
                //resultParents = tempResultParents;
                //queryPoly = tempQueryPoly;
            }

            return status;
        }

        public DtStatus FindRandomPointAroundCircle(DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f spos, RcVec3f epos, IDtQueryFilter filter, bool constrainByCircle, int count,
            ref List<RcVec3f> points)
        {
            if (startRef == 0 || endRef == 0)
            {
                return DtStatus.DT_FAILURE;
            }

            float dx = epos.X - spos.X;
            float dz = epos.Z - spos.Z;
            float dist = MathF.Sqrt(dx * dx + dz * dz);

            IDtPolygonByCircleConstraint constraint = constrainByCircle
                ? DtStrictDtPolygonByCircleConstraint.Shared
                : DtNoOpDtPolygonByCircleConstraint.Shared;

            var frand = new RcRand();
            int prevCnt = points.Count;

            points = new List<RcVec3f>();
            while (0 < count && points.Count < prevCnt + count)
            {
                var status = navQuery.FindRandomPointAroundCircle(startRef, spos, dist, filter, frand, constraint,
                    out var randomRef, out var randomPt);

                if (status.Succeeded())
                {
                    points.Add(randomPt);
                }
            }

            return DtStatus.DT_SUCCESS;
        }
    }
}