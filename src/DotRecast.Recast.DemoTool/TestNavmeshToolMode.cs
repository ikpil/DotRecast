using System.Collections.Immutable;

namespace DotRecast.Recast.DemoTool
{
    public class TestNavmeshToolMode
    {
        public static readonly TestNavmeshToolMode PATHFIND_FOLLOW = new TestNavmeshToolMode(0, "Pathfind Follow");
        public static readonly TestNavmeshToolMode PATHFIND_STRAIGHT = new TestNavmeshToolMode(1, "Pathfind Straight");
        public static readonly TestNavmeshToolMode PATHFIND_SLICED = new TestNavmeshToolMode(2, "Pathfind Sliced");
        public static readonly TestNavmeshToolMode DISTANCE_TO_WALL = new TestNavmeshToolMode(3, "Distance to Wall");
        public static readonly TestNavmeshToolMode RAYCAST = new TestNavmeshToolMode(4, "Raycast");
        public static readonly TestNavmeshToolMode FIND_POLYS_IN_CIRCLE = new TestNavmeshToolMode(5, "Find Polys in Circle");
        public static readonly TestNavmeshToolMode FIND_POLYS_IN_SHAPE = new TestNavmeshToolMode(6, "Find Polys in Shape");
        public static readonly TestNavmeshToolMode FIND_LOCAL_NEIGHBOURHOOD = new TestNavmeshToolMode(7, "Find Local Neighbourhood");
        public static readonly TestNavmeshToolMode RANDOM_POINTS_IN_CIRCLE = new TestNavmeshToolMode(8, "Random Points in Circle");

        public static readonly ImmutableArray<TestNavmeshToolMode> Values = ImmutableArray.Create(
            PATHFIND_FOLLOW,
            PATHFIND_STRAIGHT,
            PATHFIND_SLICED,
            DISTANCE_TO_WALL,
            RAYCAST,
            FIND_POLYS_IN_CIRCLE,
            FIND_POLYS_IN_SHAPE,
            FIND_LOCAL_NEIGHBOURHOOD,
            RANDOM_POINTS_IN_CIRCLE
        );


        public int Idx { get; }
        public string Label { get; }

        private TestNavmeshToolMode(int idx, string label)
        {
            Idx = idx;
            Label = label;
        }
    }
}