namespace DotRecast.Detour
{
    public interface IPolyQuery
    {
        void Process(MeshTile tile, Poly poly, long refs);
    }
}