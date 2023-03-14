using System;

namespace DotRecast.Detour.Extras.Jumplink;

public class Trajectory {

    public float lerp(float f, float g, float u) {
        return u * g + (1f - u) * f;
    }

    public virtual float[] apply(float[] start, float[] end, float u)
    {
        throw new NotImplementedException();
    }

}
