namespace DotRecast.Detour
{
    public static class DtStatusExtension
    {
        public static bool IsSuccess(this DtStatus @this)
        {
            return @this == DtStatus.SUCCSESS || @this == DtStatus.PARTIAL_RESULT;
        }
        
        public static bool IsFailed(this DtStatus @this)
        {
            return @this == DtStatus.FAILURE || @this == DtStatus.FAILURE_INVALID_PARAM;
        }

        public static bool IsInProgress(this DtStatus @this)
        {
            return @this == DtStatus.IN_PROGRESS;
        }

        public static bool IsPartial(this DtStatus @this)
        {
            return @this == DtStatus.PARTIAL_RESULT;
        }
    }
}