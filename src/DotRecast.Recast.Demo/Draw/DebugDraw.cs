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
using DotRecast.Core;
using DotRecast.Recast.Demo.Builder;
using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo.Draw;

public class DebugDraw
{
    private readonly GLCheckerTexture g_tex;
    private readonly OpenGLDraw openGlDraw;
    private readonly int[] boxIndices = { 7, 6, 5, 4, 0, 1, 2, 3, 1, 5, 6, 2, 3, 7, 4, 0, 2, 6, 7, 3, 0, 4, 5, 1, };

    private readonly float[][] frustumPlanes = ArrayUtils.Of<float>(6, 4);
    // {
    //     new[] { 0f, 0f, 0f, 0f },
    //     new[] { 0f, 0f, 0f, 0f },
    //     new[] { 0f, 0f, 0f, 0f },
    //     new[] { 0f, 0f, 0f, 0f },
    //     new[] { 0f, 0f, 0f, 0f },
    //     new[] { 0f, 0f, 0f, 0f },
    // };

    public DebugDraw(GL gl)
    {
        g_tex = new GLCheckerTexture(gl);
        openGlDraw = new ModernOpenGLDraw(gl);
    }


    public void begin(DebugDrawPrimitives prim)
    {
        begin(prim, 1f);
    }

    public void debugDrawCylinderWire(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col,
        float lineWidth)
    {
        begin(DebugDrawPrimitives.LINES, lineWidth);
        appendCylinderWire(minx, miny, minz, maxx, maxy, maxz, col);
        end();
    }

    private const int CYLINDER_NUM_SEG = 16;
    private readonly float[] cylinderDir = new float[CYLINDER_NUM_SEG * 2];
    private bool cylinderInit = false;

    private void initCylinder()
    {
        if (!cylinderInit)
        {
            cylinderInit = true;
            for (int i = 0; i < CYLINDER_NUM_SEG; ++i)
            {
                float a = (float)(i * Math.PI * 2 / CYLINDER_NUM_SEG);
                cylinderDir[i * 2] = (float)Math.Cos(a);
                cylinderDir[i * 2 + 1] = (float)Math.Sin(a);
            }
        }
    }

    void appendCylinderWire(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col)
    {
        initCylinder();

        float cx = (maxx + minx) / 2;
        float cz = (maxz + minz) / 2;
        float rx = (maxx - minx) / 2;
        float rz = (maxz - minz) / 2;

        for (int i = 0, j = CYLINDER_NUM_SEG - 1; i < CYLINDER_NUM_SEG; j = i++)
        {
            vertex(cx + cylinderDir[j * 2 + 0] * rx, miny, cz + cylinderDir[j * 2 + 1] * rz, col);
            vertex(cx + cylinderDir[i * 2 + 0] * rx, miny, cz + cylinderDir[i * 2 + 1] * rz, col);
            vertex(cx + cylinderDir[j * 2 + 0] * rx, maxy, cz + cylinderDir[j * 2 + 1] * rz, col);
            vertex(cx + cylinderDir[i * 2 + 0] * rx, maxy, cz + cylinderDir[i * 2 + 1] * rz, col);
        }

        for (int i = 0; i < CYLINDER_NUM_SEG; i += CYLINDER_NUM_SEG / 4)
        {
            vertex(cx + cylinderDir[i * 2 + 0] * rx, miny, cz + cylinderDir[i * 2 + 1] * rz, col);
            vertex(cx + cylinderDir[i * 2 + 0] * rx, maxy, cz + cylinderDir[i * 2 + 1] * rz, col);
        }
    }

    public void debugDrawBoxWire(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col,
        float lineWidth)
    {
        begin(DebugDrawPrimitives.LINES, lineWidth);
        appendBoxWire(minx, miny, minz, maxx, maxy, maxz, col);
        end();
    }

    public void appendBoxWire(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col)
    {
        // Top
        vertex(minx, miny, minz, col);
        vertex(maxx, miny, minz, col);
        vertex(maxx, miny, minz, col);
        vertex(maxx, miny, maxz, col);
        vertex(maxx, miny, maxz, col);
        vertex(minx, miny, maxz, col);
        vertex(minx, miny, maxz, col);
        vertex(minx, miny, minz, col);

        // bottom
        vertex(minx, maxy, minz, col);
        vertex(maxx, maxy, minz, col);
        vertex(maxx, maxy, minz, col);
        vertex(maxx, maxy, maxz, col);
        vertex(maxx, maxy, maxz, col);
        vertex(minx, maxy, maxz, col);
        vertex(minx, maxy, maxz, col);
        vertex(minx, maxy, minz, col);

        // Sides
        vertex(minx, miny, minz, col);
        vertex(minx, maxy, minz, col);
        vertex(maxx, miny, minz, col);
        vertex(maxx, maxy, minz, col);
        vertex(maxx, miny, maxz, col);
        vertex(maxx, maxy, maxz, col);
        vertex(minx, miny, maxz, col);
        vertex(minx, maxy, maxz, col);
    }

    public void appendBox(float minx, float miny, float minz, float maxx, float maxy, float maxz, int[] fcol)
    {
        float[][] verts =
        {
            new[] { minx, miny, minz },
            new[] { maxx, miny, minz },
            new[] { maxx, miny, maxz },
            new[] { minx, miny, maxz },
            new[] { minx, maxy, minz },
            new[] { maxx, maxy, minz },
            new[] { maxx, maxy, maxz },
            new[] { minx, maxy, maxz }
        };

        int idx = 0;
        for (int i = 0; i < 6; ++i)
        {
            vertex(verts[boxIndices[idx]], fcol[i]);
            idx++;
            vertex(verts[boxIndices[idx]], fcol[i]);
            idx++;
            vertex(verts[boxIndices[idx]], fcol[i]);
            idx++;
            vertex(verts[boxIndices[idx]], fcol[i]);
            idx++;
        }
    }

    public void debugDrawArc(float x0, float y0, float z0, float x1, float y1, float z1, float h, float as0, float as1, int col,
        float lineWidth)
    {
        begin(DebugDrawPrimitives.LINES, lineWidth);
        appendArc(x0, y0, z0, x1, y1, z1, h, as0, as1, col);
        end();
    }

    public void begin(DebugDrawPrimitives prim, float size)
    {
        getOpenGlDraw().begin(prim, size);
    }

    public void vertex(float[] pos, int color)
    {
        getOpenGlDraw().vertex(pos, color);
    }
    
    public void vertex(Vector3f pos, int color)
    {
        getOpenGlDraw().vertex(pos, color);
    }


    public void vertex(float x, float y, float z, int color)
    {
        getOpenGlDraw().vertex(x, y, z, color);
    }

    public void vertex(Vector3f pos, int color, Vector2f uv)
    {
        getOpenGlDraw().vertex(pos, color, uv);
    }

    public void vertex(float x, float y, float z, int color, float u, float v)
    {
        getOpenGlDraw().vertex(x, y, z, color, u, v);
    }

    public void end()
    {
        getOpenGlDraw().end();
    }

    public void debugDrawCircle(float x, float y, float z, float r, int col, float lineWidth)
    {
        begin(DebugDrawPrimitives.LINES, lineWidth);
        appendCircle(x, y, z, r, col);
        end();
    }

    private bool circleInit = false;
    private const int CIRCLE_NUM_SEG = 40;
    private readonly float[] circeDir = new float[CIRCLE_NUM_SEG * 2];
    private float[] _viewMatrix = new float[16];
    private readonly float[] _projectionMatrix = new float[16];

    public void appendCircle(float x, float y, float z, float r, int col)
    {
        if (!circleInit)
        {
            circleInit = true;
            for (int i = 0; i < CIRCLE_NUM_SEG; ++i)
            {
                float a = (float)(i * Math.PI * 2 / CIRCLE_NUM_SEG);
                circeDir[i * 2] = (float)Math.Cos(a);
                circeDir[i * 2 + 1] = (float)Math.Sin(a);
            }
        }

        for (int i = 0, j = CIRCLE_NUM_SEG - 1; i < CIRCLE_NUM_SEG; j = i++)
        {
            vertex(x + circeDir[j * 2 + 0] * r, y, z + circeDir[j * 2 + 1] * r, col);
            vertex(x + circeDir[i * 2 + 0] * r, y, z + circeDir[i * 2 + 1] * r, col);
        }
    }

    private static readonly int NUM_ARC_PTS = 8;
    private static readonly float PAD = 0.05f;
    private static readonly float ARC_PTS_SCALE = (1.0f - PAD * 2) / NUM_ARC_PTS;

    public void appendArc(float x0, float y0, float z0, float x1, float y1, float z1, float h, float as0, float as1, int col)
    {
        float dx = x1 - x0;
        float dy = y1 - y0;
        float dz = z1 - z0;
        float len = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        Vector3f prev = new Vector3f();
        evalArc(x0, y0, z0, dx, dy, dz, len * h, PAD, ref prev);
        for (int i = 1; i <= NUM_ARC_PTS; ++i)
        {
            float u = PAD + i * ARC_PTS_SCALE;
            Vector3f pt = new Vector3f();
            evalArc(x0, y0, z0, dx, dy, dz, len * h, u, ref pt);
            vertex(prev.x, prev.y, prev.z, col);
            vertex(pt.x, pt.y, pt.z, col);
            prev.x = pt.x;
            prev.y = pt.y;
            prev.z = pt.z;
        }

        // End arrows
        if (as0 > 0.001f)
        {
            Vector3f p = new Vector3f();
            Vector3f q = new Vector3f();
            evalArc(x0, y0, z0, dx, dy, dz, len * h, PAD, ref p);
            evalArc(x0, y0, z0, dx, dy, dz, len * h, PAD + 0.05f, ref q);
            appendArrowHead(p, q, as0, col);
        }

        if (as1 > 0.001f)
        {
            Vector3f p = new Vector3f();
            Vector3f q = new Vector3f();
            evalArc(x0, y0, z0, dx, dy, dz, len * h, 1 - PAD, ref p);
            evalArc(x0, y0, z0, dx, dy, dz, len * h, 1 - (PAD + 0.05f), ref q);
            appendArrowHead(p, q, as1, col);
        }
    }

    private void evalArc(float x0, float y0, float z0, float dx, float dy, float dz, float h, float u, ref Vector3f res)
    {
        res.x = x0 + dx * u;
        res.y = y0 + dy * u + h * (1 - (u * 2 - 1) * (u * 2 - 1));
        res.z = z0 + dz * u;
    }

    public void debugDrawCross(float x, float y, float z, float size, int col, float lineWidth)
    {
        begin(DebugDrawPrimitives.LINES, lineWidth);
        appendCross(x, y, z, size, col);
        end();
    }

    private void appendCross(float x, float y, float z, float s, int col)
    {
        vertex(x - s, y, z, col);
        vertex(x + s, y, z, col);
        vertex(x, y - s, z, col);
        vertex(x, y + s, z, col);
        vertex(x, y, z - s, col);
        vertex(x, y, z + s, col);
    }

    public void debugDrawBox(float minx, float miny, float minz, float maxx, float maxy, float maxz, int[] fcol)
    {
        begin(DebugDrawPrimitives.QUADS);
        appendBox(minx, miny, minz, maxx, maxy, maxz, fcol);
        end();
    }

    public void debugDrawCylinder(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col)
    {
        begin(DebugDrawPrimitives.TRIS);
        appendCylinder(minx, miny, minz, maxx, maxy, maxz, col);
        end();
    }

    public void appendCylinder(float minx, float miny, float minz, float maxx, float maxy, float maxz, int col)
    {
        initCylinder();

        int col2 = duMultCol(col, 160);

        float cx = (maxx + minx) / 2;
        float cz = (maxz + minz) / 2;
        float rx = (maxx - minx) / 2;
        float rz = (maxz - minz) / 2;

        for (int i = 2; i < CYLINDER_NUM_SEG; ++i)
        {
            int a = 0, b = i - 1, c = i;
            vertex(cx + cylinderDir[a * 2 + 0] * rx, miny, cz + cylinderDir[a * 2 + 1] * rz, col2);
            vertex(cx + cylinderDir[b * 2 + 0] * rx, miny, cz + cylinderDir[b * 2 + 1] * rz, col2);
            vertex(cx + cylinderDir[c * 2 + 0] * rx, miny, cz + cylinderDir[c * 2 + 1] * rz, col2);
        }

        for (int i = 2; i < CYLINDER_NUM_SEG; ++i)
        {
            int a = 0, b = i, c = i - 1;
            vertex(cx + cylinderDir[a * 2 + 0] * rx, maxy, cz + cylinderDir[a * 2 + 1] * rz, col);
            vertex(cx + cylinderDir[b * 2 + 0] * rx, maxy, cz + cylinderDir[b * 2 + 1] * rz, col);
            vertex(cx + cylinderDir[c * 2 + 0] * rx, maxy, cz + cylinderDir[c * 2 + 1] * rz, col);
        }

        for (int i = 0, j = CYLINDER_NUM_SEG - 1; i < CYLINDER_NUM_SEG; j = i++)
        {
            vertex(cx + cylinderDir[i * 2 + 0] * rx, miny, cz + cylinderDir[i * 2 + 1] * rz, col2);
            vertex(cx + cylinderDir[j * 2 + 0] * rx, miny, cz + cylinderDir[j * 2 + 1] * rz, col2);
            vertex(cx + cylinderDir[j * 2 + 0] * rx, maxy, cz + cylinderDir[j * 2 + 1] * rz, col);

            vertex(cx + cylinderDir[i * 2 + 0] * rx, miny, cz + cylinderDir[i * 2 + 1] * rz, col2);
            vertex(cx + cylinderDir[j * 2 + 0] * rx, maxy, cz + cylinderDir[j * 2 + 1] * rz, col);
            vertex(cx + cylinderDir[i * 2 + 0] * rx, maxy, cz + cylinderDir[i * 2 + 1] * rz, col);
        }
    }

    public void debugDrawArrow(float x0, float y0, float z0, float x1, float y1, float z1, float as0, float as1, int col,
        float lineWidth)
    {
        begin(DebugDrawPrimitives.LINES, lineWidth);
        appendArrow(x0, y0, z0, x1, y1, z1, as0, as1, col);
        end();
    }

    public void appendArrow(float x0, float y0, float z0, float x1, float y1, float z1, float as0, float as1, int col)
    {
        vertex(x0, y0, z0, col);
        vertex(x1, y1, z1, col);

        // End arrows
        Vector3f p = Vector3f.Of(x0, y0, z0);
        Vector3f q = Vector3f.Of(x1, y1, z1);
        if (as0 > 0.001f)
            appendArrowHead(p, q, as0, col);
        if (as1 > 0.001f)
            appendArrowHead(q, p, as1, col);
    }

    void appendArrowHead(Vector3f p, Vector3f q, float s, int col)
    {
        float eps = 0.001f;
        if (vdistSqr(p, q) < eps * eps)
        {
            return;
        }

        Vector3f ax = new Vector3f();
        Vector3f ay = Vector3f.Of(0, 1, 0);
        Vector3f az = new Vector3f();
        vsub(ref az, q, p);
        vnormalize(ref az);
        vcross(ref ax, ay, az);
        vcross(ref ay, az, ax);
        vnormalize(ref ay);

        vertex(p, col);
        // vertex(p.x+az.x*s+ay.x*s/2, p.y+az.y*s+ay.y*s/2, p.z+az.z*s+ay.z*s/2, col);
        vertex(p.x + az.x * s + ax.x * s / 3, p.y + az.y * s + ax.y * s / 3, p.z + az.z * s + ax.z * s / 3, col);

        vertex(p, col);
        // vertex(p.x+az.x*s-ay.x*s/2, p.y+az.y*s-ay.y*s/2, p.z+az.z*s-ay.z*s/2, col);
        vertex(p.x + az.x * s - ax.x * s / 3, p.y + az.y * s - ax.y * s / 3, p.z + az.z * s - ax.z * s / 3, col);
    }

    public void vcross(ref Vector3f dest, Vector3f v1, Vector3f v2)
    {
        dest.x = v1.y * v2.z - v1.z * v2.y;
        dest.y = v1.z * v2.x - v1.x * v2.z;
        dest.z = v1.x * v2.y - v1.y * v2.x;
    }

    public void vnormalize(ref Vector3f v)
    {
        float d = (float)(1.0f / Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
        v.x *= d;
        v.y *= d;
        v.z *= d;
    }

    public void vsub(ref Vector3f dest, Vector3f v1, Vector3f v2)
    {
        dest.x = v1.x - v2.x;
        dest.y = v1.y - v2.y;
        dest.z = v1.z - v2.z;
    }

    public float vdistSqr(Vector3f v1, Vector3f v2)
    {
        float x = v1.x - v2.x;
        float y = v1.y - v2.y;
        float z = v1.z - v2.z;
        return x * x + y * y + z * z;
    }

//    public static int areaToCol(int area) {
//        if (area == 0) {
//            return duRGBA(0, 192, 255, 255);
//        } else {
//            return duIntToCol(area, 255);
//        }
//    }

    public static int areaToCol(int area)
    {
        switch (area)
        {
            // Ground (0) : light blue
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE:
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND:
                return duRGBA(0, 192, 255, 255);
            // Water : blue
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER:
                return duRGBA(0, 0, 255, 255);
            // Road : brown
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD:
                return duRGBA(50, 20, 12, 255);
            // Door : cyan
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_DOOR:
                return duRGBA(0, 255, 255, 255);
            // Grass : green
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GRASS:
                return duRGBA(0, 255, 0, 255);
            // Jump : yellow
            case SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP:
                return duRGBA(255, 255, 0, 255);
            // Unexpected : red
            default:
                return duRGBA(255, 0, 0, 255);
        }
    }

    public static int duRGBA(int r, int g, int b, int a)
    {
        return (r) | (g << 8) | (b << 16) | (a << 24);
    }

    public static int duLerpCol(int ca, int cb, int u)
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
        return duRGBA(r, g, b, a);
    }

    public static int bit(int a, int b)
    {
        return (a & (1 << b)) >>> b;
    }

    public static int duIntToCol(int i, int a)
    {
        int r = bit(i, 1) + bit(i, 3) * 2 + 1;
        int g = bit(i, 2) + bit(i, 4) * 2 + 1;
        int b = bit(i, 0) + bit(i, 5) * 2 + 1;
        return duRGBA(r * 63, g * 63, b * 63, a);
    }

    public static void duCalcBoxColors(int[] colors, int colTop, int colSide)
    {
        colors[0] = duMultCol(colTop, 250);
        colors[1] = duMultCol(colSide, 140);
        colors[2] = duMultCol(colSide, 165);
        colors[3] = duMultCol(colSide, 165);
        colors[4] = duMultCol(colSide, 217);
        colors[5] = duMultCol(colSide, 217);
    }

    public static int duMultCol(int col, int d)
    {
        int r = col & 0xff;
        int g = (col >> 8) & 0xff;
        int b = (col >> 16) & 0xff;
        int a = (col >> 24) & 0xff;
        return duRGBA((r * d) >> 8, (g * d) >> 8, (b * d) >> 8, a);
    }

    public static int duTransCol(int c, int a)
    {
        return (a << 24) | (c & 0x00ffffff);
    }

    public static int duDarkenCol(int col)
    {
        return (int)(((col >> 1) & 0x007f7f7f) | (col & 0xff000000));
    }

    public void fog(float start, float end)
    {
        getOpenGlDraw().fog(start, end);
    }

    public void fog(bool state)
    {
        getOpenGlDraw().fog(state);
    }

    public void depthMask(bool state)
    {
        getOpenGlDraw().depthMask(state);
    }

    public void texture(bool state)
    {
        getOpenGlDraw().texture(g_tex, state);
    }

    public void init(float fogDistance)
    {
        getOpenGlDraw().init();
    }

    public void clear()
    {
        getOpenGlDraw().clear();
    }

    public float[] projectionMatrix(float fovy, float aspect, float near, float far)
    {
        GLU.glhPerspectivef2(_projectionMatrix, fovy, aspect, near, far);
        getOpenGlDraw().projectionMatrix(_projectionMatrix);
        updateFrustum();
        return _projectionMatrix;
    }

    public float[] viewMatrix(Vector3f cameraPos, float[] cameraEulers)
    {
        float[] rx = GLU.build_4x4_rotation_matrix(cameraEulers[0], 1, 0, 0);
        float[] ry = GLU.build_4x4_rotation_matrix(cameraEulers[1], 0, 1, 0);
        float[] r = GLU.mul(rx, ry);
        float[] t = new float[16];
        t[0] = t[5] = t[10] = t[15] = 1;
        t[12] = -cameraPos.x;
        t[13] = -cameraPos.y;
        t[14] = -cameraPos.z;
        _viewMatrix = GLU.mul(r, t);
        getOpenGlDraw().viewMatrix(_viewMatrix);
        updateFrustum();
        return _viewMatrix;
    }

    private OpenGLDraw getOpenGlDraw()
    {
        return openGlDraw;
    }

    private void updateFrustum()
    {
        float[] vpm = GLU.mul(_projectionMatrix, _viewMatrix);
        // left
        frustumPlanes[0] = normalizePlane(vpm[0 + 3] + vpm[0 + 0], vpm[4 + 3] + vpm[4 + 0], vpm[8 + 3] + vpm[8 + 0],
            vpm[12 + 3] + vpm[12 + 0]);
        // right
        frustumPlanes[1] = normalizePlane(vpm[0 + 3] - vpm[0 + 0], vpm[4 + 3] - vpm[4 + 0], vpm[8 + 3] - vpm[8 + 0],
            vpm[12 + 3] - vpm[12 + 0]);
        // top
        frustumPlanes[2] = normalizePlane(vpm[0 + 3] - vpm[0 + 1], vpm[4 + 3] - vpm[4 + 1], vpm[8 + 3] - vpm[8 + 1],
            vpm[12 + 3] - vpm[12 + 1]);
        // bottom
        frustumPlanes[3] = normalizePlane(vpm[0 + 3] + vpm[0 + 1], vpm[4 + 3] + vpm[4 + 1], vpm[8 + 3] + vpm[8 + 1],
            vpm[12 + 3] + vpm[12 + 1]);
        // near
        frustumPlanes[4] = normalizePlane(vpm[0 + 3] + vpm[0 + 2], vpm[4 + 3] + vpm[4 + 2], vpm[8 + 3] + vpm[8 + 2],
            vpm[12 + 3] + vpm[12 + 2]);
        // far
        frustumPlanes[5] = normalizePlane(vpm[0 + 3] - vpm[0 + 2], vpm[4 + 3] - vpm[4 + 2], vpm[8 + 3] - vpm[8 + 2],
            vpm[12 + 3] - vpm[12 + 2]);
    }

    private float[] normalizePlane(float px, float py, float pz, float pw)
    {
        float length = (float)Math.Sqrt(px * px + py * py + pz * pz);
        if (length != 0)
        {
            length = 1f / length;
            px *= length;
            py *= length;
            pz *= length;
            pw *= length;
        }

        return new float[] { px, py, pz, pw };
    }

    public bool frustumTest(float[] bounds)
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

    public bool frustumTest(Vector3f bmin, Vector3f bmax)
    {
        return frustumTest(new float[] { bmin.x, bmin.y, bmin.z, bmax.x, bmax.y, bmax.z });
    }
}
