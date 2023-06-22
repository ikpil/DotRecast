namespace DotRecast.Detour
{
    public readonly struct DtFindPathOption
    {
        public static readonly DtFindPathOption Zero = new DtFindPathOption(DefaultQueryHeuristic.Default, 0, 0);

        public readonly IQueryHeuristic heuristic;
        public readonly int options;
        public readonly float raycastLimit;

        public DtFindPathOption(IQueryHeuristic heuristic, int options, float raycastLimit)
        {
            this.heuristic = heuristic;
            this.options = options;
            this.raycastLimit = raycastLimit;
        }

        public DtFindPathOption(int options, float raycastLimit)
            : this(DefaultQueryHeuristic.Default, options, raycastLimit)
        {
        }
    }
}