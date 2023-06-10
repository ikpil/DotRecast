namespace DotRecast.Detour
{
    public static class DtStatusExtension
    {
        public static bool IsSuccess(this DtStatus @this)
        {
            return @this == DtStatus.DT_SUCCSESS || @this == DtStatus.DT_PARTIAL_RESULT;
        }
        
        public static bool IsFailed(this DtStatus @this)
        {
            return @this == DtStatus.DT_FAILURE || @this == DtStatus.DT_INVALID_PARAM;
        }

        public static bool IsInProgress(this DtStatus @this)
        {
            return @this == DtStatus.DT_IN_PROGRESS;
        }

        public static bool IsPartial(this DtStatus @this)
        {
            return @this == DtStatus.DT_PARTIAL_RESULT;
        }
    }
}