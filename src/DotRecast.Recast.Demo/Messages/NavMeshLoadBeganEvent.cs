namespace DotRecast.Recast.Demo.Messages;

public class NavMeshLoadBeganEvent : IRecastDemoMessage
{
    public required string FilePath { get; init; }
}