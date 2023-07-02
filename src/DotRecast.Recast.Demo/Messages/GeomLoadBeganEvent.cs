namespace DotRecast.Recast.Demo.Messages;

public class GeomLoadBeganEvent : IRecastDemoMessage
{
    public required string FilePath { get; init; }
}