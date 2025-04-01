using System;
using Silk.NET.OpenGL;
using System.Numerics;
using DotRecast.Core.Numerics;

namespace DotRecast.Recast.Demo.Draw;

// TODO use a lot of Memory, 2GB+

public class ModernOpenGLDraw : IOpenGLDraw
{
    private GL _gl;
    private uint program;
    private int uniformTexture;
    private int uniformProjectionMatrix;
    private uint vbo;
    private uint ebo;
    private uint vao;
    private DebugDrawPrimitives currentPrim;
    private float fogStart;
    private float fogEnd;
    private bool fogEnabled;
    private int uniformViewMatrix;
    private readonly ArrayBuffer<OpenGLVertex> vertices = new(512);
    private readonly ArrayBuffer<int> elements = new(512);
    private GLCheckerTexture _texture;
    private readonly float[] _viewMatrix = new float[16];
    private readonly float[] _projectionMatrix = new float[16];
    private int uniformUseTexture;
    private int uniformFog;
    private int uniformFogStart;
    private int uniformFogEnd;
    private GLEnum _windingOrder = GLEnum.Ccw;

    public ModernOpenGLDraw(GL gl)
    {
        _gl = gl;
    }

    public unsafe void Init()
    {
        const string SHADER_VERSION = "#version 400\n";
        const string vertex_shader = $@"
{SHADER_VERSION}
uniform mat4 ProjMtx;
uniform mat4 ViewMtx;
in vec3 Position;
in vec2 TexCoord;
in vec4 Color;
out vec2 Frag_UV;
out vec4 Frag_Color;
out float Frag_Depth;
void main() {{
   Frag_UV = TexCoord;
   Frag_Color = Color;
   vec4 VSPosition = ViewMtx * vec4(Position, 1);
   Frag_Depth = -VSPosition.z;
   gl_Position = ProjMtx * VSPosition;
}}
";
        const string fragment_shader = $@"
{SHADER_VERSION}
precision mediump float;
uniform sampler2D Texture;
uniform float UseTexture;
uniform float EnableFog;
uniform float FogStart;
uniform float FogEnd;
const vec4 FogColor = vec4(0.3f, 0.3f, 0.32f, 1.0f);
in vec2 Frag_UV;
in vec4 Frag_Color;
in float Frag_Depth;
out vec4 Out_Color;
void main(){{
   Out_Color = mix(FogColor, Frag_Color * mix(vec4(1), texture(Texture, Frag_UV.st), UseTexture), 1.0 - EnableFog * clamp( (Frag_Depth - FogStart) / (FogEnd - FogStart), 0.0, 1.0) );
}}
";

        program = _gl.CreateProgram();
        uint vert_shdr = _gl.CreateShader(GLEnum.VertexShader);
        _gl.ShaderSource(vert_shdr, vertex_shader);
        _gl.CompileShader(vert_shdr);
        _gl.GetShader(vert_shdr, GLEnum.CompileStatus, out var status);
        if (status != (int)GLEnum.True)
        {
            throw new InvalidOperationException();
        }

        uint frag_shdr = _gl.CreateShader(GLEnum.FragmentShader);
        _gl.ShaderSource(frag_shdr, fragment_shader);
        _gl.CompileShader(frag_shdr);
        _gl.GetShader(frag_shdr, GLEnum.CompileStatus, out status);
        if (status != (int)GLEnum.True)
        {
            throw new InvalidOperationException();
        }

        _gl.AttachShader(program, vert_shdr);
        _gl.AttachShader(program, frag_shdr);
        _gl.LinkProgram(program);
        _gl.GetProgram(program, GLEnum.LinkStatus, out status);
        if (status != (int)GLEnum.True)
        {
            throw new InvalidOperationException();
        }

        _gl.DetachShader(program, vert_shdr);
        _gl.DetachShader(program, frag_shdr);
        _gl.DeleteShader(vert_shdr);
        _gl.DeleteShader(frag_shdr);

        uniformTexture = _gl.GetUniformLocation(program, "Texture");
        uniformUseTexture = _gl.GetUniformLocation(program, "UseTexture");
        uniformFog = _gl.GetUniformLocation(program, "EnableFog");
        uniformFogStart = _gl.GetUniformLocation(program, "FogStart");
        uniformFogEnd = _gl.GetUniformLocation(program, "FogEnd");
        uniformProjectionMatrix = _gl.GetUniformLocation(program, "ProjMtx");
        uniformViewMatrix = _gl.GetUniformLocation(program, "ViewMtx");

        uint attrib_pos = (uint)_gl.GetAttribLocation(program, "Position");
        uint attrib_uv = (uint)_gl.GetAttribLocation(program, "TexCoord");
        uint attrib_col = (uint)_gl.GetAttribLocation(program, "Color");

        // buffer setup
        vao = _gl.GenVertexArray();
        vbo = _gl.GenBuffer();
        ebo = _gl.GenBuffer();

        _gl.BindVertexArray(vao);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);


        _gl.EnableVertexAttribArray(attrib_pos);
        _gl.EnableVertexAttribArray(attrib_uv);
        _gl.EnableVertexAttribArray(attrib_col);

        // _gl.VertexAttribP3(attrib_pos, GLEnum.Float, false, 24);
        // _gl.VertexAttribP2(attrib_uv, GLEnum.Float, false, 24);
        // _gl.VertexAttribP4(attrib_col, GLEnum.UnsignedByte, true, 24);
        IntPtr pointer1 = (IntPtr)0;
        IntPtr pointer2 = (IntPtr)12;
        IntPtr pointer3 = (IntPtr)20;
        _gl.VertexAttribPointer(attrib_pos, 3, VertexAttribPointerType.Float, false, 24, pointer1.ToPointer());
        _gl.VertexAttribPointer(attrib_uv, 2, VertexAttribPointerType.Float, false, 24, pointer2.ToPointer());
        _gl.VertexAttribPointer(attrib_col, 4, VertexAttribPointerType.UnsignedByte, true, 24, pointer3.ToPointer());

        // _gl.VertexAttribPointer(attrib_pos, 3, GLEnum.Float, false, 24, (void*)0);
        // _gl.VertexAttribPointer(attrib_uv, 2, GLEnum.Float, false, 24, (void*)12);
        // _gl.VertexAttribPointer(attrib_col, 4, GLEnum.UnsignedByte, true, 24, (void*)20);


        // _gl.VertexAttribP3(attrib_pos, GLEnum.Float, false, 0);
        // _gl.VertexAttribP2(attrib_uv, GLEnum.Float, false, 12);
        // _gl.VertexAttribP4(attrib_col, GLEnum.UnsignedByte, true, 20);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

        //int* range = stackalloc int[2];
        //_gl.GetInteger(GetPName.LineWidthRange, range);
        //Console.WriteLine($"\nLineWidthRange: {range[0]} {range[1]}");
    }

    public void Clear()
    {
        _gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit);
        _gl.Enable(GLEnum.Blend);
        _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        _gl.Disable(GLEnum.Texture2D);
        _gl.Enable(GLEnum.DepthTest);
        _gl.Enable(GLEnum.CullFace);
        _gl.FrontFace(_windingOrder);
    }

    public void Begin(DebugDrawPrimitives prim, float size)
    {
        currentPrim = prim;
        vertices.Clear();
        _gl.LineWidth(size);
        _gl.PointSize(size);
    }

    public unsafe void End()
    {
        if (0 >= vertices.Count)
        {
            return;
        }

        _gl.UseProgram(program);
        _gl.Uniform1(uniformTexture, 0);
        _gl.UniformMatrix4(uniformViewMatrix, 1, false, _viewMatrix);
        _gl.UniformMatrix4(uniformProjectionMatrix, 1, false, _projectionMatrix);
        _gl.Uniform1(uniformFogStart, fogStart);
        _gl.Uniform1(uniformFogEnd, fogEnd);
        _gl.Uniform1(uniformFog, fogEnabled ? 1.0f : 0.0f);
        _gl.BindVertexArray(vao);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        // GlBufferData(GL_ARRAY_BUFFER, MAX_VERTEX_BUFFER, GL_STREAM_DRAW);
        // GlBufferData(GL_ELEMENT_ARRAY_BUFFER, MAX_ELEMENT_BUFFER, GL_STREAM_DRAW);

        _gl.BufferData<OpenGLVertex>(GLEnum.ArrayBuffer, vertices.AsArray(), GLEnum.DynamicDraw);

        if (currentPrim == DebugDrawPrimitives.QUADS)
        {
            for (int i = 0; i < vertices.Count; i += 4)
            {
                elements.Add(i);
                elements.Add(i + 1);
                elements.Add(i + 2);
                elements.Add(i);
                elements.Add(i + 2);
                elements.Add(i + 3);
            }
        }
        else
        {
            for (int i = elements.Count; i < vertices.Count; i++)
            {
                elements.Add(i);
            }
        }

        _gl.BufferData<int>(GLEnum.ElementArrayBuffer, elements.AsArray(), GLEnum.DynamicDraw);

        if (_texture != null)
        {
            _texture.Bind();
            _gl.Uniform1(uniformUseTexture, 1.0f);
        }
        else
        {
            _gl.Uniform1(uniformUseTexture, 0.0f);
        }

        switch (currentPrim)
        {
            case DebugDrawPrimitives.POINTS:
                _gl.DrawElements(GLEnum.Points, (uint)vertices.Count, GLEnum.UnsignedInt, (void*)0);
                break;
            case DebugDrawPrimitives.LINES:
                _gl.DrawElements(GLEnum.Lines, (uint)vertices.Count, GLEnum.UnsignedInt, (void*)0);
                break;
            case DebugDrawPrimitives.TRIS:
                _gl.DrawElements(GLEnum.Triangles, (uint)vertices.Count, GLEnum.UnsignedInt, (void*)0);
                break;
            case DebugDrawPrimitives.QUADS:
                _gl.DrawElements(GLEnum.Triangles, (uint)(vertices.Count * 6 / 4), GLEnum.UnsignedInt, (void*)0);
                break;
            default:
                break;
        }

        _gl.UseProgram(0);
        _gl.BindVertexArray(0);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        vertices.Clear();
        elements.Clear();
        _gl.LineWidth(1.0f);
        _gl.PointSize(1.0f);
    }


    public void Vertex(float x, float y, float z, int color)
    {
        vertices.Add(new OpenGLVertex(x, y, z, color));
    }

    public unsafe void Vertex(float* pos, int color)
    {
        vertices.Add(new OpenGLVertex(pos[0], pos[1], pos[2], color));
    }

    public void Vertex(float[] pos, int color)
    {
        vertices.Add(new OpenGLVertex(pos, color));
    }

    public void Vertex(Vector3 pos, int color)
    {
        vertices.Add(new OpenGLVertex(pos, color));
    }


    public void Vertex(Vector3 pos, int color, Vector2 uv)
    {
        vertices.Add(new OpenGLVertex(pos, uv, color));
    }

    public void Vertex(float x, float y, float z, int color, float u, float v)
    {
        vertices.Add(new OpenGLVertex(x, y, z, u, v, color));
    }

    public void DepthMask(bool state)
    {
        _gl.DepthMask(state);
    }

    public void Texture(GLCheckerTexture g_tex, bool state)
    {
        _texture = state ? g_tex : null;
        if (_texture != null)
        {
            _texture.Bind();
        }
    }

    public void ProjectionMatrix(ref RcMatrix4x4f projectionMatrix)
    {
        projectionMatrix.CopyTo(_projectionMatrix);
    }

    public void ViewMatrix(ref RcMatrix4x4f viewMatrix)
    {
        viewMatrix.CopyTo(_viewMatrix);
    }

    public void Fog(float start, float end)
    {
        fogStart = start;
        fogEnd = end;
    }

    public void Fog(bool state)
    {
        fogEnabled = state;
    }

    public void SetWindingOrder(GLEnum windingOrder)
    {
        _windingOrder = windingOrder;
    }
}