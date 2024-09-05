namespace DotRecast.Recast.Demo.Messages;

public class NavMeshLoadBeganEvent : IRecastDemoMessage
{
    public string FilePath { get; init; }
}