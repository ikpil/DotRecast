/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

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

namespace DotRecast.Detour.Dynamic.Io
{
    public class VoxelFileReader
    {
        private readonly LZ4VoxelTileCompressor compressor = new LZ4VoxelTileCompressor();

        public VoxelFile read(BinaryReader stream)
        {
            ByteBuffer buf = IOUtils.toByteBuffer(stream);
            VoxelFile file = new VoxelFile();
            int magic = buf.getInt();
            if (magic != VoxelFile.MAGIC)
            {
                magic = IOUtils.swapEndianness(magic);
                if (magic != VoxelFile.MAGIC)
                {
                    throw new IOException("Invalid magic");
                }

                buf.order(buf.order() == ByteOrder.BIG_ENDIAN ? ByteOrder.LITTLE_ENDIAN : ByteOrder.BIG_ENDIAN);
            }

            file.version = buf.getInt();
            bool isExportedFromAstar = (file.version & VoxelFile.VERSION_EXPORTER_MASK) == 0;
            bool compression = (file.version & VoxelFile.VERSION_COMPRESSION_MASK) == VoxelFile.VERSION_COMPRESSION_LZ4;
            file.walkableRadius = buf.getFloat();
            file.walkableHeight = buf.getFloat();
            file.walkableClimb = buf.getFloat();
            file.walkableSlopeAngle = buf.getFloat();
            file.cellSize = buf.getFloat();
            file.maxSimplificationError = buf.getFloat();
            file.maxEdgeLen = buf.getFloat();
            file.minRegionArea = (int)buf.getFloat();
            if (!isExportedFromAstar)
            {
                file.regionMergeArea = buf.getFloat();
                file.vertsPerPoly = buf.getInt();
                file.buildMeshDetail = buf.get() != 0;
                file.detailSampleDistance = buf.getFloat();
                file.detailSampleMaxError = buf.getFloat();
            }
            else
            {
                file.regionMergeArea = 6 * file.minRegionArea;
                file.vertsPerPoly = 6;
                file.buildMeshDetail = true;
                file.detailSampleDistance = file.maxEdgeLen * 0.5f;
                file.detailSampleMaxError = file.maxSimplificationError * 0.8f;
            }

            file.useTiles = buf.get() != 0;
            file.tileSizeX = buf.getInt();
            file.tileSizeZ = buf.getInt();
            file.rotation.x = buf.getFloat();
            file.rotation.y = buf.getFloat();
            file.rotation.z = buf.getFloat();
            file.bounds[0] = buf.getFloat();
            file.bounds[1] = buf.getFloat();
            file.bounds[2] = buf.getFloat();
            file.bounds[3] = buf.getFloat();
            file.bounds[4] = buf.getFloat();
            file.bounds[5] = buf.getFloat();
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

            int tileCount = buf.getInt();
            for (int tile = 0; tile < tileCount; tile++)
            {
                int tileX = buf.getInt();
                int tileZ = buf.getInt();
                int width = buf.getInt();
                int depth = buf.getInt();
                int borderSize = buf.getInt();
                Vector3f boundsMin = new Vector3f();
                boundsMin.x = buf.getFloat();
                boundsMin.y = buf.getFloat();
                boundsMin.z = buf.getFloat();
                Vector3f boundsMax = new Vector3f();
                boundsMax.x = buf.getFloat();
                boundsMax.y = buf.getFloat();
                boundsMax.z = buf.getFloat();
                if (isExportedFromAstar)
                {
                    // bounds are local
                    boundsMin.x += file.bounds[0];
                    boundsMin.y += file.bounds[1];
                    boundsMin.z += file.bounds[2];
                    boundsMax.x += file.bounds[0];
                    boundsMax.y += file.bounds[1];
                    boundsMax.z += file.bounds[2];
                }

                float cellSize = buf.getFloat();
                float cellHeight = buf.getFloat();
                int voxelSize = buf.getInt();
                int position = buf.position();
                byte[] bytes = buf.ReadBytes(voxelSize).ToArray();
                if (compression)
                {
                    bytes = compressor.decompress(bytes);
                }

                ByteBuffer data = new ByteBuffer(bytes);
                data.order(buf.order());
                file.addTile(new VoxelTile(tileX, tileZ, width, depth, boundsMin, boundsMax, cellSize, cellHeight, borderSize, data));
                buf.position(position + voxelSize);
            }

            return file;
        }
    }
}
