using DotRecast.Core.Numerics;

namespace DotRecast.Recast.Demo.Messages;

public class RaycastEvent : IRecastDemoMessage
{
    public RcVec3f Start { get; init; }
    public RcVec3f End { get; init; }
}