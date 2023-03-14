/*
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
using System.IO.Compression;
using DotRecast.Core;
using DotRecast.Detour.Io;

namespace DotRecast.Detour.Extras.Unity.Astar;

public abstract class ZipBinaryReader {

    protected ByteBuffer toByteBuffer(ZipArchive file, string filename) {
        ZipArchiveEntry graphReferences = file.GetEntry(filename);
        using var entryStream = graphReferences.Open();
        using var bis = new BinaryReader(entryStream);
        ByteBuffer buffer = IOUtils.toByteBuffer(bis);
        buffer.order(ByteOrder.LITTLE_ENDIAN);
        return buffer;
    }

}
