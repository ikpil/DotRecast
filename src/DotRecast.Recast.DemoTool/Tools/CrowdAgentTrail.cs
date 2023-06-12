namespace DotRecast.Recast.DemoTool.Tools
{
    public class CrowdAgentTrail
    {
        public const int AGENT_MAX_TRAIL = 64;
        public float[] trail = new float[AGENT_MAX_TRAIL * 3];
        public int htrail;
    }
}