using System.Collections.Immutable;

namespace DotRecast.Recast.Demo.Tools;

public class DynamicUpdateToolMode
{
    public static readonly DynamicUpdateToolMode BUILD = new(0, "Build");
    public static readonly DynamicUpdateToolMode COLLIDERS = new(1, "Colliders");
    public static readonly DynamicUpdateToolMode RAYCAST = new(2, "Raycast");

    public static readonly ImmutableArray<DynamicUpdateToolMode> Values = ImmutableArray.Create(
        BUILD, COLLIDERS, RAYCAST
    );

    public int Idx { get; }
    public string Label { get; }

    private DynamicUpdateToolMode(int idx, string label)
    {
        Idx = idx;
        Label = label;
    }
}