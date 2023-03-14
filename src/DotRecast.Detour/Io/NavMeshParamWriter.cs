using System.IO;
using DotRecast.Core;

namespace DotRecast.Detour.Io;

public class NavMeshParamWriter : DetourWriter {

	public void write(BinaryWriter stream, NavMeshParams option, ByteOrder order)  {
		write(stream, option.orig[0], order);
		write(stream, option.orig[1], order);
		write(stream, option.orig[2], order);
		write(stream, option.tileWidth, order);
		write(stream, option.tileHeight, order);
		write(stream, option.maxTiles, order);
		write(stream, option.maxPolys, order);
	}

}
