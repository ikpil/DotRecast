/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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
using System.Collections.Generic;
using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    using static DotRecast.Core.RecastMath;

    public class LocalBoundary
    {
        public const int MAX_LOCAL_SEGS = 8;

        private class Segment
        {
            /** Segment start/end */
            public float[] s = new float[6];

            /** Distance for pruning. */
            public float d;
        }

        Vector3f m_center = new Vector3f();
        List<Segment> m_segs = new List<Segment>();
        List<long> m_polys = new List<long>();

        public LocalBoundary()
        {
            m_center[0] = m_center[1] = m_center[2] = float.MaxValue;
        }

        public void reset()
        {
            m_center[0] = m_center[1] = m_center[2] = float.MaxValue;
            m_polys.Clear();
            m_segs.Clear();
        }

        protected void addSegment(float dist, float[] s)
        {
            // Insert neighbour based on the distance.
            Segment seg = new Segment();
            Array.Copy(s, seg.s, 6);
            seg.d = dist;
            if (0 == m_segs.Count)
            {
                m_segs.Add(seg);
            }
            else if (dist >= m_segs[m_segs.Count - 1].d)
            {
                if (m_segs.Count >= MAX_LOCAL_SEGS)
                {
                    return;
                }

                m_segs.Add(seg);
            }
            else
            {
                // Insert inbetween.
                int i;
                for (i = 0; i < m_segs.Count; ++i)
                {
                    if (dist <= m_segs[i].d)
                    {
                        break;
                    }
                }

                m_segs.Insert(i, seg);
            }

            while (m_segs.Count > MAX_LOCAL_SEGS)
            {
                m_segs.RemoveAt(m_segs.Count - 1);
            }
        }

        public void update(long refs, Vector3f pos, float collisionQueryRange, NavMeshQuery navquery, QueryFilter filter)
        {
            if (refs == 0)
            {
                reset();
                return;
            }

            vCopy(ref m_center, pos);
            // First query non-overlapping polygons.
            Result<FindLocalNeighbourhoodResult> res = navquery.findLocalNeighbourhood(refs, pos, collisionQueryRange,
                filter);
            if (res.succeeded())
            {
                m_polys = res.result.getRefs();
                m_segs.Clear();
                // Secondly, store all polygon edges.
                for (int j = 0; j < m_polys.Count; ++j)
                {
                    Result<GetPolyWallSegmentsResult> result = navquery.getPolyWallSegments(m_polys[j], false, filter);
                    if (result.succeeded())
                    {
                        GetPolyWallSegmentsResult gpws = result.result;
                        for (int k = 0; k < gpws.getSegmentRefs().Count; ++k)
                        {
                            float[] s = gpws.getSegmentVerts()[k];
                            // Skip too distant segments.
                            Tuple<float, float> distseg = distancePtSegSqr2D(pos, s, 0, 3);
                            if (distseg.Item1 > sqr(collisionQueryRange))
                            {
                                continue;
                            }

                            addSegment(distseg.Item1, s);
                        }
                    }
                }
            }
        }

        public bool isValid(NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_polys.Count == 0)
            {
                return false;
            }

            // Check that all polygons still pass query filter.
            foreach (long refs in m_polys)
            {
                if (!navquery.isValidPolyRef(refs, filter))
                {
                    return false;
                }
            }

            return true;
        }

        public Vector3f getCenter()
        {
            return m_center;
        }

        public float[] getSegment(int j)
        {
            return m_segs[j].s;
        }

        public int getSegmentCount()
        {
            return m_segs.Count;
        }
    }
}