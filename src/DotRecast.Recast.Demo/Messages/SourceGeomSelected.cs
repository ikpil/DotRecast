namespace DotRecast.Recast.Demo.Messages;

public class SourceGeomSelected : IRecastDemoMessage
{
    public required string FilePath { get; init; }
}