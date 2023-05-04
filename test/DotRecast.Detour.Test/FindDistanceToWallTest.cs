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

using DotRecast.Core;
using DotRecast.Detour.QueryResults;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

[Parallelizable]
public class FindDistanceToWallTest : AbstractDetourTest
{
    private static readonly float[] DISTANCES_TO_WALL = { 0.597511f, 3.201085f, 0.603713f, 2.791475f, 2.815544f };

    private static readonly Vector3f[] HIT_POSITION =
    {
        Vector3f.Of(23.177608f, 10.197294f, -45.742954f),
        Vector3f.Of(22.331268f, 10.197294f, -4.241272f),
        Vector3f.Of(18.108675f, 15.743596f, -73.236839f),
        Vector3f.Of(1.984785f, 10.197294f, -8.441269f),
        Vector3f.Of(-22.315216f, 4.997294f, -11.441269f),
    };

    private static readonly Vector3f[] HIT_NORMAL =
    {
        Vector3f.Of(-0.955779f, 0.0f, -0.29408592f),
        Vector3f.Of(0.0f, 0.0f, 1.0f),
        Vector3f.Of(0.97014254f, 0.0f, 0.24253564f),
        Vector3f.Of(-1.0f, 0.0f, 0.0f),
        Vector3f.Of(1.0f, 0.0f, 0.0f),
    };

    [Test]
    public void TestFindDistanceToWall()
    {
        QueryFilter filter = new DefaultQueryFilter();
        for (int i = 0; i < startRefs.Length; i++)
        {
            Vector3f startPos = startPoss[i];
            Result<FindDistanceToWallResult> result = query.FindDistanceToWall(startRefs[i], startPos, 3.5f, filter);
            FindDistanceToWallResult hit = result.result;
            Assert.That(hit.GetDistance(), Is.EqualTo(DISTANCES_TO_WALL[i]).Within(0.001f));
            
            Assert.That(hit.GetPosition().x, Is.EqualTo(HIT_POSITION[i].x).Within(0.001f));
            Assert.That(hit.GetPosition().y, Is.EqualTo(HIT_POSITION[i].y).Within(0.001f));
            Assert.That(hit.GetPosition().z, Is.EqualTo(HIT_POSITION[i].z).Within(0.001f));

            Assert.That(hit.GetNormal().x, Is.EqualTo(HIT_NORMAL[i].x).Within(0.001f));
            Assert.That(hit.GetNormal().y, Is.EqualTo(HIT_NORMAL[i].y).Within(0.001f));
            Assert.That(hit.GetNormal().z, Is.EqualTo(HIT_NORMAL[i].z).Within(0.001f));
        }
    }
}