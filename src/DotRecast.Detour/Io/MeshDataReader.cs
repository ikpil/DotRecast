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

using System.IO;
using DotRecast.Core;

namespace DotRecast.Detour.Io;

public class MeshDataReader {

    public const int DT_POLY_DETAIL_SIZE = 10;

    public MeshData read(BinaryReader stream, int maxVertPerPoly) {
        ByteBuffer buf = IOUtils.toByteBuffer(stream);
        return read(buf, maxVertPerPoly, false);
    }

    public MeshData read(ByteBuffer buf, int maxVertPerPoly) {
        return read(buf, maxVertPerPoly, false);
    }

    public MeshData read32Bit(BinaryReader stream, int maxVertPerPoly) {
        ByteBuffer buf = IOUtils.toByteBuffer(stream);
        return read(buf, maxVertPerPoly, true);
    }

    public MeshData read32Bit(ByteBuffer buf, int maxVertPerPoly) {
        return read(buf, maxVertPerPoly, true);
    }

    public MeshData read(ByteBuffer buf, int maxVertPerPoly, bool is32Bit)
    {
        MeshData data = new MeshData();
        MeshHeader header = new MeshHeader();
        data.header = header;
        header.magic = buf.getInt();
        if (header.magic != MeshHeader.DT_NAVMESH_MAGIC) {
            header.magic = IOUtils.swapEndianness(header.magic);
            if (header.magic != MeshHeader.DT_NAVMESH_MAGIC) {
                throw new IOException("Invalid magic");
            }
            buf.order(buf.order() == ByteOrder.BIG_ENDIAN ? ByteOrder.LITTLE_ENDIAN : ByteOrder.BIG_ENDIAN);
        }
        header.version = buf.getInt();
        if (header.version != MeshHeader.DT_NAVMESH_VERSION) {
            if (header.version < MeshHeader.DT_NAVMESH_VERSION_RECAST4J_FIRST
                    || header.version > MeshHeader.DT_NAVMESH_VERSION_RECAST4J_LAST) {
                throw new IOException("Invalid version " + header.version);
            }
        }
        bool cCompatibility = header.version == MeshHeader.DT_NAVMESH_VERSION;
        header.x = buf.getInt();
        header.y = buf.getInt();
        header.layer = buf.getInt();
        header.userId = buf.getInt();
        header.polyCount = buf.getInt();
        header.vertCount = buf.getInt();
        header.maxLinkCount = buf.getInt();
        header.detailMeshCount = buf.getInt();
        header.detailVertCount = buf.getInt();
        header.detailTriCount = buf.getInt();
        header.bvNodeCount = buf.getInt();
        header.offMeshConCount = buf.getInt();
        header.offMeshBase = buf.getInt();
        header.walkableHeight = buf.getFloat();
        header.walkableRadius = buf.getFloat();
        header.walkableClimb = buf.getFloat();
        for (int j = 0; j < 3; j++) {
            header.bmin[j] = buf.getFloat();
        }
        for (int j = 0; j < 3; j++) {
            header.bmax[j] = buf.getFloat();
        }
        header.bvQuantFactor = buf.getFloat();
        data.verts = readVerts(buf, header.vertCount);
        data.polys = readPolys(buf, header, maxVertPerPoly);
        if (cCompatibility) {
            buf.position(buf.position() + header.maxLinkCount * getSizeofLink(is32Bit));
        }
        data.detailMeshes = readPolyDetails(buf, header, cCompatibility);
        data.detailVerts = readVerts(buf, header.detailVertCount);
        data.detailTris = readDTris(buf, header);
        data.bvTree = readBVTree(buf, header);
        data.offMeshCons = readOffMeshCons(buf, header);
        return data;
    }

    public const int LINK_SIZEOF = 16;
    public const int LINK_SIZEOF32BIT = 12;

    public static int getSizeofLink(bool is32Bit) {
        return is32Bit ? LINK_SIZEOF32BIT : LINK_SIZEOF;
    }

    private float[] readVerts(ByteBuffer buf, int count) {
        float[] verts = new float[count * 3];
        for (int i = 0; i < verts.Length; i++) {
            verts[i] = buf.getFloat();
        }
        return verts;
    }

    private Poly[] readPolys(ByteBuffer buf, MeshHeader header, int maxVertPerPoly) {
        Poly[] polys = new Poly[header.polyCount];
        for (int i = 0; i < polys.Length; i++) {
            polys[i] = new Poly(i, maxVertPerPoly);
            if (header.version < MeshHeader.DT_NAVMESH_VERSION_RECAST4J_NO_POLY_FIRSTLINK) {
                buf.getInt(); // polys[i].firstLink
            }
            for (int j = 0; j < polys[i].verts.Length; j++) {
                polys[i].verts[j] = buf.getShort() & 0xFFFF;
            }
            for (int j = 0; j < polys[i].neis.Length; j++) {
                polys[i].neis[j] = buf.getShort() & 0xFFFF;
            }
            polys[i].flags = buf.getShort() & 0xFFFF;
            polys[i].vertCount = buf.get() & 0xFF;
            polys[i].areaAndtype = buf.get() & 0xFF;
        }
        return polys;
    }

    private PolyDetail[] readPolyDetails(ByteBuffer buf, MeshHeader header, bool cCompatibility) {
        PolyDetail[] polys = new PolyDetail[header.detailMeshCount];
        for (int i = 0; i < polys.Length; i++) {
            polys[i] = new PolyDetail();
            polys[i].vertBase = buf.getInt();
            polys[i].triBase = buf.getInt();
            polys[i].vertCount = buf.get() & 0xFF;
            polys[i].triCount = buf.get() & 0xFF;
            if (cCompatibility) {
                buf.getShort(); // C struct padding
            }
        }
        return polys;
    }

    private int[] readDTris(ByteBuffer buf, MeshHeader header) {
        int[] tris = new int[4 * header.detailTriCount];
        for (int i = 0; i < tris.Length; i++) {
            tris[i] = buf.get() & 0xFF;
        }
        return tris;
    }

    private BVNode[] readBVTree(ByteBuffer buf, MeshHeader header) {
        BVNode[] nodes = new BVNode[header.bvNodeCount];
        for (int i = 0; i < nodes.Length; i++) {
            nodes[i] = new BVNode();
            if (header.version < MeshHeader.DT_NAVMESH_VERSION_RECAST4J_32BIT_BVTREE) {
                for (int j = 0; j < 3; j++) {
                    nodes[i].bmin[j] = buf.getShort() & 0xFFFF;
                }
                for (int j = 0; j < 3; j++) {
                    nodes[i].bmax[j] = buf.getShort() & 0xFFFF;
                }
            } else {
                for (int j = 0; j < 3; j++) {
                    nodes[i].bmin[j] = buf.getInt();
                }
                for (int j = 0; j < 3; j++) {
                    nodes[i].bmax[j] = buf.getInt();
                }
            }
            nodes[i].i = buf.getInt();
        }
        return nodes;
    }

    private OffMeshConnection[] readOffMeshCons(ByteBuffer buf, MeshHeader header) {
        OffMeshConnection[] cons = new OffMeshConnection[header.offMeshConCount];
        for (int i = 0; i < cons.Length; i++) {
            cons[i] = new OffMeshConnection();
            for (int j = 0; j < 6; j++) {
                cons[i].pos[j] = buf.getFloat();
            }
            cons[i].rad = buf.getFloat();
            cons[i].poly = buf.getShort() & 0xFFFF;
            cons[i].flags = buf.get() & 0xFF;
            cons[i].side = buf.get() & 0xFF;
            cons[i].userId = buf.getInt();
        }
        return cons;
    }

}
