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

    /// The type of navigation mesh polygon the agent is currently traversing.
    /// @ingroup crowd
    public enum CrowdAgentState
    {
        DT_CROWDAGENT_STATE_INVALID,

        /// < The agent is not in a valid state.
        DT_CROWDAGENT_STATE_WALKING,

        /// < The agent is traversing a normal navigation mesh polygon.
        DT_CROWDAGENT_STATE_OFFMESH, /// < The agent is traversing an off-mesh connection.
    };

    public enum MoveRequestState
    {
        DT_CROWDAGENT_TARGET_NONE,
        DT_CROWDAGENT_TARGET_FAILED,
        DT_CROWDAGENT_TARGET_VALID,
        DT_CROWDAGENT_TARGET_REQUESTING,
        DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE,
        DT_CROWDAGENT_TARGET_WAITING_FOR_PATH,
        DT_CROWDAGENT_TARGET_VELOCITY,
    };

    /// Represents an agent managed by a #dtCrowd object.
    /// @ingroup crowd
    public class CrowdAgent
    {
        public readonly long idx;

        /// The type of mesh polygon the agent is traversing. (See: #CrowdAgentState)
        public CrowdAgentState state;

        /// True if the agent has valid path (targetState == DT_CROWDAGENT_TARGET_VALID) and the path does not lead to the
        /// requested position, else false.
        public bool partial;

        /// The path corridor the agent is using.
        public PathCorridor corridor;

        /// The local boundary data for the agent.
        public LocalBoundary boundary;

        /// Time since the agent's path corridor was optimized.
        public float topologyOptTime;

        /// The known neighbors of the agent.
        public List<Crowd.CrowdNeighbour> neis = new List<Crowd.CrowdNeighbour>();

        /// The desired speed.
        public float desiredSpeed;

        public Vector3f npos = new Vector3f();

        /// < The current agent position. [(x, y, z)]
        public Vector3f disp = new Vector3f();

        /// < A temporary value used to accumulate agent displacement during iterative
        /// collision resolution. [(x, y, z)]
        public Vector3f dvel = new Vector3f();

        /// < The desired velocity of the agent. Based on the current path, calculated
        /// from
        /// scratch each frame. [(x, y, z)]
        public Vector3f nvel = new Vector3f();

        /// < The desired velocity adjusted by obstacle avoidance, calculated from scratch each
        /// frame. [(x, y, z)]
        public Vector3f vel = new Vector3f();

        /// < The actual velocity of the agent. The change from nvel -> vel is
        /// constrained by max acceleration. [(x, y, z)]
        /// The agent's configuration parameters.
        public CrowdAgentParams option;

        /// The local path corridor corners for the agent.
        public List<StraightPathItem> corners = new List<StraightPathItem>();

        public MoveRequestState targetState;

        /// < State of the movement request.
        public long targetRef;

        /// < Target polyref of the movement request.
        public Vector3f targetPos = new Vector3f();

        /// < Target position of the movement request (or velocity in case of
        /// DT_CROWDAGENT_TARGET_VELOCITY).
        public PathQueryResult targetPathQueryResult;

        /// < Path finder query
        public bool targetReplan;

        /// < Flag indicating that the current path is being replanned.
        public float targetReplanTime;

        /// <Time since the agent's target was replanned.
        public float targetReplanWaitTime;

        public CrowdAgentAnimation animation;

        public CrowdAgent(int idx)
        {
            this.idx = idx;
            corridor = new PathCorridor();
            boundary = new LocalBoundary();
            animation = new CrowdAgentAnimation();
        }

        public void Integrate(float dt)
        {
            // Fake dynamic constraint.
            float maxDelta = option.maxAcceleration * dt;
            Vector3f dv = VSub(nvel, vel);
            float ds = VLen(dv);
            if (ds > maxDelta)
                dv = VScale(dv, maxDelta / ds);
            vel = VAdd(vel, dv);

            // Integrate
            if (VLen(vel) > 0.0001f)
                npos = VMad(npos, vel, dt);
            else
                vel = Vector3f.Zero;
        }

        public bool OverOffmeshConnection(float radius)
        {
            if (0 == corners.Count)
                return false;

            bool offMeshConnection = ((corners[corners.Count - 1].GetFlags()
                                       & NavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                ? true
                : false;
            if (offMeshConnection)
            {
                float distSq = VDist2DSqr(npos, corners[corners.Count - 1].GetPos());
                if (distSq < radius * radius)
                    return true;
            }

            return false;
        }

        public float GetDistanceToGoal(float range)
        {
            if (0 == corners.Count)
                return range;

            bool endOfPath = ((corners[corners.Count - 1].GetFlags() & NavMeshQuery.DT_STRAIGHTPATH_END) != 0) ? true : false;
            if (endOfPath)
                return Math.Min(VDist2D(npos, corners[corners.Count - 1].GetPos()), range);

            return range;
        }

        public Vector3f CalcSmoothSteerDirection()
        {
            Vector3f dir = new Vector3f();
            if (0 < corners.Count)
            {
                int ip0 = 0;
                int ip1 = Math.Min(1, corners.Count - 1);
                var p0 = corners[ip0].GetPos();
                var p1 = corners[ip1].GetPos();

                var dir0 = VSub(p0, npos);
                var dir1 = VSub(p1, npos);
                dir0.y = 0;
                dir1.y = 0;

                float len0 = VLen(dir0);
                float len1 = VLen(dir1);
                if (len1 > 0.001f)
                    dir1 = VScale(dir1, 1.0f / len1);

                dir.x = dir0.x - dir1.x * len0 * 0.5f;
                dir.y = 0;
                dir.z = dir0.z - dir1.z * len0 * 0.5f;

                VNormalize(ref dir);
            }

            return dir;
        }

        public Vector3f CalcStraightSteerDirection()
        {
            Vector3f dir = new Vector3f();
            if (0 < corners.Count)
            {
                dir = VSub(corners[0].GetPos(), npos);
                dir.y = 0;
                VNormalize(ref dir);
            }

            return dir;
        }

        public void SetTarget(long refs, Vector3f pos)
        {
            targetRef = refs;
            targetPos = pos;
            targetPathQueryResult = null;
            if (targetRef != 0)
            {
                targetState = MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;
            }
            else
            {
                targetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
            }
        }
    }
}
