using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Recast.Demo.Messages;

public class RaycastEvent : IRecastDemoMessage
{
    public Vector3 Start { get; init; }
    public Vector3 End { get; init; }
}