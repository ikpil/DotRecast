using System;
using System.Collections.Generic;
using DotRecast.Detour;

namespace DotRecast.Recast.Toolset.Builder
{
    public class NavMeshBuildResult
    {
        public readonly bool Success;
        public readonly IList<RecastBuilderResult> RecastBuilderResults;
        public readonly DtNavMesh NavMesh;

        public NavMeshBuildResult()
        {
            Success = false;
            RecastBuilderResults = Array.Empty<RecastBuilderResult>();
            NavMesh = null;
        }
        
        public NavMeshBuildResult(IList<RecastBuilderResult> recastBuilderResults, DtNavMesh navMesh)
        {
            Success = true;
            RecastBuilderResults = recastBuilderResults;
            NavMesh = navMesh;
        }
    }
}