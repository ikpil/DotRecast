using DotRecast.Recast.Toolset.Builder;

namespace DotRecast.Recast.Toolset.Tools
{
    public class TestNavmeshToolOption
    {
        public int modeIdx = TestNavmeshToolMode.PATHFIND_FOLLOW.Idx;
        public TestNavmeshToolMode mode => TestNavmeshToolMode.Values[modeIdx];

        public int straightPathOptions;
        public bool constrainByCircle;
        
        public int includeFlags = SampleAreaModifications.SAMPLE_POLYFLAGS_ALL;
        public int excludeFlags = 0;
        
        public bool enableRaycast = true;
    }
}