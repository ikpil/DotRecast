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
using System.Numerics;
using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    /// Represents an agent managed by a #dtCrowd object.
    /// @ingroup crowd
    public class DtCrowdAgent
    {
        public readonly long idx;

        public bool active;

        /// The type of mesh polygon the agent is traversing. (See: #CrowdAgentState)
        public DtCrowdAgentState state;

        /// True if the agent has valid path (targetState == DT_CROWDAGENT_TARGET_VALID) and the path does not lead to the requested position, else false.
        public bool partial;

        /// The path corridor the agent is using.
        public DtPathCorridor corridor;

        /// The local boundary data for the agent.
        public DtLocalBoundary boundary;

        /// Time since the agent's path corridor was optimized.
        public float topologyOptTime;

        /// The known neighbors of the agent.
        public readonly DtCrowdNeighbour[] neis = new DtCrowdNeighbour[DtCrowdConst.DT_CROWDAGENT_MAX_NEIGHBOURS];

        /// The number of neighbors.
        public int nneis;

        /// The desired speed.
        public float desiredSpeed;

        public Vector3 npos; // < The current agent position. [(x, y, z)]
        public Vector3 disp; // < A temporary value used to accumulate agent displacement during iterative collision resolution. [(x, y, z)]
        public Vector3 dvel; // < The desired velocity of the agent. Based on the current path, calculated from scratch each frame. [(x, y, z)]
        public Vector3 nvel; // < The desired velocity adjusted by obstacle avoidance, calculated from scratch each frame. [(x, y, z)]
        public Vector3 vel; // < The actual velocity of the agent. The change from nvel -> vel is constrained by max acceleration. [(x, y, z)]

        /// The agent's configuration parameters.
        public DtCrowdAgentParams option;

        /// The local path corridor corners for the agent.
        public DtStraightPath[] corners = new DtStraightPath[DtCrowdConst.DT_CROWDAGENT_MAX_CORNERS];

        /// The number of corners.
        public int ncorners;

        public DtMoveRequestState targetState; // < State of the movement request.
        public long targetRef; // < Target polyref of the movement request.
        public Vector3 targetPos = new Vector3(); // < Target position of the movement request (or velocity in case of DT_CROWDAGENT_TARGET_VELOCITY).
        public uint targetPathqRef; // < Path finder refs
        public bool targetReplan; // < Flag indicating that the current path is being replanned.
        public float targetReplanTime; // <Time since the agent's target was replanned.
        public float targetReplanWaitTime;

        public DtCrowdAgentAnimation animation;

        public DtCrowdAgent(int idx)
        {
            this.idx = idx;
            corridor = new DtPathCorridor();
            boundary = new DtLocalBoundary();
        }

        public void Integrate(float dt)
        {
            // Fake dynamic constraint.
            float maxDelta = option.maxAcceleration * dt;
            Vector3 dv = Vector3.Subtract(nvel, vel);
            float ds = dv.Length();
            if (ds > maxDelta)
                dv = dv * (maxDelta / ds);
            vel = Vector3.Add(vel, dv);

            // Integrate
            if (vel.Length() > 0.0001f)
                npos = RcVec.Mad(npos, vel, dt);
            else
                vel = Vector3.Zero;
        }

        public bool OverOffmeshConnection(float radius)
        {
            if (0 == ncorners)
                return false;

            bool offMeshConnection = ((corners[ncorners - 1].flags
                                       & DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                ? true
                : false;
            if (offMeshConnection)
            {
                float distSq = RcVec.Dist2DSqr(npos, corners[ncorners - 1].pos);
                if (distSq < radius * radius)
                    return true;
            }

            return false;
        }

        public float GetDistanceToGoal(float range)
        {
            if (0 == ncorners)
                return range;

            bool endOfPath = ((corners[ncorners - 1].flags & DtStraightPathFlags.DT_STRAIGHTPATH_END) != 0) ? true : false;
            if (endOfPath)
                return Math.Min(RcVec.Dist2D(npos, corners[ncorners - 1].pos), range);

            return range;
        }

        public Vector3 CalcSmoothSteerDirection()
        {
            Vector3 dir = new Vector3();
            if (0 < ncorners)
            {
                int ip0 = 0;
                int ip1 = Math.Min(1, ncorners - 1);
                var p0 = corners[ip0].pos;
                var p1 = corners[ip1].pos;

                var dir0 = Vector3.Subtract(p0, npos);
                var dir1 = Vector3.Subtract(p1, npos);
                dir0.Y = 0;
                dir1.Y = 0;

                float len0 = dir0.Length();
                float len1 = dir1.Length();
                if (len1 > 0.001f)
                    dir1 = dir1 * (1.0f / len1);

                dir.X = dir0.X - dir1.X * len0 * 0.5f;
                dir.Y = 0;
                dir.Z = dir0.Z - dir1.Z * len0 * 0.5f;
                dir = Vector3.Normalize(dir);
            }

            return dir;
        }

        public Vector3 CalcStraightSteerDirection()
        {
            Vector3 dir = new Vector3();
            if (0 < ncorners)
            {
                dir = Vector3.Subtract(corners[0].pos, npos);
                dir.Y = 0;
                dir = Vector3.Normalize(dir);
            }

            return dir;
        }

        public void SetTarget(long refs, Vector3 pos)
        {
            targetRef = refs;
            targetPos = pos;
            targetPathqRef = DtPathQueue.DT_PATHQ_INVALID;
            if (targetRef != 0)
            {
                targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;
            }
            else
            {
                targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
            }
        }
    }
}