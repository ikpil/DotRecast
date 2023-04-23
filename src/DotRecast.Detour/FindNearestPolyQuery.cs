using System;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour
{
    using static DotRecast.Core.RecastMath;

    public class FindNearestPolyQuery : PolyQuery
    {
        private readonly NavMeshQuery query;
        private readonly Vector3f center;
        private long nearestRef;
        private Vector3f nearestPt;
        private bool overPoly;
        private float nearestDistanceSqr;

        public FindNearestPolyQuery(NavMeshQuery query, Vector3f center)
        {
            this.query = query;
            this.center = center;
            nearestDistanceSqr = float.MaxValue;
            nearestPt = center;
        }

        public void process(MeshTile tile, Poly poly, long refs)
        {
            // Find nearest polygon amongst the nearby polygons.
            Result<ClosestPointOnPolyResult> closest = query.closestPointOnPoly(refs, center);
            bool posOverPoly = closest.result.isPosOverPoly();
            var closestPtPoly = closest.result.getClosest();

            // If a point is directly over a polygon and closer than
            // climb height, favor that instead of straight line nearest point.
            float d = 0;
            Vector3f diff = vSub(center, closestPtPoly);
            if (posOverPoly)
            {
                d = Math.Abs(diff[1]) - tile.data.header.walkableClimb;
                d = d > 0 ? d * d : 0;
            }
            else
            {
                d = vLenSqr(diff);
            }

            if (d < nearestDistanceSqr)
            {
                nearestPt = closestPtPoly;
                nearestDistanceSqr = d;
                nearestRef = refs;
                overPoly = posOverPoly;
            }
        }

        public FindNearestPolyResult result()
        {
            return new FindNearestPolyResult(nearestRef, nearestPt, overPoly);
        }
    }
}