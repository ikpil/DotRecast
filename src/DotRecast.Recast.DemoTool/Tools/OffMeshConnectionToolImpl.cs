using System;
using DotRecast.Core;
using DotRecast.Recast.DemoTool.Builder;
using DotRecast.Recast.DemoTool.Geom;

namespace DotRecast.Recast.DemoTool.Tools
{
    public class OffMeshConnectionToolImpl : ISampleTool
    {
        private Sample _sample;
        private readonly OffMeshConnectionToolOption _option;

        public OffMeshConnectionToolImpl()
        {
            _option = new OffMeshConnectionToolOption();
        }

        public string GetName()
        {
            return "Create Off-Mesh Links";
        }

        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }


        public OffMeshConnectionToolOption GetOption()
        {
            return _option;
        }

        public void Add(RcVec3f start, RcVec3f end)
        {
            DemoInputGeomProvider geom = _sample.GetInputGeom();
            if (null == geom)
                return;

            int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP;
            int flags = SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP;
            geom.AddOffMeshConnection(start, end, _sample.GetSettings().agentRadius, 0 == _option.bidir, area, flags);
        }

        public void Remove(RcVec3f p)
        {
            DemoInputGeomProvider geom = _sample.GetInputGeom();
            if (null == geom)
                return;

            // Delete
            // Find nearest link end-point
            float nearestDist = float.MaxValue;
            DemoOffMeshConnection nearestConnection = null;
            foreach (DemoOffMeshConnection offMeshCon in geom.GetOffMeshConnections())
            {
                float d = Math.Min(RcVec3f.DistSqr(p, offMeshCon.verts, 0), RcVec3f.DistSqr(p, offMeshCon.verts, 3));
                if (d < nearestDist && Math.Sqrt(d) < _sample.GetSettings().agentRadius)
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