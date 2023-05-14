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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DotRecast.Core;
using DotRecast.Detour.Crowd.Tracking;
using DotRecast.Detour.QueryResults;

namespace DotRecast.Detour.Crowd
{
    using static DotRecast.Core.RcMath;

    /**
 * Members in this module implement local steering and dynamic avoidance features.
 *
 * The crowd is the big beast of the navigation features. It not only handles a lot of the path management for you, but
 * also local steering and dynamic avoidance between members of the crowd. I.e. It can keep your agents from running
 * into each other.
 *
 * Main class: Crowd
 *
 * The #dtNavMeshQuery and #dtPathCorridor classes provide perfectly good, easy to use path planning features. But in
 * the end they only give you points that your navigation client should be moving toward. When it comes to deciding
 * things like agent velocity and steering to avoid other agents, that is up to you to implement. Unless, of course, you
 * decide to use Crowd.
 *
 * Basically, you add an agent to the crowd, providing various configuration settings such as maximum speed and
 * acceleration. You also provide a local target to move toward. The crowd manager then provides, with every update, the
 * new agent position and velocity for the frame. The movement will be constrained to the navigation mesh, and steering
 * will be applied to ensure agents managed by the crowd do not collide with each other.
 *
 * This is very powerful feature set. But it comes with limitations.
 *
 * The biggest limitation is that you must give control of the agent's position completely over to the crowd manager.
 * You can update things like maximum speed and acceleration. But in order for the crowd manager to do its thing, it
 * can't allow you to constantly be giving it overrides to position and velocity. So you give up direct control of the
 * agent's movement. It belongs to the crowd.
 *
 * The second biggest limitation revolves around the fact that the crowd manager deals with local planning. So the
 * agent's target should never be more than 256 polygons away from its current position. If it is, you risk your agent
 * failing to reach its target. So you may still need to do long distance planning and provide the crowd manager with
 * intermediate targets.
 *
 * Other significant limitations:
 *
 * - All agents using the crowd manager will use the same #dtQueryFilter. - Crowd management is relatively expensive.
 * The maximum agents under crowd management at any one time is between 20 and 30. A good place to start is a maximum of
 * 25 agents for 0.5ms per frame.
 *
 * @note This is a summary list of members. Use the index or search feature to find minor members.
 *
 * @struct dtCrowdAgentParams
 * @see CrowdAgent, Crowd::AddAgent(), Crowd::UpdateAgentParameters()
 *
 * @var dtCrowdAgentParams::obstacleAvoidanceType
 * @par
 *
 * 		#dtCrowd permits agents to use different avoidance configurations. This value is the index of the
 *      #dtObstacleAvoidanceParams within the crowd.
 *
 * @see dtObstacleAvoidanceParams, dtCrowd::SetObstacleAvoidanceParams(), dtCrowd::GetObstacleAvoidanceParams()
 *
 * @var dtCrowdAgentParams::collisionQueryRange
 * @par
 *
 * 		Collision elements include other agents and navigation mesh boundaries.
 *
 *      This value is often based on the agent radius and/or maximum speed. E.g. radius * 8
 *
 * @var dtCrowdAgentParams::pathOptimizationRange
 * @par
 *
 * 		Only applicalbe if #updateFlags includes the #DT_CROWD_OPTIMIZE_VIS flag.
 *
 *      This value is often based on the agent radius. E.g. radius * 30
 *
 * @see dtPathCorridor::OptimizePathVisibility()
 *
 * @var dtCrowdAgentParams::separationWeight
 * @par
 *
 * 		A higher value will result in agents trying to stay farther away from each other at the cost of more difficult
 *      steering in tight spaces.
 *
 */
    /**
     * This is the core class of the refs crowd module. See the refs crowd documentation for a summary of the crowd
     * features. A common method for setting up the crowd is as follows: -# Allocate the crowd -# Set the avoidance
     * configurations using #SetObstacleAvoidanceParams(). -# Add agents using #AddAgent() and make an initial movement
     * request using #RequestMoveTarget(). A common process for managing the crowd is as follows: -# Call #Update() to allow
     * the crowd to manage its agents. -# Retrieve agent information using #GetActiveAgents(). -# Make movement requests
     * using #RequestMoveTarget() when movement goal changes. -# Repeat every frame. Some agent configuration settings can
     * be updated using #UpdateAgentParameters(). But the crowd owns the agent position. So it is not possible to update an
     * active agent's position. If agent position must be fed back into the crowd, the agent must be removed and re-added.
     * Notes: - Path related information is available for newly added agents only after an #Update() has been performed. -
     * Agent objects are kept in a pool and re-used. So it is important when using agent objects to check the value of
     * #dtCrowdAgent::active to determine if the agent is actually in use or not. - This class is meant to provide 'local'
     * movement. There is a limit of 256 polygons in the path corridor. So it is not meant to provide automatic pathfinding
     * services over long distances.
     *
     * @see DtAllocCrowd(), DtFreeCrowd(), Init(), dtCrowdAgent
     */
    public class Crowd
    {
        /// The maximum number of corners a crowd agent will look ahead in the path.
        /// This value is used for sizing the crowd agent corner buffers.
        /// Due to the behavior of the crowd manager, the actual number of useful
        /// corners will be one less than this number.
        /// @ingroup crowd
        public const int DT_CROWDAGENT_MAX_CORNERS = 4;

        /// The maximum number of crowd avoidance configurations supported by the
        /// crowd manager.
        /// @ingroup crowd
        /// @see dtObstacleAvoidanceParams, dtCrowd::SetObstacleAvoidanceParams(), dtCrowd::GetObstacleAvoidanceParams(),
        /// dtCrowdAgentParams::obstacleAvoidanceType
        public const int DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS = 8;

        /// The maximum number of query filter types supported by the crowd manager.
        /// @ingroup crowd
        /// @see dtQueryFilter, dtCrowd::GetFilter() dtCrowd::GetEditableFilter(),
        /// dtCrowdAgentParams::queryFilterType
        public const int DT_CROWD_MAX_QUERY_FILTER_TYPE = 16;

        private readonly RcAtomicInteger _agentId = new RcAtomicInteger();
        private readonly List<CrowdAgent> _agents;
        private readonly PathQueue _pathQ;
        private readonly ObstacleAvoidanceParams[] _obstacleQueryParams = new ObstacleAvoidanceParams[DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS];
        private readonly ObstacleAvoidanceQuery _obstacleQuery;
        private ProximityGrid _grid;
        private readonly Vector3f _ext = new Vector3f();
        private readonly IQueryFilter[] _filters = new IQueryFilter[DT_CROWD_MAX_QUERY_FILTER_TYPE];
        private NavMeshQuery _navQuery;
        private NavMesh _navMesh;
        private readonly CrowdConfig _config;
        private readonly CrowdTelemetry _telemetry = new CrowdTelemetry();
        private int _velocitySampleCount;

        public Crowd(CrowdConfig config, NavMesh nav) :
            this(config, nav, i => new DefaultQueryFilter())
        {
        }

        public Crowd(CrowdConfig config, NavMesh nav, Func<int, IQueryFilter> queryFilterFactory)
        {
            _config = config;
            VSet(ref _ext, config.maxAgentRadius * 2.0f, config.maxAgentRadius * 1.5f, config.maxAgentRadius * 2.0f);

            _obstacleQuery = new ObstacleAvoidanceQuery(config.maxObstacleAvoidanceCircles, config.maxObstacleAvoidanceSegments);

            for (int i = 0; i < DT_CROWD_MAX_QUERY_FILTER_TYPE; i++)
            {
                _filters[i] = queryFilterFactory.Invoke(i);
            }

            // Init obstacle query option.
            for (int i = 0; i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS; ++i)
            {
                _obstacleQueryParams[i] = new ObstacleAvoidanceParams();
            }

            // Allocate temp buffer for merging paths.
            _pathQ = new PathQueue(config);
            _agents = new List<CrowdAgent>();

            // The navQuery is mostly used for local searches, no need for large node pool.
            _navMesh = nav;
            _navQuery = new NavMeshQuery(nav);
        }

        public void SetNavMesh(NavMesh nav)
        {
            _navMesh = nav;
            _navQuery = new NavMeshQuery(nav);
        }

        /// Sets the shared avoidance configuration for the specified index.
        /// @param[in] idx The index. [Limits: 0 <= value <
        /// #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]
        /// @param[in] option The new configuration.
        public void SetObstacleAvoidanceParams(int idx, ObstacleAvoidanceParams option)
        {
            if (idx >= 0 && idx < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                _obstacleQueryParams[idx] = new ObstacleAvoidanceParams(option);
            }
        }

        /// Gets the shared avoidance configuration for the specified index.
        /// @param[in] idx The index of the configuration to retreive.
        /// [Limits: 0 <= value < #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]
        /// @return The requested configuration.
        public ObstacleAvoidanceParams GetObstacleAvoidanceParams(int idx)
        {
            if (idx >= 0 && idx < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                return _obstacleQueryParams[idx];
            }

            return null;
        }

        /// Updates the specified agent's configuration.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #GetAgentCount()]
        /// @param[in] params The new agent configuration.
        public void UpdateAgentParameters(CrowdAgent agent, CrowdAgentParams option)
        {
            agent.option = option;
        }

        /**
     * Adds a new agent to the crowd.
     *
     * @param pos
     *            The requested position of the agent. [(x, y, z)]
     * @param params
     *            The configutation of the agent.
     * @return The newly created agent object
     */
        public CrowdAgent AddAgent(Vector3f pos, CrowdAgentParams option)
        {
            CrowdAgent ag = new CrowdAgent(_agentId.GetAndIncrement());
            _agents.Add(ag);
            UpdateAgentParameters(ag, option);

            // Find nearest position on navmesh and place the agent there.
            Result<FindNearestPolyResult> nearestPoly = _navQuery.FindNearestPoly(pos, _ext, _filters[ag.option.queryFilterType]);

            var nearest = nearestPoly.Succeeded() ? nearestPoly.result.GetNearestPos() : pos;
            long refs = nearestPoly.Succeeded() ? nearestPoly.result.GetNearestRef() : 0L;
            ag.corridor.Reset(refs, nearest);
            ag.boundary.Reset();
            ag.partial = false;

            ag.topologyOptTime = 0;
            ag.targetReplanTime = 0;

            ag.dvel = Vector3f.Zero;
            ag.nvel = Vector3f.Zero;
            ag.vel = Vector3f.Zero;
            ag.npos = nearest;

            ag.desiredSpeed = 0;

            if (refs != 0)
            {
                ag.state = CrowdAgentState.DT_CROWDAGENT_STATE_WALKING;
            }
            else
            {
                ag.state = CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
            }

            ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;

            return ag;
        }

        /**
     * Removes the agent from the crowd.
     *
     * @param agent
     *            Agent to be removed
     */
        public void RemoveAgent(CrowdAgent agent)
        {
            _agents.Remove(agent);
        }

        private bool RequestMoveTargetReplan(CrowdAgent ag, long refs, Vector3f pos)
        {
            ag.SetTarget(refs, pos);
            ag.targetReplan = true;
            return true;
        }

        /// Submits a new move request for the specified agent.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #GetAgentCount()]
        /// @param[in] ref The position's polygon reference.
        /// @param[in] pos The position within the polygon. [(x, y, z)]
        /// @return True if the request was successfully submitted.
        ///
        /// This method is used when a new target is set.
        ///
        /// The position will be constrained to the surface of the navigation mesh.
        ///
        /// The request will be processed during the next #Update().
        public bool RequestMoveTarget(CrowdAgent agent, long refs, Vector3f pos)
        {
            if (refs == 0)
            {
                return false;
            }

            // Initialize request.
            agent.SetTarget(refs, pos);
            agent.targetReplan = false;
            return true;
        }

        /// Submits a new move request for the specified agent.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #GetAgentCount()]
        /// @param[in] vel The movement velocity. [(x, y, z)]
        /// @return True if the request was successfully submitted.
        public bool RequestMoveVelocity(CrowdAgent agent, Vector3f vel)
        {
            // Initialize request.
            agent.targetRef = 0;
            agent.targetPos = vel;
            agent.targetPathQueryResult = null;
            agent.targetReplan = false;
            agent.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY;

            return true;
        }

        /// Resets any request for the specified agent.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #GetAgentCount()]
        /// @return True if the request was successfully reseted.
        public bool ResetMoveTarget(CrowdAgent agent)
        {
            // Initialize request.
            agent.targetRef = 0;
            agent.targetPos = Vector3f.Zero;
            agent.dvel = Vector3f.Zero;
            agent.targetPathQueryResult = null;
            agent.targetReplan = false;
            agent.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;
            return true;
        }

        /**
     * Gets the active agents int the agent pool.
     *
     * @return List of active agents
     */
        public IList<CrowdAgent> GetActiveAgents()
        {
            return _agents;
        }

        public Vector3f GetQueryExtents()
        {
            return _ext;
        }

        public IQueryFilter GetFilter(int i)
        {
            return i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE ? _filters[i] : null;
        }

        public ProximityGrid GetGrid()
        {
            return _grid;
        }

        public PathQueue GetPathQueue()
        {
            return _pathQ;
        }

        public CrowdTelemetry Telemetry()
        {
            return _telemetry;
        }

        public CrowdConfig Config()
        {
            return _config;
        }

        public CrowdTelemetry Update(float dt, CrowdAgentDebugInfo debug)
        {
            _velocitySampleCount = 0;

            _telemetry.Start();

            IList<CrowdAgent> agents = GetActiveAgents();

            // Check that all agents still have valid paths.
            CheckPathValidity(agents, dt);

            // Update async move request and path finder.
            UpdateMoveRequest(agents, dt);

            // Optimize path topology.
            UpdateTopologyOptimization(agents, dt);

            // Register agents to proximity grid.
            BuildProximityGrid(agents);

            // Get nearby navmesh segments and agents to collide with.
            BuildNeighbours(agents);

            // Find next corner to steer to.
            FindCorners(agents, debug);

            // Trigger off-mesh connections (depends on corners).
            TriggerOffMeshConnections(agents);

            // Calculate steering.
            CalculateSteering(agents);

            // Velocity planning.
            PlanVelocity(debug, agents);

            // Integrate.
            Integrate(dt, agents);

            // Handle collisions.
            HandleCollisions(agents);

            MoveAgents(agents);

            // Update agents using off-mesh connection.
            UpdateOffMeshConnections(agents, dt);
            return _telemetry;
        }


        private void CheckPathValidity(IList<CrowdAgent> agents, float dt)
        {
            _telemetry.Start("checkPathValidity");

            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                ag.targetReplanTime += dt;

                bool replan = false;

                // First check that the current location is valid.
                Vector3f agentPos = new Vector3f();
                long agentRef = ag.corridor.GetFirstPoly();
                agentPos = ag.npos;
                if (!_navQuery.IsValidPolyRef(agentRef, _filters[ag.option.queryFilterType]))
                {
                    // Current location is not valid, try to reposition.
                    // TODO: this can snap agents, how to handle that?
                    Result<FindNearestPolyResult> nearestPoly = _navQuery.FindNearestPoly(ag.npos, _ext,
                        _filters[ag.option.queryFilterType]);
                    agentRef = nearestPoly.Succeeded() ? nearestPoly.result.GetNearestRef() : 0L;
                    if (nearestPoly.Succeeded())
                    {
                        agentPos = nearestPoly.result.GetNearestPos();
                    }

                    if (agentRef == 0)
                    {
                        // Could not find location in navmesh, set state to invalid.
                        ag.corridor.Reset(0, agentPos);
                        ag.partial = false;
                        ag.boundary.Reset();
                        ag.state = CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
                        continue;
                    }

                    // Make sure the first polygon is valid, but leave other valid
                    // polygons in the path so that replanner can adjust the path
                    // better.
                    ag.corridor.FixPathStart(agentRef, agentPos);
                    // ag.corridor.TrimInvalidPath(agentRef, agentPos, m_navquery,
                    // &m_filter);
                    ag.boundary.Reset();
                    ag.npos = agentPos;

                    replan = true;
                }

                // If the agent does not have move target or is controlled by
                // velocity, no need to recover the target nor replan.
                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Try to recover move request position.
                if (ag.targetState != MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    && ag.targetState != MoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                {
                    if (!_navQuery.IsValidPolyRef(ag.targetRef, _filters[ag.option.queryFilterType]))
                    {
                        // Current target is not valid, try to reposition.
                        Result<FindNearestPolyResult> fnp = _navQuery.FindNearestPoly(ag.targetPos, _ext,
                            _filters[ag.option.queryFilterType]);
                        ag.targetRef = fnp.Succeeded() ? fnp.result.GetNearestRef() : 0L;
                        if (fnp.Succeeded())
                        {
                            ag.targetPos = fnp.result.GetNearestPos();
                        }

                        replan = true;
                    }

                    if (ag.targetRef == 0)
                    {
                        // Failed to reposition target, fail moverequest.
                        ag.corridor.Reset(agentRef, agentPos);
                        ag.partial = false;
                        ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;
                    }
                }

                // If nearby corridor is not valid, replan.
                if (!ag.corridor.IsValid(_config.checkLookAhead, _navQuery, _filters[ag.option.queryFilterType]))
                {
                    // Fix current path.
                    // ag.corridor.TrimInvalidPath(agentRef, agentPos, m_navquery,
                    // &m_filter);
                    // ag.boundary.Reset();
                    replan = true;
                }

                // If the end of the path is near and it is not the requested
                // location, replan.
                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VALID)
                {
                    if (ag.targetReplanTime > _config.targetReplanDelay && ag.corridor.GetPathCount() < _config.checkLookAhead
                                                                        && ag.corridor.GetLastPoly() != ag.targetRef)
                    {
                        replan = true;
                    }
                }

                // Try to replan path to goal.
                if (replan)
                {
                    if (ag.targetState != MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                    {
                        RequestMoveTargetReplan(ag, ag.targetRef, ag.targetPos);
                    }
                }
            }

            _telemetry.Stop("checkPathValidity");
        }

        private void UpdateMoveRequest(IList<CrowdAgent> agents, float dt)
        {
            _telemetry.Start("updateMoveRequest");

            RcSortedQueue<CrowdAgent> queue = new RcSortedQueue<CrowdAgent>((a1, a2) => a2.targetReplanTime.CompareTo(a1.targetReplanTime));

            // Fire off new requests.
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state == CrowdAgentState.DT_CROWDAGENT_STATE_INVALID)
                {
                    continue;
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING)
                {
                    List<long> path = ag.corridor.GetPath();
                    if (0 == path.Count)
                    {
                        throw new ArgumentException("Empty path");
                    }

                    // Quick search towards the goal.
                    _navQuery.InitSlicedFindPath(path[0], ag.targetRef, ag.npos, ag.targetPos,
                        _filters[ag.option.queryFilterType], 0);
                    _navQuery.UpdateSlicedFindPath(_config.maxTargetFindPathIterations);
                    Result<List<long>> pathFound;
                    if (ag.targetReplan) // && npath > 10)
                    {
                        // Try to use existing steady path during replan if
                        // possible.
                        pathFound = _navQuery.FinalizeSlicedFindPathPartial(path);
                    }
                    else
                    {
                        // Try to move towards target when goal changes.
                        pathFound = _navQuery.FinalizeSlicedFindPath();
                    }

                    List<long> reqPath = pathFound.result;
                    Vector3f reqPos = new Vector3f();
                    if (pathFound.Succeeded() && reqPath.Count > 0)
                    {
                        // In progress or succeed.
                        if (reqPath[reqPath.Count - 1] != ag.targetRef)
                        {
                            // Partial path, constrain target position inside the
                            // last polygon.
                            Result<ClosestPointOnPolyResult> cr = _navQuery.ClosestPointOnPoly(reqPath[reqPath.Count - 1],
                                ag.targetPos);
                            if (cr.Succeeded())
                            {
                                reqPos = cr.result.GetClosest();
                            }
                            else
                            {
                                reqPath = new List<long>();
                            }
                        }
                        else
                        {
                            reqPos = ag.targetPos;
                        }
                    }
                    else
                    {
                        // Could not find path, start the request from current
                        // location.
                        reqPos = ag.npos;
                        reqPath = new List<long>();
                        reqPath.Add(path[0]);
                    }

                    ag.corridor.SetCorridor(reqPos, reqPath);
                    ag.boundary.Reset();
                    ag.partial = false;

                    if (reqPath[reqPath.Count - 1] == ag.targetRef)
                    {
                        ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_VALID;
                        ag.targetReplanTime = 0;
                    }
                    else
                    {
                        // The path is longer or potentially unreachable, full plan.
                        ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE;
                    }

                    ag.targetReplanWaitTime = 0;
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                {
                    queue.Enqueue(ag);
                }
            }

            while (!queue.IsEmpty())
            {
                CrowdAgent ag = queue.Dequeue();
                ag.targetPathQueryResult = _pathQ.Request(ag.corridor.GetLastPoly(), ag.targetRef, ag.corridor.GetTarget(),
                    ag.targetPos, _filters[ag.option.queryFilterType]);
                if (ag.targetPathQueryResult != null)
                {
                    ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH;
                }
                else
                {
                    _telemetry.RecordMaxTimeToEnqueueRequest(ag.targetReplanWaitTime);
                    ag.targetReplanWaitTime += dt;
                }
            }

            // Update requests.
            _telemetry.Start("pathQueueUpdate");
            _pathQ.Update(_navMesh);
            _telemetry.Stop("pathQueueUpdate");

            // Process path results.
            foreach (CrowdAgent ag in agents)
            {
                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                {
                    // _telemetry.RecordPathWaitTime(ag.targetReplanTime);
                    // Poll path queue.
                    Status status = ag.targetPathQueryResult.status;
                    if (status != null && status.IsFailed())
                    {
                        // Path find failed, retry if the target location is still
                        // valid.
                        ag.targetPathQueryResult = null;
                        if (ag.targetRef != 0)
                        {
                            ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;
                        }
                        else
                        {
                            ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                        }

                        ag.targetReplanTime = 0;
                    }
                    else if (status != null && status.IsSuccess())
                    {
                        List<long> path = ag.corridor.GetPath();
                        if (0 == path.Count)
                        {
                            throw new ArgumentException("Empty path");
                        }

                        // Apply results.
                        var targetPos = ag.targetPos;

                        bool valid = true;
                        List<long> res = ag.targetPathQueryResult.path;
                        if (status.IsFailed() || 0 == res.Count)
                        {
                            valid = false;
                        }

                        if (status.IsPartial())
                        {
                            ag.partial = true;
                        }
                        else
                        {
                            ag.partial = false;
                        }

                        // Merge result and existing path.
                        // The agent might have moved whilst the request is
                        // being processed, so the path may have changed.
                        // We assume that the end of the path is at the same
                        // location
                        // where the request was issued.

                        // The last ref in the old path should be the same as
                        // the location where the request was issued..
                        if (valid && path[path.Count - 1] != res[0])
                        {
                            valid = false;
                        }

                        if (valid)
                        {
                            // Put the old path infront of the old path.
                            if (path.Count > 1)
                            {
                                path.RemoveAt(path.Count - 1);
                                path.AddRange(res);
                                res = path;
                                // Remove trackbacks
                                for (int j = 1; j < res.Count - 1; ++j)
                                {
                                    if (j - 1 >= 0 && j + 1 < res.Count)
                                    {
                                        if (res[j - 1] == res[j + 1])
                                        {
                                            res.RemoveAt(j + 1);
                                            res.RemoveAt(j);
                                            j -= 2;
                                        }
                                    }
                                }
                            }

                            // Check for partial path.
                            if (res[res.Count - 1] != ag.targetRef)
                            {
                                // Partial path, constrain target position inside
                                // the last polygon.
                                Result<ClosestPointOnPolyResult> cr = _navQuery.ClosestPointOnPoly(res[res.Count - 1], targetPos);
                                if (cr.Succeeded())
                                {
                                    targetPos = cr.result.GetClosest();
                                }
                                else
                                {
                                    valid = false;
                                }
                            }
                        }

                        if (valid)
                        {
                            // Set current corridor.
                            ag.corridor.SetCorridor(targetPos, res);
                            // Force to update boundary.
                            ag.boundary.Reset();
                            ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_VALID;
                        }
                        else
                        {
                            // Something went wrong.
                            ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                        }

                        ag.targetReplanTime = 0;
                    }

                    _telemetry.RecordMaxTimeToFindPath(ag.targetReplanWaitTime);
                    ag.targetReplanWaitTime += dt;
                }
            }

            _telemetry.Stop("updateMoveRequest");
        }

        private void UpdateTopologyOptimization(IList<CrowdAgent> agents, float dt)
        {
            _telemetry.Start("updateTopologyOptimization");

            RcSortedQueue<CrowdAgent> queue = new RcSortedQueue<CrowdAgent>((a1, a2) => a2.topologyOptTime.CompareTo(a1.topologyOptTime));

            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if ((ag.option.updateFlags & CrowdAgentParams.DT_CROWD_OPTIMIZE_TOPO) == 0)
                {
                    continue;
                }

                ag.topologyOptTime += dt;
                if (ag.topologyOptTime >= _config.topologyOptimizationTimeThreshold)
                {
                    queue.Enqueue(ag);
                }
            }

            while (!queue.IsEmpty())
            {
                CrowdAgent ag = queue.Dequeue();
                ag.corridor.OptimizePathTopology(_navQuery, _filters[ag.option.queryFilterType], _config.maxTopologyOptimizationIterations);
                ag.topologyOptTime = 0;
            }

            _telemetry.Stop("updateTopologyOptimization");
        }

        private void BuildProximityGrid(IList<CrowdAgent> agents)
        {
            _telemetry.Start("buildProximityGrid");
            _grid = new ProximityGrid(_config.maxAgentRadius * 3);

            foreach (CrowdAgent ag in agents)
            {
                Vector3f p = ag.npos;
                float r = ag.option.radius;
                _grid.AddItem(ag, p.x - r, p.z - r, p.x + r, p.z + r);
            }

            _telemetry.Stop("buildProximityGrid");
        }

        private void BuildNeighbours(IList<CrowdAgent> agents)
        {
            _telemetry.Start("buildNeighbours");
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Update the collision boundary after certain distance has been passed or
                // if it has become invalid.
                float updateThr = ag.option.collisionQueryRange * 0.25f;
                if (VDist2DSqr(ag.npos, ag.boundary.GetCenter()) > Sqr(updateThr)
                    || !ag.boundary.IsValid(_navQuery, _filters[ag.option.queryFilterType]))
                {
                    ag.boundary.Update(ag.corridor.GetFirstPoly(), ag.npos, ag.option.collisionQueryRange, _navQuery,
                        _filters[ag.option.queryFilterType]);
                }

                // Query neighbour agents
                ag.neis = GetNeighbours(ag.npos, ag.option.height, ag.option.collisionQueryRange, ag, _grid);
            }

            _telemetry.Stop("buildNeighbours");
        }


        private List<CrowdNeighbour> GetNeighbours(Vector3f pos, float height, float range, CrowdAgent skip, ProximityGrid grid)
        {
            HashSet<CrowdAgent> proxAgents = grid.QueryItems(pos.x - range, pos.z - range, pos.x + range, pos.z + range);
            List<CrowdNeighbour> result = new List<CrowdNeighbour>(proxAgents.Count);
            foreach (CrowdAgent ag in proxAgents)
            {
                if (ag == skip)
                {
                    continue;
                }

                // Check for overlap.
                Vector3f diff = VSub(pos, ag.npos);
                if (Math.Abs(diff.y) >= (height + ag.option.height) / 2.0f)
                {
                    continue;
                }

                diff.y = 0;
                float distSqr = VLenSqr(diff);
                if (distSqr > Sqr(range))
                {
                    continue;
                }

                result.Add(new CrowdNeighbour(ag, distSqr));
            }

            result.Sort((o1, o2) => o1.dist.CompareTo(o2.dist));
            return result;
        }

        private void FindCorners(IList<CrowdAgent> agents, CrowdAgentDebugInfo debug)
        {
            _telemetry.Start("findCorners");
            CrowdAgent debugAgent = debug != null ? debug.agent : null;
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Find corners for steering
                ag.corners = ag.corridor.FindCorners(DT_CROWDAGENT_MAX_CORNERS, _navQuery, _filters[ag.option.queryFilterType]);

                // Check to see if the corner after the next corner is directly visible,
                // and short cut to there.
                if ((ag.option.updateFlags & CrowdAgentParams.DT_CROWD_OPTIMIZE_VIS) != 0 && ag.corners.Count > 0)
                {
                    Vector3f target = ag.corners[Math.Min(1, ag.corners.Count - 1)].GetPos();
                    ag.corridor.OptimizePathVisibility(target, ag.option.pathOptimizationRange, _navQuery,
                        _filters[ag.option.queryFilterType]);

                    // Copy data for debug purposes.
                    if (debugAgent == ag)
                    {
                        debug.optStart = ag.corridor.GetPos();
                        debug.optEnd = target;
                    }
                }
                else
                {
                    // Copy data for debug purposes.
                    if (debugAgent == ag)
                    {
                        debug.optStart = Vector3f.Zero;
                        debug.optEnd = Vector3f.Zero;
                    }
                }
            }

            _telemetry.Stop("findCorners");
        }

        private void TriggerOffMeshConnections(IList<CrowdAgent> agents)
        {
            _telemetry.Start("triggerOffMeshConnections");
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Check
                float triggerRadius = ag.option.radius * 2.25f;
                if (ag.OverOffmeshConnection(triggerRadius))
                {
                    // Prepare to off-mesh connection.
                    CrowdAgentAnimation anim = ag.animation;

                    // Adjust the path over the off-mesh connection.
                    long[] refs = new long[2];
                    if (ag.corridor.MoveOverOffmeshConnection(ag.corners[ag.corners.Count - 1].GetRef(), refs, ref anim.startPos,
                            ref anim.endPos, _navQuery))
                    {
                        anim.initPos = ag.npos;
                        anim.polyRef = refs[1];
                        anim.active = true;
                        anim.t = 0.0f;
                        anim.tmax = (VDist2D(anim.startPos, anim.endPos) / ag.option.maxSpeed) * 0.5f;

                        ag.state = CrowdAgentState.DT_CROWDAGENT_STATE_OFFMESH;
                        ag.corners.Clear();
                        ag.neis.Clear();
                        continue;
                    }
                    else
                    {
                        // Path validity check will ensure that bad/blocked connections will be replanned.
                    }
                }
            }

            _telemetry.Stop("triggerOffMeshConnections");
        }

        private void CalculateSteering(IList<CrowdAgent> agents)
        {
            _telemetry.Start("calculateSteering");
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                {
                    continue;
                }

                Vector3f dvel = new Vector3f();

                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    dvel = ag.targetPos;
                    ag.desiredSpeed = VLen(ag.targetPos);
                }
                else
                {
                    // Calculate steering direction.
                    if ((ag.option.updateFlags & CrowdAgentParams.DT_CROWD_ANTICIPATE_TURNS) != 0)
                    {
                        dvel = ag.CalcSmoothSteerDirection();
                    }
                    else
                    {
                        dvel = ag.CalcStraightSteerDirection();
                    }

                    // Calculate speed scale, which tells the agent to slowdown at the end of the path.
                    float slowDownRadius = ag.option.radius * 2; // TODO: make less hacky.
                    float speedScale = ag.GetDistanceToGoal(slowDownRadius) / slowDownRadius;

                    ag.desiredSpeed = ag.option.maxSpeed;
                    dvel = VScale(dvel, ag.desiredSpeed * speedScale);
                }

                // Separation
                if ((ag.option.updateFlags & CrowdAgentParams.DT_CROWD_SEPARATION) != 0)
                {
                    float separationDist = ag.option.collisionQueryRange;
                    float invSeparationDist = 1.0f / separationDist;
                    float separationWeight = ag.option.separationWeight;

                    float w = 0;
                    Vector3f disp = new Vector3f();

                    for (int j = 0; j < ag.neis.Count; ++j)
                    {
                        CrowdAgent nei = ag.neis[j].agent;

                        Vector3f diff = VSub(ag.npos, nei.npos);
                        diff.y = 0;

                        float distSqr = VLenSqr(diff);
                        if (distSqr < 0.00001f)
                        {
                            continue;
                        }

                        if (distSqr > Sqr(separationDist))
                        {
                            continue;
                        }

                        float dist = (float)Math.Sqrt(distSqr);
                        float weight = separationWeight * (1.0f - Sqr(dist * invSeparationDist));

                        disp = VMad(disp, diff, weight / dist);
                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        // Adjust desired velocity.
                        dvel = VMad(dvel, disp, 1.0f / w);
                        // Clamp desired velocity to desired speed.
                        float speedSqr = VLenSqr(dvel);
                        float desiredSqr = Sqr(ag.desiredSpeed);
                        if (speedSqr > desiredSqr)
                        {
                            dvel = VScale(dvel, desiredSqr / speedSqr);
                        }
                    }
                }

                // Set the desired velocity.
                ag.dvel = dvel;
            }

            _telemetry.Stop("calculateSteering");
        }

        private void PlanVelocity(CrowdAgentDebugInfo debug, IList<CrowdAgent> agents)
        {
            _telemetry.Start("planVelocity");
            CrowdAgent debugAgent = debug != null ? debug.agent : null;
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if ((ag.option.updateFlags & CrowdAgentParams.DT_CROWD_OBSTACLE_AVOIDANCE) != 0)
                {
                    _obstacleQuery.Reset();

                    // Add neighbours as obstacles.
                    for (int j = 0; j < ag.neis.Count; ++j)
                    {
                        CrowdAgent nei = ag.neis[j].agent;
                        _obstacleQuery.AddCircle(nei.npos, nei.option.radius, nei.vel, nei.dvel);
                    }

                    // Append neighbour segments as obstacles.
                    for (int j = 0; j < ag.boundary.GetSegmentCount(); ++j)
                    {
                        Vector3f[] s = ag.boundary.GetSegment(j);
                        Vector3f s3 = s[1];
                        //Array.Copy(s, 3, s3, 0, 3);
                        if (TriArea2D(ag.npos, s[0], s3) < 0.0f)
                        {
                            continue;
                        }

                        _obstacleQuery.AddSegment(s[0], s3);
                    }

                    ObstacleAvoidanceDebugData vod = null;
                    if (debugAgent == ag)
                    {
                        vod = debug.vod;
                    }

                    // Sample new safe velocity.
                    bool adaptive = true;
                    int ns = 0;

                    ObstacleAvoidanceParams option = _obstacleQueryParams[ag.option.obstacleAvoidanceType];

                    if (adaptive)
                    {
                        var nsnvel = _obstacleQuery.SampleVelocityAdaptive(ag.npos, ag.option.radius,
                            ag.desiredSpeed, ag.vel, ag.dvel, option, vod);
                        ns = nsnvel.Item1;
                        ag.nvel = nsnvel.Item2;
                    }
                    else
                    {
                        var nsnvel = _obstacleQuery.SampleVelocityGrid(ag.npos, ag.option.radius,
                            ag.desiredSpeed, ag.vel, ag.dvel, option, vod);
                        ns = nsnvel.Item1;
                        ag.nvel = nsnvel.Item2;
                    }

                    _velocitySampleCount += ns;
                }
                else
                {
                    // If not using velocity planning, new velocity is directly the desired velocity.
                    ag.nvel = ag.dvel;
                }
            }

            _telemetry.Stop("planVelocity");
        }

        private void Integrate(float dt, IList<CrowdAgent> agents)
        {
            _telemetry.Start("integrate");
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                ag.Integrate(dt);
            }

            _telemetry.Stop("integrate");
        }

        private void HandleCollisions(IList<CrowdAgent> agents)
        {
            _telemetry.Start("handleCollisions");
            for (int iter = 0; iter < 4; ++iter)
            {
                foreach (CrowdAgent ag in agents)
                {
                    long idx0 = ag.idx;
                    if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                    {
                        continue;
                    }

                    ag.disp = Vector3f.Zero;

                    float w = 0;

                    for (int j = 0; j < ag.neis.Count; ++j)
                    {
                        CrowdAgent nei = ag.neis[j].agent;
                        long idx1 = nei.idx;
                        Vector3f diff = VSub(ag.npos, nei.npos);
                        diff.y = 0;

                        float dist = VLenSqr(diff);
                        if (dist > Sqr(ag.option.radius + nei.option.radius))
                        {
                            continue;
                        }

                        dist = (float)Math.Sqrt(dist);
                        float pen = (ag.option.radius + nei.option.radius) - dist;
                        if (dist < 0.0001f)
                        {
                            // Agents on top of each other, try to choose diverging separation directions.
                            if (idx0 > idx1)
                            {
                                VSet(ref diff, -ag.dvel.z, 0, ag.dvel.x);
                            }
                            else
                            {
                                VSet(ref diff, ag.dvel.z, 0, -ag.dvel.x);
                            }

                            pen = 0.01f;
                        }
                        else
                        {
                            pen = (1.0f / dist) * (pen * 0.5f) * _config.collisionResolveFactor;
                        }

                        ag.disp = VMad(ag.disp, diff, pen);

                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        float iw = 1.0f / w;
                        ag.disp = VScale(ag.disp, iw);
                    }
                }

                foreach (CrowdAgent ag in agents)
                {
                    if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                    {
                        continue;
                    }

                    ag.npos = VAdd(ag.npos, ag.disp);
                }
            }

            _telemetry.Stop("handleCollisions");
        }

        private void MoveAgents(IList<CrowdAgent> agents)
        {
            _telemetry.Start("moveAgents");
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Move along navmesh.
                ag.corridor.MovePosition(ag.npos, _navQuery, _filters[ag.option.queryFilterType]);
                // Get valid constrained position back.
                ag.npos = ag.corridor.GetPos();

                // If not using path, truncate the corridor to just one poly.
                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    ag.corridor.Reset(ag.corridor.GetFirstPoly(), ag.npos);
                    ag.partial = false;
                }
            }

            _telemetry.Stop("moveAgents");
        }

        private void UpdateOffMeshConnections(IList<CrowdAgent> agents, float dt)
        {
            _telemetry.Start("updateOffMeshConnections");
            foreach (CrowdAgent ag in agents)
            {
                CrowdAgentAnimation anim = ag.animation;
                if (!anim.active)
                {
                    continue;
                }

                anim.t += dt;
                if (anim.t > anim.tmax)
                {
                    // Reset animation
                    anim.active = false;
                    // Prepare agent for walking.
                    ag.state = CrowdAgentState.DT_CROWDAGENT_STATE_WALKING;
                    continue;
                }

                // Update position
                float ta = anim.tmax * 0.15f;
                float tb = anim.tmax;
                if (anim.t < ta)
                {
                    float u = Tween(anim.t, 0.0f, ta);
                    ag.npos = VLerp(anim.initPos, anim.startPos, u);
                }
                else
                {
                    float u = Tween(anim.t, ta, tb);
                    ag.npos = VLerp(anim.startPos, anim.endPos, u);
                }

                // Update velocity.
                ag.vel = Vector3f.Zero;
                ag.dvel = Vector3f.Zero;
            }

            _telemetry.Stop("updateOffMeshConnections");
        }

        private float Tween(float t, float t0, float t1)
        {
            return Clamp((t - t0) / (t1 - t0), 0.0f, 1.0f);
        }
    }
}