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
using DotRecast.Detour.QueryResults;
using NUnit.Framework;

namespace DotRecast.Detour.Crowd.Test;

using static DotRecast.Core.RecastMath;

[Parallelizable]
public class AbstractCrowdTest
{
    protected readonly long[] startRefs =
    {
        281474976710696L, 281474976710773L, 281474976710680L, 281474976710753L,
        281474976710733L
    };

    protected readonly long[] endRefs = { 281474976710721L, 281474976710767L, 281474976710758L, 281474976710731L, 281474976710772L };

    protected readonly Vector3f[] startPoss =
    {
        Vector3f.Of(22.60652f, 10.197294f, -45.918674f),
        Vector3f.Of(22.331268f, 10.197294f, -1.0401875f),
        Vector3f.Of(18.694363f, 15.803535f, -73.090416f),
        Vector3f.Of(0.7453353f, 10.197294f, -5.94005f),
        Vector3f.Of(-20.651257f, 5.904126f, -13.712508f),
    };

    protected readonly Vector3f[] endPoss =
    {
        Vector3f.Of(6.4576626f, 10.197294f, -18.33406f),
        Vector3f.Of(-5.8023443f, 0.19729415f, 3.008419f),
        Vector3f.Of(38.423977f, 10.197294f, -0.116066754f),
        Vector3f.Of(0.8635526f, 10.197294f, -10.31032f),
        Vector3f.Of(18.784092f, 10.197294f, 3.0543678f),
    };

    protected MeshData nmd;
    protected NavMeshQuery query;
    protected NavMesh navmesh;
    protected Crowd crowd;
    protected List<CrowdAgent> agents;

    [SetUp]
    public void setUp()
    {
        nmd = new RecastTestMeshBuilder().getMeshData();
        navmesh = new NavMesh(nmd, 6, 0);
        query = new NavMeshQuery(navmesh);
        CrowdConfig config = new CrowdConfig(0.6f);
        crowd = new Crowd(config, navmesh);
        ObstacleAvoidanceParams option = new ObstacleAvoidanceParams();
        option.velBias = 0.5f;
        option.adaptiveDivs = 5;
        option.adaptiveRings = 2;
        option.adaptiveDepth = 1;
        crowd.setObstacleAvoidanceParams(0, option);
        option = new ObstacleAvoidanceParams();
        option.velBias = 0.5f;
        option.adaptiveDivs = 5;
        option.adaptiveRings = 2;
        option.adaptiveDepth = 2;
        crowd.setObstacleAvoidanceParams(1, option);
        option = new ObstacleAvoidanceParams();
        option.velBias = 0.5f;
        option.adaptiveDivs = 7;
        option.adaptiveRings = 2;
        option.adaptiveDepth = 3;
        crowd.setObstacleAvoidanceParams(2, option);
        option = new ObstacleAvoidanceParams();
        option.velBias = 0.5f;
        option.adaptiveDivs = 7;
        option.adaptiveRings = 3;
        option.adaptiveDepth = 3;
        crowd.setObstacleAvoidanceParams(3, option);
        agents = new();
    }

    protected CrowdAgentParams getAgentParams(int updateFlags, int obstacleAvoidanceType)
    {
        CrowdAgentParams ap = new CrowdAgentParams();
        ap.radius = 0.6f;
        ap.height = 2f;
        ap.maxAcceleration = 8.0f;
        ap.maxSpeed = 3.5f;
        ap.collisionQueryRange = ap.radius * 12f;
        ap.pathOptimizationRange = ap.radius * 30f;
        ap.updateFlags = updateFlags;
        ap.obstacleAvoidanceType = obstacleAvoidanceType;
        ap.separationWeight = 2f;
        return ap;
    }

    protected void addAgentGrid(int size, float distance, int updateFlags, int obstacleAvoidanceType, Vector3f startPos)
    {
        CrowdAgentParams ap = getAgentParams(updateFlags, obstacleAvoidanceType);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Vector3f pos = new Vector3f();
                pos.x = startPos.x + i * distance;
                pos.y = startPos.y;
                pos.z = startPos.z + j * distance;
                agents.Add(crowd.addAgent(pos, ap));
            }
        }
    }

    protected void setMoveTarget(Vector3f pos, bool adjust)
    {
        Vector3f ext = crowd.getQueryExtents();
        QueryFilter filter = crowd.getFilter(0);
        if (adjust)
        {
            foreach (CrowdAgent ag in crowd.getActiveAgents())
            {
                Vector3f vel = calcVel(ag.npos, pos, ag.option.maxSpeed);
                crowd.requestMoveVelocity(ag, vel);
            }
        }
        else
        {
            Result<FindNearestPolyResult> nearest = query.findNearestPoly(pos, ext, filter);
            foreach (CrowdAgent ag in crowd.getActiveAgents())
            {
                crowd.requestMoveTarget(ag, nearest.result.getNearestRef(), nearest.result.getNearestPos());
            }
        }
    }

    protected Vector3f calcVel(Vector3f pos, Vector3f tgt, float speed)
    {
        Vector3f vel = vSub(tgt, pos);
        vel.y = 0.0f;
        vNormalize(ref vel);
        vel = vScale(vel, speed);
        return vel;
    }

    protected void dumpActiveAgents(int i)
    {
        Console.WriteLine(crowd.getActiveAgents().Count);
        foreach (CrowdAgent ag in crowd.getActiveAgents())
        {
            Console.WriteLine(ag.state + ", " + ag.targetState);
            Console.WriteLine(ag.npos.x + ", " + ag.npos.y + ", " + ag.npos.z);
            Console.WriteLine(ag.nvel.x + ", " + ag.nvel.y + ", " + ag.nvel.z);
        }
    }
}