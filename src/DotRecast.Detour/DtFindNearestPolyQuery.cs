using System;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour
{
    public class DtFindNearestPolyQuery : IDtPolyQuery
    {
        private readonly DtNavMeshQuery query;
        private readonly RcVec3f center;
        private long nearestRef;
        private RcVec3f nearestPt;
        private bool overPoly;
        private float nearestDistanceSqr;

        public DtFindNearestPolyQuery(DtNavMeshQuery query, RcVec3f center)
        {
            this.query = query;
            this.center = center;
            nearestDistanceSqr = float.MaxValue;
            nearestPt = center;
        }

        public void Process(DtMeshTile tile, DtPoly poly, long refs)
        {
            // Find nearest polygon amongst the nearby polygons.
            query.ClosestPointOnPoly(refs, center, out var closestPtPoly, out var posOverPoly);

            // If a point is directly over a polygon and closer than
            // climb height, favor that instead of straight line nearest point.
            float d = 0;
            RcVec3f diff = center.Subtract(closestPtPoly);
            if (posOverPoly)
            {
                d = Math.Abs(diff.y) - tile.data.header.walkableClimb;
                d = d > 0 ? d * d : 0;
            }
            else
            {
                d = RcVec3f.LenSqr(diff);
            }

            if (d < nearestDistanceSqr)
            {
                nearestPt = closestPtPoly;
                nearestDistanceSqr = d;
                nearestRef = refs;
                overPoly = posOverPoly;
            }
        }

        public FindNearestPolyResult Result()
        {
            return new FindNearestPolyResult(nearestRef, nearestPt, overPoly);
        }
    }
}