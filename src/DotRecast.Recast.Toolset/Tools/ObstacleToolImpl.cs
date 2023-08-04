using DotRecast.Core;
using DotRecast.Detour.TileCache;

namespace DotRecast.Recast.Toolset.Tools
{
    public class ObstacleToolImpl : ISampleTool
    {
        private Sample _sample;
        
        public string GetName()
        {
            return "Create Temp Obstacles";
        }

        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }

        public void RemoveTempObstacle(RcVec3f sp, RcVec3f sq)
        {
            // ..
        }

        public void AddTempObstacle(RcVec3f pos)
        {
            //p[1] -= 0.5f;
            //m_tileCache->addObstacle(p, 1.0f, 2.0f, 0);
        }
    }
}