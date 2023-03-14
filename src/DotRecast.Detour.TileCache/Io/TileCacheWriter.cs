/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;

namespace DotRecast.Detour.TileCache.Io;

public class TileCacheWriter : DetourWriter {

    private readonly NavMeshParamWriter paramWriter = new NavMeshParamWriter();
    private readonly TileCacheBuilder builder = new TileCacheBuilder();

    public void write(BinaryWriter stream, TileCache cache, ByteOrder order, bool cCompatibility) {
        write(stream, TileCacheSetHeader.TILECACHESET_MAGIC, order);
        write(stream, cCompatibility ? TileCacheSetHeader.TILECACHESET_VERSION
                : TileCacheSetHeader.TILECACHESET_VERSION_RECAST4J, order);
        int numTiles = 0;
        for (int i = 0; i < cache.getTileCount(); ++i) {
            CompressedTile tile = cache.getTile(i);
            if (tile == null || tile.data == null)
                continue;
            numTiles++;
        }
        write(stream, numTiles, order);
        paramWriter.write(stream, cache.getNavMesh().getParams(), order);
        writeCacheParams(stream, cache.getParams(), order);
        for (int i = 0; i < cache.getTileCount(); i++) {
            CompressedTile tile = cache.getTile(i);
            if (tile == null || tile.data == null)
                continue;
            write(stream, (int) cache.getTileRef(tile), order);
            byte[] data = tile.data;
            TileCacheLayer layer = cache.decompressTile(tile);
            data = builder.compressTileCacheLayer(layer, order, cCompatibility);
            write(stream, data.Length, order);
            stream.Write(data);
        }
    }

    private void writeCacheParams(BinaryWriter stream, TileCacheParams option, ByteOrder order) {
        for (int i = 0; i < 3; i++) {
            write(stream, option.orig[i], order);
        }
        write(stream, option.cs, order);
        write(stream, option.ch, order);
        write(stream, option.width, order);
        write(stream, option.height, order);
        write(stream, option.walkableHeight, order);
        write(stream, option.walkableRadius, order);
        write(stream, option.walkableClimb, order);
        write(stream, option.maxSimplificationError, order);
        write(stream, option.maxTiles, order);
        write(stream, option.maxObstacles, order);
    }

}
