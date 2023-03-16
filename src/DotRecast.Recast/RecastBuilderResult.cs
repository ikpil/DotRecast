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

    public RecastBuilderResult(int tileX, int tileZ, Heightfield solid, CompactHeightfield chf, ContourSet cs, PolyMesh pmesh,
        PolyMeshDetail dmesh, Telemetry ctx)
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

    public PolyMesh getMesh()
    {
        return pmesh;
    }

    public PolyMeshDetail getMeshDetail()
    {
        return dmesh;
    }

    public CompactHeightfield getCompactHeightfield()
    {
        return chf;
    }

    public ContourSet getContourSet()
    {
        return cs;
    }

    public Heightfield getSolidHeightfield()
    {
        return solid;
    }

    public Telemetry getTelemetry()
    {
        return telemetry;
    }
}

}