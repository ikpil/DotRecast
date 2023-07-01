using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotRecast.Detour.TileCache
{
    public class DtTileCacheBuildResult
    {
        public readonly int tx;
        public readonly int ty;
        public readonly Task<List<byte[]>> task;

        public DtTileCacheBuildResult(int tx, int ty, Task<List<byte[]>> task)
        {
            this.tx = tx;
            this.ty = ty;
            this.task = task;
        }
    }
}