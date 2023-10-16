using System;
using DotRecast.Core;
using K4os.Compression.LZ4;

namespace DotRecast.Recast.Demo;

public class DtVoxelTileLZ4DemoCompressor : IRcCompressor
{
    public static readonly DtVoxelTileLZ4DemoCompressor Shared = new();

    private DtVoxelTileLZ4DemoCompressor()
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
        Array.Copy(compressed, 0, result, 4, compressed.Length);
        return result;
    }
}