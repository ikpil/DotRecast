using System.IO;
using DotRecast.Core;

namespace DotRecast.Detour.Io
{
    public class NavMeshParamWriter : DetourWriter
    {
        public void write(BinaryWriter stream, NavMeshParams option, ByteOrder order)
        {
            write(stream, option.orig.x, order);
            write(stream, option.orig.y, order);
            write(stream, option.orig.z, order);
            write(stream, option.tileWidth, order);
            write(stream, option.tileHeight, order);
            write(stream, option.maxTiles, order);
            write(stream, option.maxPolys, order);
        }
    }
}