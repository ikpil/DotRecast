﻿namespace DotRecast.Recast
{
    public readonly struct RcDirtyEntry
    {
        public readonly int index;
        public readonly int region;
        public readonly int distance2;

        public RcDirtyEntry(int tempIndex, int tempRegion, int tempDistance2)
        {
            index = tempIndex;
            region = tempRegion;
            distance2 = tempDistance2;
        }
    }
}