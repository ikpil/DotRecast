namespace DotRecast.Detour
{
    public interface PolyQuery
    {
        void process(MeshTile tile, Poly poly, long refs);
    }
}