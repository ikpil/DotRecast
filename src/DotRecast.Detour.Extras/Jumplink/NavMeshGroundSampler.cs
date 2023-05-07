using System;
using DotRecast.Core;
using DotRecast.Detour.QueryResults;
using DotRecast.Recast;

namespace DotRecast.Detour.Extras.Jumplink
{
    class NavMeshGroundSampler : AbstractGroundSampler
    {
        private readonly IQueryFilter filter = new NoOpFilter();

        public override void Sample(JumpLinkBuilderConfig acfg, RecastBuilderResult result, EdgeSampler es)
        {
            NavMeshQuery navMeshQuery = CreateNavMesh(result, acfg.agentRadius, acfg.agentHeight, acfg.agentClimb);
            SampleGround(acfg, es, (pt, h) => GetNavMeshHeight(navMeshQuery, pt, acfg.cellSize, h));
        }

        private NavMeshQuery CreateNavMesh(RecastBuilderResult r, float agentRadius, float agentHeight, float agentClimb)
        {
            NavMeshDataCreateParams option = new NavMeshDataCreateParams();
            option.verts = r.GetMesh().verts;
            option.vertCount = r.GetMesh().nverts;
            option.polys = r.GetMesh().polys;
            option.polyAreas = r.GetMesh().areas;
            option.polyFlags = r.GetMesh().flags;
            option.polyCount = r.GetMesh().npolys;
            option.nvp = r.GetMesh().nvp;
            option.detailMeshes = r.GetMeshDetail().meshes;
            option.detailVerts = r.GetMeshDetail().verts;
            option.detailVertsCount = r.GetMeshDetail().nverts;
            option.detailTris = r.GetMeshDetail().tris;
            option.detailTriCount = r.GetMeshDetail().ntris;
            option.walkableRadius = agentRadius;
            option.walkableHeight = agentHeight;
            option.walkableClimb = agentClimb;
            option.bmin = r.GetMesh().bmin;
            option.bmax = r.GetMesh().bmax;
            option.cs = r.GetMesh().cs;
            option.ch = r.GetMesh().ch;
            option.buildBvTree = true;
            return new NavMeshQuery(new NavMesh(NavMeshBuilder.CreateNavMeshData(option), option.nvp, 0));
        }


        private Tuple<bool, float> GetNavMeshHeight(NavMeshQuery navMeshQuery, Vector3f pt, float cs, float heightRange)
        {
            Vector3f halfExtents = new Vector3f { x = cs, y = heightRange, z = cs };
            float maxHeight = pt.y + heightRange;
            RcAtomicBoolean found = new RcAtomicBoolean();
            RcAtomicFloat minHeight = new RcAtomicFloat(pt.y);
            navMeshQuery.QueryPolygons(pt, halfExtents, filter, new PolyQueryInvoker((tile, poly, refs) =>
            {
                Result<float> h = navMeshQuery.GetPolyHeight(refs, pt);
                if (h.Succeeded())
                {
                    float y = h.result;
                    if (y > minHeight.Get() && y < maxHeight)
                    {
                        minHeight.Exchange(y);
                        found.Set(true);
                    }
                }
            }));
            if (found.Get())
            {
                return Tuple.Create(true, minHeight.Get());
            }

            return Tuple.Create(false, pt.y);
        }
    }
}