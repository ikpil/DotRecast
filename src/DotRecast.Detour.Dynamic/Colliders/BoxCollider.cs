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


public class BoxCollider : AbstractCollider {

    private readonly float[] center;
    private readonly float[][] halfEdges;
    public BoxCollider(float[] center, float[][] halfEdges, int area, float flagMergeThreshold) :
        base(area, flagMergeThreshold, bounds(center, halfEdges))
    {
        this.center = center;
        this.halfEdges = halfEdges;
    }

    private static float[] bounds(float[] center, float[][] halfEdges) {
        float[] bounds = new float[] { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity,
                float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity };
        for (int i = 0; i < 8; ++i) {
            float s0 = (i & 1) != 0 ? 1f : -1f;
            float s1 = (i & 2) != 0 ? 1f : -1f;
            float s2 = (i & 4) != 0 ? 1f : -1f;
            float vx = center[0] + s0 * halfEdges[0][0] + s1 * halfEdges[1][0] + s2 * halfEdges[2][0];
            float vy = center[1] + s0 * halfEdges[0][1] + s1 * halfEdges[1][1] + s2 * halfEdges[2][1];
            float vz = center[2] + s0 * halfEdges[0][2] + s1 * halfEdges[1][2] + s2 * halfEdges[2][2];
            bounds[0] = Math.Min(bounds[0], vx);
            bounds[1] = Math.Min(bounds[1], vy);
            bounds[2] = Math.Min(bounds[2], vz);
            bounds[3] = Math.Max(bounds[3], vx);
            bounds[4] = Math.Max(bounds[4], vy);
            bounds[5] = Math.Max(bounds[5], vz);
        }
        return bounds;
    }

    public void rasterize(Heightfield hf, Telemetry telemetry) {
        RecastFilledVolumeRasterization.rasterizeBox(hf, center, halfEdges, area, (int) Math.Floor(flagMergeThreshold / hf.ch),
                telemetry);
    }

    public static float[][] getHalfEdges(float[] up, float[] forward, float[] extent) {
        float[][] halfEdges = new float[][] { new float[3], new float[] { up[0], up[1], up[2] }, new float[3] };
        RecastVectors.normalize(halfEdges[1]);
        RecastVectors.cross(halfEdges[0], up, forward);
        RecastVectors.normalize(halfEdges[0]);
        RecastVectors.cross(halfEdges[2], halfEdges[0], up);
        RecastVectors.normalize(halfEdges[2]);
        halfEdges[0][0] *= extent[0];
        halfEdges[0][1] *= extent[0];
        halfEdges[0][2] *= extent[0];
        halfEdges[1][0] *= extent[1];
        halfEdges[1][1] *= extent[1];
        halfEdges[1][2] *= extent[1];
        halfEdges[2][0] *= extent[2];
        halfEdges[2][1] *= extent[2];
        halfEdges[2][2] *= extent[2];
        return halfEdges;
    }

}

}