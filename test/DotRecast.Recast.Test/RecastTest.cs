/*
recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

namespace DotRecast.Recast.Test;

using static RcConstants;

[Parallelizable]
public class RecastTest
{
    [Test]
    public void TestClearUnwalkableTriangles()
    {
        float walkableSlopeAngle = 45;
        float[] verts = { 0, 0, 0, 1, 0, 0, 0, 0, -1 };
        int nv = 3;
        int[] walkable_tri = { 0, 1, 2 };
        int[] unwalkable_tri = { 0, 2, 1 };
        int nt = 1;

        Telemetry ctx = new Telemetry();
        {
            int[] areas = { 42 };
            Recast.ClearUnwalkableTriangles(ctx, walkableSlopeAngle, verts, nv, unwalkable_tri, nt, areas);
            Assert.That(areas[0], Is.EqualTo(RC_NULL_AREA), "Sets area ID of unwalkable triangle to RC_NULL_AREA");
        }
        {
            int[] areas = { 42 };
            Recast.ClearUnwalkableTriangles(ctx, walkableSlopeAngle, verts, nv, walkable_tri, nt, areas);
            Assert.That(areas[0], Is.EqualTo(42), "Does not modify walkable triangle aread ID's");
        }
        {
            int[] areas = { 42 };
            walkableSlopeAngle = 0;
            Recast.ClearUnwalkableTriangles(ctx, walkableSlopeAngle, verts, nv, walkable_tri, nt, areas);
            Assert.That(areas[0], Is.EqualTo(RC_NULL_AREA), "Slopes equal to the max slope are considered unwalkable.");
        }
    }
}
