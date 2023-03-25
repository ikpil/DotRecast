using System;
using DotRecast.Recast;
using static DotRecast.Core.RecastMath;

namespace DotRecast.Detour.Extras.Jumplink
{
    class TrajectorySampler
    {
        public void sample(JumpLinkBuilderConfig acfg, Heightfield heightfield, EdgeSampler es)
        {
            int nsamples = es.start.gsamples.Length;
            for (int i = 0; i < nsamples; ++i)
            {
                GroundSample ssmp = es.start.gsamples[i];
                foreach (GroundSegment end in es.end)
                {
                    GroundSample esmp = end.gsamples[i];
                    if (!ssmp.validHeight || !esmp.validHeight)
                    {
                        continue;
                    }

                    if (!sampleTrajectory(acfg, heightfield, ssmp.p, esmp.p, es.trajectory))
                    {
                        continue;
                    }

                    ssmp.validTrajectory = true;
                    esmp.validTrajectory = true;
                }
            }
        }

        private bool sampleTrajectory(JumpLinkBuilderConfig acfg, Heightfield solid, float[] pa, float[] pb, Trajectory tra)
        {
            float cs = Math.Min(acfg.cellSize, acfg.cellHeight);
            float d = vDist2D(pa, pb) + Math.Abs(pa[1] - pb[1]);
            int nsamples = Math.Max(2, (int)Math.Ceiling(d / cs));
            for (int i = 0; i < nsamples; ++i)
            {
                float u = (float)i / (float)(nsamples - 1);
                float[] p = tra.apply(pa, pb, u);
                if (checkHeightfieldCollision(solid, p[0], p[1] + acfg.groundTolerance, p[1] + acfg.agentHeight, p[2]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool checkHeightfieldCollision(Heightfield solid, float x, float ymin, float ymax, float z)
        {
            int w = solid.width;
            int h = solid.height;
            float cs = solid.cs;
            float ch = solid.ch;
            float[] orig = solid.bmin;
            int ix = (int)Math.Floor((x - orig[0]) / cs);
            int iz = (int)Math.Floor((z - orig[2]) / cs);

            if (ix < 0 || iz < 0 || ix > w || iz > h)
            {
                return false;
            }

            Span s = solid.spans[ix + iz * w];
            if (s == null)
            {
                return false;
            }

            while (s != null)
            {
                float symin = orig[1] + s.smin * ch;
                float symax = orig[1] + s.smax * ch;
                if (overlapRange(ymin, ymax, symin, symax))
                {
                    return true;
                }

                s = s.next;
            }

            return false;
        }

        private bool overlapRange(float amin, float amax, float bmin, float bmax)
        {
            return (amin > bmax || amax < bmin) ? false : true;
        }
    }
}