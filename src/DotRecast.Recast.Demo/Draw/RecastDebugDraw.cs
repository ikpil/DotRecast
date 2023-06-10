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
using System.Numerics;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.QueryResults;
using DotRecast.Recast.DemoTool.Builder;
using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo.Draw;

public class RecastDebugDraw : DebugDraw
{
    public static readonly int DRAWNAVMESH_OFFMESHCONS = 0x01;
    public static readonly int DRAWNAVMESH_CLOSEDLIST = 0x02;
    public static readonly int DRAWNAVMESH_COLOR_TILES = 0x04;

    public RecastDebugDraw(GL gl) : base(gl)
    {
    }

    public void DebugDrawTriMeshSlope(float[] verts, int[] tris, float[] normals, float walkableSlopeAngle,
        float texScale)
    {
        float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * Math.PI);

        RcVec2f uva = RcVec2f.Zero;
        RcVec2f uvb = RcVec2f.Zero;
        RcVec2f uvc = RcVec2f.Zero;

        Texture(true);

        int unwalkable = DuRGBA(192, 128, 0, 255);
        Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < tris.Length; i += 3)
        {
            RcVec3f norm = RcVec3f.Of(normals[i], normals[i + 1], normals[i + 2]);

            int color;
            char a = (char)(220 * (2 + norm.x + norm.y) / 4);
            if (norm.y < walkableThr)
            {
                color = DuLerpCol(DuRGBA(a, a, a, 255), unwalkable, 64);
            }
            else
            {
                color = DuRGBA(a, a, a, 255);
            }

            RcVec3f va = RcVec3f.Of(verts[tris[i] * 3], verts[tris[i] * 3 + 1], verts[tris[i] * 3 + 2]);
            RcVec3f vb = RcVec3f.Of(verts[tris[i + 1] * 3], verts[tris[i + 1] * 3 + 1], verts[tris[i + 1] * 3 + 2]);
            RcVec3f vc = RcVec3f.Of(verts[tris[i + 2] * 3], verts[tris[i + 2] * 3 + 1], verts[tris[i + 2] * 3 + 2]);

            int ax = 0, ay = 0;
            if (Math.Abs(norm.y) > Math.Abs(norm[ax]))
            {
                ax = 1;
            }

            if (Math.Abs(norm.z) > Math.Abs(norm[ax]))
            {
                ax = 2;
            }

            ax = (1 << ax) & 3; // +1 mod 3
            ay = (1 << ax) & 3; // +1 mod 3

            uva.x = va[ax] * texScale;
            uva.y = va[ay] * texScale;
            uvb.x = vb[ax] * texScale;
            uvb.y = vb[ay] * texScale;
            uvc.x = vc[ax] * texScale;
            uvc.y = vc[ay] * texScale;

            Vertex(va, color, uva);
            Vertex(vb, color, uvb);
            Vertex(vc, color, uvc);
        }

        End();

        Texture(false);
    }

    public void DebugDrawNavMeshWithClosedList(DtNavMesh mesh, DtNavMeshQuery query, int flags)
    {
        DtNavMeshQuery q = (flags & DRAWNAVMESH_CLOSEDLIST) != 0 ? query : null;
        for (int i = 0; i < mesh.GetMaxTiles(); ++i)
        {
            DtMeshTile tile = mesh.GetTile(i);
            if (tile != null && tile.data != null)
            {
                DrawMeshTile(mesh, q, tile, flags);
            }
        }
    }

    private void DrawMeshTile(DtNavMesh mesh, DtNavMeshQuery query, DtMeshTile tile, int flags)
    {
        long @base = mesh.GetPolyRefBase(tile);

        int tileNum = DtNavMesh.DecodePolyIdTile(@base);
        int tileColor = DuIntToCol(tileNum, 128);
        DepthMask(false);
        Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < tile.data.header.polyCount; ++i)
        {
            DtPoly p = tile.data.polys[i];
            if (p.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                continue;
            }

            int col;
            if (query != null && query.IsInClosedList(@base | (long)i))
            {
                col = DuRGBA(255, 196, 0, 64);
            }
            else
            {
                if ((flags & DRAWNAVMESH_COLOR_TILES) != 0)
                {
                    col = tileColor;
                }
                else
                {
                    if ((p.flags & SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED) != 0)
                    {
                        col = DuRGBA(64, 64, 64, 64);
                    }
                    else
                    {
                        col = DuTransCol(AreaToCol(p.GetArea()), 64);
                    }
                }
            }

            DrawPoly(tile, i, col);
        }

        End();

        // Draw inter poly boundaries
        DrawPolyBoundaries(tile, DuRGBA(0, 48, 64, 32), 1.5f, true);

        // Draw outer poly boundaries
        DrawPolyBoundaries(tile, DuRGBA(0, 48, 64, 220), 2.5f, false);

        if ((flags & DRAWNAVMESH_OFFMESHCONS) != 0)
        {
            Begin(DebugDrawPrimitives.LINES, 2.0f);
            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                DtPoly p = tile.data.polys[i];

                if (p.GetPolyType() != DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                {
                    continue;
                }

                int col, col2;
                if (query != null && query.IsInClosedList(@base | (long)i))
                {
                    col = DuRGBA(255, 196, 0, 220);
                }
                else
                {
                    col = DuDarkenCol(DuTransCol(AreaToCol(p.GetArea()), 220));
                }

                DtOffMeshConnection con = tile.data.offMeshCons[i - tile.data.header.offMeshBase];
                RcVec3f va = RcVec3f.Of(
                    tile.data.verts[p.verts[0] * 3], tile.data.verts[p.verts[0] * 3 + 1],
                    tile.data.verts[p.verts[0] * 3 + 2]
                );
                RcVec3f vb = RcVec3f.Of(
                    tile.data.verts[p.verts[1] * 3], tile.data.verts[p.verts[1] * 3 + 1],
                    tile.data.verts[p.verts[1] * 3 + 2]
                );

                // Check to see if start and end end-points have links.
                bool startSet = false;
                bool endSet = false;
                for (int k = tile.polyLinks[p.index]; k != DtNavMesh.DT_NULL_LINK; k = tile.links[k].next)
                {
                    if (tile.links[k].edge == 0)
                    {
                        startSet = true;
                    }

                    if (tile.links[k].edge == 1)
                    {
                        endSet = true;
                    }
                }

                // End points and their on-mesh locations.
                Vertex(va.x, va.y, va.z, col);
                Vertex(con.pos[0], con.pos[1], con.pos[2], col);
                col2 = startSet ? col : DuRGBA(220, 32, 16, 196);
                AppendCircle(con.pos[0], con.pos[1] + 0.1f, con.pos[2], con.rad, col2);

                Vertex(vb.x, vb.y, vb.z, col);
                Vertex(con.pos[3], con.pos[4], con.pos[5], col);
                col2 = endSet ? col : DuRGBA(220, 32, 16, 196);
                AppendCircle(con.pos[3], con.pos[4] + 0.1f, con.pos[5], con.rad, col2);

                // End point vertices.
                Vertex(con.pos[0], con.pos[1], con.pos[2], DuRGBA(0, 48, 64, 196));
                Vertex(con.pos[0], con.pos[1] + 0.2f, con.pos[2], DuRGBA(0, 48, 64, 196));

                Vertex(con.pos[3], con.pos[4], con.pos[5], DuRGBA(0, 48, 64, 196));
                Vertex(con.pos[3], con.pos[4] + 0.2f, con.pos[5], DuRGBA(0, 48, 64, 196));

                // Connection arc.
                AppendArc(con.pos[0], con.pos[1], con.pos[2], con.pos[3], con.pos[4], con.pos[5], 0.25f,
                    (con.flags & 1) != 0 ? 0.6f : 0, 0.6f, col);
            }

            End();
        }

        int vcol = DuRGBA(0, 0, 0, 196);
        Begin(DebugDrawPrimitives.POINTS, 3.0f);
        for (int i = 0; i < tile.data.header.vertCount; i++)
        {
            int v = i * 3;
            Vertex(tile.data.verts[v], tile.data.verts[v + 1], tile.data.verts[v + 2], vcol);
        }

        End();

        DepthMask(true);
    }

    private void DrawPoly(DtMeshTile tile, int index, int col)
    {
        DtPoly p = tile.data.polys[index];
        if (tile.data.detailMeshes != null)
        {
            DtPolyDetail pd = tile.data.detailMeshes[index];
            if (pd != null)
            {
                for (int j = 0; j < pd.triCount; ++j)
                {
                    int t = (pd.triBase + j) * 4;
                    for (int k = 0; k < 3; ++k)
                    {
                        int v = tile.data.detailTris[t + k];
                        if (v < p.vertCount)
                        {
                            Vertex(tile.data.verts[p.verts[v] * 3], tile.data.verts[p.verts[v] * 3 + 1],
                                tile.data.verts[p.verts[v] * 3 + 2], col);
                        }
                        else
                        {
                            Vertex(tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3],
                                tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 1],
                                tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 2], col);
                        }
                    }
                }
            }
        }
        else
        {
            for (int j = 1; j < p.vertCount - 1; ++j)
            {
                Vertex(tile.data.verts[p.verts[0] * 3], tile.data.verts[p.verts[0] * 3 + 1],
                    tile.data.verts[p.verts[0] * 3 + 2], col);
                for (int k = 0; k < 2; ++k)
                {
                    Vertex(tile.data.verts[p.verts[j + k] * 3], tile.data.verts[p.verts[j + k] * 3 + 1],
                        tile.data.verts[p.verts[j + k] * 3 + 2], col);
                }
            }
        }
    }

    void DrawPolyBoundaries(DtMeshTile tile, int col, float linew, bool inner)
    {
        float thr = 0.01f * 0.01f;

        Begin(DebugDrawPrimitives.LINES, linew);

        for (int i = 0; i < tile.data.header.polyCount; ++i)
        {
            DtPoly p = tile.data.polys[i];

            if (p.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                continue;
            }

            for (int j = 0, nj = p.vertCount; j < nj; ++j)
            {
                int c = col;
                if (inner)
                {
                    if (p.neis[j] == 0)
                    {
                        continue;
                    }

                    if ((p.neis[j] & DtNavMesh.DT_EXT_LINK) != 0)
                    {
                        bool con = false;
                        for (int k = tile.polyLinks[p.index]; k != DtNavMesh.DT_NULL_LINK; k = tile.links[k].next)
                        {
                            if (tile.links[k].edge == j)
                            {
                                con = true;
                                break;
                            }
                        }

                        if (con)
                        {
                            c = DuRGBA(255, 255, 255, 48);
                        }
                        else
                        {
                            c = DuRGBA(0, 0, 0, 48);
                        }
                    }
                    else
                    {
                        c = DuRGBA(0, 48, 64, 32);
                    }
                }
                else
                {
                    if (p.neis[j] != 0)
                    {
                        continue;
                    }
                }

                var v0 = RcVec3f.Of(
                    tile.data.verts[p.verts[j] * 3], tile.data.verts[p.verts[j] * 3 + 1],
                    tile.data.verts[p.verts[j] * 3 + 2]
                );
                var v1 = RcVec3f.Of(
                    tile.data.verts[p.verts[(j + 1) % nj] * 3],
                    tile.data.verts[p.verts[(j + 1) % nj] * 3 + 1],
                    tile.data.verts[p.verts[(j + 1) % nj] * 3 + 2]
                );

                // Draw detail mesh edges which align with the actual poly edge.
                // This is really slow.
                if (tile.data.detailMeshes != null)
                {
                    DtPolyDetail pd = tile.data.detailMeshes[i];
                    for (int k = 0; k < pd.triCount; ++k)
                    {
                        int t = (pd.triBase + k) * 4;
                        RcVec3f[] tv = new RcVec3f[3];
                        for (int m = 0; m < 3; ++m)
                        {
                            int v = tile.data.detailTris[t + m];
                            if (v < p.vertCount)
                            {
                                tv[m] = RcVec3f.Of(
                                    tile.data.verts[p.verts[v] * 3], tile.data.verts[p.verts[v] * 3 + 1],
                                    tile.data.verts[p.verts[v] * 3 + 2]
                                );
                            }
                            else
                            {
                                tv[m] = RcVec3f.Of(
                                    tile.data.detailVerts[(pd.vertBase + (v - p.vertCount)) * 3],
                                    tile.data.detailVerts[(pd.vertBase + (v - p.vertCount)) * 3 + 1],
                                    tile.data.detailVerts[(pd.vertBase + (v - p.vertCount)) * 3 + 2]
                                );
                            }
                        }

                        for (int m = 0, n = 2; m < 3; n = m++)
                        {
                            if ((DtNavMesh.GetDetailTriEdgeFlags(tile.data.detailTris[t + 3], n) & DtNavMesh.DT_DETAIL_EDGE_BOUNDARY) == 0)
                                continue;

                            if (((tile.data.detailTris[t + 3] >> (n * 2)) & 0x3) == 0)
                            {
                                continue; // Skip inner detail edges.
                            }

                            if (DistancePtLine2d(tv[n], v0, v1) < thr && DistancePtLine2d(tv[m], v0, v1) < thr)
                            {
                                Vertex(tv[n], c);
                                Vertex(tv[m], c);
                            }
                        }
                    }
                }
                else
                {
                    Vertex(v0, c);
                    Vertex(v1, c);
                }
            }
        }

        End();
    }

    static float DistancePtLine2d(RcVec3f pt, RcVec3f p, RcVec3f q)
    {
        float pqx = q.x - p.x;
        float pqz = q.z - p.z;
        float dx = pt.x - p.x;
        float dz = pt.z - p.z;
        float d = pqx * pqx + pqz * pqz;
        float t = pqx * dx + pqz * dz;
        if (d != 0)
        {
            t /= d;
        }

        dx = p.x + t * pqx - pt.x;
        dz = p.z + t * pqz - pt.z;
        return dx * dx + dz * dz;
    }

    public void DebugDrawNavMeshBVTree(DtNavMesh mesh)
    {
        for (int i = 0; i < mesh.GetMaxTiles(); ++i)
        {
            DtMeshTile tile = mesh.GetTile(i);
            if (tile != null && tile.data != null && tile.data.header != null)
            {
                DrawMeshTileBVTree(tile);
            }
        }
    }

    private void DrawMeshTileBVTree(DtMeshTile tile)
    {
        // Draw BV nodes.
        float cs = 1.0f / tile.data.header.bvQuantFactor;
        Begin(DebugDrawPrimitives.LINES, 1.0f);
        for (int i = 0; i < tile.data.header.bvNodeCount; ++i)
        {
            DtBVNode n = tile.data.bvTree[i];
            if (n.i < 0)
            {
                continue;
            }

            AppendBoxWire(tile.data.header.bmin.x + n.bmin[0] * cs, tile.data.header.bmin.y + n.bmin[1] * cs,
                tile.data.header.bmin.z + n.bmin[2] * cs, tile.data.header.bmin.x + n.bmax[0] * cs,
                tile.data.header.bmin.y + n.bmax[1] * cs, tile.data.header.bmin.z + n.bmax[2] * cs,
                DuRGBA(255, 255, 255, 128));
        }

        End();
    }

    public void DebugDrawCompactHeightfieldSolid(RcCompactHeightfield chf)
    {
        float cs = chf.cs;
        float ch = chf.ch;

        Begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < chf.height; ++y)
        {
            for (int x = 0; x < chf.width; ++x)
            {
                float fx = chf.bmin.x + x * cs;
                float fz = chf.bmin.z + y * cs;
                RcCompactCell c = chf.cells[x + y * chf.width];

                for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                {
                    RcCompactSpan s = chf.spans[i];

                    int area = chf.areas[i];
                    int color;
                    if (area == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE)
                    {
                        color = DuRGBA(0, 192, 255, 64);
                    }
                    else if (area == RcConstants.RC_NULL_AREA)
                    {
                        color = DuRGBA(0, 0, 0, 64);
                    }
                    else
                    {
                        color = AreaToCol(area);
                    }

                    float fy = chf.bmin.y + (s.y + 1) * ch;
                    Vertex(fx, fy, fz, color);
                    Vertex(fx, fy, fz + cs, color);
                    Vertex(fx + cs, fy, fz + cs, color);
                    Vertex(fx + cs, fy, fz, color);
                }
            }
        }

        End();
    }

    public void DebugDrawRegionConnections(RcContourSet cset)
    {
        float alpha = 1f;

        RcVec3f orig = cset.bmin;
        float cs = cset.cs;
        float ch = cset.ch;

        int color = DuRGBA(0, 0, 0, 196);

        Begin(DebugDrawPrimitives.LINES, 2.0f);

        for (int i = 0; i < cset.conts.Count; ++i)
        {
            RcContour cont = cset.conts[i];
            RcVec3f pos = GetContourCenter(cont, orig, cs, ch);
            for (int j = 0; j < cont.nverts; ++j)
            {
                int v = j * 4;
                if (cont.verts[v + 3] == 0 || (short)cont.verts[v + 3] < cont.reg)
                {
                    continue;
                }

                RcContour cont2 = FindContourFromSet(cset, (short)cont.verts[v + 3]);
                if (cont2 != null)
                {
                    RcVec3f pos2 = GetContourCenter(cont2, orig, cs, ch);
                    AppendArc(pos.x, pos.y, pos.z, pos2.x, pos2.y, pos2.z, 0.25f, 0.6f, 0.6f, color);
                }
            }
        }

        End();

        char a = (char)(alpha * 255.0f);

        Begin(DebugDrawPrimitives.POINTS, 7.0f);

        for (int i = 0; i < cset.conts.Count; ++i)
        {
            RcContour cont = cset.conts[i];
            int col = DuDarkenCol(DuIntToCol(cont.reg, a));
            RcVec3f pos = GetContourCenter(cont, orig, cs, ch);
            Vertex(pos, col);
        }

        End();
    }

    private RcVec3f GetContourCenter(RcContour cont, RcVec3f orig, float cs, float ch)
    {
        RcVec3f center = new RcVec3f();
        center.x = 0;
        center.y = 0;
        center.z = 0;
        if (cont.nverts == 0)
        {
            return center;
        }

        for (int i = 0; i < cont.nverts; ++i)
        {
            int v = i * 4;
            center.x += cont.verts[v + 0];
            center.y += cont.verts[v + 1];
            center.z += cont.verts[v + 2];
        }

        float s = 1.0f / cont.nverts;
        center.x *= s * cs;
        center.y *= s * ch;
        center.z *= s * cs;
        center.x += orig.x;
        center.y += orig.y + 4 * ch;
        center.z += orig.z;
        return center;
    }

    private RcContour FindContourFromSet(RcContourSet cset, int reg)
    {
        for (int i = 0; i < cset.conts.Count; ++i)
        {
            if (cset.conts[i].reg == reg)
            {
                return cset.conts[i];
            }
        }

        return null;
    }

    public void DebugDrawRawContours(RcContourSet cset, float alpha)
    {
        RcVec3f orig = cset.bmin;
        float cs = cset.cs;
        float ch = cset.ch;

        char a = (char)(alpha * 255.0f);

        Begin(DebugDrawPrimitives.LINES, 2.0f);

        for (int i = 0; i < cset.conts.Count; ++i)
        {
            RcContour c = cset.conts[i];
            int color = DuIntToCol(c.reg, a);

            for (int j = 0; j < c.nrverts; ++j)
            {
                int v0 = c.rverts[j * 4];
                int v1 = c.rverts[j * 4 + 1];
                int v2 = c.rverts[j * 4 + 2];
                float fx = orig.x + v0 * cs;
                float fy = orig.y + (v1 + 1 + (i & 1)) * ch;
                float fz = orig.z + v2 * cs;
                Vertex(fx, fy, fz, color);
                if (j > 0)
                {
                    Vertex(fx, fy, fz, color);
                }
            }

            // Loop last segment.
            {
                int v0 = c.rverts[0];
                int v1 = c.rverts[1];
                int v2 = c.rverts[2];
                float fx = orig.x + v0 * cs;
                float fy = orig.y + (v1 + 1 + (i & 1)) * ch;
                float fz = orig.z + v2 * cs;
                Vertex(fx, fy, fz, color);
            }
        }

        End();

        Begin(DebugDrawPrimitives.POINTS, 2.0f);

        for (int i = 0; i < cset.conts.Count; ++i)
        {
            RcContour c = cset.conts[i];
            int color = DuDarkenCol(DuIntToCol(c.reg, a));

            for (int j = 0; j < c.nrverts; ++j)
            {
                int v0 = c.rverts[j * 4];
                int v1 = c.rverts[j * 4 + 1];
                int v2 = c.rverts[j * 4 + 2];
                int v3 = c.rverts[j * 4 + 3];
                float off = 0;
                int colv = color;
                if ((v3 & RcConstants.RC_BORDER_VERTEX) != 0)
                {
                    colv = DuRGBA(255, 255, 255, a);
                    off = ch * 2;
                }

                float fx = orig.x + v0 * cs;
                float fy = orig.y + (v1 + 1 + (i & 1)) * ch + off;
                float fz = orig.z + v2 * cs;
                Vertex(fx, fy, fz, colv);
            }
        }

        End();
    }

    public void DebugDrawContours(RcContourSet cset)
    {
        float alpha = 1f;
        RcVec3f orig = cset.bmin;
        float cs = cset.cs;
        float ch = cset.ch;

        char a = (char)(alpha * 255.0f);

        Begin(DebugDrawPrimitives.LINES, 2.5f);

        for (int i = 0; i < cset.conts.Count; ++i)
        {
            RcContour c = cset.conts[i];
            if (c.nverts == 0)
            {
                continue;
            }

            int color = DuIntToCol(c.reg, a);
            int bcolor = DuLerpCol(color, DuRGBA(255, 255, 255, a), 128);

            for (int j = 0, k = c.nverts - 1; j < c.nverts; k = j++)
            {
                int va0 = c.verts[k * 4];
                int va1 = c.verts[k * 4 + 1];
                int va2 = c.verts[k * 4 + 2];
                int va3 = c.verts[k * 4 + 3];
                int vb0 = c.verts[j * 4];
                int vb1 = c.verts[j * 4 + 1];
                int vb2 = c.verts[j * 4 + 2];
                int col = (va3 & RcConstants.RC_AREA_BORDER) != 0 ? bcolor : color;

                float fx = orig.x + va0 * cs;
                float fy = orig.y + (va1 + 1 + (i & 1)) * ch;
                float fz = orig.z + va2 * cs;
                Vertex(fx, fy, fz, col);

                fx = orig.x + vb0 * cs;
                fy = orig.y + (vb1 + 1 + (i & 1)) * ch;
                fz = orig.z + vb2 * cs;
                Vertex(fx, fy, fz, col);
            }
        }

        End();

        Begin(DebugDrawPrimitives.POINTS, 3.0f);

        for (int i = 0; i < cset.conts.Count; ++i)
        {
            RcContour c = cset.conts[i];
            int color = DuDarkenCol(DuIntToCol(c.reg, a));

            for (int j = 0; j < c.nverts; ++j)
            {
                int v0 = c.verts[j * 4];
                int v1 = c.verts[j * 4 + 1];
                int v2 = c.verts[j * 4 + 2];
                int v3 = c.verts[j * 4 + 3];
                float off = 0;
                int colv = color;
                if ((v3 & RcConstants.RC_BORDER_VERTEX) != 0)
                {
                    colv = DuRGBA(255, 255, 255, a);
                    off = ch * 2;
                }

                float fx = orig.x + v0 * cs;
                float fy = orig.y + (v1 + 1 + (i & 1)) * ch + off;
                float fz = orig.z + v2 * cs;
                Vertex(fx, fy, fz, colv);
            }
        }

        End();
    }

    public void DebugDrawHeightfieldSolid(RcHeightfield hf)
    {
        if (!FrustumTest(hf.bmin, hf.bmax))
        {
            return;
        }

        RcVec3f orig = hf.bmin;
        float cs = hf.cs;
        float ch = hf.ch;

        int w = hf.width;
        int h = hf.height;

        int[] fcol = new int[6];
        DuCalcBoxColors(fcol, DuRGBA(255, 255, 255, 255), DuRGBA(255, 255, 255, 255));

        Begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < h; ++y)
        {
            for (int x = 0; x < w; ++x)
            {
                float fx = orig.x + x * cs;
                float fz = orig.z + y * cs;
                RcSpan s = hf.spans[x + y * w];
                while (s != null)
                {
                    AppendBox(fx, orig.y + s.smin * ch, fz, fx + cs, orig.y + s.smax * ch, fz + cs, fcol);
                    s = s.next;
                }
            }
        }

        End();
    }

    public void DebugDrawHeightfieldWalkable(RcHeightfield hf)
    {
        RcVec3f orig = hf.bmin;
        float cs = hf.cs;
        float ch = hf.ch;

        int w = hf.width;
        int h = hf.height;

        int[] fcol = new int[6];
        DuCalcBoxColors(fcol, DuRGBA(255, 255, 255, 255), DuRGBA(217, 217, 217, 255));

        Begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < h; ++y)
        {
            for (int x = 0; x < w; ++x)
            {
                float fx = orig.x + x * cs;
                float fz = orig.z + y * cs;
                RcSpan s = hf.spans[x + y * w];
                while (s != null)
                {
                    if (s.area == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE)
                    {
                        fcol[0] = DuRGBA(64, 128, 160, 255);
                    }
                    else if (s.area == RcConstants.RC_NULL_AREA)
                    {
                        fcol[0] = DuRGBA(64, 64, 64, 255);
                    }
                    else
                    {
                        fcol[0] = DuMultCol(AreaToCol(s.area), 200);
                    }

                    AppendBox(fx, orig.y + s.smin * ch, fz, fx + cs, orig.y + s.smax * ch, fz + cs, fcol);
                    s = s.next;
                }
            }
        }

        End();
    }

    public void DebugDrawCompactHeightfieldRegions(RcCompactHeightfield chf)
    {
        float cs = chf.cs;
        float ch = chf.ch;

        Begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < chf.height; ++y)
        {
            for (int x = 0; x < chf.width; ++x)
            {
                float fx = chf.bmin.x + x * cs;
                float fz = chf.bmin.z + y * cs;
                RcCompactCell c = chf.cells[x + y * chf.width];

                for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                {
                    RcCompactSpan s = chf.spans[i];
                    float fy = chf.bmin.y + (s.y) * ch;
                    int color;
                    if (s.reg != 0)
                    {
                        color = DuIntToCol(s.reg, 192);
                    }
                    else
                    {
                        color = DuRGBA(0, 0, 0, 64);
                    }

                    Vertex(fx, fy, fz, color);
                    Vertex(fx, fy, fz + cs, color);
                    Vertex(fx + cs, fy, fz + cs, color);
                    Vertex(fx + cs, fy, fz, color);
                }
            }
        }

        End();
    }

    public void DebugDrawCompactHeightfieldDistance(RcCompactHeightfield chf)
    {
        if (chf.dist == null)
        {
            return;
        }

        float cs = chf.cs;
        float ch = chf.ch;

        float maxd = chf.maxDistance;
        if (maxd < 1.0f)
        {
            maxd = 1;
        }

        float dscale = 255.0f / maxd;

        Begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < chf.height; ++y)
        {
            for (int x = 0; x < chf.width; ++x)
            {
                float fx = chf.bmin.x + x * cs;
                float fz = chf.bmin.z + y * cs;
                RcCompactCell c = chf.cells[x + y * chf.width];

                for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                {
                    RcCompactSpan s = chf.spans[i];
                    float fy = chf.bmin.y + (s.y + 1) * ch;
                    char cd = (char)(chf.dist[i] * dscale);
                    int color = DuRGBA(cd, cd, cd, 255);
                    Vertex(fx, fy, fz, color);
                    Vertex(fx, fy, fz + cs, color);
                    Vertex(fx + cs, fy, fz + cs, color);
                    Vertex(fx + cs, fy, fz, color);
                }
            }
        }

        End();
    }

    public void DebugDrawPolyMesh(RcPolyMesh mesh)
    {
        int nvp = mesh.nvp;
        float cs = mesh.cs;
        float ch = mesh.ch;
        RcVec3f orig = mesh.bmin;

        Begin(DebugDrawPrimitives.TRIS);

        for (int i = 0; i < mesh.npolys; ++i)
        {
            int p = i * nvp * 2;
            int area = mesh.areas[i];

            int color;
            if (area == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE)
            {
                color = DuRGBA(0, 192, 255, 64);
            }
            else if (area == RcConstants.RC_NULL_AREA)
            {
                color = DuRGBA(0, 0, 0, 64);
            }
            else
            {
                color = AreaToCol(area);
            }

            int[] vi = new int[3];
            for (int j = 2; j < nvp; ++j)
            {
                if (mesh.polys[p + j] == RcConstants.RC_MESH_NULL_IDX)
                {
                    break;
                }

                vi[0] = mesh.polys[p + 0];
                vi[1] = mesh.polys[p + j - 1];
                vi[2] = mesh.polys[p + j];
                for (int k = 0; k < 3; ++k)
                {
                    int v0 = mesh.verts[vi[k] * 3];
                    int v1 = mesh.verts[vi[k] * 3 + 1];
                    int v2 = mesh.verts[vi[k] * 3 + 2];
                    float x = orig.x + v0 * cs;
                    float y = orig.y + (v1 + 1) * ch;
                    float z = orig.z + v2 * cs;
                    Vertex(x, y, z, color);
                }
            }
        }

        End();

        // Draw neighbours edges
        int coln = DuRGBA(0, 48, 64, 32);
        Begin(DebugDrawPrimitives.LINES, 1.5f);
        for (int i = 0; i < mesh.npolys; ++i)
        {
            int p = i * nvp * 2;
            for (int j = 0; j < nvp; ++j)
            {
                if (mesh.polys[p + j] == RcConstants.RC_MESH_NULL_IDX)
                {
                    break;
                }

                if ((mesh.polys[p + nvp + j] & 0x8000) != 0)
                {
                    continue;
                }

                int nj = (j + 1 >= nvp || mesh.polys[p + j + 1] == RcConstants.RC_MESH_NULL_IDX) ? 0 : j + 1;
                int[] vi = { mesh.polys[p + j], mesh.polys[p + nj] };

                for (int k = 0; k < 2; ++k)
                {
                    int v = vi[k] * 3;
                    float x = orig.x + mesh.verts[v] * cs;
                    float y = orig.y + (mesh.verts[v + 1] + 1) * ch + 0.1f;
                    float z = orig.z + mesh.verts[v + 2] * cs;
                    Vertex(x, y, z, coln);
                }
            }
        }

        End();

        // Draw boundary edges
        int colb = DuRGBA(0, 48, 64, 220);
        Begin(DebugDrawPrimitives.LINES, 2.5f);
        for (int i = 0; i < mesh.npolys; ++i)
        {
            int p = i * nvp * 2;
            for (int j = 0; j < nvp; ++j)
            {
                if (mesh.polys[p + j] == RcConstants.RC_MESH_NULL_IDX)
                {
                    break;
                }

                if ((mesh.polys[p + nvp + j] & 0x8000) == 0)
                {
                    continue;
                }

                int nj = (j + 1 >= nvp || mesh.polys[p + j + 1] == RcConstants.RC_MESH_NULL_IDX) ? 0 : j + 1;
                int[] vi = { mesh.polys[p + j], mesh.polys[p + nj] };

                int col = colb;
                if ((mesh.polys[p + nvp + j] & 0xf) != 0xf)
                {
                    col = DuRGBA(255, 255, 255, 128);
                }

                for (int k = 0; k < 2; ++k)
                {
                    int v = vi[k] * 3;
                    float x = orig.x + mesh.verts[v] * cs;
                    float y = orig.y + (mesh.verts[v + 1] + 1) * ch + 0.1f;
                    float z = orig.z + mesh.verts[v + 2] * cs;
                    Vertex(x, y, z, col);
                }
            }
        }

        End();

        Begin(DebugDrawPrimitives.POINTS, 3.0f);
        int colv = DuRGBA(0, 0, 0, 220);
        for (int i = 0; i < mesh.nverts; ++i)
        {
            int v = i * 3;
            float x = orig.x + mesh.verts[v] * cs;
            float y = orig.y + (mesh.verts[v + 1] + 1) * ch + 0.1f;
            float z = orig.z + mesh.verts[v + 2] * cs;
            Vertex(x, y, z, colv);
        }

        End();
    }

    public void DebugDrawPolyMeshDetail(RcPolyMeshDetail dmesh)
    {
        Begin(DebugDrawPrimitives.TRIS);

        for (int i = 0; i < dmesh.nmeshes; ++i)
        {
            int m = i * 4;
            int bverts = dmesh.meshes[m];
            int btris = dmesh.meshes[m + 2];
            int ntris = dmesh.meshes[m + 3];
            int verts = bverts * 3;
            int tris = btris * 4;

            int color = DuIntToCol(i, 192);

            for (int j = 0; j < ntris; ++j)
            {
                Vertex(dmesh.verts[verts + dmesh.tris[tris + j * 4 + 0] * 3],
                    dmesh.verts[verts + dmesh.tris[tris + j * 4 + 0] * 3 + 1],
                    dmesh.verts[verts + dmesh.tris[tris + j * 4 + 0] * 3 + 2], color);
                Vertex(dmesh.verts[verts + dmesh.tris[tris + j * 4 + 1] * 3],
                    dmesh.verts[verts + dmesh.tris[tris + j * 4 + 1] * 3 + 1],
                    dmesh.verts[verts + dmesh.tris[tris + j * 4 + 1] * 3 + 2], color);
                Vertex(dmesh.verts[verts + dmesh.tris[tris + j * 4 + 2] * 3],
                    dmesh.verts[verts + dmesh.tris[tris + j * 4 + 2] * 3 + 1],
                    dmesh.verts[verts + dmesh.tris[tris + j * 4 + 2] * 3 + 2], color);
            }
        }

        End();

        // Internal edges.
        Begin(DebugDrawPrimitives.LINES, 1.0f);
        int coli = DuRGBA(0, 0, 0, 64);
        for (int i = 0; i < dmesh.nmeshes; ++i)
        {
            int m = i * 4;
            int bverts = dmesh.meshes[m];
            int btris = dmesh.meshes[m + 2];
            int ntris = dmesh.meshes[m + 3];
            int verts = bverts * 3;
            int tris = btris * 4;

            for (int j = 0; j < ntris; ++j)
            {
                int t = tris + j * 4;
                for (int k = 0, kp = 2; k < 3; kp = k++)
                {
                    int ef = (dmesh.tris[t + 3] >> (kp * 2)) & 0x3;
                    if (ef == 0)
                    {
                        // Internal edge
                        if (dmesh.tris[t + kp] < dmesh.tris[t + k])
                        {
                            Vertex(dmesh.verts[verts + dmesh.tris[t + kp] * 3],
                                dmesh.verts[verts + dmesh.tris[t + kp] * 3 + 1],
                                dmesh.verts[verts + dmesh.tris[t + kp] * 3 + 2], coli);
                            Vertex(dmesh.verts[verts + dmesh.tris[t + k] * 3],
                                dmesh.verts[verts + dmesh.tris[t + k] * 3 + 1],
                                dmesh.verts[verts + dmesh.tris[t + k] * 3 + 2], coli);
                        }
                    }
                }
            }
        }

        End();

        // External edges.
        Begin(DebugDrawPrimitives.LINES, 2.0f);
        int cole = DuRGBA(0, 0, 0, 64);
        for (int i = 0; i < dmesh.nmeshes; ++i)
        {
            int m = i * 4;
            int bverts = dmesh.meshes[m];
            int btris = dmesh.meshes[m + 2];
            int ntris = dmesh.meshes[m + 3];
            int verts = bverts * 3;
            int tris = btris * 4;

            for (int j = 0; j < ntris; ++j)
            {
                int t = tris + j * 4;
                for (int k = 0, kp = 2; k < 3; kp = k++)
                {
                    int ef = (dmesh.tris[t + 3] >> (kp * 2)) & 0x3;
                    if (ef != 0)
                    {
                        // Ext edge
                        Vertex(dmesh.verts[verts + dmesh.tris[t + kp] * 3],
                            dmesh.verts[verts + dmesh.tris[t + kp] * 3 + 1],
                            dmesh.verts[verts + dmesh.tris[t + kp] * 3 + 2], cole);
                        Vertex(dmesh.verts[verts + dmesh.tris[t + k] * 3],
                            dmesh.verts[verts + dmesh.tris[t + k] * 3 + 1],
                            dmesh.verts[verts + dmesh.tris[t + k] * 3 + 2], cole);
                    }
                }
            }
        }

        End();

        Begin(DebugDrawPrimitives.POINTS, 3.0f);
        int colv = DuRGBA(0, 0, 0, 64);
        for (int i = 0; i < dmesh.nmeshes; ++i)
        {
            int m = i * 4;
            int bverts = dmesh.meshes[m];
            int nverts = dmesh.meshes[m + 1];
            int verts = bverts * 3;
            for (int j = 0; j < nverts; ++j)
            {
                Vertex(dmesh.verts[verts + j * 3], dmesh.verts[verts + j * 3 + 1], dmesh.verts[verts + j * 3 + 2],
                    colv);
            }
        }

        End();
    }

    public void DebugDrawNavMeshNodes(DtNavMeshQuery query)
    {
        DtNodePool pool = query.GetNodePool();
        if (pool != null)
        {
            float off = 0.5f;
            Begin(DebugDrawPrimitives.POINTS, 4.0f);

            foreach (List<DtNode> nodes in pool.GetNodeMap().Values)
            {
                foreach (DtNode node in nodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    Vertex(node.pos.x, node.pos.y + off, node.pos.z, DuRGBA(255, 192, 0, 255));
                }
            }

            End();

            Begin(DebugDrawPrimitives.LINES, 2.0f);
            foreach (List<DtNode> nodes in pool.GetNodeMap().Values)
            {
                foreach (DtNode node in nodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    if (node.pidx == 0)
                    {
                        continue;
                    }

                    DtNode parent = pool.GetNodeAtIdx(node.pidx);
                    if (parent == null)
                    {
                        continue;
                    }

                    Vertex(node.pos.x, node.pos.y + off, node.pos.z, DuRGBA(255, 192, 0, 128));
                    Vertex(parent.pos.x, parent.pos.y + off, parent.pos.z, DuRGBA(255, 192, 0, 128));
                }
            }

            End();
        }
    }

    public void DebugDrawNavMeshPolysWithFlags(DtNavMesh mesh, int polyFlags, int col)
    {
        for (int i = 0; i < mesh.GetMaxTiles(); ++i)
        {
            DtMeshTile tile = mesh.GetTile(i);
            if (tile == null || tile.data == null || tile.data.header == null)
            {
                continue;
            }

            long @base = mesh.GetPolyRefBase(tile);

            for (int j = 0; j < tile.data.header.polyCount; ++j)
            {
                DtPoly p = tile.data.polys[j];
                if ((p.flags & polyFlags) == 0)
                {
                    continue;
                }

                DebugDrawNavMeshPoly(mesh, @base | (long)j, col);
            }
        }
    }

    public void DebugDrawNavMeshPoly(DtNavMesh mesh, long refs, int col)
    {
        if (refs == 0)
        {
            return;
        }

        var status = mesh.GetTileAndPolyByRef(refs, out var tile, out var poly);
        if (status.Failed())
        {
            return;
        }

        DepthMask(false);

        int c = DuTransCol(col, 64);
        int ip = poly.index;

        if (poly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
        {
            DtOffMeshConnection con = tile.data.offMeshCons[ip - tile.data.header.offMeshBase];

            Begin(DebugDrawPrimitives.LINES, 2.0f);

            // Connection arc.
            AppendArc(con.pos[0], con.pos[1], con.pos[2], con.pos[3], con.pos[4], con.pos[5], 0.25f,
                (con.flags & 1) != 0 ? 0.6f : 0.0f, 0.6f, c);

            End();
        }
        else
        {
            Begin(DebugDrawPrimitives.TRIS);
            DrawPoly(tile, ip, col);
            End();
        }

        DepthMask(true);
    }

    public void DebugDrawNavMeshPortals(DtNavMesh mesh)
    {
        for (int i = 0; i < mesh.GetMaxTiles(); ++i)
        {
            DtMeshTile tile = mesh.GetTile(i);
            if (tile.data != null && tile.data.header != null)
            {
                DrawMeshTilePortal(tile);
            }
        }
    }

    private void DrawMeshTilePortal(DtMeshTile tile)
    {
        float padx = 0.04f;
        float pady = tile.data.header.walkableClimb;

        Begin(DebugDrawPrimitives.LINES, 2.0f);

        for (int side = 0; side < 8; ++side)
        {
            int m = DtNavMesh.DT_EXT_LINK | (short)side;

            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                DtPoly poly = tile.data.polys[i];

                // Create new links.
                int nv = poly.vertCount;
                for (int j = 0; j < nv; ++j)
                {
                    // Skip edges which do not point to the right side.
                    if (poly.neis[j] != m)
                        continue;

                    // Create new links
                    var va = RcVec3f.Of(
                        tile.data.verts[poly.verts[j] * 3],
                        tile.data.verts[poly.verts[j] * 3 + 1], tile.data.verts[poly.verts[j] * 3 + 2]
                    );
                    var vb = RcVec3f.Of(
                        tile.data.verts[poly.verts[(j + 1) % nv] * 3],
                        tile.data.verts[poly.verts[(j + 1) % nv] * 3 + 1],
                        tile.data.verts[poly.verts[(j + 1) % nv] * 3 + 2]
                    );

                    if (side == 0 || side == 4)
                    {
                        int col = side == 0 ? DuRGBA(128, 0, 0, 128) : DuRGBA(128, 0, 128, 128);

                        float x = va.x + ((side == 0) ? -padx : padx);

                        Vertex(x, va.y - pady, va.z, col);
                        Vertex(x, va.y + pady, va.z, col);

                        Vertex(x, va.y + pady, va.z, col);
                        Vertex(x, vb.y + pady, vb.z, col);

                        Vertex(x, vb.y + pady, vb.z, col);
                        Vertex(x, vb.y - pady, vb.z, col);

                        Vertex(x, vb.y - pady, vb.z, col);
                        Vertex(x, va.y - pady, va.z, col);
                    }
                    else if (side == 2 || side == 6)
                    {
                        int col = side == 2 ? DuRGBA(0, 128, 0, 128) : DuRGBA(0, 128, 128, 128);

                        float z = va.z + ((side == 2) ? -padx : padx);

                        Vertex(va.x, va.y - pady, z, col);
                        Vertex(va.x, va.y + pady, z, col);

                        Vertex(va.x, va.y + pady, z, col);
                        Vertex(vb.x, vb.y + pady, z, col);

                        Vertex(vb.x, vb.y + pady, z, col);
                        Vertex(vb.x, vb.y - pady, z, col);

                        Vertex(vb.x, vb.y - pady, z, col);
                        Vertex(va.x, va.y - pady, z, col);
                    }
                }
            }
        }

        End();
    }
}