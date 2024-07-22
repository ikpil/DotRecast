/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Runtime.CompilerServices;
using DotRecast.Core;
using System.Numerics;


namespace DotRecast.Detour.Crowd
{
    public class DtObstacleAvoidanceQuery
    {
        public const int DT_MAX_PATTERN_DIVS = 32; // < Max numver of adaptive divs.
        public const int DT_MAX_PATTERN_RINGS = 4;
        //public const float DT_PI = 3.14159265f; // 3.14159274

        private DtObstacleAvoidanceParams m_params;
        private float m_invHorizTime;
        private float m_vmax;
        private float m_invVmax;

        private readonly int m_maxCircles;
        private readonly DtObstacleCircle[] m_circles;
        private int m_ncircles;

        private readonly int m_maxSegments;
        private readonly DtObstacleSegment[] m_segments;
        private int m_nsegments;

        public DtObstacleAvoidanceQuery(int maxCircles, int maxSegments)
        {
            m_maxCircles = maxCircles;
            m_ncircles = 0;
            m_circles = new DtObstacleCircle[m_maxCircles];

            m_maxSegments = maxSegments;
            m_nsegments = 0;
            m_segments = new DtObstacleSegment[m_maxSegments];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            m_ncircles = 0;
            m_nsegments = 0;
        }

        public void AddCircle(Vector3 pos, float rad, Vector3 vel, Vector3 dvel)
        {
            if (m_ncircles >= m_maxCircles)
                return;

            ref DtObstacleCircle cir = ref m_circles[m_ncircles++];
            cir.p = pos;
            cir.rad = rad;
            cir.vel = vel;
            cir.dvel = dvel;
        }

        public void AddSegment(Vector3 p, Vector3 q)
        {
            if (m_nsegments >= m_maxSegments)
                return;

            ref DtObstacleSegment seg = ref m_segments[m_nsegments++];
            seg.p = p;
            seg.q = q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetObstacleCircleCount() => m_ncircles;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DtObstacleCircle GetObstacleCircle(int i) => m_circles[i];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetObstacleSegmentCount() => m_nsegments;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DtObstacleSegment GetObstacleSegment(int i) => m_segments[i];

        private void Prepare(Vector3 pos, Vector3 dvel)
        {
            // Prepare obstacles
            for (int i = 0; i < m_ncircles; ++i)
            {
                ref DtObstacleCircle cir = ref m_circles[i];

                // Side
                Vector3 pa = pos;
                Vector3 pb = cir.p;

                Vector3 orig = new Vector3();
                Vector3 dv = new Vector3();
                cir.dp = Vector3.Subtract(pb, pa);
                cir.dp = Vector3.Normalize(cir.dp);
                dv = Vector3.Subtract(cir.dvel, dvel);

                float a = DtUtils.TriArea2D(orig, cir.dp, dv);
                if (a < 0.01f)
                {
                    cir.np.X = -cir.dp.Z;
                    cir.np.Z = cir.dp.X;
                }
                else
                {
                    cir.np.X = cir.dp.Z;
                    cir.np.Z = -cir.dp.X;
                }
            }

            for (int i = 0; i < m_nsegments; ++i)
            {
                ref DtObstacleSegment seg = ref m_segments[i];

                // Precalc if the agent is really close to the segment.
                float r = 0.01f;
                var distSqr = DtUtils.DistancePtSegSqr2D(pos, seg.p, seg.q, out var t);
                seg.touch = distSqr < RcMath.Sqr(r);
            }
        }

        private bool SweepCircleCircle(Vector3 c0, float r0, Vector3 v, Vector3 c1, float r1, out float tmin, out float tmax)
        {
            const float EPS = 0.0001f;

            tmin = 0;
            tmax = 0;

            Vector3 s = Vector3.Subtract(c1, c0);
            float r = r0 + r1;
            float c = s.Dot2D(s) - r * r;
            float a = v.Dot2D(v);
            if (a < EPS)
                return false; // not moving

            // Overlap, calc time to exit.
            float b = v.Dot2D(s);
            float d = b * b - a * c;
            if (d < 0.0f)
                return false; // no intersection.

            a = 1.0f / a;
            float rd = MathF.Sqrt(d);

            tmin = (b - rd) * a;
            tmax = (b + rd) * a;

            return true;
        }

        private bool IsectRaySeg(Vector3 ap, Vector3 u, Vector3 bp, Vector3 bq, ref float t)
        {
            Vector3 v = Vector3.Subtract(bq, bp);
            Vector3 w = Vector3.Subtract(ap, bp);
            float d = RcVec.Perp2D(u, v);
            if (MathF.Abs(d) < 1e-6f)
                return false;

            d = 1.0f / d;
            t = RcVec.Perp2D(v, w) * d;
            if (t < 0 || t > 1)
                return false;

            float s = RcVec.Perp2D(u, w) * d;
            if (s < 0 || s > 1)
                return false;

            return true;
        }

        /**
     * Calculate the collision penalty for a given velocity vector
     *
     * @param vcand
     *            sampled velocity
     * @param dvel
     *            desired velocity
     * @param minPenalty
     *            threshold penalty for early out
     */
        private float ProcessSample(Vector3 vcand, float cs, Vector3 pos, float rad, Vector3 vel, Vector3 dvel,
            float minPenalty, DtObstacleAvoidanceDebugData debug)
        {
            // penalty for straying away from the desired and current velocities
            float vpen = m_params.weightDesVel * (RcVec.Dist2D(vcand, dvel) * m_invVmax);
            float vcpen = m_params.weightCurVel * (RcVec.Dist2D(vcand, vel) * m_invVmax);

            // find the threshold hit time to bail out based on the early out penalty
            // (see how the penalty is calculated below to understand)
            float minPen = minPenalty - vpen - vcpen;
            float tThresold = (m_params.weightToi / minPen - 0.1f) * m_params.horizTime;
            if (tThresold - m_params.horizTime > -float.MinValue)
                return minPenalty; // already too much

            // Find min time of impact and exit amongst all obstacles.
            float tmin = m_params.horizTime;
            float side = 0;
            int nside = 0;

            for (int i = 0; i < m_ncircles; ++i)
            {
                ref readonly DtObstacleCircle cir = ref m_circles[i];

                // RVO
                Vector3 vab = vcand * 2;
                vab = Vector3.Subtract(vab, vel);
                vab = Vector3.Subtract(vab, cir.vel);

                // Side
                side += Math.Clamp(Math.Min(cir.dp.Dot2D(vab) * 0.5f + 0.5f, cir.np.Dot2D(vab) * 2), 0.0f, 1.0f);
                nside++;

                if (!SweepCircleCircle(pos, rad, vab, cir.p, cir.rad, out var htmin, out var htmax))
                    continue;

                // Handle overlapping obstacles.
                if (htmin < 0.0f && htmax > 0.0f)
                {
                    // Avoid more when overlapped.
                    htmin = -htmin * 0.5f;
                }

                if (htmin >= 0.0f)
                {
                    // The closest obstacle is somewhere ahead of us, keep track of nearest obstacle.
                    if (htmin < tmin)
                    {
                        tmin = htmin;
                        if (tmin < tThresold)
                            return minPenalty;
                    }
                }
            }

            for (int i = 0; i < m_nsegments; ++i)
            {
                ref readonly DtObstacleSegment seg = ref m_segments[i];
                float htmin = 0;

                if (seg.touch)
                {
                    // Special case when the agent is very close to the segment.
                    Vector3 sdir = Vector3.Subtract(seg.q, seg.p);
                    Vector3 snorm = new Vector3();
                    snorm.X = -sdir.Z;
                    snorm.Z = sdir.X;
                    // If the velocity is pointing towards the segment, no collision.
                    if (snorm.Dot2D(vcand) < 0.0f)
                        continue;
                    // Else immediate collision.
                    htmin = 0.0f;
                }
                else
                {
                    if (!IsectRaySeg(pos, vcand, seg.p, seg.q, ref htmin))
                        continue;
                }

                // Avoid less when facing walls.
                htmin *= 2.0f;

                // The closest obstacle is somewhere ahead of us, keep track of nearest obstacle.
                if (htmin < tmin)
                {
                    tmin = htmin;
                    if (tmin < tThresold)
                        return minPenalty;
                }
            }

            // Normalize side bias, to prevent it dominating too much.
            if (nside != 0)
                side /= nside;

            float spen = m_params.weightSide * side;
            float tpen = m_params.weightToi * (1.0f / (0.1f + tmin * m_invHorizTime));

            float penalty = vpen + vcpen + spen + tpen;
            // Store different penalties for debug viewing
            if (debug != null)
                debug.AddSample(vcand, cs, penalty, vpen, vcpen, spen, tpen);

            return penalty;
        }

        public int SampleVelocityGrid(Vector3 pos, float rad, float vmax, Vector3 vel, Vector3 dvel, out Vector3 nvel,
            DtObstacleAvoidanceParams option, DtObstacleAvoidanceDebugData debug)
        {
            Prepare(pos, dvel);
            m_params = option;
            m_invHorizTime = 1.0f / m_params.horizTime;
            m_vmax = vmax;
            m_invVmax = vmax > 0 ? 1.0f / vmax : float.MaxValue;

            nvel = Vector3.Zero;

            if (debug != null)
                debug.Reset();

            float cvx = dvel.X * m_params.velBias;
            float cvz = dvel.Z * m_params.velBias;
            float cs = vmax * 2 * (1 - m_params.velBias) / (m_params.gridSize - 1);
            float half = (m_params.gridSize - 1) * cs * 0.5f;

            float minPenalty = float.MaxValue;
            int ns = 0;

            for (int y = 0; y < m_params.gridSize; ++y)
            {
                for (int x = 0; x < m_params.gridSize; ++x)
                {
                    Vector3 vcand = new Vector3(cvx + x * cs - half, 0f, cvz + y * cs - half);
                    if (RcMath.Sqr(vcand.X) + RcMath.Sqr(vcand.Z) > RcMath.Sqr(vmax + cs / 2))
                        continue;

                    float penalty = ProcessSample(vcand, cs, pos, rad, vel, dvel, minPenalty, debug);
                    ns++;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        nvel = vcand;
                    }
                }
            }

            return ns;
        }

        // vector normalization that ignores the y-component.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DtNormalize2D(Span<float> v)
        {
            float d = MathF.Sqrt(v[0] * v[0] + v[2] * v[2]);
            if (d == 0)
                return;
            d = 1.0f / d;
            v[0] *= d;
            v[2] *= d;
        }

        // vector normalization that ignores the y-component.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Vector3 DtRotate2D(Span<float> v, float ang)
        {
            Vector3 dest = new Vector3();
            float c = MathF.Cos(ang);
            float s = MathF.Sin(ang);
            dest.X = v[0] * c - v[2] * s;
            dest.Z = v[0] * s + v[2] * c;
            dest.Y = v[1];
            return dest;
        }


        public int SampleVelocityAdaptive(Vector3 pos, float rad, float vmax, Vector3 vel, Vector3 dvel, out Vector3 nvel,
            DtObstacleAvoidanceParams option,
            DtObstacleAvoidanceDebugData debug)
        {
            Prepare(pos, dvel);
            m_params = option;
            m_invHorizTime = 1.0f / m_params.horizTime;
            m_vmax = vmax;
            m_invVmax = vmax > 0 ? 1.0f / vmax : float.MaxValue;

            nvel = Vector3.Zero;

            if (debug != null)
                debug.Reset();

            // Build sampling pattern aligned to desired velocity.
            Span<float> pat = stackalloc float[(DT_MAX_PATTERN_DIVS * DT_MAX_PATTERN_RINGS + 1) * 2];
            int npat = 0;

            int ndivs = m_params.adaptiveDivs;
            int nrings = m_params.adaptiveRings;
            int depth = m_params.adaptiveDepth;

            int nd = Math.Clamp(ndivs, 1, DT_MAX_PATTERN_DIVS);
            int nr = Math.Clamp(nrings, 1, DT_MAX_PATTERN_RINGS);
            float da = (1.0f / nd) * MathF.PI * 2;
            float ca = MathF.Cos(da);
            float sa = MathF.Sin(da);

            // desired direction
            Span<float> ddir = stackalloc float[6];
            ddir[0] = dvel.X;
            ddir[1] = dvel.Y;
            ddir[2] = dvel.Z;
            DtNormalize2D(ddir);
            Vector3 rotated = DtRotate2D(ddir, da * 0.5f); // rotated by da/2
            ddir[3] = rotated.X;
            ddir[4] = rotated.Y;
            ddir[5] = rotated.Z;

            // Always add sample at zero
            pat[npat * 2 + 0] = 0;
            pat[npat * 2 + 1] = 0;
            npat++;

            for (int j = 0; j < nr; ++j)
            {
                float r = (nr - j) / (float)nr;
                pat[npat * 2 + 0] = ddir[(j % 2) * 3] * r;
                pat[npat * 2 + 1] = ddir[(j % 2) * 3 + 2] * r;
                int last1 = npat * 2;
                int last2 = last1;
                npat++;

                for (int i = 1; i < nd - 1; i += 2)
                {
                    // get next point on the "right" (rotate CW)
                    pat[npat * 2 + 0] = pat[last1] * ca + pat[last1 + 1] * sa;
                    pat[npat * 2 + 1] = -pat[last1] * sa + pat[last1 + 1] * ca;
                    // get next point on the "left" (rotate CCW)
                    pat[npat * 2 + 2] = pat[last2] * ca - pat[last2 + 1] * sa;
                    pat[npat * 2 + 3] = pat[last2] * sa + pat[last2 + 1] * ca;

                    last1 = npat * 2;
                    last2 = last1 + 2;
                    npat += 2;
                }

                if ((nd & 1) == 0)
                {
                    pat[npat * 2 + 2] = pat[last2] * ca - pat[last2 + 1] * sa;
                    pat[npat * 2 + 3] = pat[last2] * sa + pat[last2 + 1] * ca;
                    npat++;
                }
            }

            // Start sampling.
            float cr = vmax * (1.0f - m_params.velBias);
            Vector3 res = new Vector3(dvel.X * m_params.velBias, 0, dvel.Z * m_params.velBias);
            int ns = 0;
            for (int k = 0; k < depth; ++k)
            {
                float minPenalty = float.MaxValue;
                Vector3 bvel = new Vector3();
                bvel = Vector3.Zero;

                for (int i = 0; i < npat; ++i)
                {
                    Vector3 vcand = new Vector3(res.X + pat[i * 2 + 0] * cr, 0f, res.Z + pat[i * 2 + 1] * cr);
                    if (RcMath.Sqr(vcand.X) + RcMath.Sqr(vcand.Z) > RcMath.Sqr(vmax + 0.001f))
                        continue;

                    float penalty = ProcessSample(vcand, cr / 10, pos, rad, vel, dvel, minPenalty, debug);
                    ns++;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        bvel = vcand;
                    }
                }

                res = bvel;

                cr *= 0.5f;
            }

            nvel = res;

            return ns;
        }
    }
}