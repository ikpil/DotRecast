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

namespace DotRecast.Recast
{
    public static class RecastVectors
    {
        public static void Min(ref Vector3f a, float[] b, int i)
        {
            a.x = Math.Min(a.x, b[i + 0]);
            a.y = Math.Min(a.y, b[i + 1]);
            a.z = Math.Min(a.z, b[i + 2]);
        }
        
        public static void Min(ref Vector3f a, Vector3f b)
        {
            a.x = Math.Min(a.x, b.x);
            a.y = Math.Min(a.y, b.y);
            a.z = Math.Min(a.z, b.z);
        }
        
        public static void Max(ref Vector3f a, float[] b, int i)
        {
            a.x = Math.Max(a.x, b[i + 0]);
            a.y = Math.Max(a.y, b[i + 1]);
            a.z = Math.Max(a.z, b[i + 2]);
        }
        
        public static void Max(ref Vector3f a, Vector3f b)
        {
            a.x = Math.Max(a.x, b.x);
            a.y = Math.Max(a.y, b.y);
            a.z = Math.Max(a.z, b.z);
        }

        public static void Copy(ref Vector3f @out, float[] @in, int i)
        {
            Copy(ref @out, 0, @in, i);
        }
        
        public static void Copy(float[] @out, int n, float[] @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }
        
        public static void Copy(float[] @out, int n, Vector3f @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        public static void Copy(ref Vector3f @out, int n, float[] @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }
        
        public static void Add(ref Vector3f e0, Vector3f a, float[] verts, int i)
        {
            e0.x = a.x + verts[i];
            e0.y = a.y + verts[i + 1];
            e0.z = a.z + verts[i + 2];
        }

        
        public static void Sub(ref Vector3f e0, float[] verts, int i, int j)
        {
            e0.x = verts[i] - verts[j];
            e0.y = verts[i + 1] - verts[j + 1];
            e0.z = verts[i + 2] - verts[j + 2];
        }

        
        public static void Sub(ref Vector3f e0, Vector3f i, float[] verts, int j)
        {
            e0.x = i.x - verts[j];
            e0.y = i.y - verts[j + 1];
            e0.z = i.z - verts[j + 2];
        }


        public static void Cross(float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
        }
        
        public static void Cross(float[] dest, Vector3f v1, Vector3f v2)
        {
            dest[0] = v1.y * v2.z - v1.z * v2.y;
            dest[1] = v1.z * v2.x - v1.x * v2.z;
            dest[2] = v1.x * v2.y - v1.y * v2.x;
        }
        
        public static void Cross(ref Vector3f dest, Vector3f v1, Vector3f v2)
        {
            dest.x = v1.y * v2.z - v1.z * v2.y;
            dest.y = v1.z * v2.x - v1.x * v2.z;
            dest.z = v1.x * v2.y - v1.y * v2.x;
        }


        public static void Normalize(float[] v)
        {
            float d = (float)(1.0f / Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]));
            v[0] *= d;
            v[1] *= d;
            v[2] *= d;
        }
        
        public static void Normalize(ref Vector3f v)
        {
            float d = (float)(1.0f / Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
            v.x *= d;
            v.y *= d;
            v.z *= d;
        }

        public static float Dot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }
        
        public static float Dot(float[] v1, Vector3f v2)
        {
            return v1[0] * v2.x + v1[1] * v2.y + v1[2] * v2.z;
        }
        
        public static float Dot(Vector3f v1, Vector3f v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

    }
}
