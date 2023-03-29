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
using System.Numerics;

namespace DotRecast.Core
{
    public static class RecastMath
    {
        public const float EPS = 1e-4f;
        private static readonly float EQUAL_THRESHOLD = sqr(1.0f / 16384.0f);
        
        public static float vDistSqr(float[] v1, float[] v2, int i)
        {
            float dx = v2[i] - v1[0];
            float dy = v2[i + 1] - v1[1];
            float dz = v2[i + 2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }
        
        public static float vDistSqr(Vector3f v1, Vector3f v2, int i)
        {
            float dx = v2[i] - v1[0];
            float dy = v2[i + 1] - v1[1];
            float dz = v2[i + 2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }


        public static float[] vCross(float[] v1, float[] v2)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
            return dest.ToArray();
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

        public static float clamp(float v, float min, float max)
        {
            return Math.Max(Math.Min(v, max), min);
        }

        public static int clamp(int v, int min, int max)
        {
            return Math.Max(Math.Min(v, max), min);
        }

        public static float lerp(float f, float g, float u)
        {
            return u * g + (1f - u) * f;
        }
        

        /// Performs a scaled vector addition. (@p v1 + (@p v2 * @p s))
        /// @param[out] dest The result vector. [(x, y, z)]
        /// @param[in] v1 The base vector. [(x, y, z)]
        /// @param[in] v2 The vector to scale and add to @p v1. [(x, y, z)]
        /// @param[in] s The amount to scale @p v2 by before adding to @p v1.
        public static float[] vMad(float[] v1, float[] v2, float s)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] + v2[0] * s;
            dest[1] = v1[1] + v2[1] * s;
            dest[2] = v1[2] + v2[2] * s;
            return dest.ToArray();
        }
        
        public static Vector3f vMad(Vector3f v1, Vector3f v2, float s)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] + v2[0] * s;
            dest[1] = v1[1] + v2[1] * s;
            dest[2] = v1[2] + v2[2] * s;
            return dest;
        }


        /// Performs a linear interpolation between two vectors. (@p v1 toward @p
        /// v2)
        /// @param[out] dest The result vector. [(x, y, x)]
        /// @param[in] v1 The starting vector.
        /// @param[in] v2 The destination vector.
        /// @param[in] t The interpolation factor. [Limits: 0 <= value <= 1.0]
        public static float[] vLerp(float[] verts, int v1, int v2, float t)
        {
            Vector3f dest = new Vector3f();
            dest[0] = verts[v1 + 0] + (verts[v2 + 0] - verts[v1 + 0]) * t;
            dest[1] = verts[v1 + 1] + (verts[v2 + 1] - verts[v1 + 1]) * t;
            dest[2] = verts[v1 + 2] + (verts[v2 + 2] - verts[v1 + 2]) * t;
            return dest.ToArray();
        }

        public static float[] vLerp(float[] v1, float[] v2, float t)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] + (v2[0] - v1[0]) * t;
            dest[1] = v1[1] + (v2[1] - v1[1]) * t;
            dest[2] = v1[2] + (v2[2] - v1[2]) * t;
            return dest.ToArray();
        }
        
        public static Vector3f vLerp(Vector3f v1, Vector3f v2, float t)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] + (v2[0] - v1[0]) * t;
            dest[1] = v1[1] + (v2[1] - v1[1]) * t;
            dest[2] = v1[2] + (v2[2] - v1[2]) * t;
            return dest;
        }

        
        public static Vector3f vSub(VectorPtr v1, VectorPtr v2)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1.get(0) - v2.get(0);
            dest[1] = v1.get(1) - v2.get(1);
            dest[2] = v1.get(2) - v2.get(2);
            return dest;
        }

        public static float[] vSub(float[] v1, float[] v2)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] - v2[0];
            dest[1] = v1[1] - v2[1];
            dest[2] = v1[2] - v2[2];
            return dest.ToArray();
        }
        
        public static Vector3f vSub(Vector3f v1, Vector3f v2)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] - v2[0];
            dest[1] = v1[1] - v2[1];
            dest[2] = v1[2] - v2[2];
            return dest;
        }
        
        public static Vector3f vSub(Vector3f v1, VectorPtr v2)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] - v2.get(0);
            dest[1] = v1[1] - v2.get(1);
            dest[2] = v1[2] - v2.get(2);
            return dest;
        }

        
        public static Vector3f vSub(Vector3f v1, float[] v2)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] - v2[0];
            dest[1] = v1[1] - v2[1];
            dest[2] = v1[2] - v2[2];
            return dest;
        }



        public static float[] vAdd(float[] v1, float[] v2)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] + v2[0];
            dest[1] = v1[1] + v2[1];
            dest[2] = v1[2] + v2[2];
            return dest.ToArray();
        }
        
        public static Vector3f vAdd(Vector3f v1, Vector3f v2)
        {
            Vector3f dest = new Vector3f();
            dest[0] = v1[0] + v2[0];
            dest[1] = v1[1] + v2[1];
            dest[2] = v1[2] + v2[2];
            return dest;
        }


        public static float[] vCopy(float[] @in)
        {
            float[] @out = new float[3];
            @out[0] = @in[0];
            @out[1] = @in[1];
            @out[2] = @in[2];
            return @out;
        }

        public static void vSet(float[] @out, float a, float b, float c)
        {
            @out[0] = a;
            @out[1] = b;
            @out[2] = c;
        }
        
        public static void vSet(ref Vector3f @out, float a, float b, float c)
        {
            @out.x = a;
            @out.y = b;
            @out.z = c;
        }


        public static void vCopy(float[] @out, float[] @in)
        {
            @out[0] = @in[0];
            @out[1] = @in[1];
            @out[2] = @in[2];
        }
        
        public static void vCopy(float[] @out, Vector3f @in)
        {
            @out[0] = @in[0];
            @out[1] = @in[1];
            @out[2] = @in[2];
        }
        
        public static void vCopy(ref Vector3f @out, float[] @in)
        {
            @out.x = @in[0];
            @out.y = @in[1];
            @out.z = @in[2];
        }
        
        public static void vCopy(ref Vector3f @out, Vector3f @in)
        {
            @out.x = @in[0];
            @out.y = @in[1];
            @out.z = @in[2];
        }

        public static void vCopy(float[] @out, float[] @in, int i)
        {
            @out[0] = @in[i];
            @out[1] = @in[i + 1];
            @out[2] = @in[i + 2];
        }
        
        public static void vCopy(ref Vector3f @out, float[] @in, int i)
        {
            @out.x = @in[i];
            @out.y = @in[i + 1];
            @out.z = @in[i + 2];
        }

        public static void vMin(float[] @out, float[] @in, int i)
        {
            @out[0] = Math.Min(@out[0], @in[i]);
            @out[1] = Math.Min(@out[1], @in[i + 1]);
            @out[2] = Math.Min(@out[2], @in[i + 2]);
        }
        
        public static void vMin(ref Vector3f @out, float[] @in, int i)
        {
            @out.x = Math.Min(@out.x, @in[i]);
            @out.y = Math.Min(@out.y, @in[i + 1]);
            @out.z = Math.Min(@out.z, @in[i + 2]);
        }


        public static void vMax(float[] @out, float[] @in, int i)
        {
            @out[0] = Math.Max(@out[0], @in[i]);
            @out[1] = Math.Max(@out[1], @in[i + 1]);
            @out[2] = Math.Max(@out[2], @in[i + 2]);
        }
        
        public static void vMax(ref Vector3f @out, float[] @in, int i)
        {
            @out.x = Math.Max(@out.x, @in[i]);
            @out.y = Math.Max(@out.y, @in[i + 1]);
            @out.z = Math.Max(@out.z, @in[i + 2]);
        }


        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        public static float vDist(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        
        public static float vDist(Vector3f v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        
        public static float vDist(Vector3f v1, Vector3f v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }



        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        public static float vDistSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }
        
        public static float vDistSqr(Vector3f v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }

        
        public static float vDistSqr(Vector3f v1, Vector3f v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }


        /// Derives the square of the scalar length of the vector. (len * len)
        /// @param[in] v The vector. [(x, y, z)]
        /// @return The square of the scalar length of the vector.
        public static float vLenSqr(float[] v)
        {
            return v[0] * v[0] + v[1] * v[1] + v[2] * v[2];
        }
        
        public static float vLenSqr(Vector3f v)
        {
            return v[0] * v[0] + v[1] * v[1] + v[2] * v[2];
        }


        public static float vLen(float[] v)
        {
            return (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        }
        
        public static float vLen(Vector3f v)
        {
            return (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        }


        public static float vDist(float[] v1, float[] verts, int i)
        {
            float dx = verts[i] - v1[0];
            float dy = verts[i + 1] - v1[1];
            float dz = verts[i + 2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        /// Derives the distance between the specified points on the xz-plane.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the point on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        public static float vDist2D(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }
        
        public static float vDist2D(Vector3f v1, Vector3f v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }


        public static float vDist2DSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return dx * dx + dz * dz;
        }
        
        public static float vDist2DSqr(Vector3f v1, Vector3f v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return dx * dx + dz * dz;
        }


        public static float vDist2DSqr(Vector3f p, float[] verts, int i)
        {
            float dx = verts[i] - p[0];
            float dz = verts[i + 2] - p[2];
            return dx * dx + dz * dz;
        }

        /// Normalizes the vector.
        /// @param[in,out] v The vector to normalize. [(x, y, z)]
        public static void vNormalize(float[] v)
        {
            float d = (float)(1.0f / Math.Sqrt(sqr(v[0]) + sqr(v[1]) + sqr(v[2])));
            if (d != 0)
            {
                v[0] *= d;
                v[1] *= d;
                v[2] *= d;
            }
        }
        
        public static void vNormalize(ref Vector3f v)
        {
            float d = (float)(1.0f / Math.Sqrt(sqr(v[0]) + sqr(v[1]) + sqr(v[2])));
            if (d != 0)
            {
                v.x *= d;
                v.y *= d;
                v.z *= d;
            }
        }


        /// Performs a 'sloppy' colocation check of the specified points.
        /// @param[in] p0 A point. [(x, y, z)]
        /// @param[in] p1 A point. [(x, y, z)]
        /// @return True if the points are considered to be at the same location.
        ///
        /// Basically, this function will return true if the specified points are
        /// close enough to eachother to be considered colocated.
        public static bool vEqual(float[] p0, float[] p1)
        {
            return vEqual(p0, p1, EQUAL_THRESHOLD);
        }
        
        public static bool vEqual(Vector3f p0, Vector3f p1)
        {
            return vEqual(p0, p1, EQUAL_THRESHOLD);
        }


        public static bool vEqual(float[] p0, float[] p1, float thresholdSqr)
        {
            float d = vDistSqr(p0, p1);
            return d < thresholdSqr;
        }
        
        public static bool vEqual(Vector3f p0, Vector3f p1, float thresholdSqr)
        {
            float d = vDistSqr(p0, p1);
            return d < thresholdSqr;
        }


        /// Derives the dot product of two vectors on the xz-plane. (@p u . @p v)
        /// @param[in] u A vector [(x, y, z)]
        /// @param[in] v A vector [(x, y, z)]
        /// @return The dot product on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        public static float vDot2D(float[] u, float[] v)
        {
            return u[0] * v[0] + u[2] * v[2];
        }
        
        public static float vDot2D(Vector3f u, Vector3f v)
        {
            return u[0] * v[0] + u[2] * v[2];
        }


        public static float vDot2D(float[] u, float[] v, int vi)
        {
            return u[0] * v[vi] + u[2] * v[vi + 2];
        }

        /// Derives the xz-plane 2D perp product of the two vectors. (uz*vx - ux*vz)
        /// @param[in] u The LHV vector [(x, y, z)]
        /// @param[in] v The RHV vector [(x, y, z)]
        /// @return The dot product on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        public static float vPerp2D(float[] u, float[] v)
        {
            return u[2] * v[0] - u[0] * v[2];
        }
        
        public static float vPerp2D(Vector3f u, Vector3f v)
        {
            return u[2] * v[0] - u[0] * v[2];
        }
        

        /// @}
        /// @name Computational geometry helper functions.
        /// @{
        /// Derives the signed xz-plane area of the triangle ABC, or the
        /// relationship of line AB to point C.
        /// @param[in] a Vertex A. [(x, y, z)]
        /// @param[in] b Vertex B. [(x, y, z)]
        /// @param[in] c Vertex C. [(x, y, z)]
        /// @return The signed xz-plane area of the triangle.
        public static float triArea2D(float[] verts, int a, int b, int c)
        {
            float abx = verts[b] - verts[a];
            float abz = verts[b + 2] - verts[a + 2];
            float acx = verts[c] - verts[a];
            float acz = verts[c + 2] - verts[a + 2];
            return acx * abz - abx * acz;
        }

        public static float triArea2D(float[] a, float[] b, float[] c)
        {
            float abx = b[0] - a[0];
            float abz = b[2] - a[2];
            float acx = c[0] - a[0];
            float acz = c[2] - a[2];
            return acx * abz - abx * acz;
        }
        
        public static float triArea2D(Vector3f a, Vector3f b, Vector3f c)
        {
            float abx = b[0] - a[0];
            float abz = b[2] - a[2];
            float acx = c[0] - a[0];
            float acz = c[2] - a[2];
            return acx * abz - abx * acz;
        }


        /// Determines if two axis-aligned bounding boxes overlap.
        /// @param[in] amin Minimum bounds of box A. [(x, y, z)]
        /// @param[in] amax Maximum bounds of box A. [(x, y, z)]
        /// @param[in] bmin Minimum bounds of box B. [(x, y, z)]
        /// @param[in] bmax Maximum bounds of box B. [(x, y, z)]
        /// @return True if the two AABB's overlap.
        /// @see dtOverlapBounds
        public static bool overlapQuantBounds(int[] amin, int[] amax, int[] bmin, int[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }

        /// Determines if two axis-aligned bounding boxes overlap.
        /// @param[in] amin Minimum bounds of box A. [(x, y, z)]
        /// @param[in] amax Maximum bounds of box A. [(x, y, z)]
        /// @param[in] bmin Minimum bounds of box B. [(x, y, z)]
        /// @param[in] bmax Maximum bounds of box B. [(x, y, z)]
        /// @return True if the two AABB's overlap.
        /// @see dtOverlapQuantBounds
        public static bool overlapBounds(float[] amin, float[] amax, float[] bmin, float[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }
        
        public static bool overlapBounds(Vector3f amin, Vector3f amax, Vector3f bmin, Vector3f bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }

        public static Tuple<float, float> distancePtSegSqr2D(Vector3f pt, Vector3f p, Vector3f q)
        {
            float pqx = q[0] - p[0];
            float pqz = q[2] - p[2];
            float dx = pt[0] - p[0];
            float dz = pt[2] - p[2];
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }

            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = p[0] + t * pqx - pt[0];
            dz = p[2] + t * pqz - pt[2];
            return Tuple.Create(dx * dx + dz * dz, t);
        }

        public static float? closestHeightPointTriangle(Vector3f p, Vector3f a, Vector3f b, Vector3f c)
        {
            Vector3f v0 = vSub(c, a);
            Vector3f v1 = vSub(b, a);
            Vector3f v2 = vSub(p, a);

            // Compute scaled barycentric coordinates
            float denom = v0[0] * v1[2] - v0[2] * v1[0];
            if (Math.Abs(denom) < EPS)
            {
                return null;
            }

            float u = v1[2] * v2[0] - v1[0] * v2[2];
            float v = v0[0] * v2[2] - v0[2] * v2[0];

            if (denom < 0)
            {
                denom = -denom;
                u = -u;
                v = -v;
            }

            // If point lies inside the triangle, return interpolated ycoord.
            if (u >= 0.0f && v >= 0.0f && (u + v) <= denom)
            {
                float h = a[1] + (v0[1] * u + v1[1] * v) / denom;
                return h;
            }

            return null;
        }

        /// @par
        ///
        /// All points are projected onto the xz-plane, so the y-values are ignored.
        public static bool pointInPolygon(Vector3f pt, float[] verts, int nverts)
        {
            // TODO: Replace pnpoly with triArea2D tests?
            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                int vi = i * 3;
                int vj = j * 3;
                if (((verts[vi + 2] > pt[2]) != (verts[vj + 2] > pt[2])) && (pt[0] < (verts[vj + 0] - verts[vi + 0])
                        * (pt[2] - verts[vi + 2]) / (verts[vj + 2] - verts[vi + 2]) + verts[vi + 0]))
                {
                    c = !c;
                }
            }

            return c;
        }

        public static bool distancePtPolyEdgesSqr(Vector3f pt, float[] verts, int nverts, float[] ed, float[] et)
        {
            // TODO: Replace pnpoly with triArea2D tests?
            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                int vi = i * 3;
                int vj = j * 3;
                if (((verts[vi + 2] > pt[2]) != (verts[vj + 2] > pt[2])) && (pt[0] < (verts[vj + 0] - verts[vi + 0])
                        * (pt[2] - verts[vi + 2]) / (verts[vj + 2] - verts[vi + 2]) + verts[vi + 0]))
                {
                    c = !c;
                }

                Tuple<float, float> edet = distancePtSegSqr2D(pt, verts, vj, vi);
                ed[j] = edet.Item1;
                et[j] = edet.Item2;
            }

            return c;
        }

        public static float[] projectPoly(float[] axis, float[] poly, int npoly)
        {
            float rmin, rmax;
            rmin = rmax = vDot2D(axis, poly, 0);
            for (int i = 1; i < npoly; ++i)
            {
                float d = vDot2D(axis, poly, i * 3);
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }

            return new float[] { rmin, rmax };
        }

        public static bool overlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return ((amin + eps) > bmax || (amax - eps) < bmin) ? false : true;
        }

        const float eps = 1e-4f;

        /// @par
        ///
        /// All vertices are projected onto the xz-plane, so the y-values are ignored.
        public static bool overlapPolyPoly2D(float[] polya, int npolya, float[] polyb, int npolyb)
        {
            for (int i = 0, j = npolya - 1; i < npolya; j = i++)
            {
                int va = j * 3;
                int vb = i * 3;

                float[] n = new float[] { polya[vb + 2] - polya[va + 2], 0, -(polya[vb + 0] - polya[va + 0]) };

                float[] aminmax = projectPoly(n, polya, npolya);
                float[] bminmax = projectPoly(n, polyb, npolyb);
                if (!overlapRange(aminmax[0], aminmax[1], bminmax[0], bminmax[1], eps))
                {
                    // Found separating axis
                    return false;
                }
            }

            for (int i = 0, j = npolyb - 1; i < npolyb; j = i++)
            {
                int va = j * 3;
                int vb = i * 3;

                float[] n = new float[] { polyb[vb + 2] - polyb[va + 2], 0, -(polyb[vb + 0] - polyb[va + 0]) };

                float[] aminmax = projectPoly(n, polya, npolya);
                float[] bminmax = projectPoly(n, polyb, npolyb);
                if (!overlapRange(aminmax[0], aminmax[1], bminmax[0], bminmax[1], eps))
                {
                    // Found separating axis
                    return false;
                }
            }

            return true;
        }

        // Returns a random point in a convex polygon.
        // Adapted from Graphics Gems article.
        public static Vector3f randomPointInConvexPoly(float[] pts, int npts, float[] areas, float s, float t)
        {
            // Calc triangle araes
            float areasum = 0.0f;
            for (int i = 2; i < npts; i++)
            {
                areas[i] = triArea2D(pts, 0, (i - 1) * 3, i * 3);
                areasum += Math.Max(0.001f, areas[i]);
            }

            // Find sub triangle weighted by area.
            float thr = s * areasum;
            float acc = 0.0f;
            float u = 1.0f;
            int tri = npts - 1;
            for (int i = 2; i < npts; i++)
            {
                float dacc = areas[i];
                if (thr >= acc && thr < (acc + dacc))
                {
                    u = (thr - acc) / dacc;
                    tri = i;
                    break;
                }

                acc += dacc;
            }

            float v = (float)Math.Sqrt(t);

            float a = 1 - v;
            float b = (1 - u) * v;
            float c = u * v;
            int pa = 0;
            int pb = (tri - 1) * 3;
            int pc = tri * 3;

            return new Vector3f()
            {
                x = a * pts[pa] + b * pts[pb] + c * pts[pc],
                y = a * pts[pa + 1] + b * pts[pb + 1] + c * pts[pc + 1],
                z = a * pts[pa + 2] + b * pts[pb + 2] + c * pts[pc + 2]
            };
        }

        public static int nextPow2(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        public static int ilog2(int v)
        {
            int r;
            int shift;
            r = (v > 0xffff ? 1 : 0) << 4;
            v >>= r;
            shift = (v > 0xff ? 1 : 0) << 3;
            v >>= shift;
            r |= shift;
            shift = (v > 0xf ? 1 : 0) << 2;
            v >>= shift;
            r |= shift;
            shift = (v > 0x3 ? 1 : 0) << 1;
            v >>= shift;
            r |= shift;
            r |= (v >> 1);
            return r;
        }

        public class IntersectResult
        {
            public bool intersects;
            public float tmin;
            public float tmax = 1f;
            public int segMin = -1;
            public int segMax = -1;
        }

        public static IntersectResult intersectSegmentPoly2D(Vector3f p0, Vector3f p1, float[] verts, int nverts)
        {
            IntersectResult result = new IntersectResult();
            float EPS = 0.000001f;
            var dir = vSub(p1, p0);

            var p0v = p0;
            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                VectorPtr vpj = new VectorPtr(verts, j * 3);
                var edge = vSub(new VectorPtr(verts, i * 3), vpj);
                var diff = vSub(p0v, vpj);
                float n = vPerp2D(edge, diff);
                float d = vPerp2D(dir, edge);
                if (Math.Abs(d) < EPS)
                {
                    // S is nearly parallel to this edge
                    if (n < 0)
                    {
                        return result;
                    }
                    else
                    {
                        continue;
                    }
                }

                float t = n / d;
                if (d < 0)
                {
                    // segment S is entering across this edge
                    if (t > result.tmin)
                    {
                        result.tmin = t;
                        result.segMin = j;
                        // S enters after leaving polygon
                        if (result.tmin > result.tmax)
                        {
                            return result;
                        }
                    }
                }
                else
                {
                    // segment S is leaving across this edge
                    if (t < result.tmax)
                    {
                        result.tmax = t;
                        result.segMax = j;
                        // S leaves before entering polygon
                        if (result.tmax < result.tmin)
                        {
                            return result;
                        }
                    }
                }
            }

            result.intersects = true;
            return result;
        }

        public static Tuple<float, float> distancePtSegSqr2D(Vector3f pt, float[] verts, int p, int q)
        {
            float pqx = verts[q + 0] - verts[p + 0];
            float pqz = verts[q + 2] - verts[p + 2];
            float dx = pt[0] - verts[p + 0];
            float dz = pt[2] - verts[p + 2];
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }

            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = verts[p + 0] + t * pqx - pt[0];
            dz = verts[p + 2] + t * pqz - pt[2];
            return Tuple.Create(dx * dx + dz * dz, t);
        }

        public static int oppositeTile(int side)
        {
            return (side + 4) & 0x7;
        }

        public static float vperpXZ(float[] a, float[] b)
        {
            return a[0] * b[2] - a[2] * b[0];
        }
        
        public static float vperpXZ(Vector3f a, Vector3f b)
        {
            return a[0] * b[2] - a[2] * b[0];
        }


        public static Tuple<float, float>? intersectSegSeg2D(float[] ap, float[] aq, float[] bp, float[] bq)
        {
            float[] u = vSub(aq, ap);
            float[] v = vSub(bq, bp);
            float[] w = vSub(ap, bp);
            float d = vperpXZ(u, v);
            if (Math.Abs(d) < 1e-6f)
            {
                return null;
            }

            float s = vperpXZ(v, w) / d;
            float t = vperpXZ(u, w) / d;
            return Tuple.Create(s, t);
        }
        
        public static Tuple<float, float>? intersectSegSeg2D(Vector3f ap, Vector3f aq, Vector3f bp, Vector3f bq)
        {
            Vector3f u = vSub(aq, ap);
            Vector3f v = vSub(bq, bp);
            Vector3f w = vSub(ap, bp);
            float d = vperpXZ(u, v);
            if (Math.Abs(d) < 1e-6f)
            {
                return null;
            }

            float s = vperpXZ(v, w) / d;
            float t = vperpXZ(u, w) / d;
            return Tuple.Create(s, t);
        }


        public static float[] vScale(float[] @in, float scale)
        {
            float[] @out = new float[3];
            @out[0] = @in[0] * scale;
            @out[1] = @in[1] * scale;
            @out[2] = @in[2] * scale;
            return @out;
        }
        
        public static Vector3f vScale(Vector3f @in, float scale)
        {
            var @out = new Vector3f();
            @out[0] = @in[0] * scale;
            @out[1] = @in[1] * scale;
            @out[2] = @in[2] * scale;
            return @out;
        }


        /// Checks that the specified vector's components are all finite.
        /// @param[in] v A point. [(x, y, z)]
        /// @return True if all of the point's components are finite, i.e. not NaN
        /// or any of the infinities.
        public static bool vIsFinite(float[] v)
        {
            return float.IsFinite(v[0]) && float.IsFinite(v[1]) && float.IsFinite(v[2]);
        }
        
        public static bool vIsFinite(Vector3f v)
        {
            return float.IsFinite(v[0]) && float.IsFinite(v[1]) && float.IsFinite(v[2]);
        }

        /// Checks that the specified vector's 2D components are finite.
        /// @param[in] v A point. [(x, y, z)]
        public static bool vIsFinite2D(float[] v)
        {
            return float.IsFinite(v[0]) && float.IsFinite(v[2]);
        }

        public static bool vIsFinite2D(Vector3f v)
        {
            return float.IsFinite(v[0]) && float.IsFinite(v[2]);
        }
    }
}