namespace DotRecast.Recast.Toolset
{
    public class RcNavMeshBuildSettings
    {
        public float cellSize = 0.3f;
        public float cellHeight = 0.2f;

        public float agentHeight = 2.0f;
        public float agentRadius = 0.6f;
        public float agentMaxClimb = 0.9f;
        public float agentMaxSlope = 45f;

        public float agentMaxAcceleration = 8.0f;
        public float agentMaxSpeed = 3.5f;

        public int minRegionSize = 8;
        public int mergedRegionSize = 20;

        public int partitioning = RcPartitionType.WATERSHED.Value;

        public bool filterLowHangingObstacles = true;
        public bool filterLedgeSpans = true;
        public bool filterWalkableLowHeightSpans = true;

        public float edgeMaxLen = 12f;
        public float edgeMaxError = 1.3f;
        public int vertsPerPoly = 6;

        public float detailSampleDist = 6f;
        public float detailSampleMaxError = 1f;

        public bool tiled = true;
        public int tileSize = 32;
        
        public bool keepInterResults = false;
        public bool buildAll = true;
    }
}