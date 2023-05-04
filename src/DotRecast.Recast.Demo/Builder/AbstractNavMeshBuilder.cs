using DotRecast.Detour;
using DotRecast.Recast.Demo.Geom;

namespace DotRecast.Recast.Demo.Builder;

public abstract class AbstractNavMeshBuilder
{
    protected NavMeshDataCreateParams GetNavMeshCreateParams(DemoInputGeomProvider m_geom, float m_cellSize,
        float m_cellHeight, float m_agentHeight, float m_agentRadius, float m_agentMaxClimb,
        RecastBuilderResult rcResult)
    {
        PolyMesh m_pmesh = rcResult.GetMesh();
        PolyMeshDetail m_dmesh = rcResult.GetMeshDetail();
        NavMeshDataCreateParams option = new NavMeshDataCreateParams();
        for (int i = 0; i < m_pmesh.npolys; ++i)
        {
            m_pmesh.flags[i] = 1;
        }

        option.verts = m_pmesh.verts;
        option.vertCount = m_pmesh.nverts;
        option.polys = m_pmesh.polys;
        option.polyAreas = m_pmesh.areas;
        option.polyFlags = m_pmesh.flags;
        option.polyCount = m_pmesh.npolys;
        option.nvp = m_pmesh.nvp;
        if (m_dmesh != null)
        {
            option.detailMeshes = m_dmesh.meshes;
            option.detailVerts = m_dmesh.verts;
            option.detailVertsCount = m_dmesh.nverts;
            option.detailTris = m_dmesh.tris;
            option.detailTriCount = m_dmesh.ntris;
        }

        option.walkableHeight = m_agentHeight;
        option.walkableRadius = m_agentRadius;
        option.walkableClimb = m_agentMaxClimb;
        option.bmin = m_pmesh.bmin;
        option.bmax = m_pmesh.bmax;
        option.cs = m_cellSize;
        option.ch = m_cellHeight;
        option.buildBvTree = true;

        option.offMeshConCount = m_geom.GetOffMeshConnections().Count;
        option.offMeshConVerts = new float[option.offMeshConCount * 6];
        option.offMeshConRad = new float[option.offMeshConCount];
        option.offMeshConDir = new int[option.offMeshConCount];
        option.offMeshConAreas = new int[option.offMeshConCount];
        option.offMeshConFlags = new int[option.offMeshConCount];
        option.offMeshConUserID = new int[option.offMeshConCount];
        for (int i = 0; i < option.offMeshConCount; i++)
        {
            DemoOffMeshConnection offMeshCon = m_geom.GetOffMeshConnections()[i];
            for (int j = 0; j < 6; j++)
            {
                option.offMeshConVerts[6 * i + j] = offMeshCon.verts[j];
            }

            option.offMeshConRad[i] = offMeshCon.radius;
            option.offMeshConDir[i] = offMeshCon.bidir ? 1 : 0;
            option.offMeshConAreas[i] = offMeshCon.area;
            option.offMeshConFlags[i] = offMeshCon.flags;
        }

        return option;
    }

    protected MeshData UpdateAreaAndFlags(MeshData meshData)
    {
        // Update poly flags from areas.
        for (int i = 0; i < meshData.polys.Length; ++i)
        {
            if (meshData.polys[i].GetArea() == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE)
            {
                meshData.polys[i].SetArea(SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND);
            }

            if (meshData.polys[i].GetArea() == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND
                || meshData.polys[i].GetArea() == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GRASS
                || meshData.polys[i].GetArea() == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD)
            {
                meshData.polys[i].flags = SampleAreaModifications.SAMPLE_POLYFLAGS_WALK;
            }
            else if (meshData.polys[i].GetArea() == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER)
            {
                meshData.polys[i].flags = SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM;
            }
            else if (meshData.polys[i].GetArea() == SampleAreaModifications.SAMPLE_POLYAREA_TYPE_DOOR)
            {
                meshData.polys[i].flags = SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR;
            }
        }

        return meshData;
    }
}