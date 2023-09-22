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
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Toolset.Geom;
using static DotRecast.Recast.RcCommons;

namespace DotRecast.Recast.Demo.Draw;

public class NavMeshRenderer
{
    private readonly RecastDebugDraw _debugDraw;
    private readonly int _navMeshDrawFlags = RecastDebugDraw.DRAWNAVMESH_OFFMESHCONS | RecastDebugDraw.DRAWNAVMESH_CLOSEDLIST;

    public NavMeshRenderer(RecastDebugDraw debugDraw)
    {
        _debugDraw = debugDraw;
    }

    public RecastDebugDraw GetDebugDraw()
    {
        return _debugDraw;
    }

    public void Render(DemoSample sample, DrawMode drawMode)
    {
        if (sample == null)
        {
            return;
        }

        DtNavMeshQuery navQuery = sample.GetNavMeshQuery();
        DemoInputGeomProvider geom = sample.GetInputGeom();
        IList<RcBuilderResult> rcBuilderResults = sample.GetRecastResults();
        DtNavMesh navMesh = sample.GetNavMesh();
        var settings = sample.GetSettings();
        _debugDraw.Fog(true);
        _debugDraw.DepthMask(true);

        float texScale = 1.0f / (settings.cellSize * 10.0f);
        float agentMaxSlope = settings.agentMaxSlope;

        if (drawMode != DrawMode.DRAWMODE_NAVMESH_TRANS)
        {
            // Draw mesh
            if (geom != null)
            {
                _debugDraw.DebugDrawTriMeshSlope(geom.vertices, geom.faces, geom.normals, agentMaxSlope, texScale);
                DrawOffMeshConnections(geom, false);
            }
        }

        _debugDraw.Fog(false);
        _debugDraw.DepthMask(false);
        if (geom != null)
        {
            DrawGeomBounds(geom);
        }

        if (geom != null)
        {
            int gw = 0, gh = 0;
            RcVec3f bmin = geom.GetMeshBoundsMin();
            RcVec3f bmax = geom.GetMeshBoundsMax();
            CalcGridSize(bmin, bmax, settings.cellSize, out gw, out gh);
            int tw = (gw + settings.tileSize - 1) / settings.tileSize;
            int th = (gh + settings.tileSize - 1) / settings.tileSize;
            float s = settings.tileSize * settings.cellSize;
            _debugDraw.DebugDrawGridXZ(bmin[0], bmin[1], bmin[2], tw, th, s, DebugDraw.DuRGBA(0, 0, 0, 64), 1.0f);
        }

        if (navMesh != null && navQuery != null
                            && (drawMode == DrawMode.DRAWMODE_NAVMESH
                                || drawMode == DrawMode.DRAWMODE_NAVMESH_TRANS
                                || drawMode == DrawMode.DRAWMODE_NAVMESH_BVTREE
                                || drawMode == DrawMode.DRAWMODE_NAVMESH_NODES
                                || drawMode == DrawMode.DRAWMODE_NAVMESH_INVIS
                                || drawMode == DrawMode.DRAWMODE_NAVMESH_PORTALS))
        {
            if (drawMode != DrawMode.DRAWMODE_NAVMESH_INVIS)
            {
                _debugDraw.DebugDrawNavMeshWithClosedList(navMesh, navQuery, _navMeshDrawFlags);
            }

            if (drawMode == DrawMode.DRAWMODE_NAVMESH_BVTREE)
            {
                _debugDraw.DebugDrawNavMeshBVTree(navMesh);
            }

            if (drawMode == DrawMode.DRAWMODE_NAVMESH_PORTALS)
            {
                _debugDraw.DebugDrawNavMeshPortals(navMesh);
            }

            if (drawMode == DrawMode.DRAWMODE_NAVMESH_NODES)
            {
                _debugDraw.DebugDrawNavMeshNodes(navQuery);
                _debugDraw.DebugDrawNavMeshPolysWithFlags(navMesh, SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED, DebugDraw.DuRGBA(0, 0, 0, 128));
            }
        }

        _debugDraw.DepthMask(true);

        foreach (RcBuilderResult rcBuilderResult in rcBuilderResults)
        {
            if (rcBuilderResult.GetCompactHeightfield() != null && drawMode == DrawMode.DRAWMODE_COMPACT)
            {
                _debugDraw.DebugDrawCompactHeightfieldSolid(rcBuilderResult.GetCompactHeightfield());
            }

            if (rcBuilderResult.GetCompactHeightfield() != null && drawMode == DrawMode.DRAWMODE_COMPACT_DISTANCE)
            {
                _debugDraw.DebugDrawCompactHeightfieldDistance(rcBuilderResult.GetCompactHeightfield());
            }

            if (rcBuilderResult.GetCompactHeightfield() != null && drawMode == DrawMode.DRAWMODE_COMPACT_REGIONS)
            {
                _debugDraw.DebugDrawCompactHeightfieldRegions(rcBuilderResult.GetCompactHeightfield());
            }

            if (rcBuilderResult.GetSolidHeightfield() != null && drawMode == DrawMode.DRAWMODE_VOXELS)
            {
                _debugDraw.Fog(true);
                _debugDraw.DebugDrawHeightfieldSolid(rcBuilderResult.GetSolidHeightfield());
                _debugDraw.Fog(false);
            }

            if (rcBuilderResult.GetSolidHeightfield() != null && drawMode == DrawMode.DRAWMODE_VOXELS_WALKABLE)
            {
                _debugDraw.Fog(true);
                _debugDraw.DebugDrawHeightfieldWalkable(rcBuilderResult.GetSolidHeightfield());
                _debugDraw.Fog(false);
            }

            if (rcBuilderResult.GetContourSet() != null && drawMode == DrawMode.DRAWMODE_RAW_CONTOURS)
            {
                _debugDraw.DepthMask(false);
                _debugDraw.DebugDrawRawContours(rcBuilderResult.GetContourSet(), 1f);
                _debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetContourSet() != null && drawMode == DrawMode.DRAWMODE_BOTH_CONTOURS)
            {
                _debugDraw.DepthMask(false);
                _debugDraw.DebugDrawRawContours(rcBuilderResult.GetContourSet(), 0.5f);
                _debugDraw.DebugDrawContours(rcBuilderResult.GetContourSet());
                _debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetContourSet() != null && drawMode == DrawMode.DRAWMODE_CONTOURS)
            {
                _debugDraw.DepthMask(false);
                _debugDraw.DebugDrawContours(rcBuilderResult.GetContourSet());
                _debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetCompactHeightfield() != null && drawMode == DrawMode.DRAWMODE_REGION_CONNECTIONS)
            {
                _debugDraw.DebugDrawCompactHeightfieldRegions(rcBuilderResult.GetCompactHeightfield());
                _debugDraw.DepthMask(false);
                if (rcBuilderResult.GetContourSet() != null)
                {
                    _debugDraw.DebugDrawRegionConnections(rcBuilderResult.GetContourSet());
                }

                _debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetMesh() != null && drawMode == DrawMode.DRAWMODE_POLYMESH)
            {
                _debugDraw.DepthMask(false);
                _debugDraw.DebugDrawPolyMesh(rcBuilderResult.GetMesh());
                _debugDraw.DepthMask(true);
            }

            if (rcBuilderResult.GetMeshDetail() != null && drawMode == DrawMode.DRAWMODE_POLYMESH_DETAIL)
            {
                _debugDraw.DepthMask(false);
                _debugDraw.DebugDrawPolyMeshDetail(rcBuilderResult.GetMeshDetail());
                _debugDraw.DepthMask(true);
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
        _debugDraw.DebugDrawBoxWire(bmin.x, bmin.y, bmin.z, bmax.x, bmax.y, bmax.z,
            DebugDraw.DuRGBA(255, 255, 255, 128), 1.0f);
        _debugDraw.Begin(DebugDrawPrimitives.POINTS, 5.0f);
        _debugDraw.Vertex(bmin.x, bmin.y, bmin.z, DebugDraw.DuRGBA(255, 255, 255, 128));
        _debugDraw.End();
    }

    public void DrawOffMeshConnections(DemoInputGeomProvider geom, bool hilight)
    {
        int conColor = DebugDraw.DuRGBA(192, 0, 128, 192);
        int baseColor = DebugDraw.DuRGBA(0, 0, 0, 64);
        _debugDraw.DepthMask(false);

        _debugDraw.Begin(DebugDrawPrimitives.LINES, 2.0f);
        foreach (var con in geom.GetOffMeshConnections())
        {
            float[] v = con.verts;
            _debugDraw.Vertex(v[0], v[1], v[2], baseColor);
            _debugDraw.Vertex(v[0], v[1] + 0.2f, v[2], baseColor);

            _debugDraw.Vertex(v[3], v[4], v[5], baseColor);
            _debugDraw.Vertex(v[3], v[4] + 0.2f, v[5], baseColor);

            _debugDraw.AppendCircle(v[0], v[1] + 0.1f, v[2], con.radius, baseColor);
            _debugDraw.AppendCircle(v[3], v[4] + 0.1f, v[5], con.radius, baseColor);

            if (hilight)
            {
                _debugDraw.AppendArc(v[0], v[1], v[2], v[3], v[4], v[5], 0.25f, con.bidir ? 0.6f : 0.0f, 0.6f, conColor);
            }
        }

        _debugDraw.End();

        _debugDraw.DepthMask(true);
    }

    void DrawConvexVolumes(DemoInputGeomProvider geom)
    {
        _debugDraw.DepthMask(false);

        _debugDraw.Begin(DebugDrawPrimitives.TRIS);

        foreach (RcConvexVolume vol in geom.ConvexVolumes())
        {
            int col = DebugDraw.DuTransCol(DebugDraw.AreaToCol(vol.areaMod.GetMaskedValue()), 32);
            for (int j = 0, k = vol.verts.Length - 3; j < vol.verts.Length; k = j, j += 3)
            {
                var va = RcVec3f.Of(vol.verts[k], vol.verts[k + 1], vol.verts[k + 2]);
                var vb = RcVec3f.Of(vol.verts[j], vol.verts[j + 1], vol.verts[j + 2]);

                _debugDraw.Vertex(vol.verts[0], vol.hmax, vol.verts[2], col);
                _debugDraw.Vertex(vb.x, vol.hmax, vb.z, col);
                _debugDraw.Vertex(va.x, vol.hmax, va.z, col);

                _debugDraw.Vertex(va.x, vol.hmin, va.z, DebugDraw.DuDarkenCol(col));
                _debugDraw.Vertex(va.x, vol.hmax, va.z, col);
                _debugDraw.Vertex(vb.x, vol.hmax, vb.z, col);

                _debugDraw.Vertex(va.x, vol.hmin, va.z, DebugDraw.DuDarkenCol(col));
                _debugDraw.Vertex(vb.x, vol.hmax, vb.z, col);
                _debugDraw.Vertex(vb.x, vol.hmin, vb.z, DebugDraw.DuDarkenCol(col));
            }
        }

        _debugDraw.End();

        _debugDraw.Begin(DebugDrawPrimitives.LINES, 2.0f);
        foreach (RcConvexVolume vol in geom.ConvexVolumes())
        {
            int col = DebugDraw.DuTransCol(DebugDraw.AreaToCol(vol.areaMod.GetMaskedValue()), 220);
            for (int j = 0, k = vol.verts.Length - 3; j < vol.verts.Length; k = j, j += 3)
            {
                var va = RcVec3f.Of(vol.verts[k], vol.verts[k + 1], vol.verts[k + 2]);
                var vb = RcVec3f.Of(vol.verts[j], vol.verts[j + 1], vol.verts[j + 2]);
                _debugDraw.Vertex(va.x, vol.hmin, va.z, DebugDraw.DuDarkenCol(col));
                _debugDraw.Vertex(vb.x, vol.hmin, vb.z, DebugDraw.DuDarkenCol(col));
                _debugDraw.Vertex(va.x, vol.hmax, va.z, col);
                _debugDraw.Vertex(vb.x, vol.hmax, vb.z, col);
                _debugDraw.Vertex(va.x, vol.hmin, va.z, DebugDraw.DuDarkenCol(col));
                _debugDraw.Vertex(va.x, vol.hmax, va.z, col);
            }
        }

        _debugDraw.End();

        _debugDraw.Begin(DebugDrawPrimitives.POINTS, 3.0f);
        foreach (RcConvexVolume vol in geom.ConvexVolumes())
        {
            int col = DebugDraw.DuDarkenCol(DebugDraw.DuTransCol(DebugDraw.AreaToCol(vol.areaMod.GetMaskedValue()), 220));
            for (int j = 0; j < vol.verts.Length; j += 3)
            {
                _debugDraw.Vertex(vol.verts[j + 0], vol.verts[j + 1] + 0.1f, vol.verts[j + 2], col);
                _debugDraw.Vertex(vol.verts[j + 0], vol.hmin, vol.verts[j + 2], col);
                _debugDraw.Vertex(vol.verts[j + 0], vol.hmax, vol.verts[j + 2], col);
            }
        }

        _debugDraw.End();

        _debugDraw.DepthMask(true);
    }
}