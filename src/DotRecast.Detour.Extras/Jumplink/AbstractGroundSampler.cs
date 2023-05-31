using System;
using DotRecast.Core;
using DotRecast.Recast;
using static DotRecast.Core.RcMath;

namespace DotRecast.Detour.Extras.Jumplink
{
    public abstract class AbstractGroundSampler : IGroundSampler
    {
        protected void SampleGround(JumpLinkBuilderConfig acfg, EdgeSampler es, Func<Vector3f, float, Tuple<bool, float>> heightFunc)
        {
            float cs = acfg.cellSize;
            float dist = (float)Math.Sqrt(Vector3f.Dist2DSqr(es.start.p, es.start.q));
            int ngsamples = Math.Max(2, (int)Math.Ceiling(dist / cs));
            SampleGroundSegment(heightFunc, es.start, ngsamples);
            foreach (GroundSegment end in es.end)
            {
                SampleGroundSegment(heightFunc, end, ngsamples);
            }
        }

        public abstract void Sample(JumpLinkBuilderConfig acfg, RecastBuilderResult result, EdgeSampler es);

        protected void SampleGroundSegment(Func<Vector3f, float, Tuple<bool, float>> heightFunc, GroundSegment seg, int nsamples)
        {
            seg.gsamples = new GroundSample[nsamples];

            for (int i = 0; i < nsamples; ++i)
            {
                float u = i / (float)(nsamples - 1);

                GroundSample s = new GroundSample();
                seg.gsamples[i] = s;
                Vector3f pt = Vector3f.Lerp(seg.p, seg.q, u);
                Tuple<bool, float> height = heightFunc.Invoke(pt, seg.height);
                s.p.x = pt.x;
                s.p.y = height.Item2;
                s.p.z = pt.z;

                if (!height.Item1)
                {
                    continue;
                }

                s.validHeight = true;
            }
        }
    }
}