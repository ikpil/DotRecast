using DotRecast.Core;
using DotRecast.Detour.TileCache.Io.Compress;

namespace DotRecast.Detour.TileCache.Test.Io;

public class DtTileCacheCompressorForTestFactory : IDtTileCacheCompressorFactory
{
    public static readonly DtTileCacheCompressorForTestFactory Shared = new();

    private DtTileCacheCompressorForTestFactory()
    {
    }

    public IRcCompressor Get(bool cCompatibility)
    {
        if (cCompatibility)
            return DtTileCacheFastLzCompressor.Shared;

        return DtTileCacheLZ4ForTestCompressor.Shared;
    }
}