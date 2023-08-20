using DotRecast.Recast.Toolset.Builder;

namespace DotRecast.Recast.Toolset.Tools
{
    public class RcTestNavmeshToolOption
    {
        public int modeIdx = RcTestNavmeshToolMode.PATHFIND_FOLLOW.Idx;
        public RcTestNavmeshToolMode mode => RcTestNavmeshToolMode.Values[modeIdx];

        public int straightPathOptions;
        public bool constrainByCircle;
        
        public int includeFlags = SampleAreaModifications.SAMPLE_POLYFLAGS_ALL;
        public int excludeFlags = 0;
        
        public bool enableRaycast = true;
    }
}