using System;
using System.Collections.Generic;
using DotRecast.Core;
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
            ref List<long> polys, ref List<RcVec3f> smoothPath)
        {
            navQuery.FindPath(startRef, endRef, startPt, endPt, filter, ref polys,
                new DtFindPathOption(enableRaycast ? DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE : 0, float.MaxValue));

            if (0 >= polys.Count)
                return DtStatus.DT_FAILURE;

            // Iterate over the path to find smooth path on the detail mesh surface.
            navQuery.ClosestPointOnPoly(startRef, startPt, out var iterPos, out var _);
            navQuery.ClosestPointOnPoly(polys[polys.Count - 1], endPt, out var targetPos, out var _);

            float STEP_SIZE = 0.5f;
            float SLOP = 0.01f;

            smoothPath.Clear();
            smoothPath.Add(iterPos);
            var visited = new List<long>();

            // Move towards target a small advancement at a time until target reached or
            // when ran out of memory to store the path.
            while (0 < polys.Count && smoothPath.Count < MAX_SMOOTH)
            {
                // Find location to steer towards.
                if (!PathUtils.GetSteerTarget(navQuery, iterPos, targetPos, SLOP,
                        polys, out var steerPos, out var steerPosFlag, out var steerPosRef))
                {
                    break;
                }

                bool endOfPath = (steerPosFlag & DtNavMeshQuery.DT_STRAIGHTPATH_END) != 0
                    ? true
                    : false;
                bool offMeshConnection = (steerPosFlag & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0
                    ? true
                    : false;

                // Find movement delta.
                RcVec3f delta = steerPos.Subtract(iterPos);
                float len = (float)Math.Sqrt(RcVec3f.Dot(delta, delta));
                // If the steer target is end of path or off-mesh link, do not move past the location.
                if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                {
                    len = 1;
                }
                else
                {
                    len = STEP_SIZE / len;
                }

                RcVec3f moveTgt = RcVec3f.Mad(iterPos, delta, len);

                // Move
                navQuery.MoveAlongSurface(polys[0], iterPos, moveTgt, filter, out var result, ref visited);

                iterPos = result;

                polys = PathUtils.MergeCorridorStartMoved(polys, visited);
                polys = PathUtils.FixupShortcuts(polys, navQuery);

                var status = navQuery.GetPolyHeight(polys[0], result, out var h);
                if (status.Succeeded())
                {
                    iterPos.y = h;
                }

                // Handle end of path and off-mesh links when close enough.
                if (endOfPath && PathUtils.InRange(iterPos, steerPos, SLOP, 1.0f))
                {
                    // Reached end of path.
                    iterPos = targetPos;
                    if (smoothPath.Count < MAX_SMOOTH)
                    {
                        smoothPath.Add(iterPos);
                    }

                    break;
                }
                else if (offMeshConnection && PathUtils.InRange(iterPos, steerPos, SLOP, 1.0f))
                {
                    // Reached off-mesh connection.
                    RcVec3f startPos = RcVec3f.Zero;
                    RcVec3f endPos = RcVec3f.Zero;

                    // Advance the path up to and over the off-mesh connection.
                    long prevRef = 0;
                    long polyRef = polys[0];
                    int npos = 0;
                    while (npos < polys.Count && polyRef != steerPosRef)
                    {
                        prevRef = polyRef;
                        polyRef = polys[npos];
                        npos++;
                    }

                    polys = polys.GetRange(npos, polys.Count - npos);

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
                        navQuery.GetPolyHeight(polys[0], iterPos, out var eh);
                        iterPos.y = eh;
                    }
                }

                // Store results.
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }
            }

            return DtStatus.DT_SUCCSESS;
        }

        public DtStatus FindStraightPath(DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f startPt, RcVec3f endPt, IDtQueryFilter filter, bool enableRaycast,
            ref List<long> polys, ref List<StraightPathItem> straightPath, int straightPathOptions)
        {
            navQuery.FindPath(startRef, endRef, startPt, endPt, filter, ref polys,
                new DtFindPathOption(enableRaycast ? DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE : 0, float.MaxValue));

            if (0 >= polys.Count)
                return DtStatus.DT_FAILURE;

            // In case of partial path, make sure the end point is clamped to the last polygon.
            var epos = RcVec3f.Of(endPt.x, endPt.y, endPt.z);
            if (polys[polys.Count - 1] != endRef)
            {
                var result = navQuery.ClosestPointOnPoly(polys[polys.Count - 1], endPt, out var closest, out var _);
                if (result.Succeeded())
                {
                    epos = closest;
                }
            }

            navQuery.FindStraightPath(startPt, epos, polys, ref straightPath, MAX_POLYS, straightPathOptions);

            return DtStatus.DT_SUCCSESS;
        }

        public DtStatus InitSlicedFindPath(DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f startPos, RcVec3f m_epos, IDtQueryFilter filter, bool enableRaycast)
        {
            return navQuery.InitSlicedFindPath(startRef, endRef, startPos, m_epos, filter,
                enableRaycast ? DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE : 0,
                float.MaxValue
            );
        }

        public DtStatus Raycast(DtNavMeshQuery navQuery, long startRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter,
            ref List<long> polys, ref List<StraightPathItem> straightPath, out RcVec3f hitPos, out RcVec3f hitNormal, out bool hitResult)
        {
            hitPos = RcVec3f.Zero;
            hitNormal = RcVec3f.Zero;
            hitResult = false;

            var status = navQuery.Raycast(startRef, startPos, endPos, filter, 0, 0, out var rayHit);
            if (!status.Succeeded())
                return status;

            polys = rayHit.path;
            if (rayHit.t > 1)
            {
                // No hit
                hitPos = endPos;
                hitResult = false;
            }
            else
            {
                // Hit
                hitPos = RcVec3f.Lerp(startPos, endPos, rayHit.t);
                hitNormal = rayHit.hitNormal;
                hitResult = true;
            }

            // Adjust height.
            if (rayHit.path.Count > 0)
            {
                var result = navQuery.GetPolyHeight(rayHit.path[rayHit.path.Count - 1], hitPos, out var h);
                if (result.Succeeded())
                {
                    hitPos.y = h;
                }
            }

            straightPath.Clear();
            straightPath.Add(new StraightPathItem(startPos, 0, 0));
            straightPath.Add(new StraightPathItem(hitPos, 0, 0));

            return status;
        }

        public DtStatus FindPolysAroundCircle(DtNavMeshQuery navQuery, long startRef, RcVec3f spos, RcVec3f epos, IDtQueryFilter filter, ref List<long> resultRef, ref List<long> resultParent)
        {
            float dx = epos.x - spos.x;
            float dz = epos.z - spos.z;
            float dist = (float)Math.Sqrt(dx * dx + dz * dz);

            List<float> costs = new List<float>();
            return navQuery.FindPolysAroundCircle(startRef, spos, dist, filter, ref resultRef, ref resultParent, ref costs);
        }

        public DtStatus FindPolysAroundShape(DtNavMeshQuery navQuery, RcNavMeshBuildSettings settings, long startRef, RcVec3f spos, RcVec3f epos, IDtQueryFilter filter, ref List<long> resultRef, ref List<long> resultParent, out RcVec3f[] queryPoly)
        {
            float nx = (epos.z - spos.z) * 0.25f;
            float nz = -(epos.x - spos.x) * 0.25f;
            float agentHeight = settings != null ? settings.agentHeight : 0;

            queryPoly = new RcVec3f[4];
            queryPoly[0].x = spos.x + nx * 1.2f;
            queryPoly[0].y = spos.y + agentHeight / 2;
            queryPoly[0].z = spos.z + nz * 1.2f;

            queryPoly[1].x = spos.x - nx * 1.3f;
            queryPoly[1].y = spos.y + agentHeight / 2;
            queryPoly[1].z = spos.z - nz * 1.3f;

            queryPoly[2].x = epos.x - nx * 0.8f;
            queryPoly[2].y = epos.y + agentHeight / 2;
            queryPoly[2].z = epos.z - nz * 0.8f;

            queryPoly[3].x = epos.x + nx;
            queryPoly[3].y = epos.y + agentHeight / 2;
            queryPoly[3].z = epos.z + nz;

            var costs = new List<float>();
            return navQuery.FindPolysAroundShape(startRef, queryPoly, filter, ref resultRef, ref resultParent, ref costs);
        }

        public DtStatus FindRandomPointAroundCircle(DtNavMeshQuery navQuery, long startRef, RcVec3f spos, RcVec3f epos, IDtQueryFilter filter, bool constrainByCircle, int count, ref List<RcVec3f> points)
        {
            float dx = epos.x - spos.x;
            float dz = epos.z - spos.z;
            float dist = (float)Math.Sqrt(dx * dx + dz * dz);

            IDtPolygonByCircleConstraint constraint = constrainByCircle
                ? DtStrictDtPolygonByCircleConstraint.Shared
                : DtNoOpDtPolygonByCircleConstraint.Shared;

            var frand = new FRand();
            int prevCnt = points.Count;

            while (0 < count && points.Count < prevCnt + count)
            {
                var status = navQuery.FindRandomPointAroundCircle(startRef, spos, dist, filter, frand, constraint,
                    out var randomRef, out var randomPt);

                if (status.Succeeded())
                {
                    points.Add(randomPt);
                }
            }

            return DtStatus.DT_SUCCSESS;
        }
    }
}