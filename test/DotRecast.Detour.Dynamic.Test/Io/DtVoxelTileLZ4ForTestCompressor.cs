/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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

using System;
using DotRecast.Core;
using K4os.Compression.LZ4;

namespace DotRecast.Detour.Dynamic.Test.Io
{
    public class DtVoxelTileLZ4ForTestCompressor : IRcCompressor
    {
        public static readonly DtVoxelTileLZ4ForTestCompressor Shared = new();

        private DtVoxelTileLZ4ForTestCompressor()
        {
        }

        public byte[] Decompress(byte[] data)
        {
            int compressedSize = RcByteUtils.GetIntBE(data, 0);
            return LZ4Pickler.Unpickle(data.AsSpan(4, compressedSize));
        }

        public byte[] Decompress(byte[] buf, int offset, int len, int outputlen)
        {
            return LZ4Pickler.Unpickle(buf, offset, len);
        }

        public byte[] Compress(byte[] data)
        {
            byte[] compressed = LZ4Pickler.Pickle(data, LZ4Level.L12_MAX);
            byte[] result = new byte[4 + compressed.Length];
            RcByteUtils.PutInt(compressed.Length, result, 0, RcByteOrder.BIG_ENDIAN);
            RcArrays.Copy(compressed, 0, result, 4, compressed.Length);
            return result;
        }
    }
}