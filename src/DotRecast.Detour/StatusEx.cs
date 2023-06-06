namespace DotRecast.Detour
{
    public static class StatusEx
    {
        public static bool IsFailed(this Status @this)
        {
            return @this == Status.FAILURE || @this == Status.FAILURE_INVALID_PARAM;
        }

        public static bool IsInProgress(this Status @this)
        {
            return @this == Status.IN_PROGRESS;
        }

        public static bool IsSuccess(this Status @this)
        {
            return @this == Status.SUCCSESS || @this == Status.PARTIAL_RESULT;
        }

        public static bool IsPartial(this Status @this)
        {
            return @this == Status.PARTIAL_RESULT;
        }
    }
}