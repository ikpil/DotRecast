using System;
using DotRecast.Core;
using DotRecast.Recast.Geom;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Toolset.Geom;

namespace DotRecast.Recast.Toolset.Tools
{
    public class OffMeshConnectionToolImpl : IRcToolable
    {
        private readonly OffMeshConnectionToolOption _option;

        public OffMeshConnectionToolImpl()
        {
            _option = new OffMeshConnectionToolOption();
        }

        public string GetName()
        {
            return "Create Off-Mesh Links";
        }

        public OffMeshConnectionToolOption GetOption()
        {
            return _option;
        }

        public void Add(IInputGeomProvider geom, RcNavMeshBuildSettings settings, RcVec3f start, RcVec3f end)
        {
            if (null == geom)
                return;

            int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP;
            int flags = SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP;
            geom.AddOffMeshConnection(start, end, settings.agentRadius, 0 == _option.bidir, area, flags);
        }

        public void Remove(IInputGeomProvider geom, RcNavMeshBuildSettings settings, RcVec3f p)
        {
            // Delete
            // Find nearest link end-point
            float nearestDist = float.MaxValue;
            DtOffMeshConnectionParam nearestConnection = null;
            foreach (DtOffMeshConnectionParam offMeshCon in geom.GetOffMeshConnections())
            {
                float d = Math.Min(RcVec3f.DistSqr(p, offMeshCon.verts, 0), RcVec3f.DistSqr(p, offMeshCon.verts, 3));
                if (d < nearestDist && Math.Sqrt(d) < settings.agentRadius)
                {
                    nearestDist = d;
                    nearestConnection = offMeshCon;
                }
            }

            if (nearestConnection != null)
            {
                geom.GetOffMeshConnections().Remove(nearestConnection);
            }
        }
    }
}