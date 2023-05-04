namespace DotRecast.Recast
{
    public class RecastBuilderResult
    {
        public readonly int tileX;
        public readonly int tileZ;
        private readonly CompactHeightfield chf;
        private readonly ContourSet cs;
        private readonly PolyMesh pmesh;
        private readonly PolyMeshDetail dmesh;
        private readonly Heightfield solid;
        private readonly Telemetry telemetry;

        public RecastBuilderResult(int tileX, int tileZ, Heightfield solid, CompactHeightfield chf, ContourSet cs, PolyMesh pmesh, PolyMeshDetail dmesh, Telemetry ctx)
        {
            this.tileX = tileX;
            this.tileZ = tileZ;
            this.solid = solid;
            this.chf = chf;
            this.cs = cs;
            this.pmesh = pmesh;
            this.dmesh = dmesh;
            telemetry = ctx;
        }

        public PolyMesh GetMesh()
        {
            return pmesh;
        }

        public PolyMeshDetail GetMeshDetail()
        {
            return dmesh;
        }

        public CompactHeightfield GetCompactHeightfield()
        {
            return chf;
        }

        public ContourSet GetContourSet()
        {
            return cs;
        }

        public Heightfield GetSolidHeightfield()
        {
            return solid;
        }

        public Telemetry GetTelemetry()
        {
            return telemetry;
        }
    }
}