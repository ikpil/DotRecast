using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Detour.TileCache.Test.Io;
using NUnit.Framework;

namespace DotRecast.Detour.TileCache.Test;

[SetUpFixture]
public class TileCacheTestSetUpFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // add lz4
        DtTileCacheCompressorFactory.Shared.TryAdd(1, DtTileCacheLZ4ForTestCompressor.Shared);
    }
}