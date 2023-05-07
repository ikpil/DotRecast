using DotRecast.Core;

namespace DotRecast.Recast.Demo.Tools;

public class AgentData
{
    public readonly AgentType type;
    public readonly Vector3f home = new Vector3f();

    public AgentData(AgentType type, Vector3f home)
    {
        this.type = type;
        this.home = home;
    }
}