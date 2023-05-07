namespace DotRecast.Detour.Crowd
{
    /// Provides neighbor data for agents managed by the crowd.
    /// @ingroup crowd
    /// @see dtCrowdAgent::neis, dtCrowd
    public class CrowdNeighbour
    {
        public readonly CrowdAgent agent;

        /// < The index of the neighbor in the crowd.
        public readonly float dist;

        /// < The distance between the current agent and the neighbor.
        public CrowdNeighbour(CrowdAgent agent, float dist)
        {
            this.agent = agent;
            this.dist = dist;
        }
    };
}