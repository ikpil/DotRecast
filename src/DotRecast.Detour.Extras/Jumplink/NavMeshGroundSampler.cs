using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Recast;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class NavMeshGroundSampler : AbstractGroundSampler
    {
        public override void Sample(JumpLinkBuilderConfig acfg, RcBuilderResult result, EdgeSampler es)
        {
            DtNavMeshQuery navMeshQuery = CreateNavMesh(result, acfg.agentRadius, acfg.agentHeight, acfg.agentClimb);
            SampleGround(acfg, es, (RcVec3f pt, float heightRange, out float height) => GetNavMeshHeight(navMeshQuery, pt, acfg.cellSize, heightRange, out height));
        }

        private DtNavMeshQuery CreateNavMesh(RcBuilderResult r, float agentRadius, float agentHeight, float agentClimb)
        {
            DtNavMeshCreateParams option = new DtNavMeshCreateParams();
            option.verts = r.Mesh.verts;
            option.vertCount = r.Mesh.nverts;
            option.polys = r.Mesh.polys;
            option.polyAreas = r.Mesh.areas;
            option.polyFlags = r.Mesh.flags;
            option.polyCount = r.Mesh.npolys;
            option.nvp = r.Mesh.nvp;
            option.detailMeshes = r.MeshDetail.meshes;
            option.detailVerts = r.MeshDetail.verts;
            option.detailVertsCount = r.MeshDetail.nverts;
            option.detailTris = r.MeshDetail.tris;
            option.detailTriCount = r.MeshDetail.ntris;
            option.walkableRadius = agentRadius;
            option.walkableHeight = agentHeight;
            option.walkableClimb = agentClimb;
            option.bmin = r.Mesh.bmin;
            option.bmax = r.Mesh.bmax;
            option.cs = r.Mesh.cs;
            option.ch = r.Mesh.ch;
            option.buildBvTree = true;
            var mesh = new DtNavMesh();
            var status = mesh.Init(DtNavMeshBuilder.CreateNavMeshData(option), option.nvp, 0);
            if (status.Failed())
            {
                return null;
            }

            return new DtNavMeshQuery(mesh);
        }


        private bool GetNavMeshHeight(DtNavMeshQuery navMeshQuery, RcVec3f pt, float cs, float heightRange, out float height)
        {
            height = default;

            RcVec3f halfExtents = new RcVec3f { X = cs, Y = heightRange, Z = cs };
            float maxHeight = pt.Y + heightRange;
            var query = new DtHeightSamplePolyQuery(navMeshQuery, pt, pt.Y, maxHeight);
            navMeshQuery.QueryPolygons(pt, halfExtents, DtQueryNoOpFilter.Shared, ref query);

            if (query.Found)
            {
                height = query.MinHeight;
                return true;
            }

            height = pt.Y;
            return false;
        }
    }
}