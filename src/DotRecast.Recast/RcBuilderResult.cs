using DotRecast.Core;

namespace DotRecast.Recast
{
    public class RcBuilderResult
    {
        public readonly int tileX;
        public readonly int tileZ;
        
        private readonly RcCompactHeightfield chf;
        private readonly RcContourSet cs;
        private readonly RcPolyMesh pmesh;
        private readonly RcPolyMeshDetail dmesh;
        private readonly RcHeightfield solid;
        private readonly RcContext _context;

        public RcBuilderResult(int tileX, int tileZ, RcHeightfield solid, RcCompactHeightfield chf, RcContourSet cs, RcPolyMesh pmesh, RcPolyMeshDetail dmesh, RcContext ctx)
        {
            this.tileX = tileX;
            this.tileZ = tileZ;
            this.solid = solid;
            this.chf = chf;
            this.cs = cs;
            this.pmesh = pmesh;
            this.dmesh = dmesh;
            _context = ctx;
        }

        public RcPolyMesh GetMesh()
        {
            return pmesh;
        }

        public RcPolyMeshDetail GetMeshDetail()
        {
            return dmesh;
        }

        public RcCompactHeightfield GetCompactHeightfield()
        {
            return chf;
        }

        public RcContourSet GetContourSet()
        {
            return cs;
        }

        public RcHeightfield GetSolidHeightfield()
        {
            return solid;
        }

        public RcContext GetTelemetry()
        {
            return _context;
        }
    }
}