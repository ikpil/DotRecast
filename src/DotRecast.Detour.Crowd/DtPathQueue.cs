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
using DotRecast.Core.Numerics;


namespace DotRecast.Detour.Crowd
{
    public class DtPathQueue
    {
        private readonly DtCrowdConfig m_config;
        private LinkedList<DtPathQuery> m_queue;
        private int m_maxPathSize;

        public DtPathQueue(DtCrowdConfig config)
        {
            m_config = config;
        }

        public bool Init(int maxPathSize)
        {
            m_queue = new LinkedList<DtPathQuery>();
            m_maxPathSize = maxPathSize;
            return true;
        }


        public void Update(DtNavMesh navMesh)
        {
            // Update path request until there is nothing to update
            // or upto maxIters pathfinder iterations has been consumed.
            int iterCount = m_config.maxFindPathIterations;
            while (iterCount > 0)
            {
                DtPathQuery q = m_queue.First?.Value;
                if (q == null)
                {
                    break;
                }

                m_queue.RemoveFirst();

                // Handle query start.
                if (q.result.status.IsEmpty())
                {
                    q.navQuery = new DtNavMeshQuery(navMesh);
                    q.result.status = q.navQuery.InitSlicedFindPath(q.startRef, q.endRef, q.startPos, q.endPos, q.filter, 0);
                }

                // Handle query in progress.
                if (q.result.status.InProgress())
                {
                    q.result.status = q.navQuery.UpdateSlicedFindPath(iterCount, out var iters);
                    iterCount -= iters;
                }

                if (q.result.status.Succeeded())
                {
                    q.result.status = q.navQuery.FinalizeSlicedFindPath(q.result.path, out q.result.npath, m_maxPathSize);
                }

                if (!(q.result.status.Failed() || q.result.status.Succeeded()))
                {
                    m_queue.AddFirst(q);
                }
            }
        }

        public DtPathQueryResult Request(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter)
        {
            if (m_queue.Count >= m_config.pathQueueSize)
            {
                return null;
            }

            DtPathQuery q = new DtPathQuery();
            q.startPos = startPos;
            q.startRef = startRef;
            q.endPos = endPos;
            q.endRef = endRef;
            q.filter = filter;
            q.result.path = new long[m_maxPathSize];
            m_queue.AddLast(q);
            return q.result;
        }
    }
}