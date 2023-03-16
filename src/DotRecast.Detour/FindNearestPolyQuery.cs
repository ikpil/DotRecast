using System;

namespace DotRecast.Detour
{
    using static DetourCommon;

    public class FindNearestPolyQuery : PolyQuery
    {
        private readonly NavMeshQuery query;
        private readonly float[] center;
        private long nearestRef;
        private float[] nearestPt;
        private bool overPoly;
        private float nearestDistanceSqr;

        public FindNearestPolyQuery(NavMeshQuery query, float[] center)
        {
            this.query = query;
            this.center = center;
            nearestDistanceSqr = float.MaxValue;
            nearestPt = new float[] { center[0], center[1], center[2] };
        }

        public void process(MeshTile tile, Poly poly, long refs)
        {
            // Find nearest polygon amongst the nearby polygons.
            Result<ClosestPointOnPolyResult> closest = query.closestPointOnPoly(refs, center);
            bool posOverPoly = closest.result.isPosOverPoly();
            float[] closestPtPoly = closest.result.getClosest();

            // If a point is directly over a polygon and closer than
            // climb height, favor that instead of straight line nearest point.
            float d = 0;
            float[] diff = vSub(center, closestPtPoly);
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