namespace DotRecast.Detour
{
    public interface PolyQuery
    {
        void Process(MeshTile tile, Poly poly, long refs);
    }
}