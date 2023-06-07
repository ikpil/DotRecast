/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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

namespace DotRecast.Recast
{
    public static class PolyUtils
    {
        public static bool PointInPoly(float[] verts, RcVec3f p)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = verts.Length / 3 - 1; i < verts.Length / 3; j = i++)
            {
                RcVec3f vi = RcVec3f.Of(verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]);
                RcVec3f vj = RcVec3f.Of(verts[j * 3], verts[j * 3 + 1], verts[j * 3 + 2]);
                if (((vi.z > p.z) != (vj.z > p.z))
                    && (p.x < (vj.x - vi.x) * (p.z - vi.z) / (vj.z - vi.z) + vi.x))
                {
                    c = !c;
                }
            }

            return c;
        }

        public static int OffsetPoly(float[] verts, int nverts, float offset, float[] outVerts, int maxOutVerts)
        {
            float MITER_LIMIT = 1.20f;

            int n = 0;

            for (int i = 0; i < nverts; i++)
            {
                int a = (i + nverts - 1) % nverts;
                int b = i;
                int c = (i + 1) % nverts;
                int va = a * 3;
                int vb = b * 3;
                int vc = c * 3;
                float dx0 = verts[vb] - verts[va];
                float dy0 = verts[vb + 2] - verts[va + 2];
                float d0 = dx0 * dx0 + dy0 * dy0;
                if (d0 > 1e-6f)
                {
                    d0 = (float)(1.0f / Math.Sqrt(d0));
                    dx0 *= d0;
                    dy0 *= d0;
                }

                float dx1 = verts[vc] - verts[vb];
                float dy1 = verts[vc + 2] - verts[vb + 2];
                float d1 = dx1 * dx1 + dy1 * dy1;
                if (d1 > 1e-6f)
                {
                    d1 = (float)(1.0f / Math.Sqrt(d1));
                    dx1 *= d1;
                    dy1 *= d1;
                }

                float dlx0 = -dy0;
                float dly0 = dx0;
                float dlx1 = -dy1;
                float dly1 = dx1;
                float cross = dx1 * dy0 - dx0 * dy1;
                float dmx = (dlx0 + dlx1) * 0.5f;
                float dmy = (dly0 + dly1) * 0.5f;
                float dmr2 = dmx * dmx + dmy * dmy;
                bool bevel = dmr2 * MITER_LIMIT * MITER_LIMIT < 1.0f;
                if (dmr2 > 1e-6f)
                {
                    float scale = 1.0f / dmr2;
                    dmx *= scale;
                    dmy *= scale;
                }

                if (bevel && cross < 0.0f)
                {
                    if (n + 2 >= maxOutVerts)
                    {
                        return 0;
                    }

                    float d = (1.0f - (dx0 * dx1 + dy0 * dy1)) * 0.5f;
                    outVerts[n * 3 + 0] = verts[vb] + (-dlx0 + dx0 * d) * offset;
                    outVerts[n * 3 + 1] = verts[vb + 1];
                    outVerts[n * 3 + 2] = verts[vb + 2] + (-dly0 + dy0 * d) * offset;
                    n++;
                    outVerts[n * 3 + 0] = verts[vb] + (-dlx1 - dx1 * d) * offset;
                    outVerts[n * 3 + 1] = verts[vb + 1];
                    outVerts[n * 3 + 2] = verts[vb + 2] + (-dly1 - dy1 * d) * offset;
                    n++;
                }
                else
                {
                    if (n + 1 >= maxOutVerts)
                    {
                        return 0;
                    }

                    outVerts[n * 3 + 0] = verts[vb] - dmx * offset;
                    outVerts[n * 3 + 1] = verts[vb + 1];
                    outVerts[n * 3 + 2] = verts[vb + 2] - dmy * offset;
                    n++;
                }
            }

            return n;
        }
    }
}