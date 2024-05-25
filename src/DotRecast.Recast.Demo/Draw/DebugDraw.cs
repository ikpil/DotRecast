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
using Silk.NET.OpenGL;
using DotRecast.Core.Numerics;
using DotRecast.Recast.Toolset.Builder;

namespace DotRecast.Recast.Demo.Draw;

public class DebugDraw
{
    private readonly GLCheckerTexture g_tex;
    private readonly ModernOpenGLDraw openGlDraw;

    public DebugDraw(GL gl)
    {
        g_tex = new GLCheckerTexture(gl);
        openGlDraw = new ModernOpenGLDraw(gl);
    }

    private ModernOpenGLDraw GetOpenGlDraw()
    {
        return openGlDraw;
    }

    public void Init(float fogDistance)
    {
        GetOpenGlDraw().Init();
    }

    public void Clear()
    {
        GetOpenGlDraw().Clear();
    }

    public void End()
    {
        GetOpenGlDraw().End();
    }

    public void Begin(DebugDrawPrimitives prim)
    {
        Begin(prim, 1f);
    }

    public void Begin(DebugDrawPrimitives prim, float size)
    {
        GetOpenGlDraw().Begin(prim, size);
    }


    public void Fog(float start, float end)
    {
        GetOpenGlDraw().Fog(start, end);
    }

    public void Fog(bool state)
    {
        GetOpenGlDraw().Fog(state);
    }

    public void DepthMask(bool state)
    {
        GetOpenGlDraw().DepthMask(state);
    }

    public void Texture(bool state)
    {
        GetOpenGlDraw().Texture(g_tex, state);
    }

    public void Vertex(float[] pos, int color)
    {
        GetOpenGlDraw().Vertex(pos, color);
    }

    public void Vertex(RcVec3f pos, int color)
    {
        GetOpenGlDraw().Vertex(pos, color);
    }

    public void Vertex(float x, float y, float z, int color)
    {
        GetOpenGlDraw().Vertex(x, y, z, color);
    }

    public void Vertex(RcVec3f pos, int color, RcVec2f uv)
    {
        GetOpenGlDraw().Vertex(pos, color, uv);
    }

    public void Vertex(float x, float y, float z, int color, float u, float v)
    {
        GetOpenGlDraw().Vertex(x, y, z, color, u, v);
    }


    public void DebugDrawCylinderWire(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col,
        float lineWidth)
    {
        Begin(DebugDrawPrimitives.LINES, lineWidth);
        AppendCylinderWire(minx, miny, minz, maxx, maxy, maxz, col);
        End();
    }

    private const int CYLINDER_NUM_SEG = 16;
    private readonly float[] cylinderDir = new float[CYLINDER_NUM_SEG * 2];
    private bool cylinderInit = false;

    private void InitCylinder()
    {
        if (!cylinderInit)
        {
            cylinderInit = true;
            for (int i = 0; i < CYLINDER_NUM_SEG; ++i)
            {
                float a = (float)(i * MathF.PI * 2 / CYLINDER_NUM_SEG);
                cylinderDir[i * 2] = MathF.Cos(a);
                cylinderDir[i * 2 + 1] = MathF.Sin(a);
            }
        }
    }

    void AppendCylinderWire(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col)
    {
        InitCylinder();

        float cx = (maxx + minx) / 2;
        float cz = (maxz + minz) / 2;
        float rx = (maxx - minx) / 2;
        float rz = (maxz - minz) / 2;

        for (int i = 0, j = CYLINDER_NUM_SEG - 1; i < CYLINDER_NUM_SEG; j = i++)
        {
            Vertex(cx + cylinderDir[j * 2 + 0] * rx, miny, cz + cylinderDir[j * 2 + 1] * rz, col);
            Vertex(cx + cylinderDir[i * 2 + 0] * rx, miny, cz + cylinderDir[i * 2 + 1] * rz, col);
            Vertex(cx + cylinderDir[j * 2 + 0] * rx, maxy, cz + cylinderDir[j * 2 + 1] * rz, col);
            Vertex(cx + cylinderDir[i * 2 + 0] * rx, maxy, cz + cylinderDir[i * 2 + 1] * rz, col);
        }

        for (int i = 0; i < CYLINDER_NUM_SEG; i += CYLINDER_NUM_SEG / 4)
        {
            Vertex(cx + cylinderDir[i * 2 + 0] * rx, miny, cz + cylinderDir[i * 2 + 1] * rz, col);
            Vertex(cx + cylinderDir[i * 2 + 0] * rx, maxy, cz + cylinderDir[i * 2 + 1] * rz, col);
        }
    }

    public void DebugDrawBoxWire(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col,
        float lineWidth)
    {
        Begin(DebugDrawPrimitives.LINES, lineWidth);
        AppendBoxWire(minx, miny, minz, maxx, maxy, maxz, col);
        End();
    }

    public void DebugDrawGridXZ(float ox, float oy, float oz, int w, int h, float size, int col, float lineWidth)
    {
        Begin(DebugDrawPrimitives.LINES, lineWidth);
        for (int i = 0; i <= h; ++i)
        {
            Vertex(ox, oy, oz + i * size, col);
            Vertex(ox + w * size, oy, oz + i * size, col);
        }

        for (int i = 0; i <= w; ++i)
        {
            Vertex(ox + i * size, oy, oz, col);
            Vertex(ox + i * size, oy, oz + h * size, col);
        }

        End();
    }


    public void AppendBoxWire(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col)
    {
        // Top
        Vertex(minx, miny, minz, col);
        Vertex(maxx, miny, minz, col);
        Vertex(maxx, miny, minz, col);
        Vertex(maxx, miny, maxz, col);
        Vertex(maxx, miny, maxz, col);
        Vertex(minx, miny, maxz, col);
        Vertex(minx, miny, maxz, col);
        Vertex(minx, miny, minz, col);

        // bottom
        Vertex(minx, maxy, minz, col);
        Vertex(maxx, maxy, minz, col);
        Vertex(maxx, maxy, minz, col);
        Vertex(maxx, maxy, maxz, col);
        Vertex(maxx, maxy, maxz, col);
        Vertex(minx, maxy, maxz, col);
        Vertex(minx, maxy, maxz, col);
        Vertex(minx, maxy, minz, col);

        // Sides
        Vertex(minx, miny, minz, col);
        Vertex(minx, maxy, minz, col);
        Vertex(maxx, miny, minz, col);
        Vertex(maxx, maxy, minz, col);
        Vertex(maxx, miny, maxz, col);
        Vertex(maxx, maxy, maxz, col);
        Vertex(minx, miny, maxz, col);
        Vertex(minx, maxy, maxz, col);
    }

    private readonly int[] boxIndices = { 7, 6, 5, 4, 0, 1, 2, 3, 1, 5, 6, 2, 3, 7, 4, 0, 2, 6, 7, 3, 0, 4, 5, 1, };

    private readonly float[][] boxVerts =
    {
        new[] { 0f, 0f, 0f },
        new[] { 0f, 0f, 0f },
        new[] { 0f, 0f, 0f },
        new[] { 0f, 0f, 0f },
        new[] { 0f, 0f, 0f },
        new[] { 0f, 0f, 0f },
        new[] { 0f, 0f, 0f },
        new[] { 0f, 0f, 0f }
    };

    public void AppendBox(float minx, float miny, float minz, float maxx, float maxy, float maxz, int[] fcol)
    {
        boxVerts[0][0] = minx;
        boxVerts[0][1] = miny;
        boxVerts[0][2] = minz;

        boxVerts[1][0] = maxx;
        boxVerts[1][1] = miny;
        boxVerts[1][2] = minz;

        boxVerts[2][0] = maxx;
        boxVerts[2][1] = miny;
        boxVerts[2][2] = maxz;

        boxVerts[3][0] = minx;
        boxVerts[3][1] = miny;
        boxVerts[3][2] = maxz;

        boxVerts[4][0] = minx;
        boxVerts[4][1] = maxy;
        boxVerts[4][2] = minz;

        boxVerts[5][0] = maxx;
        boxVerts[5][1] = maxy;
        boxVerts[5][2] = minz;

        boxVerts[6][0] = maxx;
        boxVerts[6][1] = maxy;
        boxVerts[6][2] = maxz;

        boxVerts[7][0] = minx;
        boxVerts[7][1] = maxy;
        boxVerts[7][2] = maxz;

        int idx = 0;
        for (int i = 0; i < 6; ++i)
        {
            Vertex(boxVerts[boxIndices[idx]], fcol[i]);
            idx++;
            Vertex(boxVerts[boxIndices[idx]], fcol[i]);
            idx++;
            Vertex(boxVerts[boxIndices[idx]], fcol[i]);
            idx++;
            Vertex(boxVerts[boxIndices[idx]], fcol[i]);
            idx++;
        }
    }

    public void DebugDrawArc(float x0, float y0, float z0, float x1, float y1, float z1, float h, float as0, float as1, int col,
        float lineWidth)
    {
        Begin(DebugDrawPrimitives.LINES, lineWidth);
        AppendArc(x0, y0, z0, x1, y1, z1, h, as0, as1, col);
        End();
    }

    public void DebugDrawCircle(float x, float y, float z, float r, int col, float lineWidth)
    {
        Begin(DebugDrawPrimitives.LINES, lineWidth);
        AppendCircle(x, y, z, r, col);
        End();
    }

    private bool circleInit = false;
    private const int CIRCLE_NUM_SEG = 40;
    private readonly float[] circeDir = new float[CIRCLE_NUM_SEG * 2];
    private RcMatrix4x4f _viewMatrix = new();
    private RcMatrix4x4f _projectionMatrix = new();

    public void AppendCircle(float x, float y, float z, float r, int col)
    {
        if (!circleInit)
        {
            circleInit = true;
            for (int i = 0; i < CIRCLE_NUM_SEG; ++i)
            {
                float a = (float)(i * MathF.PI * 2 / CIRCLE_NUM_SEG);
                circeDir[i * 2] = MathF.Cos(a);
                circeDir[i * 2 + 1] = MathF.Sin(a);
            }
        }

        for (int i = 0, j = CIRCLE_NUM_SEG - 1; i < CIRCLE_NUM_SEG; j = i++)
        {
            Vertex(x + circeDir[j * 2 + 0] * r, y, z + circeDir[j * 2 + 1] * r, col);
            Vertex(x + circeDir[i * 2 + 0] * r, y, z + circeDir[i * 2 + 1] * r, col);
        }
    }

    private static readonly int NUM_ARC_PTS = 8;
    private static readonly float PAD = 0.05f;
    private static readonly float ARC_PTS_SCALE = (1.0f - PAD * 2) / NUM_ARC_PTS;

    public void AppendArc(float x0, float y0, float z0, float x1, float y1, float z1, float h, float as0, float as1, int col)
    {
        float dx = x1 - x0;
        float dy = y1 - y0;
        float dz = z1 - z0;
        float len = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        RcVec3f prev = new RcVec3f();
        EvalArc(x0, y0, z0, dx, dy, dz, len * h, PAD, ref prev);
        for (int i = 1; i <= NUM_ARC_PTS; ++i)
        {
            float u = PAD + i * ARC_PTS_SCALE;
            RcVec3f pt = new RcVec3f();
            EvalArc(x0, y0, z0, dx, dy, dz, len * h, u, ref pt);
            Vertex(prev.X, prev.Y, prev.Z, col);
            Vertex(pt.X, pt.Y, pt.Z, col);
            prev.X = pt.X;
            prev.Y = pt.Y;
            prev.Z = pt.Z;
        }

        // End arrows
        if (as0 > 0.001f)
        {
            RcVec3f p = new RcVec3f();
            RcVec3f q = new RcVec3f();
            EvalArc(x0, y0, z0, dx, dy, dz, len * h, PAD, ref p);
            EvalArc(x0, y0, z0, dx, dy, dz, len * h, PAD + 0.05f, ref q);
            AppendArrowHead(p, q, as0, col);
        }

        if (as1 > 0.001f)
        {
            RcVec3f p = new RcVec3f();
            RcVec3f q = new RcVec3f();
            EvalArc(x0, y0, z0, dx, dy, dz, len * h, 1 - PAD, ref p);
            EvalArc(x0, y0, z0, dx, dy, dz, len * h, 1 - (PAD + 0.05f), ref q);
            AppendArrowHead(p, q, as1, col);
        }
    }

    private void EvalArc(float x0, float y0, float z0, float dx, float dy, float dz, float h, float u, ref RcVec3f res)
    {
        res.X = x0 + dx * u;
        res.Y = y0 + dy * u + h * (1 - (u * 2 - 1) * (u * 2 - 1));
        res.Z = z0 + dz * u;
    }

    public void DebugDrawCross(float x, float y, float z, float size, int col, float lineWidth)
    {
        Begin(DebugDrawPrimitives.LINES, lineWidth);
        AppendCross(x, y, z, size, col);
        End();
    }

    private void AppendCross(float x, float y, float z, float s, int col)
    {
        Vertex(x - s, y, z, col);
        Vertex(x + s, y, z, col);
        Vertex(x, y - s, z, col);
        Vertex(x, y + s, z, col);
        Vertex(x, y, z - s, col);
        Vertex(x, y, z + s, col);
    }

    public void DebugDrawBox(float minx, float miny, float minz, float maxx, float maxy, float maxz, int[] fcol)
    {
        Begin(DebugDrawPrimitives.QUADS);
        AppendBox(minx, miny, minz, maxx, maxy, maxz, fcol);
        End();
    }

    public void DebugDrawCylinder(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col)
    {
        Begin(DebugDrawPrimitives.TRIS);
        AppendCylinder(minx, miny, minz, maxx, maxy, maxz, col);
        End();
    }

    public void AppendCylinder(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col)
    {
        InitCylinder();

        int col2 = DuMultCol(col, 160);

        float cx = (maxx + minx) / 2;
        float cz = (maxz + minz) / 2;
        float rx = (maxx - minx) / 2;
        float rz = (maxz - minz) / 2;

        for (int i = 2; i < CYLINDER_NUM_SEG; ++i)
        {
            int a = 0, b = i - 1, c = i;
            Vertex(cx + cylinderDir[a * 2 + 0] * rx, miny, cz + cylinderDir[a * 2 + 1] * rz, col2);
            Vertex(cx + cylinderDir[b * 2 + 0] * rx, miny, cz + cylinderDir[b * 2 + 1] * rz, col2);
            Vertex(cx + cylinderDir[c * 2 + 0] * rx, miny, cz + cylinderDir[c * 2 + 1] * rz, col2);
        }

        for (int i = 2; i < CYLINDER_NUM_SEG; ++i)
        {
            int a = 0, b = i, c = i - 1;
            Vertex(cx + cylinderDir[a * 2 + 0] * rx, maxy, cz + cylinderDir[a * 2 + 1] * rz, col);
            Vertex(cx + cylinderDir[b * 2 + 0] * rx, maxy, cz + cylinderDir[b * 2 + 1] * rz, col);
            Vertex(cx + cylinderDir[c * 2 + 0] * rx, maxy, cz + cylinderDir[c * 2 + 1] * rz, col);
        }

        for (int i = 0, j = CYLINDER_NUM_SEG - 1; i < CYLINDER_NUM_SEG; j = i++)
        {
            Vertex(cx + cylinderDir[i * 2 + 0] * rx, miny, cz + cylinderDir[i * 2 + 1] * rz, col2);
            Vertex(cx + cylinderDir[j * 2 + 0] * rx, miny, cz + cylinderDir[j * 2 + 1] * rz, col2);
            Vertex(cx + cylinderDir[j * 2 + 0] * rx, maxy, cz + cylinderDir[j * 2 + 1] * rz, col);

            Vertex(cx + cylinderDir[i * 2 + 0] * rx, miny, cz + cylinderDir[i * 2 + 1] * rz, col2);
            Vertex(cx + cylinderDir[j * 2 + 0] * rx, maxy, cz + cylinderDir[j * 2 + 1] * rz, col);
            Vertex(cx + cylinderDir[i * 2 + 0] * rx, maxy, cz + cylinderDir[i * 2 + 1] * rz, col);
        }
    }

    public void DebugDrawArrow(float x0, float y0, float z0, float x1, float y1, float z1, float as0, float as1, int col,
        float lineWidth)
    {
        Begin(DebugDrawPrimitives.LINES, lineWidth);
        AppendArrow(x0, y0, z0, x1, y1, z1, as0, as1, col);
        End();
    }

    public void AppendArrow(float x0, float y0, float z0, float x1, float y1, float z1, float as0, float as1, int col)
    {
        Vertex(x0, y0, z0, col);
        Vertex(x1, y1, z1, col);

        // End arrows
        RcVec3f p = new RcVec3f(x0, y0, z0);
        RcVec3f q = new RcVec3f(x1, y1, z1);
        if (as0 > 0.001f)
            AppendArrowHead(p, q, as0, col);
        if (as1 > 0.001f)
            AppendArrowHead(q, p, as1, col);
    }

    void AppendArrowHead(RcVec3f p, RcVec3f q, float s, int col)
    {
        const float eps = 0.001f;
        if (VdistSqr(p, q) < eps * eps)
        {
            return;
        }

        RcVec3f ax = new RcVec3f();
        RcVec3f ay = new RcVec3f(0, 1, 0);
        RcVec3f az = new RcVec3f();
        Vsub(ref az, q, p);
        Vnormalize(ref az);
        Vcross(ref ax, ay, az);
        Vcross(ref ay, az, ax);
        Vnormalize(ref ay);

        Vertex(p, col);
        // Vertex(p.x+az.x*s+ay.x*s/2, p.y+az.y*s+ay.y*s/2, p.z+az.z*s+ay.z*s/2, col);
        Vertex(p.X + az.X * s + ax.X * s / 3, p.Y + az.Y * s + ax.Y * s / 3, p.Z + az.Z * s + ax.Z * s / 3, col);

        Vertex(p, col);
        // Vertex(p.x+az.x*s-ay.x*s/2, p.y+az.y*s-ay.y*s/2, p.z+az.z*s-ay.z*s/2, col);
        Vertex(p.X + az.X * s - ax.X * s / 3, p.Y + az.Y * s - ax.Y * s / 3, p.Z + az.Z * s - ax.Z * s / 3, col);
    }

    public void Vcross(ref RcVec3f dest, RcVec3f v1, RcVec3f v2)
    {
        dest.X = v1.Y * v2.Z - v1.Z * v2.Y;
        dest.Y = v1.Z * v2.X - v1.X * v2.Z;
        dest.Z = v1.X * v2.Y - v1.Y * v2.X;
    }

    public void Vnormalize(ref RcVec3f v)
    {
        float d = (float)(1.0f / Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z));
        v.X *= d;
        v.Y *= d;
        v.Z *= d;
    }

    public void Vsub(ref RcVec3f dest, RcVec3f v1, RcVec3f v2)
    {
        dest.X = v1.X - v2.X;
        dest.Y = v1.Y - v2.Y;
        dest.Z = v1.Z - v2.Z;
    }

    public float VdistSqr(RcVec3f v1, RcVec3f v2)
    {
        float x = v1.X - v2.X;
        float y = v1.Y - v2.Y;
        float z = v1.Z - v2.Z;
        return x * x + y * y + z * z;
    }

//    public static int AreaToCol(int area) {
//        if (area == 0) {
//            return DuRGBA(0, 192, 255, 255);
//        } else {
//            return DuIntToCol(area, 255);
//        }
//    }

    public static int AreaToCol(int area)
    {
        switch (area)
        {
            // Ground (0) : light blue
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE:
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND:
                return DuRGBA(0, 192, 255, 255);
            // Water : blue
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER:
                return DuRGBA(0, 0, 255, 255);
            // Road : brown
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD:
                return DuRGBA(50, 20, 12, 255);
            // Door : cyan
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_DOOR:
                return DuRGBA(0, 255, 255, 255);
            // Grass : green
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GRASS:
                return DuRGBA(0, 255, 0, 255);
            // Jump : yellow
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP:
                return DuRGBA(255, 255, 0, 255);
            // Unexpected : red
            default:
                return DuRGBA(255, 0, 0, 255);
        }
    }

    public static int DuRGBA(int r, int g, int b, int a)
    {
        return (r) | (g << 8) | (b << 16) | (a << 24);
    }

    public static int DuLerpCol(int ca, int cb, int u)
    {
        int ra = ca & 0xff;
        int ga = (ca >> 8) & 0xff;
        int ba = (ca >> 16) & 0xff;
        int aa = (ca >> 24) & 0xff;
        int rb = cb & 0xff;
        int gb = (cb >> 8) & 0xff;
        int bb = (cb >> 16) & 0xff;
        int ab = (cb >> 24) & 0xff;

        int r = (ra * (255 - u) + rb * u) / 255;
        int g = (ga * (255 - u) + gb * u) / 255;
        int b = (ba * (255 - u) + bb * u) / 255;
        int a = (aa * (255 - u) + ab * u) / 255;
        return DuRGBA(r, g, b, a);
    }

    public static int Bit(int a, int b)
    {
        //return (a & (1 << b)) >>> b;
        return (a >> b) & 1;
    }

    public static int DuIntToCol(int i, int a)
    {
        int r = Bit(i, 1) + Bit(i, 3) * 2 + 1;
        int g = Bit(i, 2) + Bit(i, 4) * 2 + 1;
        int b = Bit(i, 0) + Bit(i, 5) * 2 + 1;
        return DuRGBA(r * 63, g * 63, b * 63, a);
    }

    public static void DuCalcBoxColors(int[] colors, int colTop, int colSide)
    {
        colors[0] = DuMultCol(colTop, 250);
        colors[1] = DuMultCol(colSide, 140);
        colors[2] = DuMultCol(colSide, 165);
        colors[3] = DuMultCol(colSide, 165);
        colors[4] = DuMultCol(colSide, 217);
        colors[5] = DuMultCol(colSide, 217);
    }

    public static int DuMultCol(int col, int d)
    {
        int r = col & 0xff;
        int g = (col >> 8) & 0xff;
        int b = (col >> 16) & 0xff;
        int a = (col >> 24) & 0xff;
        return DuRGBA((r * d) >> 8, (g * d) >> 8, (b * d) >> 8, a);
    }

    public static int DuTransCol(int c, int a)
    {
        return (a << 24) | (c & 0x00ffffff);
    }

    public static int DuDarkenCol(int col)
    {
        return (int)((uint)((col >> 1) & 0x007f7f7f) | (col & 0xff000000));
    }


    public RcMatrix4x4f ProjectionMatrix(float fovy, float aspect, float near, float far)
    {
        GLU.GlhPerspectivef2(ref _projectionMatrix, fovy, aspect, near, far);
        GetOpenGlDraw().ProjectionMatrix(ref _projectionMatrix);
        UpdateFrustum();
        return _projectionMatrix;
    }

    public RcMatrix4x4f ViewMatrix(RcVec3f cameraPos, RcVec2f cameraEulers)
    {
        var rx = RcMatrix4x4f.CreateFromRotate(cameraEulers.X, 1, 0, 0);
        var ry = RcMatrix4x4f.CreateFromRotate(cameraEulers.Y, 0, 1, 0);
        var r = RcMatrix4x4f.Mul(ref rx, ref ry);

        var t = new RcMatrix4x4f();
        t.M11 = t.M22 = t.M33 = t.M44 = 1;
        t.M41 = -cameraPos.X;
        t.M42 = -cameraPos.Y;
        t.M43 = -cameraPos.Z;
        _viewMatrix = RcMatrix4x4f.Mul(ref r, ref t);
        GetOpenGlDraw().ViewMatrix(ref _viewMatrix);
        UpdateFrustum();
        return _viewMatrix;
    }


    private readonly float[][] frustumPlanes =
    {
        new[] { 0f, 0f, 0f, 0f },
        new[] { 0f, 0f, 0f, 0f },
        new[] { 0f, 0f, 0f, 0f },
        new[] { 0f, 0f, 0f, 0f },
        new[] { 0f, 0f, 0f, 0f },
        new[] { 0f, 0f, 0f, 0f },
    };

    private void UpdateFrustum()
    {
        var vpm = RcMatrix4x4f.Mul(ref _projectionMatrix, ref _viewMatrix);
        NormalizePlane(vpm.M14 + vpm.M11, vpm.M24 + vpm.M21, vpm.M34 + vpm.M31, vpm.M44 + vpm.M41, ref frustumPlanes[0]); // left
        NormalizePlane(vpm.M14 - vpm.M11, vpm.M24 - vpm.M21, vpm.M34 - vpm.M31, vpm.M44 - vpm.M41, ref frustumPlanes[1]); // right
        NormalizePlane(vpm.M14 - vpm.M12, vpm.M24 - vpm.M22, vpm.M34 - vpm.M32, vpm.M44 - vpm.M42, ref frustumPlanes[2]); // top
        NormalizePlane(vpm.M14 + vpm.M12, vpm.M24 + vpm.M22, vpm.M34 + vpm.M32, vpm.M44 + vpm.M42, ref frustumPlanes[3]); // bottom
        NormalizePlane(vpm.M14 + vpm.M13, vpm.M24 + vpm.M23, vpm.M34 + vpm.M33, vpm.M44 + vpm.M43, ref frustumPlanes[4]); // near
        NormalizePlane(vpm.M14 - vpm.M13, vpm.M24 - vpm.M23, vpm.M34 - vpm.M33, vpm.M44 - vpm.M43, ref frustumPlanes[5]); // far
    }

    private void NormalizePlane(float px, float py, float pz, float pw, ref float[] plane)
    {
        float length = MathF.Sqrt(px * px + py * py + pz * pz);
        if (length != 0)
        {
            length = 1f / length;
            px *= length;
            py *= length;
            pz *= length;
            pw *= length;
        }

        plane[0] = px;
        plane[1] = py;
        plane[2] = pz;
        plane[3] = pw;
    }

    public bool FrustumTest(Span<float> bounds)
    {
        foreach (float[] plane in frustumPlanes)
        {
            float p_x;
            float p_y;
            float p_z;
            float n_x;
            float n_y;
            float n_z;
            if (plane[0] >= 0)
            {
                p_x = bounds[3];
                n_x = bounds[0];
            }
            else
            {
                p_x = bounds[0];
                n_x = bounds[3];
            }

            if (plane[1] >= 0)
            {
                p_y = bounds[4];
                n_y = bounds[1];
            }
            else
            {
                p_y = bounds[1];
                n_y = bounds[4];
            }

            if (plane[2] >= 0)
            {
                p_z = bounds[5];
                n_z = bounds[2];
            }
            else
            {
                p_z = bounds[2];
                n_z = bounds[5];
            }

            if (plane[0] * p_x + plane[1] * p_y + plane[2] * p_z + plane[3] < 0)
            {
                return false;
            }
        }

        return true;
    }

    public bool FrustumTest(RcVec3f bmin, RcVec3f bmax)
    {
        return FrustumTest(stackalloc float[] { bmin.X, bmin.Y, bmin.Z, bmax.X, bmax.Y, bmax.Z });
    }
}