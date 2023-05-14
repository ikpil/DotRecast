using System;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour
{
    using static DotRecast.Core.RcMath;

    public class FindNearestPolyQuery : IPolyQuery
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

        public void Process(MeshTile tile, Poly poly, long refs)
        {
            // Find nearest polygon amongst the nearby polygons.
            Result<ClosestPointOnPolyResult> closest = query.ClosestPointOnPoly(refs, center);
            bool posOverPoly = closest.result.IsPosOverPoly();
            var closestPtPoly = closest.result.GetClosest();

            // If a point is directly over a polygon and closer than
            // climb height, favor that instead of straight line nearest point.
            float d = 0;
            Vector3f diff = center.Subtract(closestPtPoly);
            if (posOverPoly)
            {
                d = Math.Abs(diff.y) - tile.data.header.walkableClimb;
                d = d > 0 ? d * d : 0;
            }
            else
            {
                d = VLenSqr(diff);
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