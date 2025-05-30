/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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
using DotRecast.Core.Numerics;

namespace DotRecast.Detour.Dynamic.Io
{
    public class DtVoxelFileReader
    {
        private readonly IRcCompressor _compressor;

        public DtVoxelFileReader(IRcCompressor compressor)
        {
            _compressor = compressor;
        }

        public DtVoxelFile Read(BinaryReader stream)
        {
            RcByteBuffer buf = RcIO.ToByteBuffer(stream);
            DtVoxelFile file = new DtVoxelFile();
            int magic = buf.GetInt();
            if (magic != DtVoxelFile.MAGIC)
            {
                magic = RcIO.SwapEndianness(magic);
                if (magic != DtVoxelFile.MAGIC)
                {
                    throw new IOException("Invalid magic");
                }

                buf.Order(buf.Order() == RcByteOrder.BIG_ENDIAN ? RcByteOrder.LITTLE_ENDIAN : RcByteOrder.BIG_ENDIAN);
            }

            file.version = buf.GetInt();
            bool isExportedFromAstar = (file.version & DtVoxelFile.VERSION_EXPORTER_MASK) == 0;
            bool compression = (file.version & DtVoxelFile.VERSION_COMPRESSION_MASK) == DtVoxelFile.VERSION_COMPRESSION_LZ4;
            file.walkableRadius = buf.GetFloat();
            file.walkableHeight = buf.GetFloat();
            file.walkableClimb = buf.GetFloat();
            file.walkableSlopeAngle = buf.GetFloat();
            file.cellSize = buf.GetFloat();
            file.maxSimplificationError = buf.GetFloat();
            file.maxEdgeLen = buf.GetFloat();
            file.minRegionArea = (int)buf.GetFloat();
            if (!isExportedFromAstar)
            {
                file.regionMergeArea = buf.GetFloat();
                file.vertsPerPoly = buf.GetInt();
                file.buildMeshDetail = buf.Get() != 0;
                file.detailSampleDistance = buf.GetFloat();
                file.detailSampleMaxError = buf.GetFloat();
            }
            else
            {
                file.regionMergeArea = 6 * file.minRegionArea;
                file.vertsPerPoly = 6;
                file.buildMeshDetail = true;
                file.detailSampleDistance = file.maxEdgeLen * 0.5f;
                file.detailSampleMaxError = file.maxSimplificationError * 0.8f;
            }

            file.useTiles = buf.Get() != 0;
            file.tileSizeX = buf.GetInt();
            file.tileSizeZ = buf.GetInt();
            file.rotation.X = buf.GetFloat();
            file.rotation.Y = buf.GetFloat();
            file.rotation.Z = buf.GetFloat();
            file.bounds[0] = buf.GetFloat();
            file.bounds[1] = buf.GetFloat();
            file.bounds[2] = buf.GetFloat();
            file.bounds[3] = buf.GetFloat();
            file.bounds[4] = buf.GetFloat();
            file.bounds[5] = buf.GetFloat();
            if (isExportedFromAstar)
            {
                // bounds are saved as center + size
                file.bounds[0] -= 0.5f * file.bounds[3];
                file.bounds[1] -= 0.5f * file.bounds[4];
                file.bounds[2] -= 0.5f * file.bounds[5];
                file.bounds[3] += file.bounds[0];
                file.bounds[4] += file.bounds[1];
                file.bounds[5] += file.bounds[2];
            }

            int tileCount = buf.GetInt();
            for (int tile = 0; tile < tileCount; tile++)
            {
                int tileX = buf.GetInt();
                int tileZ = buf.GetInt();
                int width = buf.GetInt();
                int depth = buf.GetInt();
                int borderSize = buf.GetInt();
                RcVec3f boundsMin = new RcVec3f();
                boundsMin.X = buf.GetFloat();
                boundsMin.Y = buf.GetFloat();
                boundsMin.Z = buf.GetFloat();
                RcVec3f boundsMax = new RcVec3f();
                boundsMax.X = buf.GetFloat();
                boundsMax.Y = buf.GetFloat();
                boundsMax.Z = buf.GetFloat();
                if (isExportedFromAstar)
                {
                    // bounds are local
                    boundsMin.X += file.bounds[0];
                    boundsMin.Y += file.bounds[1];
                    boundsMin.Z += file.bounds[2];
                    boundsMax.X += file.bounds[0];
                    boundsMax.Y += file.bounds[1];
                    boundsMax.Z += file.bounds[2];
                }

                float cellSize = buf.GetFloat();
                float cellHeight = buf.GetFloat();
                int voxelSize = buf.GetInt();
                int position = buf.Position();
                byte[] bytes = buf.ReadBytes(voxelSize).ToArray();
                if (compression)
                {
                    bytes = _compressor.Decompress(bytes);
                }

                RcByteBuffer data = new RcByteBuffer(bytes);
                data.Order(buf.Order());
                file.AddTile(new DtVoxelTile(tileX, tileZ, width, depth, boundsMin, boundsMax, cellSize, cellHeight, borderSize, data));
                buf.Position(position + voxelSize);
            }

            return file;
        }
    }
}