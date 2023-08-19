using DotRecast.Core;
using DotRecast.Detour.TileCache.Io.Compress;

namespace DotRecast.Recast.Demo;

public class DtTileCacheCompressorDemoFactory : IDtTileCacheCompressorFactory
{
    public static readonly DtTileCacheCompressorDemoFactory Shared = new();

    private DtTileCacheCompressorDemoFactory()
    {
    }

    public IRcCompressor Get(bool cCompatibility)
    {
        if (cCompatibility)
            return DtTileCacheFastLzCompressor.Shared;

        return DtTileCacheLZ4DemoCompressor.Shared;
    }
}