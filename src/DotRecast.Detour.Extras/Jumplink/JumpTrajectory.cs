using System;

namespace DotRecast.Detour.Extras.Jumplink
{


public class JumpTrajectory : Trajectory {

    private readonly float jumpHeight;

    public JumpTrajectory(float jumpHeight) {
        this.jumpHeight = jumpHeight;
    }

    public override float[] apply(float[] start, float[] end, float u) {
        return new float[] { lerp(start[0], end[0], u), interpolateHeight(start[1], end[1], u),
                lerp(start[2], end[2], u) };
    }

    private float interpolateHeight(float ys, float ye, float u) {
        if (u == 0f) {
            return ys;
        } else if (u == 1.0f) {
            return ye;
        }
        float h1, h2;
        if (ys >= ye) { // jump down
            h1 = jumpHeight;
            h2 = jumpHeight + ys - ye;
        } else { // jump up
            h1 = jumpHeight + ys - ye;
            h2 = jumpHeight;
        }
        float t = (float) (Math.Sqrt(h1) / (Math.Sqrt(h2) + Math.Sqrt(h1)));
        if (u <= t) {
            float v1 = 1.0f - (u / t);
            return ys + h1 - h1 * v1 * v1;
        }
        float v = (u - t) / (1.0f - t);
        return ys + h1 - h2 * v * v;
    }

}

}