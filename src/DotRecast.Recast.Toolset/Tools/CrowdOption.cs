namespace DotRecast.Recast.Toolset.Tools
{
    public class CrowdOption
    {
        public int expandOptions = 1;
        public bool anticipateTurns = true;
        public bool optimizeVis = true;
        public bool optimizeTopo = true;
        public bool obstacleAvoidance = true;
        public int obstacleAvoidanceType = 3;
        public bool separation;
        public float separationWeight = 2f;
    }
}