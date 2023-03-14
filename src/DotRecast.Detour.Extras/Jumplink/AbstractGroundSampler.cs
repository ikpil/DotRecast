
using System;
using DotRecast.Recast;
using static DotRecast.Detour.DetourCommon;

namespace DotRecast.Detour.Extras.Jumplink;

public abstract class AbstractGroundSampler : GroundSampler {

    protected void sampleGround(JumpLinkBuilderConfig acfg, EdgeSampler es,
            Func<float[], float, Tuple<bool, float>> heightFunc) {
        float cs = acfg.cellSize;
        float dist = (float) Math.Sqrt(vDist2DSqr(es.start.p, es.start.q));
        int ngsamples = Math.Max(2, (int) Math.Ceiling(dist / cs));
        sampleGroundSegment(heightFunc, es.start, ngsamples);
        foreach (GroundSegment end in es.end) {
            sampleGroundSegment(heightFunc, end, ngsamples);
        }
    }

    public void sample(JumpLinkBuilderConfig acfg, RecastBuilderResult result, EdgeSampler es)
    {
        throw new NotImplementedException();
    }

    protected void sampleGroundSegment(Func<float[], float, Tuple<bool, float>> heightFunc, GroundSegment seg,
            int nsamples) {
        seg.gsamples = new GroundSample[nsamples];

        for (int i = 0; i < nsamples; ++i) {
            float u = i / (float) (nsamples - 1);

            GroundSample s = new GroundSample();
            seg.gsamples[i] = s;
            float[] pt = vLerp(seg.p, seg.q, u);
            Tuple<bool, float> height = heightFunc.Invoke(pt, seg.height);
            s.p[0] = pt[0];
            s.p[1] = height.Item2;
            s.p[2] = pt[2];

            if (!height.Item1) {
                continue;
            }
            s.validHeight = true;
        }
    }

}
