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

using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour.Crowd
{
    using static DotRecast.Core.RcMath;

    public class PathQueue
    {
        private readonly CrowdConfig config;
        private readonly LinkedList<PathQuery> queue = new LinkedList<PathQuery>();

        public PathQueue(CrowdConfig config)
        {
            this.config = config;
        }

        public void Update(NavMesh navMesh)
        {
            // Update path request until there is nothing to update or up to maxIters pathfinder iterations has been
            // consumed.
            int iterCount = config.maxFindPathIterations;
            while (iterCount > 0)
            {
                PathQuery q = queue.First?.Value;
                if (q == null)
                {
                    break;
                }
                queue.RemoveFirst();

                // Handle query start.
                if (q.result.status == null)
                {
                    q.navQuery = new NavMeshQuery(navMesh);
                    q.result.status = q.navQuery.InitSlicedFindPath(q.startRef, q.endRef, q.startPos, q.endPos, q.filter, 0);
                }

                // Handle query in progress.
                if (q.result.status.IsInProgress())
                {
                    Result<int> res = q.navQuery.UpdateSlicedFindPath(iterCount);
                    q.result.status = res.status;
                    iterCount -= res.result;
                }

                if (q.result.status.IsSuccess())
                {
                    Result<List<long>> path = q.navQuery.FinalizeSlicedFindPath();
                    q.result.status = path.status;
                    q.result.path = path.result;
                }

                if (!(q.result.status.IsFailed() || q.result.status.IsSuccess()))
                {
                    queue.AddFirst(q);
                }
            }
        }

        public PathQueryResult Request(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IQueryFilter filter)
        {
            if (queue.Count >= config.pathQueueSize)
            {
                return null;
            }

            PathQuery q = new PathQuery();
            q.startPos = startPos;
            q.startRef = startRef;
            q.endPos = endPos;
            q.endRef = endRef;
            q.result.status = null;
            q.filter = filter;
            queue.AddLast(q);
            return q.result;
        }
    }
}