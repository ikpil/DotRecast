/*
recast4j Copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
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

using System.Numerics;

using NUnit.Framework;

namespace DotRecast.Detour.Test;


public class FindDistanceToWallTest : AbstractDetourTest
{
    private static readonly float[] DISTANCES_TO_WALL = { 0.597511f, 3.201085f, 0.603713f, 2.791475f, 2.815544f };

    private static readonly Vector3[] HIT_POSITION =
    {
        new Vector3(23.177608f, 10.197294f, -45.742954f),
        new Vector3(22.331268f, 10.197294f, -4.241272f),
        new Vector3(18.108675f, 15.743596f, -73.236839f),
        new Vector3(1.984785f, 10.197294f, -8.441269f),
        new Vector3(-22.315216f, 4.997294f, -11.441269f),
    };

    private static readonly Vector3[] HIT_NORMAL =
    {
        new Vector3(-0.955779f, 0.0f, -0.29408592f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.97014254f, 0.0f, 0.24253564f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
    };

    [Test]
    public void TestFindDistanceToWall()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        for (int i = 0; i < startRefs.Length; i++)
        {
            Vector3 startPos = startPoss[i];
            query.FindDistanceToWall(startRefs[i], startPos, 3.5f, filter,
                out var hitDist, out var hitPos, out var hitNormal);
            Assert.That(hitDist, Is.EqualTo(DISTANCES_TO_WALL[i]).Within(0.001f));

            Assert.That(hitPos.X, Is.EqualTo(HIT_POSITION[i].X).Within(0.001f));
            Assert.That(hitPos.Y, Is.EqualTo(HIT_POSITION[i].Y).Within(0.001f));
            Assert.That(hitPos.Z, Is.EqualTo(HIT_POSITION[i].Z).Within(0.001f));

            Assert.That(hitNormal.X, Is.EqualTo(HIT_NORMAL[i].X).Within(0.001f));
            Assert.That(hitNormal.Y, Is.EqualTo(HIT_NORMAL[i].Y).Within(0.001f));
            Assert.That(hitNormal.Z, Is.EqualTo(HIT_NORMAL[i].Z).Within(0.001f));
        }
    }
}