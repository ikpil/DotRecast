using System;
using DotRecast.Core;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour
{
    public struct DtHeightSamplePolyQuery : IDtPolyQuery
    {
        private readonly DtNavMeshQuery _navMeshQuery;
        private readonly RcVec3f _pt;
        private readonly float _maxHeight;
        public float MinHeight { get; private set; }
        public bool Found { get; private set; }

        public DtHeightSamplePolyQuery(DtNavMeshQuery navMeshQuery, RcVec3f pt, float minHeight, float maxHeight)
        {
            _navMeshQuery = navMeshQuery;
            _pt = pt;
            MinHeight = minHeight;
            _maxHeight = maxHeight;
            Found = default;
        }

        public void Process(DtMeshTile tile, Span<DtPoly> poly, Span<long> refs, int count)
        {
            for (int i = 0; i < count; i++)
            {
                ProcessSingle(refs[i]);
            }
        }

        private void ProcessSingle(long refs)
        {
            var status = _navMeshQuery.GetPolyHeight(refs, _pt, out var h);
            if (!status.Succeeded())
                return;

            if (!(h > MinHeight) || !(h < _maxHeight)) 
                return;

            MinHeight = h;
            Found = true;
        }
    }
}