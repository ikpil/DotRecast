/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Runtime.InteropServices;
using DotRecast.Core;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using ImGuiNET;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Extras.Unity.Astar;
using DotRecast.Detour.Io;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Messages;
using DotRecast.Recast.Toolset.Geom;
using DotRecast.Recast.Demo.Tools;
using DotRecast.Recast.Demo.UI;
using MouseButton = Silk.NET.Input.MouseButton;
using Window = Silk.NET.Windowing.Window;

namespace DotRecast.Recast.Demo;

public class RecastDemo : IRecastDemoChannel
{
    private static readonly ILogger Logger = Log.ForContext<RecastDemo>();

    private IWindow window;
    private GL _gl;
    private IInputContext _input;
    private ImGuiController _imgui;
    private RcCanvas _canvas;

    private Vector2D<int> _resolution;
    private int width = 1000;
    private int height = 900;

    private readonly string title = "DotRecast Demo";

    //private readonly RecastDebugDraw dd;
    private NavMeshRenderer renderer;
    private float timeAcc = 0;
    private float camr = 1000;

    private readonly SoloNavMeshBuilder soloNavMeshBuilder = new SoloNavMeshBuilder();
    private readonly TileNavMeshBuilder tileNavMeshBuilder = new TileNavMeshBuilder();

    private string _lastGeomFileName;
    private DemoSample _sample;

    private bool processHitTest = false;
    private bool processHitTestShift;
    private int _modState;

    private RcVec2f mousePos = new RcVec2f();

    private bool _mouseOverMenu;
    private bool pan;
    private bool movedDuringPan;
    private bool rotate;
    private bool movedDuringRotate;
    private float scrollZoom;
    private RcVec2f origMousePos = new RcVec2f();
    private RcVec2f origCameraEulers = new RcVec2f();
    private RcVec3f origCameraPos = new RcVec3f();

    private RcVec2f cameraEulers = new RcVec2f(45, -45);
    private RcVec3f cameraPos = new RcVec3f(0, 0, 0);


    private float[] projectionMatrix = new float[16];
    private float[] modelviewMatrix = new float[16];

    private float _moveFront;
    private float _moveLeft;
    private float _moveBack;
    private float _moveRight;
    private float _moveUp;
    private float _moveDown;
    private float _moveAccel;

    private int[] viewport;
    private bool markerPositionSet;
    private RcVec3f markerPosition = new RcVec3f();

    private RcMenuView _menuView;
    private RcToolsetView _toolsetView;
    private RcSettingsView settingsView;
    private RcLogView logView;

    private long prevFrameTime;
    private RecastDebugDraw dd;
    private readonly Queue<IRecastDemoMessage> _messages;

    public RecastDemo()
    {
        _messages = new();
    }

    public void Run()
    {
        window = CreateWindow();
        window.Run();
    }

    public void OnMouseScrolled(IMouse mice, ScrollWheel scrollWheel)
    {
        if (scrollWheel.Y < 0)
        {
            // wheel down
            if (!_mouseOverMenu)
            {
                scrollZoom += 1.0f;
            }
        }
        else
        {
            if (!_mouseOverMenu)
            {
                scrollZoom -= 1.0f;
            }
        }

        var modelviewMatrix = dd.ViewMatrix(cameraPos, cameraEulers);
        cameraPos.X += scrollZoom * 2.0f * modelviewMatrix.M13;
        cameraPos.Y += scrollZoom * 2.0f * modelviewMatrix.M23;
        cameraPos.Z += scrollZoom * 2.0f * modelviewMatrix.M33 * (settingsView.RenderAsLeftHanded ? -1 : 1);
        scrollZoom = 0;
    }

    public void OnMouseMoved(IMouse mouse, Vector2 position)
    {
        mousePos.X = position.X;
        mousePos.Y = position.Y;
        int dx = (int)(mousePos.X - origMousePos.X);
        int dy = (int)(mousePos.Y - origMousePos.Y);
        if (rotate)
        {
            cameraEulers.X = origCameraEulers.X + dy * 0.25f;
            cameraEulers.Y = origCameraEulers.Y + dx * 0.25f;
            if (dx * dx + dy * dy > 3 * 3)
            {
                movedDuringRotate = true;
            }
        }

        if (pan)
        {
            var modelviewMatrix = dd.ViewMatrix(cameraPos, cameraEulers);
            cameraPos = origCameraPos;

            cameraPos.X -= 0.1f * dx * modelviewMatrix.M11;
            cameraPos.Y -= 0.1f * dx * modelviewMatrix.M21;
            cameraPos.Z -= 0.1f * dx * modelviewMatrix.M31 * (settingsView.RenderAsLeftHanded ? -1 : 1);

            cameraPos.X += 0.1f * dy * modelviewMatrix.M12;
            cameraPos.Y += 0.1f * dy * modelviewMatrix.M22;
            cameraPos.Z += 0.1f * dy * modelviewMatrix.M32 * (settingsView.RenderAsLeftHanded ? -1 : 1);
            if (dx * dx + dy * dy > 3 * 3)
            {
                movedDuringPan = true;
            }
        }
    }

    public void OnMouseUpAndDown(IMouse mouse, MouseButton button, bool down)
    {
        if (down)
        {
            if (button == MouseButton.Right)
            {
                if (!_mouseOverMenu)
                {
                    // Rotate view
                    rotate = true;
                    movedDuringRotate = false;
                    origMousePos = mousePos;
                    origCameraEulers = cameraEulers;
                }
            }
            else if (button == MouseButton.Middle)
            {
                if (!_mouseOverMenu)
                {
                    // Pan view
                    pan = true;
                    movedDuringPan = false;
                    origMousePos = mousePos;
                    origCameraPos = cameraPos;
                }
            }
        }
        else
        {
            // Handle mouse clicks here.
            if (button == MouseButton.Right)
            {
                rotate = false;
                if (!_mouseOverMenu)
                {
                    if (!movedDuringRotate)
                    {
                        processHitTest = true;
                        processHitTestShift = true;
                    }
                }
            }
            else if (button == MouseButton.Left)
            {
                if (!_mouseOverMenu)
                {
                    processHitTest = true;
                    processHitTestShift = 0 != (_modState & KeyModState.Shift);
                }
            }
            else if (button == MouseButton.Middle)
            {
                pan = false;
            }
        }
    }


    private IWindow CreateWindow()
    {
        var monitor = Window.Platforms.First().GetMainMonitor();
        _resolution = monitor.VideoMode.Resolution.Value;

        float aspect = 16.0f / 9.0f;
        width = Math.Min(_resolution.X, (int)(_resolution.Y * aspect)) - 100;
        height = _resolution.Y - 100;
        viewport = new int[] { 0, 0, width, height };

        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        options.Position = new Vector2D<int>((_resolution.X - width) / 2, (_resolution.Y - height) / 2);
        options.VSync = true;
        options.ShouldSwapAutomatically = false;
        options.PreferredDepthBufferBits = 24;
        window = Window.Create(options);

        if (window == null)
        {
            throw new Exception("Failed to create the GLFW window");
        }

        window.Closing += OnWindowClosing;
        window.Load += OnWindowOnLoad;
        window.Resize += OnWindowResize;
        window.FramebufferResize += OnWindowFramebufferSizeChanged;
        window.Update += OnWindowUpdate;
        window.Render += OnWindowRender;


        // // -- move somewhere else:
        // glfw.SetWindowPos(window, (mode->Width - width) / 2, (mode->Height - height) / 2);
        // // GlfwSetWindowMonitor(window.GetWindow(), monitor, 0, 0, mode.Width(), mode.Height(), mode.RefreshRate());
        // glfw.ShowWindow(window);
        // glfw.MakeContextCurrent(window);
        //}

        //glfw.SwapInterval(1);

        return window;
    }

    private DemoInputGeomProvider LoadInputMesh(string filename)
    {
        DemoInputGeomProvider geom = DemoInputGeomProvider.LoadFile(filename);
        _lastGeomFileName = filename;
        return geom;
    }

    private void LoadNavMesh(FileStream file, string filename)
    {
        try
        {
            DtNavMesh mesh = null;
            if (filename.EndsWith(".zip"))
            {
                UnityAStarPathfindingImporter importer = new UnityAStarPathfindingImporter();
                mesh = importer.Load(file)[0];
            }
            else
            {
                using var br = new BinaryReader(file);
                DtMeshSetReader reader = new DtMeshSetReader();
                mesh = reader.Read(br, 6);
            }

            if (null != mesh)
            {
                _sample.Update(_sample.GetInputGeom(), _sample.GetRecastConfig(), ImmutableArray<RcBuilderResult>.Empty, mesh);
                _toolsetView.SetEnabled(true);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "");
        }
    }

    private void OnWindowClosing()
    {
    }

    private void OnWindowResize(Vector2D<int> size)
    {
        width = size.X;
        height = size.Y;
    }

    private void OnWindowFramebufferSizeChanged(Vector2D<int> size)
    {
        _gl.Viewport(size);
        viewport = new int[] { 0, 0, width, height };
    }


    private void OnWindowOnLoad()
    {
        _input = window.CreateInput();

        // mouse input
        foreach (var mice in _input.Mice)
        {
            mice.Scroll += OnMouseScrolled;
            mice.MouseDown += (m, b) => OnMouseUpAndDown(m, b, true);
            mice.MouseUp += (m, b) => OnMouseUpAndDown(m, b, false);
            mice.MouseMove += OnMouseMoved;
        }

        _gl = window.CreateOpenGL();

        dd = new RecastDebugDraw(_gl);
        renderer = new NavMeshRenderer(dd);

        dd.Init(camr);


        var scale = (float)_resolution.X / 1920;
        int fontSize = Math.Max(10, (int)(16 * scale));

        // for windows : Microsoft Visual C++ Redistributable Package
        // link - https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist
        var imGuiFontConfig = new ImGuiFontConfig(Path.Combine("resources\\fonts", "DroidSans.ttf"), fontSize, null);
        _imgui = new ImGuiController(_gl, window, _input, imGuiFontConfig);

        ImGui.GetStyle().ScaleAllSizes(scale);
        //ImGui.GetIO().FontGlobalScale = 2.0f;

        DemoInputGeomProvider geom = LoadInputMesh("nav_test.obj");
        _sample = new DemoSample(geom, ImmutableArray<RcBuilderResult>.Empty, null);

        _menuView = new RcMenuView();
        settingsView = new RcSettingsView(this);
        settingsView.SetSample(_sample);

        _toolsetView = new RcToolsetView(
            new TestNavmeshSampleTool(),
            new TileSampleTool(),
            new ObstacleSampleTool(),
            new OffMeshConnectionSampleTool(),
            new ConvexVolumeSampleTool(),
            new CrowdSampleTool(),
            new CrowdAgentProfilingSampleTool(),
            new JumpLinkBuilderSampleTool(),
            new DynamicUpdateSampleTool()
        );
        _toolsetView.SetEnabled(true);
        logView = new RcLogView();

        _canvas = new RcCanvas(window, _menuView, settingsView, _toolsetView, logView);

        var vendor = _gl.GetStringS(GLEnum.Vendor);
        var version = _gl.GetStringS(GLEnum.Version);
        var rendererGl = _gl.GetStringS(GLEnum.Renderer);
        var glslString = _gl.GetStringS(GLEnum.ShadingLanguageVersion);
        var currentCulture = CultureInfo.CurrentCulture;
        string bitness = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";

        var workingDirectory = Directory.GetCurrentDirectory();
        Logger.Information($"Working directory - {workingDirectory}");
        Logger.Information($"OS Version - {Environment.OSVersion} {bitness}");
        Logger.Information($"{RuntimeInformation.OSArchitecture} {RuntimeInformation.OSDescription}");
        Logger.Information($"{RuntimeInformation.ProcessArchitecture} {RuntimeInformation.FrameworkDescription}");
        Logger.Information($"Dotnet - {Environment.Version.ToString()} culture({currentCulture.Name})");
        Logger.Information($"Processor Count : {Environment.ProcessorCount}");

        Logger.Information($"Server garbage collection : {(GCSettings.IsServerGC ? "Enabled" : "Disabled")}");
        Logger.Information($"Current latency mode for garbage collection: {GCSettings.LatencyMode}");
        Logger.Information("");

        Logger.Information($"ImGui.Net - version({ImGui.GetVersion()}) UI scale({scale}) fontSize({fontSize})");
        Logger.Information($"{vendor} {rendererGl}");
        Logger.Information($"gl version({version}) lang version({glslString})");
    }

    private float GetKeyValue(IKeyboard keyboard, Key primaryKey, Key secondaryKey)
    {
        return keyboard.IsKeyPressed(primaryKey) || keyboard.IsKeyPressed(secondaryKey) ? 1.0f : -1.0f;
    }

    private void UpdateKeyboard(float dt)
    {
        _modState = 0;

        // keyboard input
        foreach (var keyboard in _input.Keyboards)
        {
            var tempMoveFront = GetKeyValue(keyboard, Key.W, Key.Up);
            var tempMoveLeft = GetKeyValue(keyboard, Key.A, Key.Left);
            var tempMoveBack = GetKeyValue(keyboard, Key.S, Key.Down);
            var tempMoveRight = GetKeyValue(keyboard, Key.D, Key.Right);
            var tempMoveUp = GetKeyValue(keyboard, Key.Q, Key.PageUp);
            var tempMoveDown = GetKeyValue(keyboard, Key.E, Key.PageDown);
            var tempMoveAccel = GetKeyValue(keyboard, Key.ShiftLeft, Key.ShiftRight);
            var tempControl = GetKeyValue(keyboard, Key.ControlLeft, Key.ControlRight);

            _modState |= 0 < tempControl ? KeyModState.Control : KeyModState.None;
            _modState |= 0 < tempMoveAccel ? KeyModState.Shift : KeyModState.None;

            //Logger.Information($"{_modState}");
            _moveFront = Math.Clamp(_moveFront + tempMoveFront * dt * 4.0f, 0, 2.0f);
            _moveLeft = Math.Clamp(_moveLeft + tempMoveLeft * dt * 4.0f, 0, 2.0f);
            _moveBack = Math.Clamp(_moveBack + tempMoveBack * dt * 4.0f, 0, 2.0f);
            _moveRight = Math.Clamp(_moveRight + tempMoveRight * dt * 4.0f, 0, 2.0f);
            _moveUp = Math.Clamp(_moveUp + tempMoveUp * dt * 4.0f, 0, 2.0f);
            _moveDown = Math.Clamp(_moveDown + tempMoveDown * dt * 4.0f, 0, 2.0f);
            _moveAccel = Math.Clamp(_moveAccel + tempMoveAccel * dt * 4.0f, 0, 2.0f);
        }
    }

    private void OnWindowUpdate(double dt)
    {
        /*
         * try (MemoryStack stack = StackPush()) { int[] w = stack.MallocInt(1); int[] h =
         * stack.MallocInt(1); GlfwGetWindowSize(win, w, h); width = w.x; height = h.x; }
         */
        if (_sample.GetInputGeom() != null)
        {
            var settings = _sample.GetSettings();
            RcVec3f bmin = _sample.GetInputGeom().GetMeshBoundsMin();
            RcVec3f bmax = _sample.GetInputGeom().GetMeshBoundsMax();
            RcRecast.CalcGridSize(bmin, bmax, settings.cellSize, out var gw, out var gh);
            settingsView.SetVoxels(gw, gh);
            settingsView.SetTiles(tileNavMeshBuilder.GetTiles(_sample.GetInputGeom(), settings.cellSize, settings.tileSize));
            settingsView.SetMaxTiles(tileNavMeshBuilder.GetMaxTiles(_sample.GetInputGeom(), settings.cellSize, settings.tileSize));
            settingsView.SetMaxPolys(tileNavMeshBuilder.GetMaxPolysPerTile(_sample.GetInputGeom(), settings.cellSize, settings.tileSize));
        }

        //if ()

        UpdateKeyboard((float)dt);

        // camera move
        float keySpeed = 22.0f;
        if (0 < _moveAccel)
        {
            keySpeed *= _moveAccel * 2.0f;
        }

        double movex = (_moveRight - _moveLeft) * keySpeed * dt;
        double movey = (_moveBack - _moveFront) * keySpeed * dt + scrollZoom * 2.0f;
        scrollZoom = 0;

        cameraPos.X += (float)(movex * modelviewMatrix[0]);
        cameraPos.Y += (float)(movex * modelviewMatrix[4]);
        cameraPos.Z += (float)(movex * modelviewMatrix[8]) * (settingsView.RenderAsLeftHanded ? -1 : 1);

        cameraPos.X += (float)(movey * modelviewMatrix[2]);
        cameraPos.Y += (float)(movey * modelviewMatrix[6]);
        cameraPos.Z += (float)(movey * modelviewMatrix[10]) * (settingsView.RenderAsLeftHanded ? -1 : 1);

        cameraPos.Y += (float)((_moveUp - _moveDown) * keySpeed * dt);

        long time = RcFrequency.Ticks;
        prevFrameTime = time;

        // Update sample simulation.
        float SIM_RATE = 20;
        float DELTA_TIME = 1.0f / SIM_RATE;
        timeAcc = Math.Clamp((float)(timeAcc + dt), -1.0f, 1.0f);
        int simIter = 0;
        while (timeAcc > DELTA_TIME)
        {
            timeAcc -= DELTA_TIME;
            if (simIter < 5 && _sample != null)
            {
                var tool = _toolsetView.GetTool();
                if (null != tool)
                {
                    tool.HandleUpdate(DELTA_TIME);
                }
            }

            simIter++;
        }

        if (processHitTest)
        {
            processHitTest = false;

            RcVec3f rayStart = new RcVec3f();
            RcVec3f rayEnd = new RcVec3f();

            GLU.GlhUnProjectf(mousePos.X, viewport[3] - 1 - mousePos.Y, 0.0f, modelviewMatrix, projectionMatrix, viewport, ref rayStart);
            GLU.GlhUnProjectf(mousePos.X, viewport[3] - 1 - mousePos.Y, 1.0f, modelviewMatrix, projectionMatrix, viewport, ref rayEnd);

            SendMessage(new RaycastEvent()
            {
                Start = rayStart,
                End = rayEnd,
            });
        }

        if (_sample.IsChanged())
        {
            bool hasBound = false;
            RcVec3f bminN = RcVec3f.Zero;
            RcVec3f bmaxN = RcVec3f.Zero;

            if (_sample.GetInputGeom() != null)
            {
                bminN = _sample.GetInputGeom().GetMeshBoundsMin();
                bmaxN = _sample.GetInputGeom().GetMeshBoundsMax();
                hasBound = true;
            }
            else if (_sample.GetNavMesh() != null)
            {
                _sample.GetNavMesh().ComputeBounds(out bminN, out bmaxN);
                hasBound = true;
            }
            else if (0 < _sample.GetRecastResults().Count)
            {
                foreach (RcBuilderResult result in _sample.GetRecastResults())
                {
                    if (result.CompactHeightfield != null)
                    {
                        if (!hasBound)
                        {
                            bminN = new RcVec3f(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                            bmaxN = new RcVec3f(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                        }

                        bminN = new RcVec3f(
                            Math.Min(bminN.X, result.CompactHeightfield.bmin.X),
                            Math.Min(bminN.Y, result.CompactHeightfield.bmin.Y),
                            Math.Min(bminN.Z, result.CompactHeightfield.bmin.Z)
                        );

                        bmaxN = new RcVec3f(
                            Math.Max(bmaxN.X, result.CompactHeightfield.bmax.X),
                            Math.Max(bmaxN.Y, result.CompactHeightfield.bmax.Y),
                            Math.Max(bmaxN.Z, result.CompactHeightfield.bmax.Z)
                        );

                        hasBound = true;
                    }
                }
            }

            // Reset camera and fog to match the mesh bounds.
            if (hasBound)
            {
                RcVec3f bmin = bminN;
                RcVec3f bmax = bmaxN;

                camr = (float)(Math.Sqrt(RcMath.Sqr(bmax.X - bmin.X) +
                                         RcMath.Sqr(bmax.Y - bmin.Y) +
                                         RcMath.Sqr(bmax.Z - bmin.Z)) / 2);
                cameraPos.X = (bmax.X + bmin.X) / 2 + camr;
                cameraPos.Y = (bmax.Y + bmin.Y) / 2 + camr;
                cameraPos.Z = (bmax.Z + bmin.Z) / 2 + camr;
                camr *= 5;
                cameraEulers.X = 45;
                cameraEulers.Y = -45;
            }

            _sample.SetChanged(false);
            _toolsetView.SetSample(_sample);
        }

        if (_messages.TryDequeue(out var msg))
        {
            OnMessage(msg);
        }


        var io = ImGui.GetIO();

        io.DisplaySize = new Vector2(width, height);
        io.DisplayFramebufferScale = Vector2.One;
        io.DeltaTime = (float)dt;

        _canvas.Update(dt);
        _imgui.Update((float)dt);
    }

    private void OnWindowRender(double dt)
    {
        // Clear the screen
        dd.SetAsRenderLeftHanded(settingsView.RenderAsLeftHanded);
        dd.Clear();
        dd.ProjectionMatrix(50f, (float)width / (float)height, 1.0f, camr).CopyTo(projectionMatrix);
        dd.ViewMatrix(cameraPos, cameraEulers).CopyTo(modelviewMatrix);

        dd.Fog(camr * 0.1f, camr * 1.25f);
        renderer.Render(_sample, settingsView.GetDrawMode());

        ISampleTool sampleTool = _toolsetView.GetTool();
        if (sampleTool != null)
        {
            sampleTool.HandleRender(renderer);
        }

        dd.Fog(false);

        _canvas.Draw(dt);
        _mouseOverMenu = ImGui.GetIO().WantCaptureMouse;

        _imgui.Render();

        window.SwapBuffers();
    }

    public void SendMessage(IRecastDemoMessage message)
    {
        _messages.Enqueue(message);
    }

    private void OnMessage(IRecastDemoMessage message)
    {
        if (message is GeomLoadBeganEvent args)
        {
            OnGeomLoadBegan(args);
        }
        else if (message is NavMeshBuildBeganEvent args2)
        {
            OnNavMeshBuildBegan(args2);
        }
        else if (message is NavMeshSaveBeganEvent args3)
        {
            OnNavMeshSaveBegan(args3);
        }
        else if (message is NavMeshLoadBeganEvent args4)
        {
            OnNavMeshLoadBegan(args4);
        }
        else if (message is RaycastEvent args5)
        {
            OnRaycast(args5);
        }
    }

    private void OnGeomLoadBegan(GeomLoadBeganEvent args)
    {
        var geom = LoadInputMesh(args.FilePath);

        _sample.Update(geom, null, ImmutableArray<RcBuilderResult>.Empty, null);
    }

    private void OnNavMeshBuildBegan(NavMeshBuildBeganEvent args)
    {
        if (null == _sample.GetInputGeom())
        {
            Logger.Information($"not found source geom");
            return;
        }


        long t = RcFrequency.Ticks;

        Logger.Information($"build");

        NavMeshBuildResult buildResult;

        var geom = _sample.GetInputGeom();
        var settings = _sample.GetSettings();
        if (settings.tiled)
        {
            buildResult = tileNavMeshBuilder.Build(geom, settings);
        }
        else
        {
            buildResult = soloNavMeshBuilder.Build(geom, settings);
        }

        if (!buildResult.Success)
        {
            Logger.Error("failed to build");
            return;
        }

        _sample.Update(_sample.GetInputGeom(), buildResult.Cfg, buildResult.RecastBuilderResults, buildResult.NavMesh);
        _sample.SetChanged(false);
        settingsView.SetBuildTime((RcFrequency.Ticks - t) / TimeSpan.TicksPerMillisecond);
        //settingsUI.SetBuildTelemetry(buildResult.Item1.Select(x => x.GetTelemetry()).ToList());
        _toolsetView.SetSample(_sample);

        Logger.Information($"build times");
        Logger.Information($"-----------------------------------------");
        var telemetries = buildResult.RecastBuilderResults
            .Select(x => x.Context)
            .SelectMany(x => x.ToList())
            .GroupBy(x => x.Key)
            .ToImmutableSortedDictionary(x => x.Key, x => x.Sum(y => y.Millis));

        foreach (var (key, millis) in telemetries)
        {
            Logger.Information($"{key}: {millis} ms");
        }
    }

    private void OnNavMeshSaveBegan(NavMeshSaveBeganEvent args)
    {
        var navMesh = _sample.GetNavMesh();
        if (null == navMesh)
        {
            Logger.Error("navmesh is null");
            return;
        }

        DateTime now = DateTime.Now;
        string ymdhms = $"{now:yyyyMMdd_HHmmss}";
        var filename = Path.GetFileNameWithoutExtension(_lastGeomFileName);
        var navmeshFilePath = $"{filename}_{ymdhms}.navmesh";

        using var fs = new FileStream(navmeshFilePath, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        var writer = new DtMeshSetWriter();
        writer.Write(bw, navMesh, RcByteOrder.LITTLE_ENDIAN, true);
        Logger.Information($"saved navmesh - {navmeshFilePath}");
    }

    private void OnNavMeshLoadBegan(NavMeshLoadBeganEvent args)
    {
        if (string.IsNullOrEmpty(args.FilePath))
        {
            Logger.Error("file path is empty");
            return;
        }

        if (!File.Exists(args.FilePath))
        {
            Logger.Error($"not found navmesh file - {args.FilePath}");
            return;
        }

        try
        {
            using FileStream fs = new FileStream(args.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            LoadNavMesh(fs, args.FilePath);
        }
        catch (Exception e)
        {
            Logger.Error(e, "");
        }
    }

    private void OnRaycast(RaycastEvent args)
    {
        var rayStart = args.Start;
        var rayEnd = args.End;

        // Hit test mesh.
        DemoInputGeomProvider inputGeom = _sample.GetInputGeom();
        if (_sample == null)
            return;

        float hitTime = 0.0f;
        bool hit = false;
        if (inputGeom != null)
        {
            hit = inputGeom.RaycastMesh(rayStart, rayEnd, out hitTime);
        }

        if (!hit && _sample.GetNavMesh() != null)
        {
            hit = DtNavMeshRaycast.Raycast(_sample.GetNavMesh(), rayStart, rayEnd, out hitTime);
        }

        if (!hit && _sample.GetRecastResults() != null)
        {
            hit = RcPolyMeshRaycast.Raycast(_sample.GetRecastResults(), rayStart, rayEnd, out hitTime);
        }

        RcVec3f rayDir = new RcVec3f(rayEnd.X - rayStart.X, rayEnd.Y - rayStart.Y, rayEnd.Z - rayStart.Z);
        rayDir = RcVec3f.Normalize(rayDir);

        ISampleTool raySampleTool = _toolsetView.GetTool();

        if (raySampleTool != null)
        {
            Logger.Information($"click ray - tool({raySampleTool.GetTool().GetName()}) rayStart({rayStart.X:0.#},{rayStart.Y:0.#},{rayStart.Z:0.#}) pos({rayDir.X:0.#},{rayDir.Y:0.#},{rayDir.Z:0.#}) shift({processHitTestShift})");
            raySampleTool.HandleClickRay(rayStart, rayDir, processHitTestShift);
        }

        if (hit)
        {
            if (0 != (_modState & KeyModState.Control))
            {
                // Marker
                markerPositionSet = true;
                markerPosition.X = rayStart.X + (rayEnd.X - rayStart.X) * hitTime;
                markerPosition.Y = rayStart.Y + (rayEnd.Y - rayStart.Y) * hitTime;
                markerPosition.Z = rayStart.Z + (rayEnd.Z - rayStart.Z) * hitTime;
            }
            else
            {
                RcVec3f pos = new RcVec3f();
                pos.X = rayStart.X + (rayEnd.X - rayStart.X) * hitTime;
                pos.Y = rayStart.Y + (rayEnd.Y - rayStart.Y) * hitTime;
                pos.Z = rayStart.Z + (rayEnd.Z - rayStart.Z) * hitTime;
                if (raySampleTool != null)
                {
                    Logger.Information($"click - tool({raySampleTool.GetTool().GetName()}) rayStart({rayStart.X:0.#},{rayStart.Y:0.#},{rayStart.Z:0.#}) pos({pos.X:0.#},{pos.Y:0.#},{pos.Z:0.#}) shift({processHitTestShift})");
                    raySampleTool.HandleClick(rayStart, pos, processHitTestShift);
                }
            }
        }
        else
        {
            if (0 != (_modState & KeyModState.Control))
            {
                // Marker
                markerPositionSet = false;
            }
        }
    }
}