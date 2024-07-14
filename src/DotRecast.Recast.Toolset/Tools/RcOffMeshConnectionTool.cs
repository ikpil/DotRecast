using System;
using System.Numerics;
using DotRecast.Core;
using DotRecast.Recast.Geom;
using DotRecast.Recast.Toolset.Builder;

namespace DotRecast.Recast.Toolset.Tools
{
    public class RcOffMeshConnectionTool : IRcToolable
    {
        public RcOffMeshConnectionTool()
        {
        }

        public string GetName()
        {
            return "Off-Mesh Links";
        }

        public void Add(IInputGeomProvider geom, RcNavMeshBuildSettings settings, Vector3 start, Vector3 end, bool bidir)
        {
            if (null == geom)
                return;

            int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP;
            int flags = SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP;
            geom.AddOffMeshConnection(start, end, settings.agentRadius, bidir, area, flags);
        }

        public void Remove(IInputGeomProvider geom, RcNavMeshBuildSettings settings, Vector3 p)
        {
            // Delete
            // Find nearest link end-point
            float nearestDist = float.MaxValue;
            int nearestId = -1;
            for (int i = 0; i < geom.OffMeshConCount; i++)
            {
                var verts = geom.OffMeshConVerts.AsSpan(i * 6);
                float d = Math.Min(RcVec.DistanceSquared(p, verts, 0), RcVec.DistanceSquared(p, verts, 3));
                if (d < nearestDist && Math.Sqrt(d) < settings.agentRadius)
                {
                    nearestDist = d;
                    nearestId = i;
                }
            }

            if (nearestId != -1)
            {
                geom.RemoveOffMeshConnection(nearestId);
            }
        }
    }
}