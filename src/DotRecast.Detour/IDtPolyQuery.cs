namespace DotRecast.Detour
{
    /// Provides custom polygon query behavior.
    /// Used by dtNavMeshQuery::queryPolygons.
    /// @ingroup detour
    public interface IDtPolyQuery
    {
        /// Called for each batch of unique polygons touched by the search area in dtNavMeshQuery::queryPolygons.
        /// This can be called multiple times for a single query.
        void Process(DtMeshTile tile, DtPoly[] poly, long[] refs, int count);
    }
}