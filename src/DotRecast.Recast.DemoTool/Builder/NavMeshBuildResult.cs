using System.Collections.Generic;
using DotRecast.Detour;

namespace DotRecast.Recast.DemoTool.Builder
{
    public class NavMeshBuildResult
    {
        public readonly IList<RecastBuilderResult> RecastBuilderResults;
        public readonly DtNavMesh NavMesh;

        public NavMeshBuildResult(IList<RecastBuilderResult> recastBuilderResults, DtNavMesh navMesh)
        {
            RecastBuilderResults = recastBuilderResults;
            NavMesh = navMesh;
        }
    }
}