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
using DotRecast.Detour;
using DotRecast.Recast.Demo.Builder;

namespace DotRecast.Recast.Demo.Draw;

public class RecastDebugDraw : DebugDraw {

    public static readonly int DRAWNAVMESH_OFFMESHCONS = 0x01;
    public static readonly int DRAWNAVMESH_CLOSEDLIST = 0x02;
    public static readonly int DRAWNAVMESH_COLOR_TILES = 0x04;

    public void debugDrawTriMeshSlope(float[] verts, int[] tris, float[] normals, float walkableSlopeAngle,
            float texScale) {

        float walkableThr = (float) Math.Cos(walkableSlopeAngle / 180.0f * Math.PI);

        float[] uva = new float[2];
        float[] uvb = new float[2];
        float[] uvc = new float[2];

        texture(true);

        int unwalkable = duRGBA(192, 128, 0, 255);
        begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < tris.Length; i += 3) {
            float[] norm = new float[] { normals[i], normals[i + 1], normals[i + 2] };

            int color;
            char a = (char) (220 * (2 + norm[0] + norm[1]) / 4);
            if (norm[1] < walkableThr) {
                color = duLerpCol(duRGBA(a, a, a, 255), unwalkable, 64);
            } else {
                color = duRGBA(a, a, a, 255);
            }

            float[] va = new float[] { verts[tris[i] * 3], verts[tris[i] * 3 + 1], verts[tris[i] * 3 + 2] };
            float[] vb = new float[] { verts[tris[i + 1] * 3], verts[tris[i + 1] * 3 + 1], verts[tris[i + 1] * 3 + 2] };
            float[] vc = new float[] { verts[tris[i + 2] * 3], verts[tris[i + 2] * 3 + 1], verts[tris[i + 2] * 3 + 2] };

            int ax = 0, ay = 0;
            if (Math.Abs(norm[1]) > Math.Abs(norm[ax])) {
                ax = 1;
            }
            if (Math.Abs(norm[2]) > Math.Abs(norm[ax])) {
                ax = 2;
            }
            ax = (1 << ax) & 3; // +1 mod 3
            ay = (1 << ax) & 3; // +1 mod 3

            uva[0] = va[ax] * texScale;
            uva[1] = va[ay] * texScale;
            uvb[0] = vb[ax] * texScale;
            uvb[1] = vb[ay] * texScale;
            uvc[0] = vc[ax] * texScale;
            uvc[1] = vc[ay] * texScale;

            vertex(va, color, uva);
            vertex(vb, color, uvb);
            vertex(vc, color, uvc);
        }
        end();

        texture(false);
    }

    public void debugDrawNavMeshWithClosedList(NavMesh mesh, NavMeshQuery query, int flags) {
        NavMeshQuery q = (flags & DRAWNAVMESH_CLOSEDLIST) != 0 ? query : null;
        for (int i = 0; i < mesh.getMaxTiles(); ++i) {
            MeshTile tile = mesh.getTile(i);
            if (tile != null && tile.data != null) {
                drawMeshTile(mesh, q, tile, flags);
            }
        }
    }

    private void drawMeshTile(NavMesh mesh, NavMeshQuery query, MeshTile tile, int flags) {
        long @base = mesh.getPolyRefBase(tile);

        int tileNum = NavMesh.decodePolyIdTile(@base);
        int tileColor = duIntToCol(tileNum, 128);
        depthMask(false);
        begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < tile.data.header.polyCount; ++i) {
            Poly p = tile.data.polys[i];
            if (p.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
                continue;
            }
            int col;
            if (query != null && query.isInClosedList(@base | i)) {
                col = duRGBA(255, 196, 0, 64);
            } else {
                if ((flags & DRAWNAVMESH_COLOR_TILES) != 0) {
                    col = tileColor;
                } else {
                    if ((p.flags & SampleAreaModifications.SAMPLE_POLYFLAGS_DISABLED) != 0) {
                        col = duRGBA(64, 64, 64, 64);
                    } else {
                        col = duTransCol(areaToCol(p.getArea()), 64);
                    }
                }
            }

            drawPoly(tile, i, col);

        }
        end();

        // Draw inter poly boundaries
        drawPolyBoundaries(tile, duRGBA(0, 48, 64, 32), 1.5f, true);

        // Draw outer poly boundaries
        drawPolyBoundaries(tile, duRGBA(0, 48, 64, 220), 2.5f, false);

        if ((flags & DRAWNAVMESH_OFFMESHCONS) != 0) {
            begin(DebugDrawPrimitives.LINES, 2.0f);
            for (int i = 0; i < tile.data.header.polyCount; ++i) {
                Poly p = tile.data.polys[i];

                if (p.getType() != Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
                    continue;
                }

                int col, col2;
                if (query != null && query.isInClosedList(@base | i)) {
                    col = duRGBA(255, 196, 0, 220);
                } else {
                    col = duDarkenCol(duTransCol(areaToCol(p.getArea()), 220));
                }

                OffMeshConnection con = tile.data.offMeshCons[i - tile.data.header.offMeshBase];
                float[] va = new float[] { tile.data.verts[p.verts[0] * 3], tile.data.verts[p.verts[0] * 3 + 1],
                        tile.data.verts[p.verts[0] * 3 + 2] };
                float[] vb = new float[] { tile.data.verts[p.verts[1] * 3], tile.data.verts[p.verts[1] * 3 + 1],
                        tile.data.verts[p.verts[1] * 3 + 2] };

                // Check to see if start and end end-points have links.
                bool startSet = false;
                bool endSet = false;
                for (int k = tile.polyLinks[p.index]; k != NavMesh.DT_NULL_LINK; k = tile.links[k].next) {
                    if (tile.links[k].edge == 0) {
                        startSet = true;
                    }
                    if (tile.links[k].edge == 1) {
                        endSet = true;
                    }
                }

                // End points and their on-mesh locations.
                vertex(va[0], va[1], va[2], col);
                vertex(con.pos[0], con.pos[1], con.pos[2], col);
                col2 = startSet ? col : duRGBA(220, 32, 16, 196);
                appendCircle(con.pos[0], con.pos[1] + 0.1f, con.pos[2], con.rad, col2);

                vertex(vb[0], vb[1], vb[2], col);
                vertex(con.pos[3], con.pos[4], con.pos[5], col);
                col2 = endSet ? col : duRGBA(220, 32, 16, 196);
                appendCircle(con.pos[3], con.pos[4] + 0.1f, con.pos[5], con.rad, col2);

                // End point vertices.
                vertex(con.pos[0], con.pos[1], con.pos[2], duRGBA(0, 48, 64, 196));
                vertex(con.pos[0], con.pos[1] + 0.2f, con.pos[2], duRGBA(0, 48, 64, 196));

                vertex(con.pos[3], con.pos[4], con.pos[5], duRGBA(0, 48, 64, 196));
                vertex(con.pos[3], con.pos[4] + 0.2f, con.pos[5], duRGBA(0, 48, 64, 196));

                // Connection arc.
                appendArc(con.pos[0], con.pos[1], con.pos[2], con.pos[3], con.pos[4], con.pos[5], 0.25f,
                        (con.flags & 1) != 0 ? 0.6f : 0, 0.6f, col);

            }

            end();
        }

        int vcol = duRGBA(0, 0, 0, 196);
        begin(DebugDrawPrimitives.POINTS, 3.0f);
        for (int i = 0; i < tile.data.header.vertCount; i++) {
            int v = i * 3;
            vertex(tile.data.verts[v], tile.data.verts[v + 1], tile.data.verts[v + 2], vcol);
        }
        end();

        depthMask(true);
    }

    private void drawPoly(MeshTile tile, int index, int col) {
        Poly p = tile.data.polys[index];
        if (tile.data.detailMeshes != null) {
            PolyDetail pd = tile.data.detailMeshes[index];
            if (pd != null) {
                for (int j = 0; j < pd.triCount; ++j) {
                    int t = (pd.triBase + j) * 4;
                    for (int k = 0; k < 3; ++k) {
                        int v = tile.data.detailTris[t + k];
                        if (v < p.vertCount) {
                            vertex(tile.data.verts[p.verts[v] * 3], tile.data.verts[p.verts[v] * 3 + 1],
                                    tile.data.verts[p.verts[v] * 3 + 2], col);
                        } else {
                            vertex(tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3],
                                    tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 1],
                                    tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 2], col);
                        }
                    }
                }
            }
        } else {
            for (int j = 1; j < p.vertCount - 1; ++j) {
                vertex(tile.data.verts[p.verts[0] * 3], tile.data.verts[p.verts[0] * 3 + 1],
                        tile.data.verts[p.verts[0] * 3 + 2], col);
                for (int k = 0; k < 2; ++k) {
                    vertex(tile.data.verts[p.verts[j + k] * 3], tile.data.verts[p.verts[j + k] * 3 + 1],
                            tile.data.verts[p.verts[j + k] * 3 + 2], col);
                }
            }
        }
    }

    void drawPolyBoundaries(MeshTile tile, int col, float linew, bool inner) {
        float thr = 0.01f * 0.01f;

        begin(DebugDrawPrimitives.LINES, linew);

        for (int i = 0; i < tile.data.header.polyCount; ++i) {
            Poly p = tile.data.polys[i];

            if (p.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
                continue;
            }

            for (int j = 0, nj = p.vertCount; j < nj; ++j) {
                int c = col;
                if (inner) {
                    if (p.neis[j] == 0) {
                        continue;
                    }
                    if ((p.neis[j] & NavMesh.DT_EXT_LINK) != 0) {
                        bool con = false;
                        for (int k = tile.polyLinks[p.index]; k != NavMesh.DT_NULL_LINK; k = tile.links[k].next) {
                            if (tile.links[k].edge == j) {
                                con = true;
                                break;
                            }
                        }
                        if (con) {
                            c = duRGBA(255, 255, 255, 48);
                        } else {
                            c = duRGBA(0, 0, 0, 48);
                        }
                    } else {
                        c = duRGBA(0, 48, 64, 32);
                    }
                } else {
                    if (p.neis[j] != 0) {
                        continue;
                    }
                }

                float[] v0 = new float[] { tile.data.verts[p.verts[j] * 3], tile.data.verts[p.verts[j] * 3 + 1],
                        tile.data.verts[p.verts[j] * 3 + 2] };
                float[] v1 = new float[] { tile.data.verts[p.verts[(j + 1) % nj] * 3],
                        tile.data.verts[p.verts[(j + 1) % nj] * 3 + 1],
                        tile.data.verts[p.verts[(j + 1) % nj] * 3 + 2] };

                // Draw detail mesh edges which align with the actual poly edge.
                // This is really slow.
                if (tile.data.detailMeshes != null) {
                    PolyDetail pd = tile.data.detailMeshes[i];
                    for (int k = 0; k < pd.triCount; ++k) {
                        int t = (pd.triBase + k) * 4;
                        float[][] tv = new float[3][];
                        for (int m = 0; m < 3; ++m) {
                            int v = tile.data.detailTris[t + m];
                            if (v < p.vertCount) {
                                tv[m] = new float[] { tile.data.verts[p.verts[v] * 3], tile.data.verts[p.verts[v] * 3 + 1],
                                        tile.data.verts[p.verts[v] * 3 + 2] };
                            } else {
                                tv[m] = new float[] { tile.data.detailVerts[(pd.vertBase + (v - p.vertCount)) * 3],
                                        tile.data.detailVerts[(pd.vertBase + (v - p.vertCount)) * 3 + 1],
                                        tile.data.detailVerts[(pd.vertBase + (v - p.vertCount)) * 3 + 2] };
                            }
                        }
                        for (int m = 0, n = 2; m < 3; n = m++) {
                            if ((NavMesh.getDetailTriEdgeFlags(tile.data.detailTris[t + 3], n) & NavMesh.DT_DETAIL_EDGE_BOUNDARY) == 0)
                                continue;

                            if (((tile.data.detailTris[t + 3] >> (n * 2)) & 0x3) == 0) {
                                continue; // Skip inner detail edges.
                            }
                            if (distancePtLine2d(tv[n], v0, v1) < thr && distancePtLine2d(tv[m], v0, v1) < thr) {
                                vertex(tv[n], c);
                                vertex(tv[m], c);
                            }
                        }
                    }
                } else {
                    vertex(v0, c);
                    vertex(v1, c);
                }
            }
        }
        end();
    }

    static float distancePtLine2d(float[] pt, float[] p, float[] q) {
        float pqx = q[0] - p[0];
        float pqz = q[2] - p[2];
        float dx = pt[0] - p[0];
        float dz = pt[2] - p[2];
        float d = pqx * pqx + pqz * pqz;
        float t = pqx * dx + pqz * dz;
        if (d != 0) {
            t /= d;
        }
        dx = p[0] + t * pqx - pt[0];
        dz = p[2] + t * pqz - pt[2];
        return dx * dx + dz * dz;
    }

    public void debugDrawNavMeshBVTree(NavMesh mesh) {

        for (int i = 0; i < mesh.getMaxTiles(); ++i) {
            MeshTile tile = mesh.getTile(i);
            if (tile != null && tile.data != null && tile.data.header != null) {
                drawMeshTileBVTree(tile);
            }
        }
    }

    private void drawMeshTileBVTree(MeshTile tile) {
        // Draw BV nodes.
        float cs = 1.0f / tile.data.header.bvQuantFactor;
        begin(DebugDrawPrimitives.LINES, 1.0f);
        for (int i = 0; i < tile.data.header.bvNodeCount; ++i) {
            BVNode n = tile.data.bvTree[i];
            if (n.i < 0) {
                continue;
            }
            appendBoxWire(tile.data.header.bmin[0] + n.bmin[0] * cs, tile.data.header.bmin[1] + n.bmin[1] * cs,
                    tile.data.header.bmin[2] + n.bmin[2] * cs, tile.data.header.bmin[0] + n.bmax[0] * cs,
                    tile.data.header.bmin[1] + n.bmax[1] * cs, tile.data.header.bmin[2] + n.bmax[2] * cs,
                    duRGBA(255, 255, 255, 128));
        }
        end();
    }

    public void debugDrawCompactHeightfieldSolid(CompactHeightfield chf) {
        float cs = chf.cs;
        float ch = chf.ch;

        begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < chf.height; ++y) {
            for (int x = 0; x < chf.width; ++x) {
                float fx = chf.bmin[0] + x * cs;
                float fz = chf.bmin[2] + y * cs;
                CompactCell c = chf.cells[x + y * chf.width];

                for (int i = c.index, ni = c.index + c.count; i < ni; ++i) {
                    CompactSpan s = chf.spans[i];

                    int area = chf.areas[i];
                    int color;
                    if (area == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE) {
                        color = duRGBA(0, 192, 255, 64);
                    } else if (area == RecastConstants.RC_NULL_AREA) {
                        color = duRGBA(0, 0, 0, 64);
                    } else {
                        color = areaToCol(area);
                    }

                    float fy = chf.bmin[1] + (s.y + 1) * ch;
                    vertex(fx, fy, fz, color);
                    vertex(fx, fy, fz + cs, color);
                    vertex(fx + cs, fy, fz + cs, color);
                    vertex(fx + cs, fy, fz, color);
                }
            }
        }
        end();
    }

    public void debugDrawRegionConnections(ContourSet cset) {
        float alpha = 1f;

        float[] orig = cset.bmin;
        float cs = cset.cs;
        float ch = cset.ch;

        int color = duRGBA(0, 0, 0, 196);

        begin(DebugDrawPrimitives.LINES, 2.0f);

        for (int i = 0; i < cset.conts.Count; ++i) {
            Contour cont = cset.conts[i];
            float[] pos = getContourCenter(cont, orig, cs, ch);
            for (int j = 0; j < cont.nverts; ++j) {
                int v = j * 4;
                if (cont.verts[v + 3] == 0 || (short) cont.verts[v + 3] < cont.reg) {
                    continue;
                }
                Contour cont2 = findContourFromSet(cset, (short) cont.verts[v + 3]);
                if (cont2 != null) {
                    float[] pos2 = getContourCenter(cont2, orig, cs, ch);
                    appendArc(pos[0], pos[1], pos[2], pos2[0], pos2[1], pos2[2], 0.25f, 0.6f, 0.6f, color);
                }
            }
        }

        end();

        char a = (char) (alpha * 255.0f);

        begin(DebugDrawPrimitives.POINTS, 7.0f);

        for (int i = 0; i < cset.conts.Count; ++i) {
            Contour cont = cset.conts[i];
            int col = duDarkenCol(duIntToCol(cont.reg, a));
            float[] pos = getContourCenter(cont, orig, cs, ch);
            vertex(pos, col);
        }
        end();
    }

    private float[] getContourCenter(Contour cont, float[] orig, float cs, float ch) {
        float[] center = new float[3];
        center[0] = 0;
        center[1] = 0;
        center[2] = 0;
        if (cont.nverts == 0) {
            return center;
        }
        for (int i = 0; i < cont.nverts; ++i) {
            int v = i * 4;
            center[0] += cont.verts[v + 0];
            center[1] += cont.verts[v + 1];
            center[2] += cont.verts[v + 2];
        }
        float s = 1.0f / cont.nverts;
        center[0] *= s * cs;
        center[1] *= s * ch;
        center[2] *= s * cs;
        center[0] += orig[0];
        center[1] += orig[1] + 4 * ch;
        center[2] += orig[2];
        return center;
    }

    private Contour findContourFromSet(ContourSet cset, int reg) {
        for (int i = 0; i < cset.conts.Count; ++i) {
            if (cset.conts[i].reg == reg) {
                return cset.conts[i];
            }
        }
        return null;
    }

    public void debugDrawRawContours(ContourSet cset, float alpha) {

        float[] orig = cset.bmin;
        float cs = cset.cs;
        float ch = cset.ch;

        char a = (char) (alpha * 255.0f);

        begin(DebugDrawPrimitives.LINES, 2.0f);

        for (int i = 0; i < cset.conts.Count; ++i) {
            Contour c = cset.conts[i];
            int color = duIntToCol(c.reg, a);

            for (int j = 0; j < c.nrverts; ++j) {
                int v0 = c.rverts[j * 4];
                int v1 = c.rverts[j * 4 + 1];
                int v2 = c.rverts[j * 4 + 2];
                float fx = orig[0] + v0 * cs;
                float fy = orig[1] + (v1 + 1 + (i & 1)) * ch;
                float fz = orig[2] + v2 * cs;
                vertex(fx, fy, fz, color);
                if (j > 0) {
                    vertex(fx, fy, fz, color);
                }
            }

            // Loop last segment.
            {
                int v0 = c.rverts[0];
                int v1 = c.rverts[1];
                int v2 = c.rverts[2];
                float fx = orig[0] + v0 * cs;
                float fy = orig[1] + (v1 + 1 + (i & 1)) * ch;
                float fz = orig[2] + v2 * cs;
                vertex(fx, fy, fz, color);
            }
        }
        end();

        begin(DebugDrawPrimitives.POINTS, 2.0f);

        for (int i = 0; i < cset.conts.Count; ++i) {
            Contour c = cset.conts[i];
            int color = duDarkenCol(duIntToCol(c.reg, a));

            for (int j = 0; j < c.nrverts; ++j) {
                int v0 = c.rverts[j * 4];
                int v1 = c.rverts[j * 4 + 1];
                int v2 = c.rverts[j * 4 + 2];
                int v3 = c.rverts[j * 4 + 3];
                float off = 0;
                int colv = color;
                if ((v3 & RecastConstants.RC_BORDER_VERTEX) != 0) {
                    colv = duRGBA(255, 255, 255, a);
                    off = ch * 2;
                }

                float fx = orig[0] + v0 * cs;
                float fy = orig[1] + (v1 + 1 + (i & 1)) * ch + off;
                float fz = orig[2] + v2 * cs;
                vertex(fx, fy, fz, colv);
            }
        }
        end();
    }

    public void debugDrawContours(ContourSet cset) {
        float alpha = 1f;
        float[] orig = cset.bmin;
        float cs = cset.cs;
        float ch = cset.ch;

        char a = (char) (alpha * 255.0f);

        begin(DebugDrawPrimitives.LINES, 2.5f);

        for (int i = 0; i < cset.conts.Count; ++i) {
            Contour c = cset.conts[i];
            if (c.nverts == 0) {
                continue;
            }
            int color = duIntToCol(c.reg, a);
            int bcolor = duLerpCol(color, duRGBA(255, 255, 255, a), 128);

            for (int j = 0, k = c.nverts - 1; j < c.nverts; k = j++) {
                int va0 = c.verts[k * 4];
                int va1 = c.verts[k * 4 + 1];
                int va2 = c.verts[k * 4 + 2];
                int va3 = c.verts[k * 4 + 3];
                int vb0 = c.verts[j * 4];
                int vb1 = c.verts[j * 4 + 1];
                int vb2 = c.verts[j * 4 + 2];
                int col = (va3 & RecastConstants.RC_AREA_BORDER) != 0 ? bcolor : color;

                float fx = orig[0] + va0 * cs;
                float fy = orig[1] + (va1 + 1 + (i & 1)) * ch;
                float fz = orig[2] + va2 * cs;
                vertex(fx, fy, fz, col);

                fx = orig[0] + vb0 * cs;
                fy = orig[1] + (vb1 + 1 + (i & 1)) * ch;
                fz = orig[2] + vb2 * cs;
                vertex(fx, fy, fz, col);
            }
        }
        end();

        begin(DebugDrawPrimitives.POINTS, 3.0f);

        for (int i = 0; i < cset.conts.Count; ++i) {
            Contour c = cset.conts[i];
            int color = duDarkenCol(duIntToCol(c.reg, a));

            for (int j = 0; j < c.nverts; ++j) {
                int v0 = c.verts[j * 4];
                int v1 = c.verts[j * 4 + 1];
                int v2 = c.verts[j * 4 + 2];
                int v3 = c.verts[j * 4 + 3];
                float off = 0;
                int colv = color;
                if ((v3 & RecastConstants.RC_BORDER_VERTEX) != 0) {
                    colv = duRGBA(255, 255, 255, a);
                    off = ch * 2;
                }

                float fx = orig[0] + v0 * cs;
                float fy = orig[1] + (v1 + 1 + (i & 1)) * ch + off;
                float fz = orig[2] + v2 * cs;
                vertex(fx, fy, fz, colv);
            }
        }
        end();
    }

    public void debugDrawHeightfieldSolid(Heightfield hf) {

        if (!frustumTest(hf.bmin, hf.bmax)) {
            return;
        }

        float[] orig = hf.bmin;
        float cs = hf.cs;
        float ch = hf.ch;

        int w = hf.width;
        int h = hf.height;

        int[] fcol = new int[6];
        duCalcBoxColors(fcol, duRGBA(255, 255, 255, 255), duRGBA(255, 255, 255, 255));

        begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                float fx = orig[0] + x * cs;
                float fz = orig[2] + y * cs;
                Span s = hf.spans[x + y * w];
                while (s != null) {
                    appendBox(fx, orig[1] + s.smin * ch, fz, fx + cs, orig[1] + s.smax * ch, fz + cs, fcol);
                    s = s.next;
                }
            }
        }
        end();
    }

    public void debugDrawHeightfieldWalkable(Heightfield hf) {
        float[] orig = hf.bmin;
        float cs = hf.cs;
        float ch = hf.ch;

        int w = hf.width;
        int h = hf.height;

        int[] fcol = new int[6];
        duCalcBoxColors(fcol, duRGBA(255, 255, 255, 255), duRGBA(217, 217, 217, 255));

        begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                float fx = orig[0] + x * cs;
                float fz = orig[2] + y * cs;
                Span s = hf.spans[x + y * w];
                while (s != null) {
                    if (s.area == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE) {
                        fcol[0] = duRGBA(64, 128, 160, 255);
                    } else if (s.area == RecastConstants.RC_NULL_AREA) {
                        fcol[0] = duRGBA(64, 64, 64, 255);
                    } else {
                        fcol[0] = duMultCol(areaToCol(s.area), 200);
                    }

                    appendBox(fx, orig[1] + s.smin * ch, fz, fx + cs, orig[1] + s.smax * ch, fz + cs, fcol);
                    s = s.next;
                }
            }
        }

        end();
    }

    public void debugDrawCompactHeightfieldRegions(CompactHeightfield chf) {
        float cs = chf.cs;
        float ch = chf.ch;

        begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < chf.height; ++y) {
            for (int x = 0; x < chf.width; ++x) {
                float fx = chf.bmin[0] + x * cs;
                float fz = chf.bmin[2] + y * cs;
                CompactCell c = chf.cells[x + y * chf.width];

                for (int i = c.index, ni = c.index + c.count; i < ni; ++i) {
                    CompactSpan s = chf.spans[i];
                    float fy = chf.bmin[1] + (s.y) * ch;
                    int color;
                    if (s.reg != 0) {
                        color = duIntToCol(s.reg, 192);
                    } else {
                        color = duRGBA(0, 0, 0, 64);
                    }

                    vertex(fx, fy, fz, color);
                    vertex(fx, fy, fz + cs, color);
                    vertex(fx + cs, fy, fz + cs, color);
                    vertex(fx + cs, fy, fz, color);
                }
            }
        }

        end();
    }

    public void debugDrawCompactHeightfieldDistance(CompactHeightfield chf) {
        if (chf.dist == null) {
            return;
        }

        float cs = chf.cs;
        float ch = chf.ch;

        float maxd = chf.maxDistance;
        if (maxd < 1.0f) {
            maxd = 1;
        }
        float dscale = 255.0f / maxd;

        begin(DebugDrawPrimitives.QUADS);

        for (int y = 0; y < chf.height; ++y) {
            for (int x = 0; x < chf.width; ++x) {
                float fx = chf.bmin[0] + x * cs;
                float fz = chf.bmin[2] + y * cs;
                CompactCell c = chf.cells[x + y * chf.width];

                for (int i = c.index, ni = c.index + c.count; i < ni; ++i) {
                    CompactSpan s = chf.spans[i];
                    float fy = chf.bmin[1] + (s.y + 1) * ch;
                    char cd = (char) (chf.dist[i] * dscale);
                    int color = duRGBA(cd, cd, cd, 255);
                    vertex(fx, fy, fz, color);
                    vertex(fx, fy, fz + cs, color);
                    vertex(fx + cs, fy, fz + cs, color);
                    vertex(fx + cs, fy, fz, color);
                }
            }
        }
        end();
    }

    public void debugDrawPolyMesh(PolyMesh mesh) {
        int nvp = mesh.nvp;
        float cs = mesh.cs;
        float ch = mesh.ch;
        float[] orig = mesh.bmin;

        begin(DebugDrawPrimitives.TRIS);

        for (int i = 0; i < mesh.npolys; ++i) {
            int p = i * nvp * 2;
            int area = mesh.areas[i];

            int color;
            if (area == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE) {
                color = duRGBA(0, 192, 255, 64);
            } else if (area == RecastConstants.RC_NULL_AREA) {
                color = duRGBA(0, 0, 0, 64);
            } else {
                color = areaToCol(area);
            }

            int[] vi = new int[3];
            for (int j = 2; j < nvp; ++j) {
                if (mesh.polys[p + j] == RecastConstants.RC_MESH_NULL_IDX) {
                    break;
                }
                vi[0] = mesh.polys[p + 0];
                vi[1] = mesh.polys[p + j - 1];
                vi[2] = mesh.polys[p + j];
                for (int k = 0; k < 3; ++k) {
                    int v0 = mesh.verts[vi[k] * 3];
                    int v1 = mesh.verts[vi[k] * 3 + 1];
                    int v2 = mesh.verts[vi[k] * 3 + 2];
                    float x = orig[0] + v0 * cs;
                    float y = orig[1] + (v1 + 1) * ch;
                    float z = orig[2] + v2 * cs;
                    vertex(x, y, z, color);
                }
            }
        }
        end();

        // Draw neighbours edges
        int coln = duRGBA(0, 48, 64, 32);
        begin(DebugDrawPrimitives.LINES, 1.5f);
        for (int i = 0; i < mesh.npolys; ++i) {
            int p = i * nvp * 2;
            for (int j = 0; j < nvp; ++j) {
                if (mesh.polys[p + j] == RecastConstants.RC_MESH_NULL_IDX) {
                    break;
                }
                if ((mesh.polys[p + nvp + j] & 0x8000) != 0) {
                    continue;
                }
                int nj = (j + 1 >= nvp || mesh.polys[p + j + 1] == RecastConstants.RC_MESH_NULL_IDX) ? 0 : j + 1;
                int[] vi = { mesh.polys[p + j], mesh.polys[p + nj] };

                for (int k = 0; k < 2; ++k) {
                    int v = vi[k] * 3;
                    float x = orig[0] + mesh.verts[v] * cs;
                    float y = orig[1] + (mesh.verts[v + 1] + 1) * ch + 0.1f;
                    float z = orig[2] + mesh.verts[v + 2] * cs;
                    vertex(x, y, z, coln);
                }
            }
        }
        end();

        // Draw boundary edges
        int colb = duRGBA(0, 48, 64, 220);
        begin(DebugDrawPrimitives.LINES, 2.5f);
        for (int i = 0; i < mesh.npolys; ++i) {
            int p = i * nvp * 2;
            for (int j = 0; j < nvp; ++j) {
                if (mesh.polys[p + j] == RecastConstants.RC_MESH_NULL_IDX) {
                    break;
                }
                if ((mesh.polys[p + nvp + j] & 0x8000) == 0) {
                    continue;
                }
                int nj = (j + 1 >= nvp || mesh.polys[p + j + 1] == RecastConstants.RC_MESH_NULL_IDX) ? 0 : j + 1;
                int[] vi = { mesh.polys[p + j], mesh.polys[p + nj] };

                int col = colb;
                if ((mesh.polys[p + nvp + j] & 0xf) != 0xf) {
                    col = duRGBA(255, 255, 255, 128);
                }
                for (int k = 0; k < 2; ++k) {
                    int v = vi[k] * 3;
                    float x = orig[0] + mesh.verts[v] * cs;
                    float y = orig[1] + (mesh.verts[v + 1] + 1) * ch + 0.1f;
                    float z = orig[2] + mesh.verts[v + 2] * cs;
                    vertex(x, y, z, col);
                }
            }
        }
        end();

        begin(DebugDrawPrimitives.POINTS, 3.0f);
        int colv = duRGBA(0, 0, 0, 220);
        for (int i = 0; i < mesh.nverts; ++i) {
            int v = i * 3;
            float x = orig[0] + mesh.verts[v] * cs;
            float y = orig[1] + (mesh.verts[v + 1] + 1) * ch + 0.1f;
            float z = orig[2] + mesh.verts[v + 2] * cs;
            vertex(x, y, z, colv);
        }
        end();
    }

    public void debugDrawPolyMeshDetail(PolyMeshDetail dmesh) {

        begin(DebugDrawPrimitives.TRIS);

        for (int i = 0; i < dmesh.nmeshes; ++i) {
            int m = i * 4;
            int bverts = dmesh.meshes[m];
            int btris = dmesh.meshes[m + 2];
            int ntris = dmesh.meshes[m + 3];
            int verts = bverts * 3;
            int tris = btris * 4;

            int color = duIntToCol(i, 192);

            for (int j = 0; j < ntris; ++j) {
                vertex(dmesh.verts[verts + dmesh.tris[tris + j * 4 + 0] * 3],
                        dmesh.verts[verts + dmesh.tris[tris + j * 4 + 0] * 3 + 1],
                        dmesh.verts[verts + dmesh.tris[tris + j * 4 + 0] * 3 + 2], color);
                vertex(dmesh.verts[verts + dmesh.tris[tris + j * 4 + 1] * 3],
                        dmesh.verts[verts + dmesh.tris[tris + j * 4 + 1] * 3 + 1],
                        dmesh.verts[verts + dmesh.tris[tris + j * 4 + 1] * 3 + 2], color);
                vertex(dmesh.verts[verts + dmesh.tris[tris + j * 4 + 2] * 3],
                        dmesh.verts[verts + dmesh.tris[tris + j * 4 + 2] * 3 + 1],
                        dmesh.verts[verts + dmesh.tris[tris + j * 4 + 2] * 3 + 2], color);
            }
        }
        end();

        // Internal edges.
        begin(DebugDrawPrimitives.LINES, 1.0f);
        int coli = duRGBA(0, 0, 0, 64);
        for (int i = 0; i < dmesh.nmeshes; ++i) {
            int m = i * 4;
            int bverts = dmesh.meshes[m];
            int btris = dmesh.meshes[m + 2];
            int ntris = dmesh.meshes[m + 3];
            int verts = bverts * 3;
            int tris = btris * 4;

            for (int j = 0; j < ntris; ++j) {
                int t = tris + j * 4;
                for (int k = 0, kp = 2; k < 3; kp = k++) {
                    int ef = (dmesh.tris[t + 3] >> (kp * 2)) & 0x3;
                    if (ef == 0) {
                        // Internal edge
                        if (dmesh.tris[t + kp] < dmesh.tris[t + k]) {
                            vertex(dmesh.verts[verts + dmesh.tris[t + kp] * 3],
                                    dmesh.verts[verts + dmesh.tris[t + kp] * 3 + 1],
                                    dmesh.verts[verts + dmesh.tris[t + kp] * 3 + 2], coli);
                            vertex(dmesh.verts[verts + dmesh.tris[t + k] * 3],
                                    dmesh.verts[verts + dmesh.tris[t + k] * 3 + 1],
                                    dmesh.verts[verts + dmesh.tris[t + k] * 3 + 2], coli);
                        }
                    }
                }
            }
        }
        end();

        // External edges.
        begin(DebugDrawPrimitives.LINES, 2.0f);
        int cole = duRGBA(0, 0, 0, 64);
        for (int i = 0; i < dmesh.nmeshes; ++i) {
            int m = i * 4;
            int bverts = dmesh.meshes[m];
            int btris = dmesh.meshes[m + 2];
            int ntris = dmesh.meshes[m + 3];
            int verts = bverts * 3;
            int tris = btris * 4;

            for (int j = 0; j < ntris; ++j) {
                int t = tris + j * 4;
                for (int k = 0, kp = 2; k < 3; kp = k++) {
                    int ef = (dmesh.tris[t + 3] >> (kp * 2)) & 0x3;
                    if (ef != 0) {
                        // Ext edge
                        vertex(dmesh.verts[verts + dmesh.tris[t + kp] * 3],
                                dmesh.verts[verts + dmesh.tris[t + kp] * 3 + 1],
                                dmesh.verts[verts + dmesh.tris[t + kp] * 3 + 2], cole);
                        vertex(dmesh.verts[verts + dmesh.tris[t + k] * 3],
                                dmesh.verts[verts + dmesh.tris[t + k] * 3 + 1],
                                dmesh.verts[verts + dmesh.tris[t + k] * 3 + 2], cole);
                    }
                }
            }
        }
        end();

        begin(DebugDrawPrimitives.POINTS, 3.0f);
        int colv = duRGBA(0, 0, 0, 64);
        for (int i = 0; i < dmesh.nmeshes; ++i) {
            int m = i * 4;
            int bverts = dmesh.meshes[m];
            int nverts = dmesh.meshes[m + 1];
            int verts = bverts * 3;
            for (int j = 0; j < nverts; ++j) {
                vertex(dmesh.verts[verts + j * 3], dmesh.verts[verts + j * 3 + 1], dmesh.verts[verts + j * 3 + 2],
                        colv);
            }
        }
        end();
    }

    public void debugDrawNavMeshNodes(NavMeshQuery query) {
        NodePool pool = query.getNodePool();
        if (pool != null) {
            float off = 0.5f;
            begin(DebugDrawPrimitives.POINTS, 4.0f);

            foreach (List<Node> nodes in pool.getNodeMap().Values) {

                foreach (Node node in nodes) {
                    if (node == null) {
                        continue;
                    }
                    vertex(node.pos[0], node.pos[1] + off, node.pos[2], duRGBA(255, 192, 0, 255));
                }
            }
            end();

            begin(DebugDrawPrimitives.LINES, 2.0f);
            foreach (List<Node> nodes in pool.getNodeMap().Values) {

                foreach (Node node in nodes) {
                    if (node == null) {
                        continue;
                    }
                    if (node.pidx == 0) {
                        continue;
                    }
                    Node parent = pool.getNodeAtIdx(node.pidx);
                    if (parent == null) {
                        continue;
                    }
                    vertex(node.pos[0], node.pos[1] + off, node.pos[2], duRGBA(255, 192, 0, 128));
                    vertex(parent.pos[0], parent.pos[1] + off, parent.pos[2], duRGBA(255, 192, 0, 128));
                }
            }
            end();
        }
    }

    public void debugDrawNavMeshPolysWithFlags(NavMesh mesh, int polyFlags, int col) {

        for (int i = 0; i < mesh.getMaxTiles(); ++i) {
            MeshTile tile = mesh.getTile(i);
            if (tile == null || tile.data == null || tile.data.header == null) {
                continue;
            }
            long @base = mesh.getPolyRefBase(tile);

            for (int j = 0; j < tile.data.header.polyCount; ++j) {
                Poly p = tile.data.polys[j];
                if ((p.flags & polyFlags) == 0) {
                    continue;
                }
                debugDrawNavMeshPoly(mesh, @base | j, col);
            }
        }
    }

    public void debugDrawNavMeshPoly(NavMesh mesh, long refs, int col) {
        if (refs == 0) {
            return;
        }
        Result<Tuple<MeshTile, Poly>> tileAndPolyResult = mesh.getTileAndPolyByRef(refs);
        if (tileAndPolyResult.failed()) {
            return;
        }
        Tuple<MeshTile, Poly> tileAndPoly = tileAndPolyResult.result;
        MeshTile tile = tileAndPoly.Item1;
        Poly poly = tileAndPoly.Item2;

        depthMask(false);

        int c = duTransCol(col, 64);
        int ip = poly.index;

        if (poly.getType() == Poly.DT_POLYTYPE_OFFMESH_CONNECTION) {
            OffMeshConnection con = tile.data.offMeshCons[ip - tile.data.header.offMeshBase];

            begin(DebugDrawPrimitives.LINES, 2.0f);

            // Connection arc.
            appendArc(con.pos[0], con.pos[1], con.pos[2], con.pos[3], con.pos[4], con.pos[5], 0.25f,
                    (con.flags & 1) != 0 ? 0.6f : 0.0f, 0.6f, c);

            end();
        } else {
            begin(DebugDrawPrimitives.TRIS);
            drawPoly(tile, ip, col);
            end();
        }

        depthMask(true);

    }

    public void debugDrawNavMeshPortals(NavMesh mesh) {
        for (int i = 0; i < mesh.getMaxTiles(); ++i) {
            MeshTile tile = mesh.getTile(i);
            if (tile.data != null && tile.data.header != null) {
                drawMeshTilePortal(tile);
            }
        }
    }

    private void drawMeshTilePortal(MeshTile tile) {
        float padx = 0.04f;
        float pady = tile.data.header.walkableClimb;

        begin(DebugDrawPrimitives.LINES, 2.0f);

        for (int side = 0; side < 8; ++side) {
            int m = NavMesh.DT_EXT_LINK | (short) side;

            for (int i = 0; i < tile.data.header.polyCount; ++i) {
                Poly poly = tile.data.polys[i];

                // Create new links.
                int nv = poly.vertCount;
                for (int j = 0; j < nv; ++j) {
                    // Skip edges which do not point to the right side.
                    if (poly.neis[j] != m)
                        continue;

                    // Create new links
                    float[] va = new float[] { tile.data.verts[poly.verts[j] * 3],
                            tile.data.verts[poly.verts[j] * 3 + 1], tile.data.verts[poly.verts[j] * 3 + 2] };
                    float[] vb = new float[] { tile.data.verts[poly.verts[(j + 1) % nv] * 3],
                            tile.data.verts[poly.verts[(j + 1) % nv] * 3 + 1],
                            tile.data.verts[poly.verts[(j + 1) % nv] * 3 + 2] };

                    if (side == 0 || side == 4) {
                        int col = side == 0 ? duRGBA(128, 0, 0, 128) : duRGBA(128, 0, 128, 128);

                        float x = va[0] + ((side == 0) ? -padx : padx);

                        vertex(x, va[1] - pady, va[2], col);
                        vertex(x, va[1] + pady, va[2], col);

                        vertex(x, va[1] + pady, va[2], col);
                        vertex(x, vb[1] + pady, vb[2], col);

                        vertex(x, vb[1] + pady, vb[2], col);
                        vertex(x, vb[1] - pady, vb[2], col);

                        vertex(x, vb[1] - pady, vb[2], col);
                        vertex(x, va[1] - pady, va[2], col);
                    } else if (side == 2 || side == 6) {
                        int col = side == 2 ? duRGBA(0, 128, 0, 128) : duRGBA(0, 128, 128, 128);

                        float z = va[2] + ((side == 2) ? -padx : padx);

                        vertex(va[0], va[1] - pady, z, col);
                        vertex(va[0], va[1] + pady, z, col);

                        vertex(va[0], va[1] + pady, z, col);
                        vertex(vb[0], vb[1] + pady, z, col);

                        vertex(vb[0], vb[1] + pady, z, col);
                        vertex(vb[0], vb[1] - pady, z, col);

                        vertex(vb[0], vb[1] - pady, z, col);
                        vertex(va[0], va[1] - pady, z, col);
                    }

                }
            }
        }

        end();

    }

}
