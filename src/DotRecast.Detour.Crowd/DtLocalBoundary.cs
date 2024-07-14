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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DotRecast.Core;
using System.Numerics;
using System;


namespace DotRecast.Detour.Crowd
{
    public sealed class DtLocalBoundary
    {
        public const int MAX_LOCAL_SEGS = 8;
        public const int MAX_LOCAL_POLYS = 16;

        private Vector3 m_center;
        private DtSegment[] m_segs = new DtSegment[MAX_LOCAL_SEGS];
        private int m_nsegs;
        private long[] m_polys = new long[MAX_LOCAL_POLYS];
        private int m_npolys;

        public DtLocalBoundary()
        {
            m_center.X = m_center.Y = m_center.Z = float.MaxValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            m_center.X = m_center.Y = m_center.Z = float.MaxValue;
            m_npolys = 0;
            m_nsegs = 0;
        }

        protected unsafe void AddSegment(float dist, RcSegmentVert s)
        {
            // Insert neighbour based on the distance.
            var p_segs = (DtSegment*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(m_segs));
            DtSegment* seg = null;
            if (0 == m_nsegs)
            {
                seg = &p_segs[0];
            }
            else if (dist >= m_segs[m_nsegs - 1].d)
            {
                if (m_nsegs >= MAX_LOCAL_SEGS)
                    return;

                seg = &p_segs[m_nsegs];
            }
            else
            {
                // Insert inbetween.
                int i;
                for (i = 0; i < m_nsegs; ++i)
                    if (dist <= m_segs[i].d)
                        break;

                int tgt = i + 1;
                int n = Math.Min(m_nsegs - i, MAX_LOCAL_SEGS - tgt);
                System.Diagnostics.Debug.Assert(tgt + n <= MAX_LOCAL_SEGS);
                if (n > 0)
                    m_segs.AsSpan(i, n).CopyTo(m_segs.AsSpan(tgt));
                seg = &p_segs[i];
            }

            seg->d = dist;
            Unsafe.CopyBlockUnaligned((float*)&s, seg->s, (uint)sizeof(RcSegmentVert));

            if (m_nsegs < MAX_LOCAL_SEGS)
                m_nsegs++;
        }

        public void Update(long startRef, Vector3 pos, float collisionQueryRange, DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            const int MAX_SEGS_PER_POLY = DtDetour.DT_VERTS_PER_POLYGON * 3;

            if (startRef == 0)
            {
                Reset();
                return;
            }

            m_center = pos;

            // First query non-overlapping polygons.
            var status = navquery.FindLocalNeighbourhood(startRef, pos, collisionQueryRange, filter, m_polys, null, out m_npolys, MAX_LOCAL_POLYS);
            if (status.Succeeded())
            {
                // Secondly, store all polygon edges.
                m_nsegs = 0;
                Span<RcSegmentVert> segs = stackalloc RcSegmentVert[MAX_SEGS_PER_POLY];
                int nsegs = 0;

                for (int j = 0; j < m_npolys; ++j)
                {
                    var result = navquery.GetPolyWallSegments(m_polys[j], filter, segs, null, out nsegs, MAX_SEGS_PER_POLY);
                    if (result.Succeeded())
                    {
                        for (int k = 0; k < nsegs; ++k)
                        {
                            ref readonly RcSegmentVert s = ref segs[k];
                            var s0 = s.vmin;
                            var s3 = s.vmax;

                            // Skip too distant segments.
                            var distSqr = DtUtils.DistancePtSegSqr2D(pos, s0, s3, out var tseg);
                            if (distSqr > RcMath.Sqr(collisionQueryRange))
                            {
                                continue;
                            }

                            AddSegment(distSqr, s);
                        }
                    }
                }
            }
        }

        public bool IsValid(DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            if (m_npolys == 0)
            {
                return false;
            }

            // Check that all polygons still pass query filter.
            foreach (long refs in m_polys)
            {
                if (!navquery.IsValidPolyRef(refs, filter))
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetCenter() => m_center;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref DtSegment GetSegment(int j) => ref m_segs[j];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSegmentCount() => m_nsegs;
    }
}