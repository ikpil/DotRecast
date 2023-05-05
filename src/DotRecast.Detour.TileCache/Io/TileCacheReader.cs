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
using DotRecast.Detour.TileCache.Io.Compress;

namespace DotRecast.Detour.TileCache.Io
{
    public class TileCacheReader
    {
        private readonly NavMeshParamReader paramReader = new NavMeshParamReader();

        public TileCache Read(BinaryReader @is, int maxVertPerPoly, ITileCacheMeshProcess meshProcessor)
        {
            ByteBuffer bb = IOUtils.ToByteBuffer(@is);
            return Read(bb, maxVertPerPoly, meshProcessor);
        }

        public TileCache Read(ByteBuffer bb, int maxVertPerPoly, ITileCacheMeshProcess meshProcessor)
        {
            TileCacheSetHeader header = new TileCacheSetHeader();
            header.magic = bb.GetInt();
            if (header.magic != TileCacheSetHeader.TILECACHESET_MAGIC)
            {
                header.magic = IOUtils.SwapEndianness(header.magic);
                if (header.magic != TileCacheSetHeader.TILECACHESET_MAGIC)
                {
                    throw new IOException("Invalid magic");
                }

                bb.Order(bb.Order() == ByteOrder.BIG_ENDIAN ? ByteOrder.LITTLE_ENDIAN : ByteOrder.BIG_ENDIAN);
            }

            header.version = bb.GetInt();
            if (header.version != TileCacheSetHeader.TILECACHESET_VERSION)
            {
                if (header.version != TileCacheSetHeader.TILECACHESET_VERSION_RECAST4J)
                {
                    throw new IOException("Invalid version");
                }
            }

            bool cCompatibility = header.version == TileCacheSetHeader.TILECACHESET_VERSION;
            header.numTiles = bb.GetInt();
            header.meshParams = paramReader.Read(bb);
            header.cacheParams = ReadCacheParams(bb, cCompatibility);
            NavMesh mesh = new NavMesh(header.meshParams, maxVertPerPoly);
            ITileCacheCompressor compressor = TileCacheCompressorFactory.Get(cCompatibility);
            TileCache tc = new TileCache(header.cacheParams, new TileCacheStorageParams(bb.Order(), cCompatibility), mesh,
                compressor, meshProcessor);
            // Read tiles.
            for (int i = 0; i < header.numTiles; ++i)
            {
                long tileRef = bb.GetInt();
                int dataSize = bb.GetInt();
                if (tileRef == 0 || dataSize == 0)
                {
                    break;
                }

                byte[] data = bb.ReadBytes(dataSize).ToArray();
                long tile = tc.AddTile(data, 0);
                if (tile != 0)
                {
                    tc.BuildNavMeshTile(tile);
                }
            }

            return tc;
        }

        private TileCacheParams ReadCacheParams(ByteBuffer bb, bool cCompatibility)
        {
            TileCacheParams option = new TileCacheParams();
            
            option.orig.x = bb.GetFloat();
            option.orig.y = bb.GetFloat();
            option.orig.z = bb.GetFloat();

            option.cs = bb.GetFloat();
            option.ch = bb.GetFloat();
            option.width = bb.GetInt();
            option.height = bb.GetInt();
            option.walkableHeight = bb.GetFloat();
            option.walkableRadius = bb.GetFloat();
            option.walkableClimb = bb.GetFloat();
            option.maxSimplificationError = bb.GetFloat();
            option.maxTiles = bb.GetInt();
            option.maxObstacles = bb.GetInt();
            return option;
        }
    }
}