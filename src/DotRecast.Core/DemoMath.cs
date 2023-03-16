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

namespace DotRecast.Core
{
    public class DemoMath
    {
        public static float vDistSqr(float[] v1, float[] v2, int i)
        {
            float dx = v2[i] - v1[0];
            float dy = v2[i + 1] - v1[1];
            float dz = v2[i + 2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }

        public static float[] vCross(float[] v1, float[] v2)
        {
            float[] dest = new float[3];
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
            return dest;
        }

        public static float vDot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }

        public static float sqr(float f)
        {
            return f * f;
        }

        public static float getPathLen(float[] path, int npath)
        {
            float totd = 0;
            for (int i = 0; i < npath - 1; ++i)
            {
                totd += (float)Math.Sqrt(vDistSqr(path, i * 3, (i + 1) * 3));
            }

            return totd;
        }

        public static float vDistSqr(float[] v, int i, int j)
        {
            float dx = v[i] - v[j];
            float dy = v[i + 1] - v[j + 1];
            float dz = v[i + 2] - v[j + 2];
            return dx * dx + dy * dy + dz * dz;
        }

        public static float step(float threshold, float v)
        {
            return v < threshold ? 0.0f : 1.0f;
        }

        public static int clamp(int v, int min, int max)
        {
            return Math.Max(Math.Min(v, max), min);
        }

        public static float clamp(float v, float min, float max)
        {
            return Math.Max(Math.Min(v, max), min);
        }

        public static float lerp(float f, float g, float u)
        {
            return u * g + (1f - u) * f;
        }
    }
}