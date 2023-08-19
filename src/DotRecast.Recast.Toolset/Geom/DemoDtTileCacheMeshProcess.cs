using DotRecast.Detour;
using DotRecast.Detour.TileCache;
using DotRecast.Recast.Geom;

namespace DotRecast.Recast.Toolset.Geom
{
    public class DemoDtTileCacheMeshProcess : IDtTileCacheMeshProcess
    {
        private IInputGeomProvider _geom;

        public DemoDtTileCacheMeshProcess()
        {
        }

        public void Init(IInputGeomProvider geom)
        {
            _geom = geom;
        }

        public void Process(DtNavMeshCreateParams option)
        {
            // // Update poly flags from areas.
            // for (int i = 0; i < option.polyCount; ++i)
            // {
            //     if (option.polyAreas[i] == DtTileCacheBuilder.DT_TILECACHE_WALKABLE_AREA)
            //         option.polyAreas[i] = SAMPLE_POLYAREA_GROUND;
            //
            //     if (option.polyAreas[i] == SAMPLE_POLYAREA_GROUND ||
            //         option.polyAreas[i] == SAMPLE_POLYAREA_GRASS ||
            //         option.polyAreas[i] == SAMPLE_POLYAREA_ROAD)
            //     {
            //         option.polyFlags[i] = SAMPLE_POLYFLAGS_WALK;
            //     }
            //     else if (option.polyAreas[i] == SAMPLE_POLYAREA_WATER)
            //     {
            //         option.polyFlags[i] = SAMPLE_POLYFLAGS_SWIM;
            //     }
            //     else if (option.polyAreas[i] == SAMPLE_POLYAREA_DOOR)
            //     {
            //         option.polyFlags[i] = SAMPLE_POLYFLAGS_WALK | SAMPLE_POLYFLAGS_DOOR;
            //     }
            // }
            //
            // // Pass in off-mesh connections.
            // if (null != _geom)
            // {
            //     option.offMeshConVerts = _geom.GetOffMeshConnectionVerts();
            //     option.offMeshConRad = _geom.GetOffMeshConnectionRads();
            //     option.offMeshConDir = _geom.GetOffMeshConnectionDirs();
            //     option.offMeshConAreas = _geom.GetOffMeshConnectionAreas();
            //     option.offMeshConFlags = _geom.GetOffMeshConnectionFlags();
            //     option.offMeshConUserID = _geom.GetOffMeshConnectionId();
            //     option.offMeshConCount = _geom.GetOffMeshConnectionCount();
            // }
        }
    }
}