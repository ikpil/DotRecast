using DotRecast.Core;

namespace DotRecast.Recast.Demo.Tools;

public class AgentData
{
    public readonly AgentType type;
    public readonly RcVec3f home = new RcVec3f();

    public AgentData(AgentType type, RcVec3f home)
    {
        this.type = type;
        this.home = home;
    }
}