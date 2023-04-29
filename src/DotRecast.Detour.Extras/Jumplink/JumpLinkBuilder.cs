using System;
using System.Collections.Generic;
using System.Linq;
using DotRecast.Core;
using DotRecast.Recast;
using static DotRecast.Core.RecastMath;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class JumpLinkBuilder
    {
        private readonly EdgeExtractor edgeExtractor = new EdgeExtractor();
        private readonly EdgeSamplerFactory edgeSamplerFactory = new EdgeSamplerFactory();
        private readonly GroundSampler groundSampler = new NavMeshGroundSampler();
        private readonly TrajectorySampler trajectorySampler = new TrajectorySampler();
        private readonly JumpSegmentBuilder jumpSegmentBuilder = new JumpSegmentBuilder();

        private readonly List<Edge[]> edges;
        private readonly IList<RecastBuilderResult> results;

        public JumpLinkBuilder(IList<RecastBuilderResult> results)
        {
            this.results = results;
            edges = results.Select(r => edgeExtractor.extractEdges(r.getMesh())).ToList();
        }

        public List<JumpLink> build(JumpLinkBuilderConfig acfg, JumpLinkType type)
        {
            List<JumpLink> links = new List<JumpLink>();
            for (int tile = 0; tile < results.Count; tile++)
            {
                Edge[] edges = this.edges[tile];
                foreach (Edge edge in edges)
                {
                    links.AddRange(processEdge(acfg, results[tile], type, edge));
                }
            }

            return links;
        }

        private List<JumpLink> processEdge(JumpLinkBuilderConfig acfg, RecastBuilderResult result, JumpLinkType type, Edge edge)
        {
            EdgeSampler es = edgeSamplerFactory.get(acfg, type, edge);
            groundSampler.sample(acfg, result, es);
            trajectorySampler.sample(acfg, result.getSolidHeightfield(), es);
            JumpSegment[] jumpSegments = jumpSegmentBuilder.build(acfg, es);
            return buildJumpLinks(acfg, es, jumpSegments);
        }


        private List<JumpLink> buildJumpLinks(JumpLinkBuilderConfig acfg, EdgeSampler es, JumpSegment[] jumpSegments)
        {
            List<JumpLink> links = new List<JumpLink>();
            foreach (JumpSegment js in jumpSegments)
            {
                Vector3f sp = es.start.gsamples[js.startSample].p;
                Vector3f sq = es.start.gsamples[js.startSample + js.samples - 1].p;
                GroundSegment end = es.end[js.groundSegment];
                Vector3f ep = end.gsamples[js.startSample].p;
                Vector3f eq = end.gsamples[js.startSample + js.samples - 1].p;
                float d = Math.Min(vDist2DSqr(sp, sq), vDist2DSqr(ep, eq));
                if (d >= 4 * acfg.agentRadius * acfg.agentRadius)
                {
                    JumpLink link = new JumpLink();
                    links.Add(link);
                    link.startSamples = ArrayUtils.CopyOf(es.start.gsamples, js.startSample, js.samples);
                    link.endSamples = ArrayUtils.CopyOf(end.gsamples, js.startSample, js.samples);
                    link.start = es.start;
                    link.end = end;
                    link.trajectory = es.trajectory;
                    for (int j = 0; j < link.nspine; ++j)
                    {
                        float u = ((float)j) / (link.nspine - 1);
                        Vector3f p = es.trajectory.apply(sp, ep, u);
                        link.spine0[j * 3] = p.x;
                        link.spine0[j * 3 + 1] = p.y;
                        link.spine0[j * 3 + 2] = p.z;

                        p = es.trajectory.apply(sq, eq, u);
                        link.spine1[j * 3] = p.x;
                        link.spine1[j * 3 + 1] = p.y;
                        link.spine1[j * 3 + 2] = p.z;
                    }
                }
            }

            return links;
        }

        public List<Edge[]> getEdges()
        {
            return edges;
        }
    }
}