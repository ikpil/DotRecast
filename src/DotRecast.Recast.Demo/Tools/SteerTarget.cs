namespace DotRecast.Recast.Demo.Tools;

public class SteerTarget {
    public readonly float[] steerPos;
    public readonly int steerPosFlag;
    public readonly long steerPosRef;
    public readonly float[] steerPoints;

    public SteerTarget(float[] steerPos, int steerPosFlag, long steerPosRef, float[] steerPoints) {
        this.steerPos = steerPos;
        this.steerPosFlag = steerPosFlag;
        this.steerPosRef = steerPosRef;
        this.steerPoints = steerPoints;
    }
}
