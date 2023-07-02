namespace DotRecast.Recast.Demo.Messages;

public class SourceGeomFileSelectedEvent : IRecastDemoMessage
{
    public required string FilePath { get; init; }
}