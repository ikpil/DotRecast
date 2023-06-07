using DotRecast.Core;

namespace DotRecast.Recast.DemoTool
{
    public class CrowdAgentData
    {
        public readonly CrowdAgentType type;
        public readonly RcVec3f home = new RcVec3f();

        public CrowdAgentData(CrowdAgentType type, RcVec3f home)
        {
            this.type = type;
            this.home = home;
        }
    }
}