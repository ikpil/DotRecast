using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour.Extras.Jumplink;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Toolset.Geom;

namespace DotRecast.Recast.Toolset.Tools
{
    public class JumpLinkBuilderToolImpl : ISampleTool
    {
        private Sample _sample;

        private readonly List<JumpLink> _links;
        private JumpLinkBuilder _annotationBuilder;
        private readonly int _selEdge = -1;

        public JumpLinkBuilderToolImpl()
        {
            _links = new List<JumpLink>();
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

        public void Build(bool buildOffMeshConnections, int buildTypes,
            float groundTolerance, float climbDownDistance, float climbDownMaxHeight, float climbDownMinHeight,
            float edgeJumpEndDistance, float edgeJumpHeight, float edgeJumpDownMaxHeight, float edgeJumpUpMaxHeight)
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

                if ((buildTypes & JumpLinkType.EDGE_CLIMB_DOWN.Bit) != 0)
                {
                    JumpLinkBuilderConfig config = new JumpLinkBuilderConfig(
                        cellSize,
                        cellHeight,
                        agentRadius,
                        agentHeight,
                        agentClimb,
                        groundTolerance,
                        -agentRadius * 0.2f,
                        cellSize + 2 * agentRadius + climbDownDistance,
                        -climbDownMaxHeight,
                        -climbDownMinHeight,
                        0
                    );
                    _links.AddRange(_annotationBuilder.Build(config, JumpLinkType.EDGE_CLIMB_DOWN));
                }

                if ((buildTypes & JumpLinkType.EDGE_JUMP.Bit) != 0)
                {
                    JumpLinkBuilderConfig config = new JumpLinkBuilderConfig(
                        cellSize,
                        cellHeight,
                        agentRadius,
                        agentHeight,
                        agentClimb,
                        groundTolerance,
                        -agentRadius * 0.2f,
                        edgeJumpEndDistance,
                        -edgeJumpDownMaxHeight,
                        edgeJumpUpMaxHeight,
                        edgeJumpHeight
                    );
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