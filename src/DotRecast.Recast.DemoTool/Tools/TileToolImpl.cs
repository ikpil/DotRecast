using DotRecast.Core;

namespace DotRecast.Recast.DemoTool.Tools
{
    public class TileToolImpl : ISampleTool
    {
        private Sample _sample;
        
        public string GetName()
        {
            return "Create Tiles";
        }
        
        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }

        public void BuildTile(RcVec3f pos)
        {
            var settings = _sample.GetSettings();
            var geom = _sample.GetInputGeom();
            var navMesh = _sample.GetNavMesh();
            
            float ts = settings.tileSize * settings.cellSize;
            
            var bmin = geom.GetMeshBoundsMin();

            int tx = (int)((pos.x - bmin[0]) / ts);
            int ty = (int)((pos.z - bmin[2]) / ts);

            var tileRef = navMesh.GetTileRefAt(tx, ty, 0);
            navMesh.RemoveTile(tileRef); 
            
            navMesh.AddTile()
        }

        public void RemoveTile(RcVec3f pos)
        {
            var settings = _sample.GetSettings();
            var geom = _sample.GetInputGeom();
            var navMesh = _sample.GetNavMesh();
            
            float ts = settings.tileSize * settings.cellSize;
            
            var bmin = geom.GetMeshBoundsMin();

            int tx = (int)((pos.x - bmin[0]) / ts);
            int ty = (int)((pos.z - bmin[2]) / ts);

            var tileRef = navMesh.GetTileRefAt(tx, ty, 0);
            navMesh.RemoveTile(tileRef);
        }
    }
}