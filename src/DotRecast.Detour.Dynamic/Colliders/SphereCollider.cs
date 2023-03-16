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

using System;
using DotRecast.Recast;

namespace DotRecast.Detour.Dynamic.Colliders
{


public class SphereCollider : AbstractCollider {

    private readonly float[] center;
    private readonly float radius;

    public SphereCollider(float[] center, float radius, int area, float flagMergeThreshold) : 
        base(area, flagMergeThreshold, bounds(center, radius)) {
        this.center = center;
        this.radius = radius;
    }

    public void rasterize(Heightfield hf, Telemetry telemetry) {
        RecastFilledVolumeRasterization.rasterizeSphere(hf, center, radius, area, (int) Math.Floor(flagMergeThreshold / hf.ch),
                telemetry);
    }

    private static float[] bounds(float[] center, float radius) {
        return new float[] { center[0] - radius, center[1] - radius, center[2] - radius, center[0] + radius, center[1] + radius,
                center[2] + radius };
    }

}

}