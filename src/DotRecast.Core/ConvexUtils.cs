/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

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

namespace DotRecast.Core
{
    public static class ConvexUtils
    {
        // Calculates convex hull on xz-plane of points on 'pts',
        // stores the indices of the resulting hull in 'out' and
        // returns number of points on hull.
        public static List<int> convexhull(List<float> pts)
        {
            int npts = pts.Count / 3;
            List<int> @out = new List<int>();
            // Find lower-leftmost point.
            int hull = 0;
            for (int i = 1; i < npts; ++i)
            {
                Vector3f a = Vector3f.Of(pts[i * 3], pts[i * 3 + 1], pts[i * 3 + 2]);
                Vector3f b = Vector3f.Of(pts[hull * 3], pts[hull * 3 + 1], pts[hull * 3 + 2]);
                if (cmppt(a, b))
                {
                    hull = i;
                }
            }

            // Gift wrap hull.
            int endpt = 0;
            do
            {
                @out.Add(hull);
                endpt = 0;
                for (int j = 1; j < npts; ++j)
                {
                    Vector3f a = Vector3f.Of(pts[hull * 3], pts[hull * 3 + 1], pts[hull * 3 + 2]);
                    Vector3f b = Vector3f.Of(pts[endpt * 3], pts[endpt * 3 + 1], pts[endpt * 3 + 2]);
                    Vector3f c = Vector3f.Of(pts[j * 3], pts[j * 3 + 1], pts[j * 3 + 2]);
                    if (hull == endpt || left(a, b, c))
                    {
                        endpt = j;
                    }
                }

                hull = endpt;
            } while (endpt != @out[0]);

            return @out;
        }

        // Returns true if 'a' is more lower-left than 'b'.
        private static bool cmppt(Vector3f a, Vector3f b)
        {
            if (a.x < b.x)
            {
                return true;
            }

            if (a.x > b.x)
            {
                return false;
            }

            if (a.z < b.z)
            {
                return true;
            }

            if (a.z > b.z)
            {
                return false;
            }

            return false;
        }

        // Returns true if 'c' is left of line 'a'-'b'.
        private static bool left(Vector3f a, Vector3f b, Vector3f c)
        {
            float u1 = b.x - a.x;
            float v1 = b.z - a.z;
            float u2 = c.x - a.x;
            float v2 = c.z - a.z;
            return u1 * v2 - v1 * u2 < 0;
        }
    }
}
