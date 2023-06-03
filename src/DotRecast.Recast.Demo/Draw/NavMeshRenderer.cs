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
using DotRecast.Detour;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Geom;
using DotRecast.Recast.Demo.UI;

namespace DotRecast.Recast.Demo.Draw;

public class NavMeshRenderer
{
    private readonly RecastDebugDraw debugDraw;

    private readonly int navMeshDrawFlags = RecastDebugDraw.DRAWNAVMESH_OFFMESHCONS
                                            | RecastDebugDraw.DRAWNAVMESH_CLOSEDLIST;

    public NavMeshRenderer(RecastDebugDraw debugDraw)
    {
        this.debugDraw = debugDraw;
    }

    public RecastDebugDraw GetDebugDraw()
    {
        return debugDraw;
    }

    public void Render(Sample sample)
    {
        if (sample == null)
        {
            return;
        }

        NavMeshQuery navQuery = sample.GetNavMeshQuery();
        DemoInputGeomProvider geom = sample.GetInputGeom();
        IList<RecastBuilderResult> rcBuilderResults = sample.GetRecastResults();
        NavMesh navMesh = sample.GetNavMesh();
        RcSettingsView rcSettingsView = sample.GetSettingsUI();
        debugDraw.Fog(true);
        debugDraw.DepthMask(true);
        var drawMode = rcSettingsView.GetDrawMode();

        float texScale = 1.0f / (rcSettingsView.GetCellSize() * 10.0f);
        float m_agentMaxSlope = rcSettingsView.GetAgentMaxSlope();

        if (drawMode != DrawMode.DRAWMODE_NAVMESH_TRANS)
        {
            // Draw mesh
            if (geom != null)
            {
                debugDraw.DebugDrawTriMeshSlope(geom.vertices, geom.faces, geom.normals, m_agentMaxSlope, texScale);
                DrawOffMeshConnections(geom, false);
            }
        }

        debugDraw.Fog(false);
        debugDraw.DepthMask(false);
        if (geom != null)
        {
            DrawGeomBounds(geom);
        }

        if (navMesh != null && navQuery != null
                            && (drawMode == DrawMode.DRAWMODE_NAVMESH || drawMode == DrawMode.DRAWMODE_NAVMESH_TRANS
                                                                      || drawMode == DrawMode.DRAWMODE_NAVMESH_BVTREE || drawMode == DrawMode.DRAWMODE_NAVMESH_NODES
                                                                      || drawMode == DrawMode.DRAWMODE_NAVMESH_INVIS
                                                                      || drawMode == DrawMode.DRAWMODE_NAVMESH_PORTALS))
        {
            if (drawMode != DrawMode.DRAWMODE_NAVMESH_INVIS)
            {
                debugDraw.DebugDrawNavMeshWithClosedList(navMesh, navQuery, navMeshDrawFlags);
            }

            if (drawMode == DrawMode.DRAWMODE_NAVMESH_BVTREE)
            {
                debugDraw.DebugDrawNavMeshBVTree(navMesh);
            }

            if (drawMode == DrawMode.DRAWMODE_NAVMESH_PORTALS)
            {
                debugDraw.DebugDrawNavMeshPortals(navMesh);
            }

            if (drawMode == DrawMode.DRAWMODE_NAVMESH_NODES)
            {
                debugDraw.DebugDrawNavMeshNodes(navQuery);
                debugDraw.DebugDrawNavMeshPolysWithFlags(navMesh, SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED,
                    DebugDraw.DuRGBA(0, 0, 0, 128));
            }
        }

        debugDraw.DepthMask(true);

        foreach (RecastBuilderResult rcBuilderResult in rcBuilderResults)
        {
            if (rcBuilderResult.GetCompactHeightfield() != null && drawMode == DrawMode.DRAWMODE_COMPACT)
            {
                debugDraw.DebugDrawCompactHeightfieldSolid(rcBuilderResult.GetCompactHeightfield());
            }

            if (rcBuilderResult.GetCompactHeightfield() != null && drawMode == DrawMode.DRAWMODE_COMPACT_DISTANCE)
            {
                debugDraw.DebugDrawCompactHeightfieldDistance(rcBuilderResult.GetCompactHeightfield());
            }

            if (rcBuilderResult.GetCompactHeightfield() != null && drawMode == DrawMode.DRAWMODE_COMPACT_REGIONS)
            {
                debugDraw.DebugDrawCompactHeightfieldRegions(rcBuilderResult.GetCompactHeightfield());
            }

            if (rcBuilderResult.GetSolidHeightfield() != null && drawMode == DrawMode.DRAWMODE_VOXELS)
            {
                debugDraw.Fog(true);
                debugDraw.DebugDrawHeightfieldSolid(rcBuilderResult.GetSolidHeightfield());
                debugDraw.Fog(false);
            }

            if (rcBuilderResult.GetSolidHeightfield() != null && drawMode == DrawMode.DRAWMODE_VOXELS_WALKABLE)
            {
                debugDraw.Fog(true);
                debugDraw.DebugDrawHeightfieldWalkable(rcBuilderResult.GetSolidHeightfield());
                debugDraw.Fog(false);
            }

            if (rcBuilderResult.GetContourSet() != null && drawMode == DrawMode.DRAWMODE_RAW_CONTOURS)
            {
                debugDraw.DepthMask(false);
                debugDraw.DebugDrawRawContours(rcBuilderResult.GetContourSet(), 1f);
                debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetContourSet() != null && drawMode == DrawMode.DRAWMODE_BOTH_CONTOURS)
            {
                debugDraw.DepthMask(false);
                debugDraw.DebugDrawRawContours(rcBuilderResult.GetContourSet(), 0.5f);
                debugDraw.DebugDrawContours(rcBuilderResult.GetContourSet());
                debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetContourSet() != null && drawMode == DrawMode.DRAWMODE_CONTOURS)
            {
                debugDraw.DepthMask(false);
                debugDraw.DebugDrawContours(rcBuilderResult.GetContourSet());
                debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetCompactHeightfield() != null && drawMode == DrawMode.DRAWMODE_REGION_CONNECTIONS)
            {
                debugDraw.DebugDrawCompactHeightfieldRegions(rcBuilderResult.GetCompactHeightfield());
                debugDraw.DepthMask(false);
                if (rcBuilderResult.GetContourSet() != null)
                {
                    debugDraw.DebugDrawRegionConnections(rcBuilderResult.GetContourSet());
                }

                debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetMesh() != null && drawMode == DrawMode.DRAWMODE_POLYMESH)
            {
                debugDraw.DepthMask(false);
                debugDraw.DebugDrawPolyMesh(rcBuilderResult.GetMesh());
                debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetMeshDetail() != null && drawMode == DrawMode.DRAWMODE_POLYMESH_DETAIL)
            {
                debugDraw.DepthMask(false);
                debugDraw.DebugDrawPolyMeshDetail(rcBuilderResult.GetMeshDetail());
                debugDraw.DepthMask(true);
            }
        }

        if (geom != null)
        {
            DrawConvexVolumes(geom);
        }
    }

    private void DrawGeomBounds(DemoInputGeomProvider geom)
    {
        // Draw bounds
        RcVec3f bmin = geom.GetMeshBoundsMin();
        RcVec3f bmax = geom.GetMeshBoundsMax();
        debugDraw.DebugDrawBoxWire(bmin.x, bmin.y, bmin.z, bmax.x, bmax.y, bmax.z,
            DebugDraw.DuRGBA(255, 255, 255, 128), 1.0f);
        debugDraw.Begin(DebugDrawPrimitives.POINTS, 5.0f);
        debugDraw.Vertex(bmin.x, bmin.y, bmin.z, DebugDraw.DuRGBA(255, 255, 255, 128));
        debugDraw.End();
    }

    public void DrawOffMeshConnections(DemoInputGeomProvider geom, bool hilight)
    {
        int conColor = DebugDraw.DuRGBA(192, 0, 128, 192);
        int baseColor = DebugDraw.DuRGBA(0, 0, 0, 64);
        debugDraw.DepthMask(false);

        debugDraw.Begin(DebugDrawPrimitives.LINES, 2.0f);
        foreach (DemoOffMeshConnection con in geom.GetOffMeshConnections())
        {
            float[] v = con.verts;
            debugDraw.Vertex(v[0], v[1], v[2], baseColor);
            debugDraw.Vertex(v[0], v[1] + 0.2f, v[2], baseColor);

            debugDraw.Vertex(v[3], v[4], v[5], baseColor);
            debugDraw.Vertex(v[3], v[4] + 0.2f, v[5], baseColor);

            debugDraw.AppendCircle(v[0], v[1] + 0.1f, v[2], con.radius, baseColor);
            debugDraw.AppendCircle(v[3], v[4] + 0.1f, v[5], con.radius, baseColor);

            if (hilight)
            {
                debugDraw.AppendArc(v[0], v[1], v[2], v[3], v[4], v[5], 0.25f, con.bidir ? 0.6f : 0.0f, 0.6f, conColor);
            }
        }

        debugDraw.End();

        debugDraw.DepthMask(true);
    }

    void DrawConvexVolumes(DemoInputGeomProvider geom)
    {
        debugDraw.DepthMask(false);

        debugDraw.Begin(DebugDrawPrimitives.TRIS);

        foreach (ConvexVolume vol in geom.ConvexVolumes())
        {
            int col = DebugDraw.DuTransCol(DebugDraw.AreaToCol(vol.areaMod.GetMaskedValue()), 32);
            for (int j = 0, k = vol.verts.Length - 3; j < vol.verts.Length; k = j, j += 3)
            {
                var va = RcVec3f.Of(vol.verts[k], vol.verts[k + 1], vol.verts[k + 2]);
                var vb = RcVec3f.Of(vol.verts[j], vol.verts[j + 1], vol.verts[j + 2]);

                debugDraw.Vertex(vol.verts[0], vol.hmax, vol.verts[2], col);
                debugDraw.Vertex(vb.x, vol.hmax, vb.z, col);
                debugDraw.Vertex(va.x, vol.hmax, va.z, col);

                debugDraw.Vertex(va.x, vol.hmin, va.z, DebugDraw.DuDarkenCol(col));
                debugDraw.Vertex(va.x, vol.hmax, va.z, col);
                debugDraw.Vertex(vb.x, vol.hmax, vb.z, col);

                debugDraw.Vertex(va.x, vol.hmin, va.z, DebugDraw.DuDarkenCol(col));
                debugDraw.Vertex(vb.x, vol.hmax, vb.z, col);
                debugDraw.Vertex(vb.x, vol.hmin, vb.z, DebugDraw.DuDarkenCol(col));
            }
        }

        debugDraw.End();

        debugDraw.Begin(DebugDrawPrimitives.LINES, 2.0f);
        foreach (ConvexVolume vol in geom.ConvexVolumes())
        {
            int col = DebugDraw.DuTransCol(DebugDraw.AreaToCol(vol.areaMod.GetMaskedValue()), 220);
            for (int j = 0, k = vol.verts.Length - 3; j < vol.verts.Length; k = j, j += 3)
            {
                var va = RcVec3f.Of(vol.verts[k], vol.verts[k + 1], vol.verts[k + 2]);
                var vb = RcVec3f.Of(vol.verts[j], vol.verts[j + 1], vol.verts[j + 2]);
                debugDraw.Vertex(va.x, vol.hmin, va.z, DebugDraw.DuDarkenCol(col));
                debugDraw.Vertex(vb.x, vol.hmin, vb.z, DebugDraw.DuDarkenCol(col));
                debugDraw.Vertex(va.x, vol.hmax, va.z, col);
                debugDraw.Vertex(vb.x, vol.hmax, vb.z, col);
                debugDraw.Vertex(va.x, vol.hmin, va.z, DebugDraw.DuDarkenCol(col));
                debugDraw.Vertex(va.x, vol.hmax, va.z, col);
            }
        }

        debugDraw.End();

        debugDraw.Begin(DebugDrawPrimitives.POINTS, 3.0f);
        foreach (ConvexVolume vol in geom.ConvexVolumes())
        {
            int col = DebugDraw.DuDarkenCol(DebugDraw.DuTransCol(DebugDraw.AreaToCol(vol.areaMod.GetMaskedValue()), 220));
            for (int j = 0; j < vol.verts.Length; j += 3)
            {
                debugDraw.Vertex(vol.verts[j + 0], vol.verts[j + 1] + 0.1f, vol.verts[j + 2], col);
                debugDraw.Vertex(vol.verts[j + 0], vol.hmin, vol.verts[j + 2], col);
                debugDraw.Vertex(vol.verts[j + 0], vol.hmax, vol.verts[j + 2], col);
            }
        }

        debugDraw.End();

        debugDraw.DepthMask(true);
    }
}
