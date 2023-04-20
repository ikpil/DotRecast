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
using DotRecast.Core;
using DotRecast.Detour.Crowd.Tracking;

namespace DotRecast.Detour.Crowd
{
    using static DotRecast.Core.RecastMath;

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
 * @see CrowdAgent, Crowd::addAgent(), Crowd::updateAgentParameters()
 *
 * @var dtCrowdAgentParams::obstacleAvoidanceType
 * @par
 *
 * 		#dtCrowd permits agents to use different avoidance configurations. This value is the index of the
 *      #dtObstacleAvoidanceParams within the crowd.
 *
 * @see dtObstacleAvoidanceParams, dtCrowd::setObstacleAvoidanceParams(), dtCrowd::getObstacleAvoidanceParams()
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
 * @see dtPathCorridor::optimizePathVisibility()
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
     * configurations using #setObstacleAvoidanceParams(). -# Add agents using #addAgent() and make an initial movement
     * request using #requestMoveTarget(). A common process for managing the crowd is as follows: -# Call #update() to allow
     * the crowd to manage its agents. -# Retrieve agent information using #getActiveAgents(). -# Make movement requests
     * using #requestMoveTarget() when movement goal changes. -# Repeat every frame. Some agent configuration settings can
     * be updated using #updateAgentParameters(). But the crowd owns the agent position. So it is not possible to update an
     * active agent's position. If agent position must be fed back into the crowd, the agent must be removed and re-added.
     * Notes: - Path related information is available for newly added agents only after an #update() has been performed. -
     * Agent objects are kept in a pool and re-used. So it is important when using agent objects to check the value of
     * #dtCrowdAgent::active to determine if the agent is actually in use or not. - This class is meant to provide 'local'
     * movement. There is a limit of 256 polygons in the path corridor. So it is not meant to provide automatic pathfinding
     * services over long distances.
     *
     * @see dtAllocCrowd(), dtFreeCrowd(), init(), dtCrowdAgent
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
        /// @see dtObstacleAvoidanceParams, dtCrowd::setObstacleAvoidanceParams(), dtCrowd::getObstacleAvoidanceParams(),
        /// dtCrowdAgentParams::obstacleAvoidanceType
        public const int DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS = 8;

        /// The maximum number of query filter types supported by the crowd manager.
        /// @ingroup crowd
        /// @see dtQueryFilter, dtCrowd::getFilter() dtCrowd::getEditableFilter(),
        /// dtCrowdAgentParams::queryFilterType
        public const int DT_CROWD_MAX_QUERY_FILTER_TYPE = 16;

        private readonly AtomicInteger agentId = new AtomicInteger();
        private readonly List<CrowdAgent> m_agents;
        private readonly PathQueue m_pathq;
        private readonly ObstacleAvoidanceQuery.ObstacleAvoidanceParams[] m_obstacleQueryParams = new ObstacleAvoidanceQuery.ObstacleAvoidanceParams[DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS];
        private readonly ObstacleAvoidanceQuery m_obstacleQuery;
        private ProximityGrid m_grid;
        private readonly Vector3f m_ext = new Vector3f();
        private readonly QueryFilter[] m_filters = new QueryFilter[DT_CROWD_MAX_QUERY_FILTER_TYPE];
        private NavMeshQuery navQuery;
        private NavMesh navMesh;
        private readonly CrowdConfig _config;
        private readonly CrowdTelemetry _telemetry = new CrowdTelemetry();
        int m_velocitySampleCount;

        public Crowd(CrowdConfig config, NavMesh nav) :
            this(config, nav, i => new DefaultQueryFilter())
        {
        }

        public Crowd(CrowdConfig config, NavMesh nav, Func<int, QueryFilter> queryFilterFactory)
        {
            _config = config;
            vSet(ref m_ext, config.maxAgentRadius * 2.0f, config.maxAgentRadius * 1.5f, config.maxAgentRadius * 2.0f);

            m_obstacleQuery = new ObstacleAvoidanceQuery(config.maxObstacleAvoidanceCircles, config.maxObstacleAvoidanceSegments);

            for (int i = 0; i < DT_CROWD_MAX_QUERY_FILTER_TYPE; i++)
            {
                m_filters[i] = queryFilterFactory.Invoke(i);
            }

            // Init obstacle query option.
            for (int i = 0; i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS; ++i)
            {
                m_obstacleQueryParams[i] = new ObstacleAvoidanceQuery.ObstacleAvoidanceParams();
            }

            // Allocate temp buffer for merging paths.
            m_pathq = new PathQueue(config);
            m_agents = new List<CrowdAgent>();

            // The navQuery is mostly used for local searches, no need for large node pool.
            navMesh = nav;
            navQuery = new NavMeshQuery(nav);
        }

        public void setNavMesh(NavMesh nav)
        {
            navMesh = nav;
            navQuery = new NavMeshQuery(nav);
        }

        /// Sets the shared avoidance configuration for the specified index.
        /// @param[in] idx The index. [Limits: 0 <= value <
        /// #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]
        /// @param[in] option The new configuration.
        public void setObstacleAvoidanceParams(int idx, ObstacleAvoidanceQuery.ObstacleAvoidanceParams option)
        {
            if (idx >= 0 && idx < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                m_obstacleQueryParams[idx] = new ObstacleAvoidanceQuery.ObstacleAvoidanceParams(option);
            }
        }

        /// Gets the shared avoidance configuration for the specified index.
        /// @param[in] idx The index of the configuration to retreive.
        /// [Limits: 0 <= value < #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]
        /// @return The requested configuration.
        public ObstacleAvoidanceQuery.ObstacleAvoidanceParams getObstacleAvoidanceParams(int idx)
        {
            if (idx >= 0 && idx < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                return m_obstacleQueryParams[idx];
            }

            return null;
        }

        /// Updates the specified agent's configuration.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #getAgentCount()]
        /// @param[in] params The new agent configuration.
        public void updateAgentParameters(CrowdAgent agent, CrowdAgentParams option)
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
        public CrowdAgent addAgent(Vector3f pos, CrowdAgentParams option)
        {
            CrowdAgent ag = new CrowdAgent(agentId.GetAndIncrement());
            m_agents.Add(ag);
            updateAgentParameters(ag, option);

            // Find nearest position on navmesh and place the agent there.
            Result<FindNearestPolyResult> nearestPoly = navQuery.findNearestPoly(pos, m_ext, m_filters[ag.option.queryFilterType]);

            var nearest = nearestPoly.succeeded() ? nearestPoly.result.getNearestPos() : pos;
            long refs = nearestPoly.succeeded() ? nearestPoly.result.getNearestRef() : 0L;
            ag.corridor.reset(refs, nearest);
            ag.boundary.reset();
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
        public void removeAgent(CrowdAgent agent)
        {
            m_agents.Remove(agent);
        }

        private bool requestMoveTargetReplan(CrowdAgent ag, long refs, Vector3f pos)
        {
            ag.setTarget(refs, pos);
            ag.targetReplan = true;
            return true;
        }

        /// Submits a new move request for the specified agent.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #getAgentCount()]
        /// @param[in] ref The position's polygon reference.
        /// @param[in] pos The position within the polygon. [(x, y, z)]
        /// @return True if the request was successfully submitted.
        ///
        /// This method is used when a new target is set.
        ///
        /// The position will be constrained to the surface of the navigation mesh.
        ///
        /// The request will be processed during the next #update().
        public bool requestMoveTarget(CrowdAgent agent, long refs, Vector3f pos)
        {
            if (refs == 0)
            {
                return false;
            }

            // Initialize request.
            agent.setTarget(refs, pos);
            agent.targetReplan = false;
            return true;
        }

        /// Submits a new move request for the specified agent.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #getAgentCount()]
        /// @param[in] vel The movement velocity. [(x, y, z)]
        /// @return True if the request was successfully submitted.
        public bool requestMoveVelocity(CrowdAgent agent, Vector3f vel)
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
        /// @param[in] idx The agent index. [Limits: 0 <= value < #getAgentCount()]
        /// @return True if the request was successfully reseted.
        public bool resetMoveTarget(CrowdAgent agent)
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
        public ICollection<CrowdAgent> getActiveAgents()
        {
            return m_agents;
        }

        public Vector3f getQueryExtents()
        {
            return m_ext;
        }

        public QueryFilter getFilter(int i)
        {
            return i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE ? m_filters[i] : null;
        }

        public ProximityGrid getGrid()
        {
            return m_grid;
        }

        public PathQueue getPathQueue()
        {
            return m_pathq;
        }

        public CrowdTelemetry telemetry()
        {
            return _telemetry;
        }

        public CrowdConfig config()
        {
            return _config;
        }

        public CrowdTelemetry update(float dt, CrowdAgentDebugInfo debug)
        {
            m_velocitySampleCount = 0;

            _telemetry.start();

            ICollection<CrowdAgent> agents = getActiveAgents();

            // Check that all agents still have valid paths.
            checkPathValidity(agents, dt);

            // Update async move request and path finder.
            updateMoveRequest(agents, dt);

            // Optimize path topology.
            updateTopologyOptimization(agents, dt);

            // Register agents to proximity grid.
            buildProximityGrid(agents);

            // Get nearby navmesh segments and agents to collide with.
            buildNeighbours(agents);

            // Find next corner to steer to.
            findCorners(agents, debug);

            // Trigger off-mesh connections (depends on corners).
            triggerOffMeshConnections(agents);

            // Calculate steering.
            calculateSteering(agents);

            // Velocity planning.
            planVelocity(debug, agents);

            // Integrate.
            integrate(dt, agents);

            // Handle collisions.
            handleCollisions(agents);

            moveAgents(agents);

            // Update agents using off-mesh connection.
            updateOffMeshConnections(agents, dt);
            return _telemetry;
        }


        private void checkPathValidity(ICollection<CrowdAgent> agents, float dt)
        {
            _telemetry.start("checkPathValidity");

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
                long agentRef = ag.corridor.getFirstPoly();
                agentPos = ag.npos;
                if (!navQuery.isValidPolyRef(agentRef, m_filters[ag.option.queryFilterType]))
                {
                    // Current location is not valid, try to reposition.
                    // TODO: this can snap agents, how to handle that?
                    Result<FindNearestPolyResult> nearestPoly = navQuery.findNearestPoly(ag.npos, m_ext,
                        m_filters[ag.option.queryFilterType]);
                    agentRef = nearestPoly.succeeded() ? nearestPoly.result.getNearestRef() : 0L;
                    if (nearestPoly.succeeded())
                    {
                        agentPos = nearestPoly.result.getNearestPos();
                    }

                    if (agentRef == 0)
                    {
                        // Could not find location in navmesh, set state to invalid.
                        ag.corridor.reset(0, agentPos);
                        ag.partial = false;
                        ag.boundary.reset();
                        ag.state = CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
                        continue;
                    }

                    // Make sure the first polygon is valid, but leave other valid
                    // polygons in the path so that replanner can adjust the path
                    // better.
                    ag.corridor.fixPathStart(agentRef, agentPos);
                    // ag.corridor.trimInvalidPath(agentRef, agentPos, m_navquery,
                    // &m_filter);
                    ag.boundary.reset();
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
                    if (!navQuery.isValidPolyRef(ag.targetRef, m_filters[ag.option.queryFilterType]))
                    {
                        // Current target is not valid, try to reposition.
                        Result<FindNearestPolyResult> fnp = navQuery.findNearestPoly(ag.targetPos, m_ext,
                            m_filters[ag.option.queryFilterType]);
                        ag.targetRef = fnp.succeeded() ? fnp.result.getNearestRef() : 0L;
                        if (fnp.succeeded())
                        {
                            ag.targetPos = fnp.result.getNearestPos();
                        }

                        replan = true;
                    }

                    if (ag.targetRef == 0)
                    {
                        // Failed to reposition target, fail moverequest.
                        ag.corridor.reset(agentRef, agentPos);
                        ag.partial = false;
                        ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;
                    }
                }

                // If nearby corridor is not valid, replan.
                if (!ag.corridor.isValid(_config.checkLookAhead, navQuery, m_filters[ag.option.queryFilterType]))
                {
                    // Fix current path.
                    // ag.corridor.trimInvalidPath(agentRef, agentPos, m_navquery,
                    // &m_filter);
                    // ag.boundary.reset();
                    replan = true;
                }

                // If the end of the path is near and it is not the requested
                // location, replan.
                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VALID)
                {
                    if (ag.targetReplanTime > _config.targetReplanDelay && ag.corridor.getPathCount() < _config.checkLookAhead
                                                                        && ag.corridor.getLastPoly() != ag.targetRef)
                    {
                        replan = true;
                    }
                }

                // Try to replan path to goal.
                if (replan)
                {
                    if (ag.targetState != MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                    {
                        requestMoveTargetReplan(ag, ag.targetRef, ag.targetPos);
                    }
                }
            }

            _telemetry.stop("checkPathValidity");
        }

        private void updateMoveRequest(ICollection<CrowdAgent> agents, float dt)
        {
            _telemetry.start("updateMoveRequest");

            SortedQueue<CrowdAgent> queue = new SortedQueue<CrowdAgent>((a1, a2) => a2.targetReplanTime.CompareTo(a1.targetReplanTime));

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
                    List<long> path = ag.corridor.getPath();
                    if (0 == path.Count)
                    {
                        throw new ArgumentException("Empty path");
                    }

                    // Quick search towards the goal.
                    navQuery.initSlicedFindPath(path[0], ag.targetRef, ag.npos, ag.targetPos,
                        m_filters[ag.option.queryFilterType], 0);
                    navQuery.updateSlicedFindPath(_config.maxTargetFindPathIterations);
                    Result<List<long>> pathFound;
                    if (ag.targetReplan) // && npath > 10)
                    {
                        // Try to use existing steady path during replan if
                        // possible.
                        pathFound = navQuery.finalizeSlicedFindPathPartial(path);
                    }
                    else
                    {
                        // Try to move towards target when goal changes.
                        pathFound = navQuery.finalizeSlicedFindPath();
                    }

                    List<long> reqPath = pathFound.result;
                    Vector3f reqPos = new Vector3f();
                    if (pathFound.succeeded() && reqPath.Count > 0)
                    {
                        // In progress or succeed.
                        if (reqPath[reqPath.Count - 1] != ag.targetRef)
                        {
                            // Partial path, constrain target position inside the
                            // last polygon.
                            Result<ClosestPointOnPolyResult> cr = navQuery.closestPointOnPoly(reqPath[reqPath.Count - 1],
                                ag.targetPos);
                            if (cr.succeeded())
                            {
                                reqPos = cr.result.getClosest();
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

                    ag.corridor.setCorridor(reqPos, reqPath);
                    ag.boundary.reset();
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
                ag.targetPathQueryResult = m_pathq.request(ag.corridor.getLastPoly(), ag.targetRef, ag.corridor.getTarget(),
                    ag.targetPos, m_filters[ag.option.queryFilterType]);
                if (ag.targetPathQueryResult != null)
                {
                    ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH;
                }
                else
                {
                    _telemetry.recordMaxTimeToEnqueueRequest(ag.targetReplanWaitTime);
                    ag.targetReplanWaitTime += dt;
                }
            }

            // Update requests.
            _telemetry.start("pathQueueUpdate");
            m_pathq.update(navMesh);
            _telemetry.stop("pathQueueUpdate");

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
                    // _telemetry.recordPathWaitTime(ag.targetReplanTime);
                    // Poll path queue.
                    Status status = ag.targetPathQueryResult.status;
                    if (status != null && status.isFailed())
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
                    else if (status != null && status.isSuccess())
                    {
                        List<long> path = ag.corridor.getPath();
                        if (0 == path.Count)
                        {
                            throw new ArgumentException("Empty path");
                        }

                        // Apply results.
                        var targetPos = ag.targetPos;

                        bool valid = true;
                        List<long> res = ag.targetPathQueryResult.path;
                        if (status.isFailed() || 0 == res.Count)
                        {
                            valid = false;
                        }

                        if (status.isPartial())
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
                                Result<ClosestPointOnPolyResult> cr = navQuery.closestPointOnPoly(res[res.Count - 1], targetPos);
                                if (cr.succeeded())
                                {
                                    targetPos = cr.result.getClosest();
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
                            ag.corridor.setCorridor(targetPos, res);
                            // Force to update boundary.
                            ag.boundary.reset();
                            ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_VALID;
                        }
                        else
                        {
                            // Something went wrong.
                            ag.targetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                        }

                        ag.targetReplanTime = 0;
                    }

                    _telemetry.recordMaxTimeToFindPath(ag.targetReplanWaitTime);
                    ag.targetReplanWaitTime += dt;
                }
            }

            _telemetry.stop("updateMoveRequest");
        }

        private void updateTopologyOptimization(ICollection<CrowdAgent> agents, float dt)
        {
            _telemetry.start("updateTopologyOptimization");

            SortedQueue<CrowdAgent> queue = new SortedQueue<CrowdAgent>((a1, a2) => a2.topologyOptTime.CompareTo(a1.topologyOptTime));

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
                ag.corridor.optimizePathTopology(navQuery, m_filters[ag.option.queryFilterType], _config.maxTopologyOptimizationIterations);
                ag.topologyOptTime = 0;
            }

            _telemetry.stop("updateTopologyOptimization");
        }

        private void buildProximityGrid(ICollection<CrowdAgent> agents)
        {
            _telemetry.start("buildProximityGrid");
            m_grid = new ProximityGrid(_config.maxAgentRadius * 3);
            foreach (CrowdAgent ag in agents)
            {
                Vector3f p = ag.npos;
                float r = ag.option.radius;
                m_grid.addItem(ag, p[0] - r, p[2] - r, p[0] + r, p[2] + r);
            }

            _telemetry.stop("buildProximityGrid");
        }

        private void buildNeighbours(ICollection<CrowdAgent> agents)
        {
            _telemetry.start("buildNeighbours");
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Update the collision boundary after certain distance has been passed or
                // if it has become invalid.
                float updateThr = ag.option.collisionQueryRange * 0.25f;
                if (vDist2DSqr(ag.npos, ag.boundary.getCenter()) > sqr(updateThr)
                    || !ag.boundary.isValid(navQuery, m_filters[ag.option.queryFilterType]))
                {
                    ag.boundary.update(ag.corridor.getFirstPoly(), ag.npos, ag.option.collisionQueryRange, navQuery,
                        m_filters[ag.option.queryFilterType]);
                }

                // Query neighbour agents
                ag.neis = getNeighbours(ag.npos, ag.option.height, ag.option.collisionQueryRange, ag, m_grid);
            }

            _telemetry.stop("buildNeighbours");
        }

        private List<CrowdNeighbour> getNeighbours(Vector3f pos, float height, float range, CrowdAgent skip, ProximityGrid grid)
        {
            List<CrowdNeighbour> result = new List<CrowdNeighbour>();
            HashSet<CrowdAgent> proxAgents = grid.queryItems(pos[0] - range, pos[2] - range, pos[0] + range, pos[2] + range);

            foreach (CrowdAgent ag in proxAgents)
            {
                if (ag == skip)
                {
                    continue;
                }

                // Check for overlap.
                Vector3f diff = vSub(pos, ag.npos);
                if (Math.Abs(diff[1]) >= (height + ag.option.height) / 2.0f)
                {
                    continue;
                }

                diff[1] = 0;
                float distSqr = vLenSqr(diff);
                if (distSqr > sqr(range))
                {
                    continue;
                }

                result.Add(new CrowdNeighbour(ag, distSqr));
            }

            result.Sort((o1, o2) => o1.dist.CompareTo(o2.dist));
            return result;
        }

        private void findCorners(ICollection<CrowdAgent> agents, CrowdAgentDebugInfo debug)
        {
            _telemetry.start("findCorners");
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
                ag.corners = ag.corridor.findCorners(DT_CROWDAGENT_MAX_CORNERS, navQuery, m_filters[ag.option.queryFilterType]);

                // Check to see if the corner after the next corner is directly visible,
                // and short cut to there.
                if ((ag.option.updateFlags & CrowdAgentParams.DT_CROWD_OPTIMIZE_VIS) != 0 && ag.corners.Count > 0)
                {
                    Vector3f target = ag.corners[Math.Min(1, ag.corners.Count - 1)].getPos();
                    ag.corridor.optimizePathVisibility(target, ag.option.pathOptimizationRange, navQuery,
                        m_filters[ag.option.queryFilterType]);

                    // Copy data for debug purposes.
                    if (debugAgent == ag)
                    {
                        debug.optStart = ag.corridor.getPos();
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

            _telemetry.stop("findCorners");
        }

        private void triggerOffMeshConnections(ICollection<CrowdAgent> agents)
        {
            _telemetry.start("triggerOffMeshConnections");
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
                if (ag.overOffmeshConnection(triggerRadius))
                {
                    // Prepare to off-mesh connection.
                    CrowdAgentAnimation anim = ag.animation;

                    // Adjust the path over the off-mesh connection.
                    long[] refs = new long[2];
                    if (ag.corridor.moveOverOffmeshConnection(ag.corners[ag.corners.Count - 1].getRef(), refs, ref anim.startPos,
                            ref anim.endPos, navQuery))
                    {
                        anim.initPos = ag.npos;
                        anim.polyRef = refs[1];
                        anim.active = true;
                        anim.t = 0.0f;
                        anim.tmax = (vDist2D(anim.startPos, anim.endPos) / ag.option.maxSpeed) * 0.5f;

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

            _telemetry.stop("triggerOffMeshConnections");
        }

        private void calculateSteering(ICollection<CrowdAgent> agents)
        {
            _telemetry.start("calculateSteering");
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
                    ag.desiredSpeed = vLen(ag.targetPos);
                }
                else
                {
                    // Calculate steering direction.
                    if ((ag.option.updateFlags & CrowdAgentParams.DT_CROWD_ANTICIPATE_TURNS) != 0)
                    {
                        dvel = ag.calcSmoothSteerDirection();
                    }
                    else
                    {
                        dvel = ag.calcStraightSteerDirection();
                    }

                    // Calculate speed scale, which tells the agent to slowdown at the end of the path.
                    float slowDownRadius = ag.option.radius * 2; // TODO: make less hacky.
                    float speedScale = ag.getDistanceToGoal(slowDownRadius) / slowDownRadius;

                    ag.desiredSpeed = ag.option.maxSpeed;
                    dvel = vScale(dvel, ag.desiredSpeed * speedScale);
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

                        Vector3f diff = vSub(ag.npos, nei.npos);
                        diff[1] = 0;

                        float distSqr = vLenSqr(diff);
                        if (distSqr < 0.00001f)
                        {
                            continue;
                        }

                        if (distSqr > sqr(separationDist))
                        {
                            continue;
                        }

                        float dist = (float)Math.Sqrt(distSqr);
                        float weight = separationWeight * (1.0f - sqr(dist * invSeparationDist));

                        disp = vMad(disp, diff, weight / dist);
                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        // Adjust desired velocity.
                        dvel = vMad(dvel, disp, 1.0f / w);
                        // Clamp desired velocity to desired speed.
                        float speedSqr = vLenSqr(dvel);
                        float desiredSqr = sqr(ag.desiredSpeed);
                        if (speedSqr > desiredSqr)
                        {
                            dvel = vScale(dvel, desiredSqr / speedSqr);
                        }
                    }
                }

                // Set the desired velocity.
                ag.dvel = dvel;
            }

            _telemetry.stop("calculateSteering");
        }

        private void planVelocity(CrowdAgentDebugInfo debug, ICollection<CrowdAgent> agents)
        {
            _telemetry.start("planVelocity");
            CrowdAgent debugAgent = debug != null ? debug.agent : null;
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if ((ag.option.updateFlags & CrowdAgentParams.DT_CROWD_OBSTACLE_AVOIDANCE) != 0)
                {
                    m_obstacleQuery.reset();

                    // Add neighbours as obstacles.
                    for (int j = 0; j < ag.neis.Count; ++j)
                    {
                        CrowdAgent nei = ag.neis[j].agent;
                        m_obstacleQuery.addCircle(nei.npos, nei.option.radius, nei.vel, nei.dvel);
                    }

                    // Append neighbour segments as obstacles.
                    for (int j = 0; j < ag.boundary.getSegmentCount(); ++j)
                    {
                        Vector3f[] s = ag.boundary.getSegment(j);
                        Vector3f s3 = s[1];
                        //Array.Copy(s, 3, s3, 0, 3);
                        if (triArea2D(ag.npos, s[0], s3) < 0.0f)
                        {
                            continue;
                        }

                        m_obstacleQuery.addSegment(s[0], s3);
                    }

                    ObstacleAvoidanceDebugData vod = null;
                    if (debugAgent == ag)
                    {
                        vod = debug.vod;
                    }

                    // Sample new safe velocity.
                    bool adaptive = true;
                    int ns = 0;

                    ObstacleAvoidanceQuery.ObstacleAvoidanceParams option = m_obstacleQueryParams[ag.option.obstacleAvoidanceType];

                    if (adaptive)
                    {
                        var nsnvel = m_obstacleQuery.sampleVelocityAdaptive(ag.npos, ag.option.radius,
                            ag.desiredSpeed, ag.vel, ag.dvel, option, vod);
                        ns = nsnvel.Item1;
                        ag.nvel = nsnvel.Item2;
                    }
                    else
                    {
                        var nsnvel = m_obstacleQuery.sampleVelocityGrid(ag.npos, ag.option.radius,
                            ag.desiredSpeed, ag.vel, ag.dvel, option, vod);
                        ns = nsnvel.Item1;
                        ag.nvel = nsnvel.Item2;
                    }

                    m_velocitySampleCount += ns;
                }
                else
                {
                    // If not using velocity planning, new velocity is directly the desired velocity.
                    ag.nvel = ag.dvel;
                }
            }

            _telemetry.stop("planVelocity");
        }

        private void integrate(float dt, ICollection<CrowdAgent> agents)
        {
            _telemetry.start("integrate");
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                ag.integrate(dt);
            }

            _telemetry.stop("integrate");
        }

        private void handleCollisions(ICollection<CrowdAgent> agents)
        {
            _telemetry.start("handleCollisions");
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
                        Vector3f diff = vSub(ag.npos, nei.npos);
                        diff[1] = 0;

                        float dist = vLenSqr(diff);
                        if (dist > sqr(ag.option.radius + nei.option.radius))
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
                                vSet(ref diff, -ag.dvel[2], 0, ag.dvel[0]);
                            }
                            else
                            {
                                vSet(ref diff, ag.dvel[2], 0, -ag.dvel[0]);
                            }

                            pen = 0.01f;
                        }
                        else
                        {
                            pen = (1.0f / dist) * (pen * 0.5f) * _config.collisionResolveFactor;
                        }

                        ag.disp = vMad(ag.disp, diff, pen);

                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        float iw = 1.0f / w;
                        ag.disp = vScale(ag.disp, iw);
                    }
                }

                foreach (CrowdAgent ag in agents)
                {
                    if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                    {
                        continue;
                    }

                    ag.npos = vAdd(ag.npos, ag.disp);
                }
            }

            _telemetry.stop("handleCollisions");
        }

        private void moveAgents(ICollection<CrowdAgent> agents)
        {
            _telemetry.start("moveAgents");
            foreach (CrowdAgent ag in agents)
            {
                if (ag.state != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Move along navmesh.
                ag.corridor.movePosition(ag.npos, navQuery, m_filters[ag.option.queryFilterType]);
                // Get valid constrained position back.
                ag.npos = ag.corridor.getPos();

                // If not using path, truncate the corridor to just one poly.
                if (ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    ag.corridor.reset(ag.corridor.getFirstPoly(), ag.npos);
                    ag.partial = false;
                }
            }

            _telemetry.stop("moveAgents");
        }

        private void updateOffMeshConnections(ICollection<CrowdAgent> agents, float dt)
        {
            _telemetry.start("updateOffMeshConnections");
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
                    float u = tween(anim.t, 0.0f, ta);
                    ag.npos = vLerp(anim.initPos, anim.startPos, u);
                }
                else
                {
                    float u = tween(anim.t, ta, tb);
                    ag.npos = vLerp(anim.startPos, anim.endPos, u);
                }

                // Update velocity.
                ag.vel = Vector3f.Zero;
                ag.dvel = Vector3f.Zero;
            }

            _telemetry.stop("updateOffMeshConnections");
        }

        private float tween(float t, float t0, float t1)
        {
            return clamp((t - t0) / (t1 - t0), 0.0f, 1.0f);
        }

        /// Provides neighbor data for agents managed by the crowd.
        /// @ingroup crowd
        /// @see dtCrowdAgent::neis, dtCrowd
        public class CrowdNeighbour
        {
            public readonly CrowdAgent agent;

            /// < The index of the neighbor in the crowd.
            public readonly float dist;

            /// < The distance between the current agent and the neighbor.
            public CrowdNeighbour(CrowdAgent agent, float dist)
            {
                this.agent = agent;
                this.dist = dist;
            }
        };
    }
}