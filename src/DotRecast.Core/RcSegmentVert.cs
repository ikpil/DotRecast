using System.Numerics;

namespace DotRecast.Core
{
    public struct RcSegmentVert
    {
        public Vector3 vmin;
        public Vector3 vmax;

        public RcSegmentVert(float v0, float v1, float v2, float v3, float v4, float v5)
        {
            vmin.X = v0;
            vmin.Y = v1;
            vmin.Z = v2;
            
            vmax.X = v3;
            vmax.Y = v4;
            vmax.Z = v5;
        }

    }
}