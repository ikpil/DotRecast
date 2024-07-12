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
using DotRecast.Core;
using System.Numerics;
using DotRecast.Detour;
using System.IO;
using System.Net.NetworkInformation;
using System;


namespace DotRecast.Detour.Crowd
{
    public class DtPathQueue
    {
        class PathQuery
        {
            public uint refs;
            /// Path find start and end location.
            public Vector3 startPos;
            public Vector3 endPos;
            public long startRef;
            public long endRef;

            public DtStatus status;
            public long[] path;
            public int npath;
            public int keepAlive;
            public IDtQueryFilter filter; // < TODO: This is potentially dangerous!
        }

        private readonly DtCrowdConfig m_config;
        private readonly DtNavMeshQuery m_navquery;

        const int MAX_QUEUE = 8;
        readonly PathQuery[] m_queue;
        uint m_nextHandle;
        int m_maxPathSize;
        uint m_queueHead;

        public const uint DT_PATHQ_INVALID = 0;

        public DtPathQueue(int maxPathSize, DtNavMesh navMesh, DtCrowdConfig config)
        {
            m_config = config;
            m_navquery = new DtNavMeshQuery(navMesh, DtCrowdConst.MAX_PATHQUEUE_NODES);

            m_maxPathSize = maxPathSize;
            m_queue = new PathQuery[MAX_QUEUE];

            for (int i = 0; i < MAX_QUEUE; i++)
            {
                m_queue[i] = new PathQuery
                {
                    refs = DT_PATHQ_INVALID,
                    //m_queue[i].result.pathCount = 0;
                    path = new long[maxPathSize]
                };
            }

            m_queueHead = 0;
        }

        public void Update()
        {
            const int MAX_KEEP_ALIVE = 2; // in update ticks.

            // Update path request until there is nothing to update
            // or upto maxIters pathfinder iterations has been consumed.
            int iterCount = m_config.maxFindPathIterations;

            for (int i = 0; i < MAX_QUEUE; i++)
            {
                PathQuery q = m_queue[m_queueHead % MAX_QUEUE];

                // Skip inactive requests.
                if (q.refs == DT_PATHQ_INVALID)
                {
                    m_queueHead++;
                    continue;
                }

                // Handle completed request.
                if (q.status.Succeeded() || q.status.Failed())
                {
                    // If the path result has not been read in few frames, free the slot.
                    q.keepAlive++;
                    if (q.keepAlive > MAX_KEEP_ALIVE)
                    {
                        q.refs = DT_PATHQ_INVALID;
                        q.status = 0;
                    }

                    m_queueHead++;
                    continue;
                }

                // Handle query start.
                if (q.status.IsEmpty())
                {
                    q.status = m_navquery.InitSlicedFindPath(q.startRef, q.endRef, q.startPos, q.endPos, q.filter, 0);
                }
                // Handle query in progress.
                if (q.status.InProgress())
                {
                    q.status = m_navquery.UpdateSlicedFindPath(iterCount, out var iters);
                    iterCount -= iters;
                }
                if (q.status.Succeeded())
                {
                    q.status = m_navquery.FinalizeSlicedFindPath(q.path, out q.npath);
                }

                if (iterCount <= 0)
                    break;
            }

            m_queueHead++;
        }

        public uint Request(long startRef, long endRef, Vector3 startPos, Vector3 endPos, IDtQueryFilter filter)
        {
            // Find empty slot
            int slot = -1;
            for (int i = 0; i < MAX_QUEUE; i++)
            {
                if (m_queue[i].refs == DT_PATHQ_INVALID)
                {
                    slot = i;
                    break;
                }
            }
            // Could not find slot.
            if (slot == -1)
                return DT_PATHQ_INVALID;

            uint refs = m_nextHandle++;
            if (m_nextHandle == DT_PATHQ_INVALID)
                m_nextHandle++;

            PathQuery q = m_queue[slot];
            q.refs = refs;
            q.startPos = startPos;
            q.startRef = startRef;
            q.endPos = endPos;
            q.endRef = endRef;

            q.status = 0;
            q.npath = 0;
            q.filter = filter;
            q.keepAlive = 0;

            return refs;
        }


        public DtStatus GetRequestStatus(uint refs)
        {
            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                if (m_queue[i].refs == refs)
                    return m_queue[i].status;
            }
            return DtStatus.DT_FAILURE;
        }

        public DtStatus GetPathResult(uint refs, Span<long> path, out int pathSize, int maxPath)
        {
            for (int i = 0; i < MAX_QUEUE; ++i)
            {
                if (m_queue[i].refs == refs)
                {
                    PathQuery q = m_queue[i];
                    DtStatus details = q.status & DtStatus.DT_STATUS_DETAIL_MASK;
                    // Free request for reuse.
                    q.refs = DT_PATHQ_INVALID;
                    q.status = 0;
                    // Copy path
                    int n = Math.Min(q.npath, maxPath);
                    //memcpy(path, q.path, sizeof(dtPolyRef) * n);
                    q.path.AsSpan(0, n).CopyTo(path);
                    pathSize = n;
                    return details | DtStatus.DT_SUCCESS;
                }
            }
            pathSize = 0;
            return DtStatus.DT_FAILURE;
        }
    }
}