using DotRecast.Core;

namespace DotRecast.Detour.Io
{


public class NavMeshParamReader {

	public NavMeshParams read(ByteBuffer bb) {
		NavMeshParams option = new NavMeshParams();
		option.orig[0] = bb.getFloat();
		option.orig[1] = bb.getFloat();
		option.orig[2] = bb.getFloat();
		option.tileWidth = bb.getFloat();
		option.tileHeight = bb.getFloat();
		option.maxTiles = bb.getInt();
		option.maxPolys = bb.getInt();
		return option;
	}

}

}