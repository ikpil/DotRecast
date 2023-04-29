using DotRecast.Core;

namespace DotRecast.Detour.Io
{
    public class NavMeshParamReader
    {
        public NavMeshParams read(ByteBuffer bb)
        {
            NavMeshParams option = new NavMeshParams();
            option.orig.x = bb.getFloat();
            option.orig.y = bb.getFloat();
            option.orig.z = bb.getFloat();
            option.tileWidth = bb.getFloat();
            option.tileHeight = bb.getFloat();
            option.maxTiles = bb.getInt();
            option.maxPolys = bb.getInt();
            return option;
        }
    }
}