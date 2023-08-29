namespace DotRecast.Recast
{
    public class RcConfigBuilder
    {
        private int _partition;
        private bool _useTiles;
        private int _tileSizeX;
        private int _tileSizeZ;
        private float _cs;
        private float _ch;
        private float _agentMaxSlope;
        private int _agentHeight;
        private int _agentMaxClimb;
        private int _agentRadius;
        private int _edgeMaxLen;
        private float _edgeMaxError;
        private int _minRegionArea;
        private int _mergeRegionArea;
        private int _vertsPerPoly;
        private float _detailSampleDist;
        private float _detailSampleMaxError;
        private RcAreaModification _walkableAreaMod;
        private bool _filterLowHangingObstacles;
        private bool _filterLedgeSpans;
        private bool _filterWalkableLowHeightSpans;

        // ..
        private bool _buildMeshDetail;
        private int _borderSize;
        private float _minRegionAreaWorld;
        private float _mergeRegionAreaWorld;
        private float _walkableHeightWorld;
        private float _walkableClimbWorld;
        private float _walkableRadiusWorld;
        private float _maxEdgeLenWorld;

        public RcConfigBuilder WithPartition(int partition)
        {
            _partition = partition;
            return this;
        }

        public RcConfigBuilder WithUseTiles(bool useTiles)
        {
            _useTiles = useTiles;
            return this;
        }

        public RcConfigBuilder WithTileSizeX(int tileSizeX)
        {
            _tileSizeX = tileSizeX;
            return this;
        }

        public RcConfigBuilder WithTileSizeZ(int tileSizeZ)
        {
            _tileSizeZ = tileSizeZ;
            return this;
        }

        public RcConfigBuilder WithCell(float cs)
        {
            _cs = cs;
            return this;
        }

        public RcConfigBuilder WithCh(float ch)
        {
            _ch = ch;
            return this;
        }

        public RcConfigBuilder WithWalkableSlopeAngle(float walkableSlopeAngle)
        {
            _agentMaxSlope = walkableSlopeAngle;
            return this;
        }

        public RcConfigBuilder WithWalkableHeight(int walkableHeight)
        {
            _agentHeight = walkableHeight;
            return this;
        }

        public RcConfigBuilder WithWalkableClimb(int walkableClimb)
        {
            _agentMaxClimb = walkableClimb;
            return this;
        }

        public RcConfigBuilder WithWalkableRadius(int walkableRadius)
        {
            _agentRadius = walkableRadius;
            return this;
        }

        public RcConfigBuilder WithMaxEdgeLen(int maxEdgeLen)
        {
            _edgeMaxLen = maxEdgeLen;
            return this;
        }

        public RcConfigBuilder WithMaxSimplificationError(float maxSimplificationError)
        {
            _edgeMaxError = maxSimplificationError;
            return this;
        }

        public RcConfigBuilder WithMinRegionArea(int minRegionArea)
        {
            _minRegionArea = minRegionArea;
            return this;
        }

        public RcConfigBuilder WithMergeRegionArea(int mergeRegionArea)
        {
            _mergeRegionArea = mergeRegionArea;
            return this;
        }

        public RcConfigBuilder WithMaxVertsPerPoly(int maxVertsPerPoly)
        {
            _vertsPerPoly = maxVertsPerPoly;
            return this;
        }

        public RcConfigBuilder WithDetailSampleDist(float detailSampleDist)
        {
            _detailSampleDist = detailSampleDist;
            return this;
        }

        public RcConfigBuilder WithDetailSampleMaxError(float detailSampleMaxError)
        {
            _detailSampleMaxError = detailSampleMaxError;
            return this;
        }

        public RcConfigBuilder WithWalkableAreaMod(RcAreaModification walkableAreaMod)
        {
            _walkableAreaMod = walkableAreaMod;
            return this;
        }

        public RcConfigBuilder WithFilterLowHangingObstacles(bool filterLowHangingObstacles)
        {
            _filterLowHangingObstacles = filterLowHangingObstacles;
            return this;
        }

        public RcConfigBuilder WithFilterLedgeSpans(bool filterLedgeSpans)
        {
            _filterLedgeSpans = filterLedgeSpans;
            return this;
        }

        public RcConfigBuilder WithFilterWalkableLowHeightSpans(bool filterWalkableLowHeightSpans)
        {
            _filterWalkableLowHeightSpans = filterWalkableLowHeightSpans;
            return this;
        }

        public RcConfig Build()
        {
            return new RcConfig(
                _useTiles,
                _tileSizeX,
                _tileSizeZ,
                _borderSize,
                RcPartition.LAYERS, // _partition
                _cs,
                _ch,
                _agentMaxSlope,
                _filterLowHangingObstacles,
                _filterLedgeSpans,
                _filterWalkableLowHeightSpans,
                _agentHeight,
                _agentRadius,
                _agentMaxClimb,
                _minRegionArea,
                _mergeRegionArea,
                _edgeMaxLen,
                _edgeMaxError,
                _vertsPerPoly,
                _buildMeshDetail,
                _detailSampleDist,
                _detailSampleMaxError,
                _walkableAreaMod);
        }
    }
}