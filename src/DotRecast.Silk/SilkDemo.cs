using System;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace DotRecast.Silk;

public class SilkDemo
{
    private static readonly ILogger Logger = Log.ForContext<SilkDemo>();

    private IWindow _win;
    private IInputContext _input;
    private GL _gl;
    private uint _vao; // vertex array object
    private uint _vbo; // vertex buffer object
    private uint _ebo; // 
    private uint _program;
    
    public void Run()
    {
        Log.Logger.Information("running");

        var options = WindowOptions.Default;
        options.Title = "silk demo";
        options.Size = new Vector2D<int>(1024, 768);
        options.VSync = false;
        options.ShouldSwapAutomatically = false;
        _win = Window.Create(options);

        _win.Closing += OnWindowClosing;
        _win.Load += OnWindowLoad;
        _win.Resize += OnWindowResize;
        _win.FramebufferResize += OnWindowFramebufferResize;
        _win.Update += OnWindowUpdate;
        _win.Render += OnWindowRender;

        _win.Run();
    }

    private void OnWindowClosing()
    {
    }

    private unsafe void OnWindowLoad()
    {
        _input = _win.CreateInput();
        _gl = _win.CreateOpenGL();

        Logger.Information($"{_win.API.Profile}");
        Logger.Information($"{_win.API.Version.MajorVersion} {_win.API.Version.MinorVersion}");

        //_gl.ClearColor(Color.CornflowerBlue);

        // Create the VAO.
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer(); // Create the VBO.
        _ebo = _gl.GenBuffer(); // Create the EBO.
        
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        // The quad vertices data.
        float[] vertices =
        {
            0.5f,  0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f
        };

        
        // Upload the vertices data to the VBO.
        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        // The quad indices data.
        uint[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };


        // Upload the indices data to the EBO.
        fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

        const string vertexCode = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
void main()
{
    gl_Position = vec4(aPosition, 1.0);
}";

        const string fragmentCode = @"
#version 330 core
out vec4 out_color;
void main()
{
    out_color = vec4(1.0, 0.5, 0.2, 1.0);
}";

        _program = _gl.CreateProgram();
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        
        _gl.ShaderSource(vertexShader, vertexCode);
        _gl.ShaderSource(fragmentShader, fragmentCode);
        
        _gl.CompileShader(vertexShader);
        _gl.CompileShader(fragmentShader);
        
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));

        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);
        _gl.LinkProgram(_program);

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
        
        var positionLoc = (uint)_gl.GetAttribLocation(_program, "aPosition");
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*) 0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }


    private void OnWindowResize(Vector2D<int> size)
    {
    }

    private void OnWindowFramebufferResize(Vector2D<int> size)
    {
    }

    private void OnWindowUpdate(double dt)
    {
    }

    private unsafe void OnWindowRender(double dt)
    {
        _gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
        _gl.Clear((uint)GLEnum.ColorBufferBit | (uint)GLEnum.DepthBufferBit);

        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);
        
        _win.SwapBuffers();
    }
}