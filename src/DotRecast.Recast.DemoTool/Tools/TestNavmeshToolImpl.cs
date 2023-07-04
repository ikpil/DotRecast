using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour;

namespace DotRecast.Recast.DemoTool.Tools
{
    public class TestNavmeshToolImpl : ISampleTool
    {
        private const int MAX_POLYS = 256;
        private const int MAX_SMOOTH = 2048;

        private Sample _sample;
        private readonly TestNavmeshToolOption _option;

        public TestNavmeshToolImpl()
        {
            _option = new TestNavmeshToolOption();
        }

        public string GetName()
        {
            return "Test Navmesh";
        }


        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }

        public TestNavmeshToolOption GetOption()
        {
            return _option;
        }

        public DtStatus FindFollowPath(long startRef, long endRef, RcVec3f startPt, RcVec3f endPt, IDtQueryFilter filter, bool enableRaycast,
            ref List<long> polys, ref List<RcVec3f> smoothPath)
        {
            var navMesh = _sample.GetNavMesh();
            var navQuery = _sample.GetNavMeshQuery();

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
                SteerTarget steerTarget = PathUtils.GetSteerTarget(navQuery, iterPos, targetPos, SLOP, polys);
                if (null == steerTarget)
                {
                    break;
                }

                bool endOfPath = (steerTarget.steerPosFlag & DtNavMeshQuery.DT_STRAIGHTPATH_END) != 0
                    ? true
                    : false;
                bool offMeshConnection = (steerTarget.steerPosFlag
                                          & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0
                    ? true
                    : false;

                // Find movement delta.
                RcVec3f delta = steerTarget.steerPos.Subtract(iterPos);
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
                if (endOfPath && PathUtils.InRange(iterPos, steerTarget.steerPos, SLOP, 1.0f))
                {
                    // Reached end of path.
                    iterPos = targetPos;
                    if (smoothPath.Count < MAX_SMOOTH)
                    {
                        smoothPath.Add(iterPos);
                    }

                    break;
                }
                else if (offMeshConnection && PathUtils.InRange(iterPos, steerTarget.steerPos, SLOP, 1.0f))
                {
                    // Reached off-mesh connection.
                    RcVec3f startPos = RcVec3f.Zero;
                    RcVec3f endPos = RcVec3f.Zero;

                    // Advance the path up to and over the off-mesh connection.
                    long prevRef = 0;
                    long polyRef = polys[0];
                    int npos = 0;
                    while (npos < polys.Count && polyRef != steerTarget.steerPosRef)
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

        public DtStatus FindStraightPath(long startRef, long endRef, RcVec3f startPt, RcVec3f endPt, IDtQueryFilter filter, bool enableRaycast,
            ref List<long> polys, ref List<StraightPathItem> straightPath, int straightPathOptions)
        {
            var navQuery = _sample.GetNavMeshQuery();

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

        public DtStatus InitSlicedFindPath(long startRef, long endRef, RcVec3f startPos, RcVec3f m_epos, IDtQueryFilter filter, bool enableRaycast)
        {
            var navQuery = _sample.GetNavMeshQuery();
            return navQuery.InitSlicedFindPath(startRef, endRef, startPos, m_epos, filter,
                enableRaycast ? DtNavMeshQuery.DT_FINDPATH_ANY_ANGLE : 0,
                float.MaxValue
            );
        }

        public DtStatus Raycast(long startRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter,
            ref List<long> polys, ref List<StraightPathItem> straightPath, out RcVec3f hitPos, out RcVec3f hitNormal, out bool hitResult)
        {
            hitPos = RcVec3f.Zero;
            hitNormal = RcVec3f.Zero;
            hitResult = false;

            var navQuery = _sample.GetNavMeshQuery();
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
    }
}