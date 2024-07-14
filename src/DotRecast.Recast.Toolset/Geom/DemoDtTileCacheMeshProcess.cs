using System;
using DotRecast.Detour;
using DotRecast.Detour.TileCache;
using DotRecast.Recast.Geom;
using DotRecast.Recast.Toolset.Builder;

namespace DotRecast.Recast.Toolset.Geom
{
    public class DemoDtTileCacheMeshProcess : IDtTileCacheMeshProcess
    {
        private IInputGeomProvider m_geom;

        public DemoDtTileCacheMeshProcess()
        {
        }

        public void Init(IInputGeomProvider geom)
        {
            m_geom = geom;
        }

        public void Process(DtNavMeshCreateParams option)
        {
            // Update poly flags from areas.
            for (int i = 0; i < option.polyCount; ++i)
            {
                if (option.polyAreas[i] == DtTileCacheBuilder.DT_TILECACHE_WALKABLE_AREA)
                    option.polyAreas[i] = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND;

                if (option.polyAreas[i] == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND ||
                    option.polyAreas[i] == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GRASS ||
                    option.polyAreas[i] == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD)
                {
                    option.polyFlags[i] = SampleAreaModifications.SAMPLE_POLYFLAGS_WALK;
                }
                else if (option.polyAreas[i] == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER)
                {
                    option.polyFlags[i] = SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM;
                }
                else if (option.polyAreas[i] == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_DOOR)
                {
                    option.polyFlags[i] = SampleAreaModifications.SAMPLE_POLYFLAGS_WALK | SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR;
                }
            }

            // Pass in off-mesh connections.
            if (null != m_geom)
            {
                option.offMeshConCount = m_geom.OffMeshConCount;
                option.offMeshConVerts = m_geom.OffMeshConVerts;
                option.offMeshConRads = m_geom.OffMeshConRads;
                option.offMeshConDirs = m_geom.OffMeshConDirs;
                option.offMeshConAreas = m_geom.OffMeshConAreas;
                option.offMeshConFlags = m_geom.OffMeshConFlags;
                option.offMeshConUserID = m_geom.OffMeshConId;
            }
        }
    }
}