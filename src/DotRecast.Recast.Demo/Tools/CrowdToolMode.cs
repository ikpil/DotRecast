using System.Collections.Immutable;

namespace DotRecast.Recast.Demo.Tools;

public class CrowdToolMode
{
    public static readonly CrowdToolMode CREATE = new(0, "Create Agents");
    public static readonly CrowdToolMode MOVE_TARGET = new(1, "Move Target");
    public static readonly CrowdToolMode SELECT = new(2, "Select Agent");
    public static readonly CrowdToolMode TOGGLE_POLYS = new(3, "Toggle Polys");
    public static readonly CrowdToolMode PROFILING = new(4, "Profiling");

    public static readonly ImmutableArray<CrowdToolMode> Values = ImmutableArray.Create(
        CREATE,
        MOVE_TARGET,
        SELECT,
        TOGGLE_POLYS,
        PROFILING
    );

    public int Idx { get; }
    public string Label { get; }

    private CrowdToolMode(int idx, string label)
    {
        Idx = idx;
        Label = label;
    }
}