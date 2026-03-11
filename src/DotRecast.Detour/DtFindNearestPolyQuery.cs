using System;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour
{
    public class DtFindNearestPolyQuery : IDtPolyQuery
    {
        private readonly DtNavMeshQuery _query;
        private RcVec3f _center;
        private float _nearestDistanceSqr;
        private long _nearestRef;
        private RcVec3f _nearestPoint;
        private bool _overPoly;

        public DtFindNearestPolyQuery(DtNavMeshQuery query, RcVec3f center)
        {
            _query = query;
            Reset(center);
        }

        public void Reset(RcVec3f center)
        {
            _center = center;
            _nearestDistanceSqr = float.MaxValue;
            _nearestRef = 0;
            _nearestPoint = center;
            _overPoly = false;
        }

        public void Process(DtMeshTile tile, ReadOnlySpan<int> polys, ReadOnlySpan<long> refs, int count)
        {
            float walkableClimb = tile.data.header.walkableClimb;

            for (int i = 0; i < count; ++i)
            {
                long polyRef = refs[i];
                float d;

                // Find nearest polygon amongst the nearby polygons.
                _query.ClosestPointOnPolyUnsafe(tile, tile.data.polys[polys[i]], _center, out var closestPtPoly, out var posOverPoly);

                // If a point is directly over a polygon and closer than
                // climb height, favor that instead of straight line nearest point.
                float dx = _center.X - closestPtPoly.X;
                float dy = _center.Y - closestPtPoly.Y;
                float dz = _center.Z - closestPtPoly.Z;
                if (posOverPoly)
                {
                    d = MathF.Abs(dy) - walkableClimb;
                    d = d > 0 ? d * d : 0;
                }
                else
                {
                    d = dx * dx + dy * dy + dz * dz;
                }

                if (d < _nearestDistanceSqr)
                {
                    _nearestPoint = closestPtPoly;
                    _nearestDistanceSqr = d;
                    _nearestRef = polyRef;
                    _overPoly = posOverPoly;
                }
            }
        }

        public long NearestRef()
        {
            return _nearestRef;
        }

        public RcVec3f NearestPt()
        {
            return _nearestPoint;
        }

        public bool OverPoly()
        {
            return _overPoly;
        }
    }
}
