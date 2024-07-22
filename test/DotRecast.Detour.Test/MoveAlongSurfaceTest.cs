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

using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class MoveAlongSurfaceTest : AbstractDetourTest
{
    private static readonly long[][] VISITED =
    [
        [
            281474976710696L, 281474976710695L, 281474976710694L, 281474976710703L, 281474976710706L,
            281474976710705L, 281474976710702L, 281474976710701L, 281474976710714L, 281474976710713L,
            281474976710712L, 281474976710727L, 281474976710730L, 281474976710717L, 281474976710721L
        ],
        [
            281474976710773L, 281474976710772L, 281474976710768L, 281474976710754L, 281474976710755L,
            281474976710753L
        ],
        [
            281474976710680L, 281474976710684L, 281474976710688L, 281474976710687L, 281474976710686L,
            281474976710697L, 281474976710695L, 281474976710694L, 281474976710703L, 281474976710706L,
            281474976710705L, 281474976710702L, 281474976710701L, 281474976710714L, 281474976710713L,
            281474976710712L, 281474976710727L, 281474976710730L, 281474976710717L, 281474976710721L,
            281474976710718L
        ],
        [281474976710753L, 281474976710748L, 281474976710752L, 281474976710731L],
        [
            281474976710733L, 281474976710736L, 281474976710738L, 281474976710737L, 281474976710728L,
            281474976710724L, 281474976710717L, 281474976710729L, 281474976710731L, 281474976710752L,
            281474976710748L, 281474976710753L, 281474976710755L, 281474976710754L, 281474976710768L,
            281474976710772L
        ]
    ];

    private static readonly Vector3[] POSITION =
    [
        new Vector3(6.457663f, 10.197294f, -18.334061f),
        new Vector3(-1.433933f, 10.197294f, -1.359993f),
        new Vector3(12.184784f, 9.997294f, -18.941269f),
        new Vector3(0.863553f, 10.197294f, -10.310320f),
        new Vector3(18.784092f, 10.197294f, 3.054368f),
    ];

    [Test]
    public void TestMoveAlongSurface()
    {
        IDtQueryFilter filter = new DtQueryDefaultFilter();
        const int MAX_VISITED = 32;
        Span<long> visited = stackalloc long[MAX_VISITED];
        for (int i = 0; i < startRefs.Length; i++)
        {
            long startRef = startRefs[i];
            Vector3 startPos = startPoss[i];
            Vector3 endPos = endPoss[i];
            var status = query.MoveAlongSurface(startRef, startPos, endPos, filter, out var result, visited, out var nvisited, MAX_VISITED);
            Assert.That(status.Succeeded(), Is.True);

            Assert.That(result.X, Is.EqualTo(POSITION[i].X).Within(0.01f));
            Assert.That(result.Y, Is.EqualTo(POSITION[i].Y).Within(0.01f));
            Assert.That(result.Z, Is.EqualTo(POSITION[i].Z).Within(0.01f));

            Assert.That(nvisited, Is.EqualTo(VISITED[i].Length));
            for (int j = 0; j < 3; j++)
            {
                Assert.That(visited[j], Is.EqualTo(VISITED[i][j]));
            }
        }
    }
}