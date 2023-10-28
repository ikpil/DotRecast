namespace DotRecast.Recast.Demo.Messages;

public class GeomLoadBeganEvent : IRecastDemoMessage
{
    public string FilePath { get; init; }
}