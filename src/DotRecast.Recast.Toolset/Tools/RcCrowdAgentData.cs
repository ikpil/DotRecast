using System.Numerics;

namespace DotRecast.Recast.Toolset.Tools
{
    public class RcCrowdAgentData
    {
        public readonly RcCrowdAgentType type;
        public readonly Vector3 home = new Vector3();

        public RcCrowdAgentData(RcCrowdAgentType type, Vector3 home)
        {
            this.type = type;
            this.home = home;
        }
    }
}