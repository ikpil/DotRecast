using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour.Extras.Jumplink;
using DotRecast.Recast.DemoTool.Builder;
using DotRecast.Recast.DemoTool.Geom;

namespace DotRecast.Recast.DemoTool.Tools
{
    public class JumpLinkBuilderToolImpl : ISampleTool
    {
        private Sample _sample;

        private readonly List<JumpLink> _links;
        private JumpLinkBuilder _annotationBuilder;
        private readonly int _selEdge = -1;
        private readonly JumpLinkBuilderToolOptions _option;

        public JumpLinkBuilderToolImpl()
        {
            _links = new List<JumpLink>();
            _option = new JumpLinkBuilderToolOptions();
        }


        public string GetName()
        {
            return "Annotation Builder";
        }

        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }

        public void Clear()
        {
            _annotationBuilder = null;
        }

        public JumpLinkBuilderToolOptions GetOption()
        {
            return _option;
        }
        
        public JumpLinkBuilder GetAnnotationBuilder()
        {
            return _annotationBuilder;
        }

        public int GetSelEdge()
        {
            return _selEdge;
        }

        public List<JumpLink> GetLinks()
        {
            return _links;
        }

        public void Build(bool buildOffMeshConnections)
        {
            if (_annotationBuilder == null)
            {
                if (_sample != null && 0 < _sample.GetRecastResults().Count)
                {
                    _annotationBuilder = new JumpLinkBuilder(_sample.GetRecastResults());
                }
            }

            _links.Clear();
            if (_annotationBuilder != null)
            {
                var settings = _sample.GetSettings();
                float cellSize = settings.cellSize;
                float agentHeight = settings.agentHeight;
                float agentRadius = settings.agentRadius;
                float agentClimb = settings.agentMaxClimb;
                float cellHeight = settings.cellHeight;

                if ((_option.buildTypes & JumpLinkType.EDGE_CLIMB_DOWN.Bit) != 0)
                {
                    JumpLinkBuilderConfig config = new JumpLinkBuilderConfig(cellSize, cellHeight, agentRadius,
                        agentHeight, agentClimb, _option.groundTolerance, -agentRadius * 0.2f,
                        cellSize + 2 * agentRadius + _option.climbDownDistance,
                        -_option.climbDownMaxHeight, -_option.climbDownMinHeight, 0);
                    _links.AddRange(_annotationBuilder.Build(config, JumpLinkType.EDGE_CLIMB_DOWN));
                }

                if ((_option.buildTypes & JumpLinkType.EDGE_JUMP.Bit) != 0)
                {
                    JumpLinkBuilderConfig config = new JumpLinkBuilderConfig(cellSize, cellHeight, agentRadius,
                        agentHeight, agentClimb, _option.groundTolerance, -agentRadius * 0.2f,
                        _option.edgeJumpEndDistance, -_option.edgeJumpDownMaxHeight,
                        _option.edgeJumpUpMaxHeight, _option.edgeJumpHeight);
                    _links.AddRange(_annotationBuilder.Build(config, JumpLinkType.EDGE_JUMP));
                }

                if (buildOffMeshConnections)
                {
                    DemoInputGeomProvider geom = _sample.GetInputGeom();
                    if (geom != null)
                    {
                        int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP_AUTO;
                        geom.RemoveOffMeshConnections(c => c.area == area);
                        _links.ForEach(l => AddOffMeshLink(l, geom, agentRadius));
                    }
                }
            }
        }

        private void AddOffMeshLink(JumpLink link, DemoInputGeomProvider geom, float agentRadius)
        {
            int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP_AUTO;
            int flags = SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP;
            RcVec3f prev = new RcVec3f();
            for (int i = 0; i < link.startSamples.Length; i++)
            {
                RcVec3f p = link.startSamples[i].p;
                RcVec3f q = link.endSamples[i].p;
                if (i == 0 || RcVec3f.Dist2D(prev, p) > agentRadius)
                {
                    geom.AddOffMeshConnection(p, q, agentRadius, false, area, flags);
                    prev = p;
                }
            }
        }
    }
}