using System.IO;
using DotRecast.Core;
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Io
{
    public class DtNavMeshParamWriter : DtWriter
    {
        public void Write(BinaryWriter stream, DtNavMeshParams option, RcByteOrder order)
        {
            Write(stream, option.orig.X, order);
            Write(stream, option.orig.Y, order);
            Write(stream, option.orig.Z, order);
            Write(stream, option.tileWidth, order);
            Write(stream, option.tileHeight, order);
            Write(stream, option.maxTiles, order);
            Write(stream, option.maxPolys, order);
        }
    }
}