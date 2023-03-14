/*
Recast4J Copyright (c) 2015 Piotr Piastucki piotr@jtilia.org

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

using System;
using System.IO;
using DotRecast.Core;

namespace DotRecast.Detour.Io;

using static DetourCommon;


public class MeshSetReader {

    private readonly MeshDataReader meshReader = new MeshDataReader();
    private readonly NavMeshParamReader paramReader = new NavMeshParamReader();

    public NavMesh read(BinaryReader @is, int maxVertPerPoly) {
        return read(IOUtils.toByteBuffer(@is), maxVertPerPoly, false);
    }

    public NavMesh read(ByteBuffer bb, int maxVertPerPoly) {
        return read(bb, maxVertPerPoly, false);
    }

    public NavMesh read32Bit(BinaryReader @is, int maxVertPerPoly) {
        return read(IOUtils.toByteBuffer(@is), maxVertPerPoly, true);
    }

    public NavMesh read32Bit(ByteBuffer bb, int maxVertPerPoly) {
        return read(bb, maxVertPerPoly, true);
    }

    public NavMesh read(BinaryReader @is) {
        return read(IOUtils.toByteBuffer(@is));
    }

    public NavMesh read(ByteBuffer bb) {
        return read(bb, -1, false);
    }

    NavMesh read(ByteBuffer bb, int maxVertPerPoly, bool is32Bit) {
        NavMeshSetHeader header = readHeader(bb, maxVertPerPoly);
        if (header.maxVertsPerPoly <= 0) {
            throw new IOException("Invalid number of verts per poly " + header.maxVertsPerPoly);
        }
        bool cCompatibility = header.version == NavMeshSetHeader.NAVMESHSET_VERSION;
        NavMesh mesh = new NavMesh(header.option, header.maxVertsPerPoly);
        readTiles(bb, is32Bit, header, cCompatibility, mesh);
        return mesh;
    }

    private NavMeshSetHeader readHeader(ByteBuffer bb, int maxVertsPerPoly) {
        NavMeshSetHeader header = new NavMeshSetHeader();
        header.magic = bb.getInt();
        if (header.magic != NavMeshSetHeader.NAVMESHSET_MAGIC) {
            header.magic = IOUtils.swapEndianness(header.magic);
            if (header.magic != NavMeshSetHeader.NAVMESHSET_MAGIC) {
                throw new IOException("Invalid magic " + header.magic);
            }
            bb.order(bb.order() == ByteOrder.BIG_ENDIAN ? ByteOrder.LITTLE_ENDIAN : ByteOrder.BIG_ENDIAN);
        }
        header.version = bb.getInt();
        if (header.version != NavMeshSetHeader.NAVMESHSET_VERSION && header.version != NavMeshSetHeader.NAVMESHSET_VERSION_RECAST4J_1
                && header.version != NavMeshSetHeader.NAVMESHSET_VERSION_RECAST4J) {
            throw new IOException("Invalid version " + header.version);
        }
        header.numTiles = bb.getInt();
        header.option = paramReader.read(bb);
        header.maxVertsPerPoly = maxVertsPerPoly;
        if (header.version == NavMeshSetHeader.NAVMESHSET_VERSION_RECAST4J) {
            header.maxVertsPerPoly = bb.getInt();
        }
        return header;
    }

    private void readTiles(ByteBuffer bb, bool is32Bit, NavMeshSetHeader header, bool cCompatibility, NavMesh mesh)
            {
        // Read tiles.
        for (int i = 0; i < header.numTiles; ++i) {
            NavMeshTileHeader tileHeader = new NavMeshTileHeader();
            if (is32Bit) {
                tileHeader.tileRef = convert32BitRef(bb.getInt(), header.option);
            } else {
                tileHeader.tileRef = bb.getLong();
            }
            tileHeader.dataSize = bb.getInt();
            if (tileHeader.tileRef == 0 || tileHeader.dataSize == 0) {
                break;
            }
            if (cCompatibility && !is32Bit) {
                bb.getInt(); // C struct padding
            }
            MeshData data = meshReader.read(bb, mesh.getMaxVertsPerPoly(), is32Bit);
            mesh.addTile(data, i, tileHeader.tileRef);
        }
    }

    private long convert32BitRef(int refs, NavMeshParams option) {
        int m_tileBits = ilog2(nextPow2(option.maxTiles));
        int m_polyBits = ilog2(nextPow2(option.maxPolys));
        // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
        int m_saltBits = Math.Min(31, 32 - m_tileBits - m_polyBits);
        int saltMask = (1 << m_saltBits) - 1;
        int tileMask = (1 << m_tileBits) - 1;
        int polyMask = (1 << m_polyBits) - 1;
        int salt = ((refs >> (m_polyBits + m_tileBits)) & saltMask);
        int it = ((refs >> m_polyBits) & tileMask);
        int ip = refs & polyMask;
        return NavMesh.encodePolyId(salt, it, ip);
    }
}
