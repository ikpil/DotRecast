using DotRecast.Core;

namespace DotRecast.Detour.QueryResults
{
    public struct PortalResult
    {
        public readonly Vector3f left;
        public readonly Vector3f right;
        public readonly int fromType;
        public readonly int toType;

        public PortalResult(Vector3f left, Vector3f right, int fromType, int toType)
        {
            this.left = left;
            this.right = right;
            this.fromType = fromType;
            this.toType = toType;
        }
    }
}