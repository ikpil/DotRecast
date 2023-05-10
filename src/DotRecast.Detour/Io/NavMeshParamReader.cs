using DotRecast.Core;

namespace DotRecast.Detour.Io
{
    public class NavMeshParamReader
    {
        public NavMeshParams Read(RcByteBuffer bb)
        {
            NavMeshParams option = new NavMeshParams();
            option.orig.x = bb.GetFloat();
            option.orig.y = bb.GetFloat();
            option.orig.z = bb.GetFloat();
            option.tileWidth = bb.GetFloat();
            option.tileHeight = bb.GetFloat();
            option.maxTiles = bb.GetInt();
            option.maxPolys = bb.GetInt();
            return option;
        }
    }
}