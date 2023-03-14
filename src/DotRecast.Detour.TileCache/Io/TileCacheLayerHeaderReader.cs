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

namespace DotRecast.Detour.TileCache.Io;

public class TileCacheLayerHeaderReader {

    public TileCacheLayerHeader read(ByteBuffer data, bool cCompatibility) {
        TileCacheLayerHeader header = new TileCacheLayerHeader();
        header.magic = data.getInt();
        header.version = data.getInt();

        if (header.magic != TileCacheLayerHeader.DT_TILECACHE_MAGIC)
            throw new IOException("Invalid magic");
        if (header.version != TileCacheLayerHeader.DT_TILECACHE_VERSION)
            throw new IOException("Invalid version");

        header.tx = data.getInt();
        header.ty = data.getInt();
        header.tlayer = data.getInt();
        for (int j = 0; j < 3; j++) {
            header.bmin[j] = data.getFloat();
        }
        for (int j = 0; j < 3; j++) {
            header.bmax[j] = data.getFloat();
        }
        header.hmin = data.getShort() & 0xFFFF;
        header.hmax = data.getShort() & 0xFFFF;
        header.width = data.get() & 0xFF;
        header.height = data.get() & 0xFF;
        header.minx = data.get() & 0xFF;
        header.maxx = data.get() & 0xFF;
        header.miny = data.get() & 0xFF;
        header.maxy = data.get() & 0xFF;
        if (cCompatibility) {
            data.getShort(); // C struct padding
        }
        return header;
    }

}
