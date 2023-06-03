using DotRecast.Core;

namespace DotRecast.Detour.QueryResults
{
    public struct PortalResult
    {
        public readonly RcVec3f left;
        public readonly RcVec3f right;
        public readonly int fromType;
        public readonly int toType;

        public PortalResult(RcVec3f left, RcVec3f right, int fromType, int toType)
        {
            this.left = left;
            this.right = right;
            this.fromType = fromType;
            this.toType = toType;
        }
    }
}