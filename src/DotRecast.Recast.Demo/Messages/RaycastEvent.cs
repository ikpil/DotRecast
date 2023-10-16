using DotRecast.Core.Numerics;

namespace DotRecast.Recast.Demo.Messages;

public class RaycastEvent : IRecastDemoMessage
{
    public required RcVec3f Start { get; init; }
    public required RcVec3f End { get; init; }
}