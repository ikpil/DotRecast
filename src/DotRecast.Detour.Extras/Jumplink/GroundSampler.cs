using DotRecast.Recast;

namespace DotRecast.Detour.Extras.Jumplink;

public interface GroundSampler {

    void sample(JumpLinkBuilderConfig acfg, RecastBuilderResult result, EdgeSampler es);

}
