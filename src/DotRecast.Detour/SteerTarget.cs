using DotRecast.Core;

namespace DotRecast.Detour
{
    public class SteerTarget
    {
        public readonly RcVec3f steerPos;
        public readonly int steerPosFlag;
        public readonly long steerPosRef;
        public readonly float[] steerPoints;

        public SteerTarget(RcVec3f steerPos, int steerPosFlag, long steerPosRef, float[] steerPoints)
        {
            this.steerPos = steerPos;
            this.steerPosFlag = steerPosFlag;
            this.steerPosRef = steerPosRef;
            this.steerPoints = steerPoints;
        }
    }
}