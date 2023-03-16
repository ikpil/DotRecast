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
    public class CapsuleCollider : AbstractCollider
    {
        private readonly float[] start;
        private readonly float[] end;
        private readonly float radius;

        public CapsuleCollider(float[] start, float[] end, float radius, int area, float flagMergeThreshold) :
            base(area, flagMergeThreshold, bounds(start, end, radius))
        {
            this.start = start;
            this.end = end;
            this.radius = radius;
        }

        public void rasterize(Heightfield hf, Telemetry telemetry)
        {
            RecastFilledVolumeRasterization.rasterizeCapsule(hf, start, end, radius, area, (int)Math.Floor(flagMergeThreshold / hf.ch),
                telemetry);
        }

        private static float[] bounds(float[] start, float[] end, float radius)
        {
            return new float[]
            {
                Math.Min(start[0], end[0]) - radius, Math.Min(start[1], end[1]) - radius,
                Math.Min(start[2], end[2]) - radius, Math.Max(start[0], end[0]) + radius, Math.Max(start[1], end[1]) + radius,
                Math.Max(start[2], end[2]) + radius
            };
        }
    }
}