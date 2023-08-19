using System;
using System.IO;
using System.Runtime.InteropServices;
using DotRecast.Core;

namespace DotRecast.Recast.Demo.Draw;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct OpenGLVertex
{
    [FieldOffset(0)]
    private readonly float x;

    [FieldOffset(4)]
    private readonly float y;

    [FieldOffset(8)]
    private readonly float z;

    [FieldOffset(12)]
    private readonly float u;

    [FieldOffset(16)]
    private readonly float v;

    [FieldOffset(20)]
    private readonly int color;

    public OpenGLVertex(RcVec3f pos, RcVec2f uv, int color) :
        this(pos.x, pos.y, pos.z, uv.x, uv.y, color)
    {
    }

    public OpenGLVertex(float[] pos, int color) :
        this(pos[0], pos[1], pos[2], 0f, 0f, color)
    {
    }

    public OpenGLVertex(RcVec3f pos, int color) :
        this(pos.x, pos.y, pos.z, 0f, 0f, color)
    {
    }


    public OpenGLVertex(float x, float y, float z, int color) :
        this(x, y, z, 0f, 0f, color)
    {
    }

    public OpenGLVertex(float x, float y, float z, float u, float v, int color)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.u = u;
        this.v = v;
        this.color = color;
    }

    public void Store(BinaryWriter writer)
    {
        // writer.Write(BitConverter.GetBytes(x));
        // writer.Write(BitConverter.GetBytes(y));
        // writer.Write(BitConverter.GetBytes(z));
        // writer.Write(BitConverter.GetBytes(u));
        // writer.Write(BitConverter.GetBytes(v));
        // writer.Write(BitConverter.GetBytes(color));

        writer.Write(x);
        writer.Write(y);
        writer.Write(z);
        writer.Write(u);
        writer.Write(v);
        writer.Write(color);
    }
}