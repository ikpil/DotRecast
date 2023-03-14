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

using DotRecast.Core;

namespace DotRecast.Detour.Dynamic.Io;

public static class ByteUtils {

    public static int getInt(byte[] data, int position, ByteOrder order) {
        return order == ByteOrder.BIG_ENDIAN ? getIntBE(data, position) : getIntLE(data, position);
    }

    public static int getIntBE(byte[] data, int position) {
        return ((data[position] & 0xff) << 24) | ((data[position + 1] & 0xff) << 16) | ((data[position + 2] & 0xff) << 8)
                | (data[position + 3] & 0xff);
    }

    public static int getIntLE(byte[] data, int position) {
        return ((data[position + 3] & 0xff) << 24) | ((data[position + 2] & 0xff) << 16) | ((data[position + 1] & 0xff) << 8)
                | (data[position] & 0xff);
    }

    public static int getShort(byte[] data, int position, ByteOrder order) {
        return order == ByteOrder.BIG_ENDIAN ? getShortBE(data, position) : getShortLE(data, position);
    }

    public static int getShortBE(byte[] data, int position) {
        return ((data[position] & 0xff) << 8) | (data[position + 1] & 0xff);
    }

    public static int getShortLE(byte[] data, int position) {
        return ((data[position + 1] & 0xff) << 8) | (data[position] & 0xff);
    }

    public static int putInt(int value, byte[] data, int position, ByteOrder order) {
        if (order == ByteOrder.BIG_ENDIAN) {
            data[position] = (byte) (value >>> 24);
            data[position + 1] = (byte) (value >>> 16);
            data[position + 2] = (byte) (value >>> 8);
            data[position + 3] = (byte) (value & 0xFF);
        } else {
            data[position] = (byte) (value & 0xFF);
            data[position + 1] = (byte) (value >>> 8);
            data[position + 2] = (byte) (value >>> 16);
            data[position + 3] = (byte) (value >>> 24);
        }
        return position + 4;
    }

    public static int putShort(int value, byte[] data, int position, ByteOrder order) {
        if (order == ByteOrder.BIG_ENDIAN) {
            data[position] = (byte) (value >>> 8);
            data[position + 1] = (byte) (value & 0xFF);
        } else {
            data[position] = (byte) (value & 0xFF);
            data[position + 1] = (byte) (value >>> 8);
        }
        return position + 2;
    }

}
