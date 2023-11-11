using System;
using System.Collections.Generic;
using System.Linq;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Recast;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class JumpLinkBuilder
    {
        private readonly EdgeExtractor edgeExtractor = new EdgeExtractor();
        private readonly EdgeSamplerFactory edgeSamplerFactory = new EdgeSamplerFactory();
        private readonly IGroundSampler groundSampler = new NavMeshGroundSampler();
        private readonly TrajectorySampler trajectorySampler = new TrajectorySampler();
        private readonly JumpSegmentBuilder jumpSegmentBuilder = new JumpSegmentBuilder();

        private readonly List<JumpEdge[]> edges;
        private readonly IList<RcBuilderResult> results;

        public JumpLinkBuilder(IList<RcBuilderResult> results)
        {
            this.results = results;
            edges = results.Select(r => edgeExtractor.ExtractEdges(r.GetMesh())).ToList();
        }

        public List<JumpLink> Build(JumpLinkBuilderConfig acfg, JumpLinkType type)
        {
            List<JumpLink> links = new List<JumpLink>();
            for (int tile = 0; tile < results.Count; tile++)
            {
                JumpEdge[] edges = this.edges[tile];
                foreach (JumpEdge edge in edges)
                {
                    links.AddRange(ProcessEdge(acfg, results[tile], type, edge));
                }
            }

            return links;
        }

        private List<JumpLink> ProcessEdge(JumpLinkBuilderConfig acfg, RcBuilderResult result, JumpLinkType type, JumpEdge edge)
        {
            EdgeSampler es = edgeSamplerFactory.Get(acfg, type, edge);
            groundSampler.Sample(acfg, result, es);
            trajectorySampler.Sample(acfg, result.GetSolidHeightfield(), es);
            JumpSegment[] jumpSegments = jumpSegmentBuilder.Build(acfg, es);
            return BuildJumpLinks(acfg, es, jumpSegments);
        }


        private List<JumpLink> BuildJumpLinks(JumpLinkBuilderConfig acfg, EdgeSampler es, JumpSegment[] jumpSegments)
        {
            List<JumpLink> links = new List<JumpLink>();
            foreach (JumpSegment js in jumpSegments)
            {
                RcVec3f sp = es.start.gsamples[js.startSample].p;
                RcVec3f sq = es.start.gsamples[js.startSample + js.samples - 1].p;
                GroundSegment end = es.end[js.groundSegment];
                RcVec3f ep = end.gsamples[js.startSample].p;
                RcVec3f eq = end.gsamples[js.startSample + js.samples - 1].p;
                float d = Math.Min(RcVecUtils.Dist2DSqr(sp, sq), RcVecUtils.Dist2DSqr(ep, eq));
                if (d >= 4 * acfg.agentRadius * acfg.agentRadius)
                {
                    JumpLink link = new JumpLink();
                    links.Add(link);
                    link.startSamples = RcArrays.CopyOf(es.start.gsamples, js.startSample, js.samples);
                    link.endSamples = RcArrays.CopyOf(end.gsamples, js.startSample, js.samples);
                    link.start = es.start;
                    link.end = end;
                    link.trajectory = es.trajectory;
                    for (int j = 0; j < link.nspine; ++j)
                    {
                        float u = ((float)j) / (link.nspine - 1);
                        RcVec3f p = es.trajectory.Apply(sp, ep, u);
                        link.spine0[j * 3] = p.X;
                        link.spine0[j * 3 + 1] = p.Y;
                        link.spine0[j * 3 + 2] = p.Z;

                        p = es.trajectory.Apply(sq, eq, u);
                        link.spine1[j * 3] = p.X;
                        link.spine1[j * 3 + 1] = p.Y;
                        link.spine1[j * 3 + 2] = p.Z;
                    }
                }
            }

            return links;
        }

        public List<JumpEdge[]> GetEdges()
        {
            return edges;
        }
    }
}