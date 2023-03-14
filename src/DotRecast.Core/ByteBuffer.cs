using System;
using System.Buffers.Binary;

namespace DotRecast.Core;

public class ByteBuffer
{
    private ByteOrder _order;
    private byte[] _bytes;
    private int _position;

    public ByteBuffer(byte[] bytes)
    {
        _order = BitConverter.IsLittleEndian
            ? ByteOrder.LITTLE_ENDIAN
            : ByteOrder.BIG_ENDIAN;

        _bytes = bytes;
        _position = 0;
    }

    public ByteOrder order()
    {
        return _order;
    }

    public void order(ByteOrder order)
    {
        _order = order;
    }

    public int limit() {
        return _bytes.Length - _position;
    }
    
    public int remaining() {
        int rem = limit();
        return rem > 0 ? rem : 0;
    }


    public void position(int pos)
    {
        _position = pos;
    }

    public int position()
    {
        return _position;
    }

    public Span<byte> ReadBytes(int length)
    {
        var nextPos = _position + length;
        (nextPos, _position) = (_position, nextPos);

        return _bytes.AsSpan(nextPos, length);
    }

    public byte get()
    {
        var span = ReadBytes(1);
        return span[0];
    }

    public short getShort()
    {
        var span = ReadBytes(2);
        if (_order == ByteOrder.BIG_ENDIAN)
        {
            return BinaryPrimitives.ReadInt16BigEndian(span);
        }
        else
        {
            return BinaryPrimitives.ReadInt16LittleEndian(span);
        }
    }


    public int getInt()
    {
        var span = ReadBytes(4);
        if (_order == ByteOrder.BIG_ENDIAN)
        {
            return BinaryPrimitives.ReadInt32BigEndian(span);
        }
        else
        {
            return BinaryPrimitives.ReadInt32LittleEndian(span);
        }
    }

    public float getFloat()
    {
        var span = ReadBytes(4);
        if (_order == ByteOrder.BIG_ENDIAN)
        {
            return BinaryPrimitives.ReadSingleBigEndian(span);
        }
        else
        {
            return BinaryPrimitives.ReadSingleLittleEndian(span);
        }
    }

    public long getLong()
    {
        var span = ReadBytes(8);
        if (_order == ByteOrder.BIG_ENDIAN)
        {
            return BinaryPrimitives.ReadInt64BigEndian(span);
        }
        else
        {
            return BinaryPrimitives.ReadInt64LittleEndian(span);
        }
    }
    
    public void putFloat(float v)
    {
        // ?
    }

    public void putInt(int v)
    {
        // ?
    }
}