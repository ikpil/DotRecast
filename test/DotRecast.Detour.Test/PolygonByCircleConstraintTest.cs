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

using DotRecast.Core;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

[Parallelizable]
public class PolygonByCircleConstraintTest
{
    private readonly IDtPolygonByCircleConstraint _constraint = DtStrictDtPolygonByCircleConstraint.Shared;

    [Test]
    public void ShouldHandlePolygonFullyInsideCircle()
    {
        float[] polygon = { -2, 0, 2, 2, 0, 2, 2, 0, -2, -2, 0, -2 };
        RcVec3f center = RcVec3f.Of(1, 0, 1);
        float[] constrained = _constraint.Apply(polygon, center, 6);

        Assert.That(constrained, Is.EqualTo(polygon));
    }

    [Test]
    public void ShouldHandleVerticalSegment()
    {
        int expectedSize = 21;
        float[] polygon = { -2, 0, 2, 2, 0, 2, 2, 0, -2, -2, 0, -2 };
        RcVec3f center = RcVec3f.Of(2, 0, 0);

        float[] constrained = _constraint.Apply(polygon, center, 3);
        Assert.That(constrained.Length, Is.EqualTo(expectedSize));
        Assert.That(constrained, Is.SupersetOf(new[] { 2f, 0f, 2f, 2f, 0f, -2f }));
    }

    [Test]
    public void ShouldHandleCircleFullyInsidePolygon()
    {
        int expectedSize = 12 * 3;
        float[] polygon = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        RcVec3f center = RcVec3f.Of(-1, 0, -1);
        float[] constrained = _constraint.Apply(polygon, center, 2);

        Assert.That(constrained.Length, Is.EqualTo(expectedSize));

        for (int i = 0; i < expectedSize; i += 3)
        {
            float x = constrained[i] + 1;
            float z = constrained[i + 2] + 1;
            Assert.That(x * x + z * z, Is.EqualTo(4).Within(1e-4f));
        }
    }

    [Test]
    public void ShouldHandleCircleInsidePolygon()
    {
        int expectedSize = 9 * 3;
        float[] polygon = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        RcVec3f center = RcVec3f.Of(-2, 0, -1);
        float[] constrained = _constraint.Apply(polygon, center, 3);

        Assert.That(constrained.Length, Is.EqualTo(expectedSize));
        Assert.That(constrained, Is.SupersetOf(new[] { -2f, 0f, -4f, -4f, 0f, 0f, -3.4641016f, 0.0f, 1.6076951f, -2.0f, 0.0f, 2.0f }));
    }

    [Test]
    public void ShouldHandleCircleOutsidePolygon()
    {
        int expectedSize = 7 * 3;
        float[] polygon = { -4, 0, 0, -3, 0, 3, 2, 0, 3, 3, 0, -3, -2, 0, -4 };
        RcVec3f center = RcVec3f.Of(4, 0, 0);
        float[] constrained = _constraint.Apply(polygon, center, 4);

        Assert.That(constrained.Length, Is.EqualTo(expectedSize));
        Assert.That(constrained, Is.SupersetOf(new[] { 1.5358982f, 0f, 3f, 2f, 0f, 3f, 3f, 0f, -3f }));
    }
}