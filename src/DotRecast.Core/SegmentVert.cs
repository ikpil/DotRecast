using System;

namespace DotRecast.Core
{
    public struct SegmentVert
    {
        public RcVec3f vmin;
        public RcVec3f vmax;

        public float this[int index]
        {
            get => GetElement(index);
        }

        public float GetElement(int index)
        {
            switch (index)
            {
                case 0: return vmin.x;
                case 1: return vmin.y;
                case 2: return vmin.z;
                case 3: return vmax.x;
                case 4: return vmax.y;
                case 5: return vmax.z;
                default: throw new IndexOutOfRangeException($"{index}");
            }
        }
    }
}