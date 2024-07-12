namespace DotRecast.Detour
{
    public readonly struct DtFindPathOption
    {
        public static readonly DtFindPathOption NoOption = new DtFindPathOption(0, 0);

        public static readonly DtFindPathOption AnyAngle = new DtFindPathOption(DtFindPathOptions.DT_FINDPATH_ANY_ANGLE, float.MaxValue);

        public readonly int options;
        public readonly float raycastLimit;

        public DtFindPathOption(int options, float raycastLimit)
        {
            this.options = options;
            this.raycastLimit = raycastLimit;
        }
    }
}