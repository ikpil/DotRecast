using DotRecast.Recast;

namespace DotRecast.Detour.Extras.Jumplink
{
    public interface GroundSampler
    {
        void Sample(JumpLinkBuilderConfig acfg, RecastBuilderResult result, EdgeSampler es);
    }
}