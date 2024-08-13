/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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

using NUnit.Framework;

namespace DotRecast.Detour.Test;


public class ConvexConvexIntersectionTest
{
    [Test]
    public void ShouldHandleSamePolygonIntersection()
    {
        float[] p = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        float[] q = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        float[] intersection = DtConvexConvexIntersections.Intersect(p, q);
        Assert.That(intersection.Length, Is.EqualTo(5 * 3));
        Assert.That(intersection, Is.EqualTo(p));
    }

    [Test]
    public void ShouldHandleIntersection()
    {
        float[] p = { -5, 0, -5, -5, 0, 4, 1, 0, 4, 1, 0, -5 };
        float[] q = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        float[] intersection = DtConvexConvexIntersections.Intersect(p, q);
        Assert.That(intersection.Length, Is.EqualTo(5 * 3));
        Assert.That(intersection, Is.EqualTo(new[] { 1, 0, 3, 1, 0, -3.4f, -2, 0, -4, -4, 0, 0, -3, 0, 3 }));
    }
}