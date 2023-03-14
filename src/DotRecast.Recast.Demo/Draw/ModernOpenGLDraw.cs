using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.PlatformAbstractions;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace DotRecast.Recast.Demo.Draw;

public class ModernOpenGLDraw : OpenGLDraw {
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
    private readonly List<OpenGLVertex> vertices = new();
    private GLCheckerTexture _texture;
    private float[] _viewMatrix;
    private float[] _projectionMatrix;
    private int uniformUseTexture;
    private int uniformFog;
    private int uniformFogStart;
    private int uniformFogEnd;

    public void init(GL gl)
    {
        _gl = gl;
        string NK_SHADER_VERSION = PlatformID.MacOSX == Environment.OSVersion.Platform ? "#version 150\n" : "#version 300 es\n";
        string vertex_shader = NK_SHADER_VERSION + "uniform mat4 ProjMtx;\n"//
                + "uniform mat4 ViewMtx;\n"//
                + "in vec3 Position;\n"//
                + "in vec2 TexCoord;\n"//
                + "in vec4 Color;\n"//
                + "out vec2 Frag_UV;\n"//
                + "out vec4 Frag_Color;\n"//
                + "out float Frag_Depth;\n"//
                + "void main() {\n"//
                + "   Frag_UV = TexCoord;\n"//
                + "   Frag_Color = Color;\n"//
                + "   vec4 VSPosition = ViewMtx * vec4(Position, 1);\n"//
                + "   Frag_Depth = -VSPosition.z;\n"//
                + "   gl_Position = ProjMtx * VSPosition;\n"//
                + "}\n";
        string fragment_shader = NK_SHADER_VERSION + "precision mediump float;\n"//
                + "uniform sampler2D Texture;\n"//
                + "uniform float UseTexture;\n"//
                + "uniform float EnableFog;\n"//
                + "uniform float FogStart;\n"//
                + "uniform float FogEnd;\n"//
                + "const vec4 FogColor = vec4(0.3f, 0.3f, 0.32f, 1.0f);\n"//
                + "in vec2 Frag_UV;\n"//
                + "in vec4 Frag_Color;\n"//
                + "in float Frag_Depth;\n"//
                + "out vec4 Out_Color;\n"//
                + "void main(){\n"//
                + "   Out_Color = mix(FogColor, Frag_Color * mix(vec4(1), texture(Texture, Frag_UV.st), UseTexture), 1.0 - EnableFog * clamp( (Frag_Depth - FogStart) / (FogEnd - FogStart), 0.0, 1.0) );\n"//
                + "}\n";
        
        program = _gl.CreateProgram();
        uint vert_shdr = _gl.CreateShader(GLEnum.VertexShader);
        uint frag_shdr = _gl.CreateShader(GLEnum.FragmentShader);
        _gl.ShaderSource(vert_shdr, vertex_shader);
        _gl.ShaderSource(frag_shdr, fragment_shader);
        _gl.CompileShader(vert_shdr);
        _gl.CompileShader(frag_shdr);
        gl.GetShader(vert_shdr, GLEnum.CompileStatus, out var status);
        if (status != (int) GLEnum.True) {
            throw new InvalidOperationException();
        }
        gl.GetShader(frag_shdr, GLEnum.CompileStatus, out status);
        if (status != (int) GLEnum.True) {
            throw new InvalidOperationException();
        }
        _gl.AttachShader(program, vert_shdr);
        _gl.AttachShader(program, frag_shdr);
        _gl.LinkProgram(program);
        _gl.GetProgram(program, GLEnum.LinkStatus, out status);
        if (status != (int) GLEnum.True) {
            throw new InvalidOperationException();
        }
        uniformTexture = _gl.GetUniformLocation(program, "Texture");
        uniformUseTexture = _gl.GetUniformLocation(program, "UseTexture");
        uniformFog = _gl.GetUniformLocation(program, "EnableFog");
        uniformFogStart = _gl.GetUniformLocation(program, "FogStart");
        uniformFogEnd = _gl.GetUniformLocation(program, "FogEnd");
        uniformProjectionMatrix = _gl.GetUniformLocation(program, "ProjMtx");
        uniformViewMatrix = _gl.GetUniformLocation(program, "ViewMtx");
        uint attrib_pos = (uint) _gl.GetAttribLocation(program, "Position");
        uint attrib_uv = (uint) _gl.GetAttribLocation(program, "TexCoord");
        uint attrib_col = (uint) _gl.GetAttribLocation(program, "Color");
        
        // buffer setup
        _gl.GenBuffers(1, out vbo);
        _gl.GenBuffers(1, out ebo);
        _gl.GenVertexArrays(1, out vao);
        
        _gl.BindVertexArray(vao);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        
        _gl.EnableVertexAttribArray(attrib_pos);
        _gl.EnableVertexAttribArray(attrib_uv);
        _gl.EnableVertexAttribArray(attrib_col);

        // _gl.VertexAttribPointer(attrib_pos, 3, GLEnum.Float, false, 24, 0);
        // _gl.VertexAttribPointer(attrib_uv, 2, GLEnum.Float, false, 24, 12);
        // _gl.VertexAttribPointer(attrib_col, 4, GLEnum.UnsignedByte, true, 24, 20);
        
        _gl.VertexAttribP3(attrib_pos, GLEnum.Float, false, 0);
        _gl.VertexAttribP2(attrib_uv, GLEnum.Float, false, 12);
        _gl.VertexAttribP4(attrib_col, GLEnum.UnsignedByte, true, 20);
        
        _gl.BindTexture(GLEnum.Texture2D, 0);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        _gl.BindVertexArray(0);
    }

    public void clear() {
        _gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit);
        _gl.Enable(GLEnum.Blend);
        _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        _gl.Disable(GLEnum.Texture2D);
        _gl.Enable(GLEnum.DepthTest);
        _gl.Enable(GLEnum.CullFace);
    }

    public void begin(DebugDrawPrimitives prim, float size) {
        currentPrim = prim;
        vertices.Clear();
        _gl.LineWidth(size);
        _gl.PointSize(size);
    }

    public void end() {
        // if (vertices.isEmpty()) {
        //     return;
        // }
        // glUseProgram(program);
        // glUniform1i(uniformTexture, 0);
        // glUniformMatrix4fv(uniformViewMatrix, false, viewMatrix);
        // glUniformMatrix4fv(uniformProjectionMatrix, false, projectionMatrix);
        // glUniform1f(uniformFogStart, fogStart);
        // glUniform1f(uniformFogEnd, fogEnd);
        // glUniform1f(uniformFog, fogEnabled ? 1.0f : 0.0f);
        // glBindVertexArray(vao);
        // glBindBuffer(GL_ARRAY_BUFFER, vbo);
        // glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        // // glBufferData(GL_ARRAY_BUFFER, MAX_VERTEX_BUFFER, GL_STREAM_DRAW);
        // // glBufferData(GL_ELEMENT_ARRAY_BUFFER, MAX_ELEMENT_BUFFER, GL_STREAM_DRAW);
        //
        // int vboSize = vertices.size() * 24;
        // int eboSize = currentPrim == DebugDrawPrimitives.QUADS ? vertices.size() * 6 : vertices.size() * 4;
        //
        // glBufferData(GL_ARRAY_BUFFER, vboSize, GL_STREAM_DRAW);
        // glBufferData(GL_ELEMENT_ARRAY_BUFFER, eboSize, GL_STREAM_DRAW);
        // // load draw vertices & elements directly into vertex + element buffer
        // ByteBuffer verts = glMapBuffer(GL_ARRAY_BUFFER, GL_WRITE_ONLY, vboSize, null);
        // ByteBuffer elems = glMapBuffer(GL_ELEMENT_ARRAY_BUFFER, GL_WRITE_ONLY, eboSize, null);
        // vertices.forEach(v => v.store(verts));
        // if (currentPrim == DebugDrawPrimitives.QUADS) {
        //     for (int i = 0; i < vertices.size(); i += 4) {
        //         elems.putInt(i);
        //         elems.putInt(i + 1);
        //         elems.putInt(i + 2);
        //         elems.putInt(i);
        //         elems.putInt(i + 2);
        //         elems.putInt(i + 3);
        //     }
        //
        // } else {
        //     for (int i = 0; i < vertices.size(); i++) {
        //         elems.putInt(i);
        //     }
        // }
        //
        // glUnmapBuffer(GL_ELEMENT_ARRAY_BUFFER);
        // glUnmapBuffer(GL_ARRAY_BUFFER);
        // if (texture != null) {
        //     texture.bind();
        //     glUniform1f(uniformUseTexture, 1.0f);
        // } else {
        //     glUniform1f(uniformUseTexture, 0.0f);
        // }
        //
        // switch (currentPrim) {
        // case POINTS:
        //     glDrawElements(GL_POINTS, vertices.size(), GL_UNSIGNED_INT, 0);
        //     break;
        // case LINES:
        //     glDrawElements(GL_LINES, vertices.size(), GL_UNSIGNED_INT, 0);
        //     break;
        // case TRIS:
        //     glDrawElements(GL_TRIANGLES, vertices.size(), GL_UNSIGNED_INT, 0);
        //     break;
        // case QUADS:
        //     glDrawElements(GL_TRIANGLES, vertices.size() * 6 / 4, GL_UNSIGNED_INT, 0);
        //     break;
        // default:
        //     break;
        // }
        //
        // glUseProgram(0);
        // glBindBuffer(GL_ARRAY_BUFFER, 0);
        // glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        // glBindVertexArray(0);
        // vertices.clear();
        // glLineWidth(1.0f);
        // glPointSize(1.0f);
    }

    public void vertex(float x, float y, float z, int color) {
        vertices.Add(new OpenGLVertex(x, y, z, color));
    }

    public void vertex(float[] pos, int color) {
        vertices.Add(new OpenGLVertex(pos, color));
    }

    public void vertex(float[] pos, int color, float[] uv) {
        vertices.Add(new OpenGLVertex(pos, uv, color));
    }

    public void vertex(float x, float y, float z, int color, float u, float v) {
        vertices.Add(new OpenGLVertex(x, y, z, u, v, color));
    }

    public void depthMask(bool state) {
        _gl.DepthMask(state);
    }

    public void texture(GLCheckerTexture g_tex, bool state) {
        _texture = state ? g_tex : null;
        if (_texture != null) {
            _texture.bind();
        }
    }

    public void projectionMatrix(float[] projectionMatrix) {
        this._projectionMatrix = projectionMatrix;
    }

    public void viewMatrix(float[] viewMatrix) {
        this._viewMatrix = viewMatrix;
    }

    public void fog(float start, float end) {
        fogStart = start;
        fogEnd = end;
    }

    public void fog(bool state) {
        fogEnabled = state;
    }

}
