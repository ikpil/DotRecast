using System;
using DotRecast.Core;
using DotRecast.Recast;

namespace DotRecast.Detour.Extras.Jumplink
{
    class NavMeshGroundSampler : AbstractGroundSampler
    {
        private readonly QueryFilter filter = new NoOpFilter();

        private class NoOpFilter : QueryFilter
        {
            public bool passFilter(long refs, MeshTile tile, Poly poly)
            {
                return true;
            }

            public float getCost(float[] pa, float[] pb, long prevRef, MeshTile prevTile, Poly prevPoly, long curRef,
                MeshTile curTile, Poly curPoly, long nextRef, MeshTile nextTile, Poly nextPoly)
            {
                return 0;
            }
        }

        public override void sample(JumpLinkBuilderConfig acfg, RecastBuilderResult result, EdgeSampler es)
        {
            NavMeshQuery navMeshQuery = createNavMesh(result, acfg.agentRadius, acfg.agentHeight, acfg.agentClimb);
            sampleGround(acfg, es, (pt, h) => getNavMeshHeight(navMeshQuery, pt, acfg.cellSize, h));
        }

        private NavMeshQuery createNavMesh(RecastBuilderResult r, float agentRadius, float agentHeight, float agentClimb)
        {
            NavMeshDataCreateParams option = new NavMeshDataCreateParams();
            option.verts = r.getMesh().verts;
            option.vertCount = r.getMesh().nverts;
            option.polys = r.getMesh().polys;
            option.polyAreas = r.getMesh().areas;
            option.polyFlags = r.getMesh().flags;
            option.polyCount = r.getMesh().npolys;
            option.nvp = r.getMesh().nvp;
            option.detailMeshes = r.getMeshDetail().meshes;
            option.detailVerts = r.getMeshDetail().verts;
            option.detailVertsCount = r.getMeshDetail().nverts;
            option.detailTris = r.getMeshDetail().tris;
            option.detailTriCount = r.getMeshDetail().ntris;
            option.walkableRadius = agentRadius;
            option.walkableHeight = agentHeight;
            option.walkableClimb = agentClimb;
            option.bmin = r.getMesh().bmin;
            option.bmax = r.getMesh().bmax;
            option.cs = r.getMesh().cs;
            option.ch = r.getMesh().ch;
            option.buildBvTree = true;
            return new NavMeshQuery(new NavMesh(NavMeshBuilder.createNavMeshData(option), option.nvp, 0));
        }

        public class PolyQueryInvoker : PolyQuery
        {
            public readonly Action<MeshTile, Poly, long> _callback;

            public PolyQueryInvoker(Action<MeshTile, Poly, long> callback)
            {
                _callback = callback;
            }

            public void process(MeshTile tile, Poly poly, long refs)
            {
                _callback?.Invoke(tile, poly, refs);
            }
        }

        private Tuple<bool, float> getNavMeshHeight(NavMeshQuery navMeshQuery, float[] pt, float cs,
            float heightRange)
        {
            float[] halfExtents = new float[] { cs, heightRange, cs };
            float maxHeight = pt[1] + heightRange;
            AtomicBoolean found = new AtomicBoolean();
            AtomicFloat minHeight = new AtomicFloat(pt[1]);
            navMeshQuery.queryPolygons(pt, halfExtents, filter, new PolyQueryInvoker((tile, poly, refs) =>
            {
                Result<float> h = navMeshQuery.getPolyHeight(refs, pt);
                if (h.succeeded())
                {
                    float y = h.result;
                    if (y > minHeight.Get() && y < maxHeight)
                    {
                        minHeight.Exchange(y);
                        found.set(true);
                    }
                }
            }));
            if (found.get())
            {
                return Tuple.Create(true, minHeight.Get());
            }

            return Tuple.Create(false, pt[1]);
        }
    }
}