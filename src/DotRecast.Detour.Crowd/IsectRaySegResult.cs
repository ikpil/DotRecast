namespace DotRecast.Detour.Crowd
{
    public struct IsectRaySegResult
    {
        public readonly bool result;
        public readonly float htmin;

        public IsectRaySegResult(bool result, float htmin)
        {
            this.result = result;
            this.htmin = htmin;
        }
    }
}