/*
recast4j copyright (c) 2021-2026 Piotr Piastucki piotr@recast4j.org
DotRecast Copyright (c) 2023-2026 Choi Ikpil ikpil@naver.com

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
using DotRecast.Core.Numerics;

namespace DotRecast.Detour
{
    // Calculate the intersection between a polygon and a circle. A dodecagon is used as an approximation of the circle.
    public class DtStrictDtPolygonByCircleConstraint : IDtPolygonByCircleConstraint
    {
        private const int CIRCLE_SEGMENTS = 12;
        private static readonly float[] UnitCircle = CreateCircle();

        public static readonly IDtPolygonByCircleConstraint Shared = new DtStrictDtPolygonByCircleConstraint();

        private DtStrictDtPolygonByCircleConstraint()
        {
        }

        public static float[] CreateCircle()
        {
            var temp = new float[CIRCLE_SEGMENTS * 3];
            for (int i = 0; i < CIRCLE_SEGMENTS; i++)
            {
                float a = i * MathF.PI * 2 / CIRCLE_SEGMENTS;
                temp[3 * i] = MathF.Cos(a);
                temp[3 * i + 1] = 0;
                temp[3 * i + 2] = -MathF.Sin(a);
            }

            return temp;
        }

        public static void ScaleCircle(ReadOnlySpan<float> src, RcVec3f center, float radius, Span<float> dst)
        {
            for (int i = 0; i < CIRCLE_SEGMENTS; i++)
            {
                dst[3 * i] = src[3 * i] * radius + center.X;
                dst[3 * i + 1] = center.Y;
                dst[3 * i + 2] = src[3 * i + 2] * radius + center.Z;
            }
        }


        public bool Apply(Span<float> verts, RcVec3f center, float radius, Span<float> constrainedVerts, out int constrainedVertCount)
        {
            float radiusSqr = radius * radius;
            int outsideVertex = -1;
            for (int pv = 0; pv < verts.Length; pv += 3)
            {
                if (RcVec.Dist2DSqr(center, verts, pv) > radiusSqr)
                {
                    outsideVertex = pv;
                    break;
                }
            }

            if (outsideVertex == -1)
            {
                // polygon inside circle
                verts.CopyTo(constrainedVerts);
                constrainedVertCount = verts.Length;
                return true;
            }

            Span<float> qCircle = stackalloc float[UnitCircle.Length];
            ScaleCircle(UnitCircle, center, radius, qCircle);

            bool result = DtConvexPolygonIntersector.Intersect(verts, qCircle, constrainedVerts, out int nverts);
            if (!result && DtUtils.PointInPolygon(center, verts, verts.Length / 3))
            {
                // A rejected thin intersection does not imply circle containment.
                // Check every circle vertex before using the containment fallback.
                bool circleInsidePolygon = true;
                for (int i = 0; i < qCircle.Length; i += 3)
                {
                    RcVec3f vertex = new RcVec3f(qCircle[i], qCircle[i + 1], qCircle[i + 2]);
                    if (!DtUtils.PointInPolygon(vertex, verts, verts.Length / 3))
                    {
                        circleInsidePolygon = false;
                        break;
                    }
                }
                if (circleInsidePolygon)
                {
                    // circle inside polygon
                    qCircle.CopyTo(constrainedVerts);
                    constrainedVertCount = qCircle.Length;
                    return true;
                }
            }

            constrainedVertCount = nverts;
            return true;
        }
    }
}
