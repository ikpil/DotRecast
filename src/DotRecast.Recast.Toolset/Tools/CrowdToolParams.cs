namespace DotRecast.Recast.Toolset.Tools
{
    public class CrowdToolParams
    {
        public int m_expandSelectedDebugDraw = 1;
        public bool m_showCorners;
        public bool m_showCollisionSegments;
        public bool m_showPath;
        public bool m_showVO;
        public bool m_showOpt;
        public bool m_showNeis;

        public int m_expandDebugDraw = 0;
        public bool m_showLabels;
        public bool m_showGrid;
        public bool m_showNodes;
        public bool m_showPerfGraph;
        public bool m_showDetailAll;

        public int m_expandOptions = 1;
        public bool m_anticipateTurns = true;
        public bool m_optimizeVis = true;
        public bool m_optimizeTopo = true;
        public bool m_obstacleAvoidance = true;
        public int m_obstacleAvoidanceType = 3;
        public bool m_separation;
        public float m_separationWeight = 2f;
    }
}