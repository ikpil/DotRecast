/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j Copyright (c) 2015-2026 Piotr Piastucki piotr@jtilia.org
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

using DotRecast.Core;
using DotRecast.Core.Numerics;
using NUnit.Framework;

namespace DotRecast.Recast.Test;

public class RecastFilterTest
{
    [Test]
    public void TestFilterLowHangingWalkableObstacles()
    {
        RcContext context = new RcContext();
        const int walkableHeight = 5;
        RcHeightfield heightfield = new RcHeightfield(1, 1, RcVec3f.Zero, RcVec3f.One, 1, 1, 0);

        // A span with no span above it is unchanged.
        RcSpan span = new RcSpan { area = 1, smin = 0, smax = 1 };
        heightfield.spans[0] = span;

        RcFilters.FilterLowHangingWalkableObstacles(context, walkableHeight, heightfield);

        Assert.That(heightfield.spans[0].area, Is.EqualTo(1));

        // A span above the walkable height is unchanged.
        RcSpan secondSpan = new RcSpan
        {
            area = RcRecast.RC_NULL_AREA,
            smin = 1 + walkableHeight,
            smax = 2 + walkableHeight,
        };
        span = new RcSpan { area = 1, next = secondSpan, smin = 0, smax = 1 };
        heightfield.spans[0] = span;

        RcFilters.FilterLowHangingWalkableObstacles(context, walkableHeight, heightfield);

        Assert.That(heightfield.spans[0].area, Is.EqualTo(1));
        Assert.That(heightfield.spans[0].next.area, Is.EqualTo(RcRecast.RC_NULL_AREA));

        secondSpan.smin += 10;
        secondSpan.smax += 10;

        RcFilters.FilterLowHangingWalkableObstacles(context, walkableHeight, heightfield);

        Assert.That(heightfield.spans[0].area, Is.EqualTo(1));
        Assert.That(heightfield.spans[0].next.area, Is.EqualTo(RcRecast.RC_NULL_AREA));

        // A low obstacle below the walkable height becomes walkable.
        secondSpan = new RcSpan
        {
            area = RcRecast.RC_NULL_AREA,
            smin = 1 + (walkableHeight - 1),
            smax = 2 + (walkableHeight - 1),
        };
        span = new RcSpan { area = 1, next = secondSpan, smin = 0, smax = 1 };
        heightfield.spans[0] = span;

        RcFilters.FilterLowHangingWalkableObstacles(context, walkableHeight, heightfield);

        Assert.That(heightfield.spans[0].area, Is.EqualTo(1));
        Assert.That(heightfield.spans[0].next.area, Is.EqualTo(1));

        // An obstacle that overlaps the walkable-height distance is unchanged.
        secondSpan = new RcSpan
        {
            area = RcRecast.RC_NULL_AREA,
            smin = 2 + (walkableHeight - 1),
            smax = 3 + (walkableHeight - 1),
        };
        span = new RcSpan { area = 1, next = secondSpan, smin = 0, smax = 1 };
        heightfield.spans[0] = span;

        RcFilters.FilterLowHangingWalkableObstacles(context, walkableHeight, heightfield);

        Assert.That(heightfield.spans[0].area, Is.EqualTo(1));
        Assert.That(heightfield.spans[0].next.area, Is.EqualTo(RcRecast.RC_NULL_AREA));
    }

    [Test]
    public void TestFilterLedgeSpans()
    {
        RcContext context = new RcContext();
        const int walkableClimb = 5;
        const int walkableHeight = 10;
        RcHeightfield heightfield = new RcHeightfield(
            10, 10, RcVec3f.Zero, new RcVec3f(10, 1, 10), 1, 1, 0);

        for (int x = 0; x < heightfield.width; ++x)
        {
            for (int z = 0; z < heightfield.height; ++z)
            {
                heightfield.spans[x + z * heightfield.width] = new RcSpan
                {
                    area = 1,
                    smin = 0,
                    smax = 1,
                };
            }
        }

        RcFilters.FilterLedgeSpans(context, walkableHeight, walkableClimb, heightfield);

        for (int x = 0; x < heightfield.width; ++x)
        {
            for (int z = 0; z < heightfield.height; ++z)
            {
                RcSpan span = heightfield.spans[x + z * heightfield.width];
                Assert.That(span, Is.Not.Null);

                if (x == 0 || z == 0 || x == 9 || z == 9)
                {
                    Assert.That(span.area, Is.EqualTo(RcRecast.RC_NULL_AREA),
                        $"Edge span at ({x}, {z}) should be marked unwalkable");
                }
                else
                {
                    Assert.That(span.area, Is.EqualTo(1),
                        $"Interior span at ({x}, {z}) should remain walkable");
                }

                Assert.That(span.next, Is.Null);
                Assert.That(span.smin, Is.Zero);
                Assert.That(span.smax, Is.EqualTo(1));
            }
        }
    }

    [Test]
    public void TestFilterWalkableLowHeightSpans()
    {
        RcContext context = new RcContext();
        const int walkableHeight = 5;
        RcHeightfield heightfield = new RcHeightfield(1, 1, RcVec3f.Zero, RcVec3f.One, 1, 1, 0);

        // A span with nothing above it is unchanged.
        RcSpan span = new RcSpan { area = 1, smin = 0, smax = 1 };
        heightfield.spans[0] = span;

        RcFilters.FilterWalkableLowHeightSpans(context, walkableHeight, heightfield);

        Assert.That(heightfield.spans[0].area, Is.EqualTo(1));

        // A span with enough room above it is unchanged.
        RcSpan overheadSpan = new RcSpan
        {
            area = RcRecast.RC_NULL_AREA,
            smin = 10,
            smax = 11,
        };
        span = new RcSpan { area = 1, next = overheadSpan, smin = 0, smax = 1 };
        heightfield.spans[0] = span;

        RcFilters.FilterWalkableLowHeightSpans(context, walkableHeight, heightfield);

        Assert.That(heightfield.spans[0].area, Is.EqualTo(1));
        Assert.That(heightfield.spans[0].next.area, Is.EqualTo(RcRecast.RC_NULL_AREA));

        // A span with a low-hanging obstacle becomes unwalkable.
        overheadSpan = new RcSpan
        {
            area = RcRecast.RC_NULL_AREA,
            smin = 3,
            smax = 4,
        };
        span = new RcSpan { area = 1, next = overheadSpan, smin = 0, smax = 1 };
        heightfield.spans[0] = span;

        RcFilters.FilterWalkableLowHeightSpans(context, walkableHeight, heightfield);

        Assert.That(heightfield.spans[0].area, Is.EqualTo(RcRecast.RC_NULL_AREA));
        Assert.That(heightfield.spans[0].next.area, Is.EqualTo(RcRecast.RC_NULL_AREA));
    }
}
