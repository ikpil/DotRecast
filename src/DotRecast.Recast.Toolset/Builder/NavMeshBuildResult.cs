using System;
using System.Collections.Generic;
using DotRecast.Detour;

namespace DotRecast.Recast.Toolset.Builder
{
    public class NavMeshBuildResult
    {
        public readonly bool Success;
        public readonly IList<RcBuilderResult> RecastBuilderResults;
        public readonly DtNavMesh NavMesh;

        public NavMeshBuildResult()
        {
            Success = false;
            RecastBuilderResults = Array.Empty<RcBuilderResult>();
            NavMesh = null;
        }
        
        public NavMeshBuildResult(IList<RcBuilderResult> recastBuilderResults, DtNavMesh navMesh)
        {
            Success = true;
            RecastBuilderResults = recastBuilderResults;
            NavMesh = navMesh;
        }
    }
}