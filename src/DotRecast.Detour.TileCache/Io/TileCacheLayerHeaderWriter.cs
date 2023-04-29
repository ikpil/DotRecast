/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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

namespace DotRecast.Detour.TileCache.Io
{
    public class TileCacheLayerHeaderWriter : DetourWriter
    {
        public void write(BinaryWriter stream, TileCacheLayerHeader header, ByteOrder order, bool cCompatibility)
        {
            write(stream, header.magic, order);
            write(stream, header.version, order);
            write(stream, header.tx, order);
            write(stream, header.ty, order);
            write(stream, header.tlayer, order);
            
            write(stream, header.bmin.x, order);
            write(stream, header.bmin.y, order);
            write(stream, header.bmin.z, order);
            write(stream, header.bmax.x, order);
            write(stream, header.bmax.y, order);
            write(stream, header.bmax.z, order);

            write(stream, (short)header.hmin, order);
            write(stream, (short)header.hmax, order);
            write(stream, (byte)header.width);
            write(stream, (byte)header.height);
            write(stream, (byte)header.minx);
            write(stream, (byte)header.maxx);
            write(stream, (byte)header.miny);
            write(stream, (byte)header.maxy);
            if (cCompatibility)
            {
                write(stream, (short)0, order); // C struct padding
            }
        }
    }
}