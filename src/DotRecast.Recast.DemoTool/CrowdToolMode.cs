using System.Collections.Immutable;

namespace DotRecast.Recast.DemoTool
{
    public class CrowdToolMode
    {
        public static readonly CrowdToolMode CREATE = new CrowdToolMode(0, "Create Agents");
        public static readonly CrowdToolMode MOVE_TARGET = new CrowdToolMode(1, "Move Target");
        public static readonly CrowdToolMode SELECT = new CrowdToolMode(2, "Select Agent");
        public static readonly CrowdToolMode TOGGLE_POLYS = new CrowdToolMode(3, "Toggle Polys");
        public static readonly CrowdToolMode PROFILING = new CrowdToolMode(4, "Profiling");

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
}