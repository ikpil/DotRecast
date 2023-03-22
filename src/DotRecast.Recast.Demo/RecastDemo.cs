/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using ImGuiNET;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Extras.Unity.Astar;
using DotRecast.Detour.Io;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;
using DotRecast.Recast.Demo.Settings;
using DotRecast.Recast.Demo.Tools;
using DotRecast.Recast.Demo.UI;
using Silk.NET.SDL;
using Silk.NET.Windowing.Sdl;
using static DotRecast.Detour.DetourCommon;
using Color = System.Drawing.Color;
using Window = Silk.NET.Windowing.Window;

namespace DotRecast.Recast.Demo;

public class RecastDemo
{
    private static readonly ILogger Logger = Log.ForContext<RecastDemo>();

    private RcViewSystem _viewSys;
    private IWindow window;
    private IInputContext _input;
    private ImGuiController _imgui;
    private GL _gl;
    private Sdl _sdl;
    private int width = 1000;
    private int height = 900;

    private readonly string title = "DotRecast Demo";

    //private readonly RecastDebugDraw dd;
    private NavMeshRenderer renderer;
    private bool building = false;
    private float timeAcc = 0;
    private float camr = 1000;

    private readonly SoloNavMeshBuilder soloNavMeshBuilder = new SoloNavMeshBuilder();
    private readonly TileNavMeshBuilder tileNavMeshBuilder = new TileNavMeshBuilder();

    private Sample sample;

    private bool processHitTest = false;
    private bool processHitTestShift;
    private int modState;

    private readonly float[] mousePos = new float[2];

    private bool mouseOverMenu;
    private bool pan;
    private bool movedDuringPan;
    private bool rotate;
    private bool movedDuringRotate;
    private float scrollZoom;
    private readonly float[] origMousePos = new float[2];
    private readonly float[] origCameraEulers = new float[2];
    private readonly float[] origCameraPos = new float[3];

    private readonly float[] cameraEulers = { 45, -45 };
    private readonly float[] cameraPos = { 0, 0, 0 };

    private readonly float[] rayStart = new float[3];
    private readonly float[] rayEnd = new float[3];

    private float[] projectionMatrix = new float[16];
    private float[] modelviewMatrix = new float[16];

    private float _moveFront;
    private float _moveLeft;
    private float _moveBack;
    private float _moveRight;
    private float _moveUp;
    private float _moveDown;
    private float _moveAccel;
    
    private bool markerPositionSet;
    private readonly float[] markerPosition = new float[3];
    private ToolsView toolsUI;
    private RcSettingsView settingsUI;
    private long prevFrameTime;
    private RecastDebugDraw dd;

    public RecastDemo()
    {
    }

    public void start()
    {
        window = CreateWindow();
        window.Run();
    }

    public void OnMouseScrolled(IMouse mice, ScrollWheel scrollWheel)
    {
        if (scrollWheel.Y < 0)
        {
            // wheel down
            // if (!mouseOverMenu)
            // {
            scrollZoom += 1.0f;
            //}
        }
        else
        {
            // if (!mouseOverMenu)
            // {
            scrollZoom -= 1.0f;
            //}
        }

        float[] modelviewMatrix = dd.viewMatrix(cameraPos, cameraEulers);
        cameraPos[0] += scrollZoom * 2.0f * modelviewMatrix[2];
        cameraPos[1] += scrollZoom * 2.0f * modelviewMatrix[6];
        cameraPos[2] += scrollZoom * 2.0f * modelviewMatrix[10];
        scrollZoom = 0;
    }

    public void OnMouseMoved(IMouse mouse, Vector2 position)
    {
        mousePos[0] = (float)position.X;
        mousePos[1] = (float)position.Y;
        int dx = (int)(mousePos[0] - origMousePos[0]);
        int dy = (int)(mousePos[1] - origMousePos[1]);
        if (rotate)
        {
            cameraEulers[0] = origCameraEulers[0] + dy * 0.25f;
            cameraEulers[1] = origCameraEulers[1] + dx * 0.25f;
            if (dx * dx + dy * dy > 3 * 3)
            {
                movedDuringRotate = true;
            }
        }

        if (pan)
        {
            float[] modelviewMatrix = dd.viewMatrix(cameraPos, cameraEulers);
            cameraPos[0] = origCameraPos[0];
            cameraPos[1] = origCameraPos[1];
            cameraPos[2] = origCameraPos[2];

            cameraPos[0] -= 0.1f * dx * modelviewMatrix[0];
            cameraPos[1] -= 0.1f * dx * modelviewMatrix[4];
            cameraPos[2] -= 0.1f * dx * modelviewMatrix[8];

            cameraPos[0] += 0.1f * dy * modelviewMatrix[1];
            cameraPos[1] += 0.1f * dy * modelviewMatrix[5];
            cameraPos[2] += 0.1f * dy * modelviewMatrix[9];
            if (dx * dx + dy * dy > 3 * 3)
            {
                movedDuringPan = true;
            }
        }
    }

    public void OnMouseUpAndDown(IMouse mouse, MouseButton button, bool down)
    {
        modState = 0;
        if (down)
        {
            if (button == MouseButton.Right)
            {
                // if (!mouseOverMenu)
                // {
                // Rotate view
                rotate = true;
                movedDuringRotate = false;
                origMousePos[0] = mousePos[0];
                origMousePos[1] = mousePos[1];
                origCameraEulers[0] = cameraEulers[0];
                origCameraEulers[1] = cameraEulers[1];
                //}
            }
            else if (button == MouseButton.Middle)
            {
                // if (!mouseOverMenu)
                // {
                // Pan view
                pan = true;
                movedDuringPan = false;
                origMousePos[0] = mousePos[0];
                origMousePos[1] = mousePos[1];
                origCameraPos[0] = cameraPos[0];
                origCameraPos[1] = cameraPos[1];
                origCameraPos[2] = cameraPos[2];
                //}
            }
        }
        else
        {
            // Handle mouse clicks here.
            if (button == MouseButton.Right)
            {
                rotate = false;
                // if (!mouseOverMenu)
                // {
                if (!movedDuringRotate)
                {
                    processHitTest = true;
                    processHitTestShift = true;
                }
                //}
            }
            else if (button == MouseButton.Left)
            {
                // if (!mouseOverMenu)
                // {
                processHitTest = true;
                //processHitTestShift = (mods & Keys.GLFW_MOD_SHIFT) != 0 ? true : false;
                //processHitTestShift = (mods & Keys.) != 0 ? true : false;
                //}
            }
            else if (button == MouseButton.Middle)
            {
                pan = false;
            }
        }
    }


    private IWindow CreateWindow()
    {
        SdlWindowing.Use();
        //var glfw = GlfwProvider.GLFW.Value;

        // glfw.SetErrorCallback(ErrorCallback);
        // if (!glfw.Init())
        // {
        //     throw new InvalidOperationException("Unable to initialize GLFW");
        // }

        // glfw.DefaultWindowHints();
        // glfw.WindowHint(WindowHintBool.Visible, false);
        // glfw.WindowHint(WindowHintBool.Resizable, false);
        // glfw.WindowHint(WindowHintBool.SrgbCapable, true);
        // glfw.WindowHint(WindowHintInt.RedBits, 8);
        // glfw.WindowHint(WindowHintInt.GreenBits, 8);
        // glfw.WindowHint(WindowHintInt.BlueBits, 8);
        // glfw.WindowHint(WindowHintInt.Samples, 4);
        // glfw.WindowHint(WindowHintBool.DoubleBuffer, true);
        // glfw.WindowHint(WindowHintInt.DepthBits, 24);
        //
        // glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
        // glfw.WindowHint(WindowHintInt.ContextVersionMinor, 2);
        // glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
        // glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
        // glfw.WindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
        // glfw.WindowHint(GLFW_CONTEXT_VERSION_MINOR, 1);

        var monitor = Window.Platforms.First().GetMainMonitor();
        // // if (monitors.limit() > 1) {
        // // monitor = monitors[1];
        // // }
        var resolution = monitor.VideoMode.Resolution.Value;

        float aspect = 16.0f / 9.0f;
        width = Math.Min(resolution.X, (int)(resolution.Y * aspect)) - 100;
        height = resolution.Y - 100;

        // glfwWindowHint(GLFW_RED_BITS, mode.redBits());
        // glfwWindowHint(GLFW_GREEN_BITS, mode.greenBits());
        // glfwWindowHint(GLFW_BLUE_BITS, mode.blueBits());
        // glfwWindowHint(GLFW_REFRESH_RATE, mode.refreshRate());

        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        options.Position = new Vector2D<int>((resolution.X - width) / 2, (resolution.Y - height) / 2);
        options.VSync = false;
        options.ShouldSwapAutomatically = false;
        window = Window.Create(options);

        if (window == null)
        {
            throw new Exception("Failed to create the GLFW window");
        }

        window.Closing += OnWindowClosing;
        window.Load += OnWindowOnLoad;
        window.Resize += OnWindowResize;
        window.FramebufferResize += OnWindowFramebufferSizeChanged;
        window.Update += OnWindowOnUpdate;
        window.Render += OnWindowOnRender;


        // // -- move somewhere else:
        // glfw.SetWindowPos(window, (mode->Width - width) / 2, (mode->Height - height) / 2);
        // // glfwSetWindowMonitor(window.getWindow(), monitor, 0, 0, mode.width(), mode.height(), mode.refreshRate());
        // glfw.ShowWindow(window);
        // glfw.MakeContextCurrent(window);
        //}

        //glfw.SwapInterval(1);

        return window;
    }

    private DemoInputGeomProvider loadInputMesh(byte[] stream)
    {
        DemoInputGeomProvider geom = DemoObjImporter.load(stream);
        sample = new Sample(geom, ImmutableArray<RecastBuilderResult>.Empty, null, settingsUI, dd);
        toolsUI.setEnabled(true);
        return geom;
    }

    private void loadNavMesh(FileStream file, string filename)
    {
        NavMesh mesh = null;
        if (filename.EndsWith(".zip") || filename.EndsWith(".bytes"))
        {
            UnityAStarPathfindingImporter importer = new UnityAStarPathfindingImporter();
            mesh = importer.load(file)[0];
        }
        else if (filename.EndsWith(".bin") || filename.EndsWith(".navmesh"))
        {
            MeshSetReader reader = new MeshSetReader();
            using (var fis = new BinaryReader(file))
            {
                mesh = reader.read(fis, 6);
            }
        }

        if (mesh != null)
        {
            //sample = new Sample(null, ImmutableArray<RecastBuilderResult>.Empty, mesh, settingsUI, dd);
            toolsUI.setEnabled(true);
        }
    }

    private void OnWindowClosing()
    {
    }

    private void OnWindowResize(Vector2D<int> size)
    {
        width = size.X;
        height = size.Y;

        //_gl.FramebufferParameter(GLEnum.Size);
        //_graphicsDevice.ResizeMainWindow((uint)size.X, (uint)size.Y);
    }

    private void OnWindowFramebufferSizeChanged(Vector2D<int> size)
    {
        _gl.Viewport(size);
    }


    private void OnWindowOnLoad()
    {
        _sdl = SdlWindowing.GetExistingApi(window);

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

        dd.init(camr);

        _imgui = new ImGuiController(_gl, window, _input);


        // // if (capabilities.OpenGL43) {
        // // GL43.glDebugMessageControl(GL43.GL_DEBUG_SOURCE_API, GL43.GL_DEBUG_TYPE_OTHER,
        // // GL43.GL_DEBUG_SEVERITY_NOTIFICATION,
        // // (int[]) null, false);
        // // } else if (capabilities.GL_ARB_debug_output) {
        // // ARBDebugOutput.glDebugMessageControlARB(ARBDebugOutput.GL_DEBUG_SOURCE_API_ARB,
        // // ARBDebugOutput.GL_DEBUG_TYPE_OTHER_ARB, ARBDebugOutput.GL_DEBUG_SEVERITY_LOW_ARB, (int[]) null, false);
        // // }
        var vendor = _gl.GetStringS(GLEnum.Vendor);
        Logger.Debug(vendor);

        var version = _gl.GetStringS(GLEnum.Version);
        Logger.Debug(version);

        var renderGl = _gl.GetStringS(GLEnum.Renderer);
        Logger.Debug(renderGl);

        var glslString = _gl.GetStringS(GLEnum.ShadingLanguageVersion);
        Logger.Debug(glslString);

        settingsUI = new RcSettingsView();
        toolsUI = new ToolsView(
            new TestNavmeshTool(),
            new OffMeshConnectionTool(),
            new ConvexVolumeTool(),
            new CrowdTool(),
            new JumpLinkBuilderTool(),
            new DynamicUpdateTool()
        );

        _viewSys = new RcViewSystem(window, _input, settingsUI, toolsUI);

        DemoInputGeomProvider geom = loadInputMesh(Loader.ToBytes("nav_test.obj"));
        sample = new Sample(geom, ImmutableArray<RecastBuilderResult>.Empty, null, settingsUI, dd);
    }

    private void UpdateKeyboard(float dt)
    {
        // keyboard input
        foreach (var keyboard in _input.Keyboards)
        {
            var tempMoveFront = keyboard.IsKeyPressed(Key.W) || keyboard.IsKeyPressed(Key.Up) ? 1.0f : -1f;
            var tempMoveLeft = keyboard.IsKeyPressed(Key.A) || keyboard.IsKeyPressed(Key.Left) ? 1.0f : -1f;
            var tempMoveBack = keyboard.IsKeyPressed(Key.S) || keyboard.IsKeyPressed(Key.Down) ? 1.0f : -1f;
            var tempMoveRight = keyboard.IsKeyPressed(Key.D) || keyboard.IsKeyPressed(Key.Right) ? 1.0f : -1f;
            var tempMoveUp = keyboard.IsKeyPressed(Key.Q) || keyboard.IsKeyPressed(Key.PageUp) ? 1.0f : -1f;
            var tempMoveDown= keyboard.IsKeyPressed(Key.E) || keyboard.IsKeyPressed(Key.PageDown) ? 1.0f : -1f;
            var tempMoveAccel= keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight) ? 1.0f : -1f;

            _moveFront = DemoMath.clamp(_moveFront + tempMoveFront * dt * 4.0f, 0, 2.0f);
            _moveLeft = DemoMath.clamp(_moveLeft + tempMoveLeft * dt * 4.0f, 0, 2.0f);
            _moveBack = DemoMath.clamp(_moveBack + tempMoveBack * dt * 4.0f, 0, 2.0f);
            _moveRight = DemoMath.clamp(_moveRight + tempMoveRight * dt * 4.0f, 0, 2.0f);
            _moveUp = DemoMath.clamp(_moveUp + tempMoveUp * dt * 4.0f, 0, 2.0f);
            _moveDown = DemoMath.clamp(_moveDown + tempMoveDown * dt * 4.0f, 0, 2.0f);
            _moveAccel = DemoMath.clamp(_moveAccel + tempMoveAccel * dt * 4.0f, 0, 2.0f);
        }
    }

    private void OnWindowOnUpdate(double dt)
    {
        /*
          * try (MemoryStack stack = stackPush()) { int[] w = stack.mallocInt(1); int[] h =
          * stack.mallocInt(1); glfwGetWindowSize(win, w, h); width = w[0]; height = h[0]; }
       */
        if (sample.getInputGeom() != null)
        {
            float[] bmin = sample.getInputGeom().getMeshBoundsMin();
            float[] bmax = sample.getInputGeom().getMeshBoundsMax();
            int[] voxels = Recast.calcGridSize(bmin, bmax, settingsUI.getCellSize());
            settingsUI.setVoxels(voxels);
            settingsUI.setTiles(tileNavMeshBuilder.getTiles(sample.getInputGeom(), settingsUI.getCellSize(), settingsUI.getTileSize()));
            settingsUI.setMaxTiles(tileNavMeshBuilder.getMaxTiles(sample.getInputGeom(), settingsUI.getCellSize(), settingsUI.getTileSize()));
            settingsUI.setMaxPolys(tileNavMeshBuilder.getMaxPolysPerTile(sample.getInputGeom(), settingsUI.getCellSize(), settingsUI.getTileSize()));
        }

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

        cameraPos[0] += (float)(movex * modelviewMatrix[0]);
        cameraPos[1] += (float)(movex * modelviewMatrix[4]);
        cameraPos[2] += (float)(movex * modelviewMatrix[8]);

        cameraPos[0] += (float)(movey * modelviewMatrix[2]);
        cameraPos[1] += (float)(movey * modelviewMatrix[6]);
        cameraPos[2] += (float)(movey * modelviewMatrix[10]);

        cameraPos[1] += (float)((_moveUp - _moveDown) * keySpeed * dt);

        // long time = Stopwatch.GetTimestamp() / 1000;
        // //float dt = (time - prevFrameTime) / 1000000.0f;
        // prevFrameTime = time;
        //
        // // Update sample simulation.
        // float SIM_RATE = 20;
        // float DELTA_TIME = 1.0f / SIM_RATE;
        // timeAcc = clamp((float)(timeAcc + dt), -1.0f, 1.0f);
        // int simIter = 0;
        // while (timeAcc > DELTA_TIME)
        // {
        //     timeAcc -= DELTA_TIME;
        //     if (simIter < 5 && sample != null)
        //     {
        //         toolsUI.handleUpdate((float)dt);
        //     }
        //
        //     simIter++;
        // }

        // if (settingsUI.isMeshInputTrigerred())
        // {
        // aFilterPatterns.put(stack.UTF8("*.obj"));
        // aFilterPatterns.flip();
        // string filename = TinyFileDialogs.tinyfd_openFileDialog("Open Mesh File", "", aFilterPatterns,
        //     "Mesh File (*.obj)", false);
        // if (filename != null) {
        //     try (InputStream stream = new FileInputStream(filename)) {
        //         sample.update(loadInputMesh(stream), null, null);
        //     } catch (IOException e) {
        //         Console.WriteLine(e).printStackTrace();
        //     }
        // }
        // }
        // else if (settingsUI.isNavMeshInputTrigerred())
        // {
        // try (MemoryStack stack = stackPush()) {
        //     PointerBuffer aFilterPatterns = stack.mallocPointer(4);
        //     aFilterPatterns.put(stack.UTF8("*.bin"));
        //     aFilterPatterns.put(stack.UTF8("*.zip"));
        //     aFilterPatterns.put(stack.UTF8("*.bytes"));
        //     aFilterPatterns.put(stack.UTF8("*.navmesh"));
        //     aFilterPatterns.flip();
        //     string filename = TinyFileDialogs.tinyfd_openFileDialog("Open Nav Mesh File", "", aFilterPatterns,
        //         "Nav Mesh File", false);
        //     if (filename != null) {
        //         File file = new File(filename);
        //         if (file.exists()) {
        //             try {
        //                 loadNavMesh(file, filename);
        //                 geom = null;
        //             } catch (Exception e) {
        //                 Console.WriteLine(e);
        //             }
        //         }
        //     }
        // }
        // }
        if (settingsUI.isBuildTriggered() && sample.getInputGeom() != null)
        {
            if (!building)
            {
                float m_cellSize = settingsUI.getCellSize();
                float m_cellHeight = settingsUI.getCellHeight();
                float m_agentHeight = settingsUI.getAgentHeight();
                float m_agentRadius = settingsUI.getAgentRadius();
                float m_agentMaxClimb = settingsUI.getAgentMaxClimb();
                float m_agentMaxSlope = settingsUI.getAgentMaxSlope();
                int m_regionMinSize = settingsUI.getMinRegionSize();
                int m_regionMergeSize = settingsUI.getMergedRegionSize();
                float m_edgeMaxLen = settingsUI.getEdgeMaxLen();
                float m_edgeMaxError = settingsUI.getEdgeMaxError();
                int m_vertsPerPoly = settingsUI.getVertsPerPoly();
                float m_detailSampleDist = settingsUI.getDetailSampleDist();
                float m_detailSampleMaxError = settingsUI.getDetailSampleMaxError();
                int m_tileSize = settingsUI.getTileSize();
                long t = Stopwatch.GetTimestamp();

                Tuple<IList<RecastBuilderResult>, NavMesh> buildResult;
                if (settingsUI.isTiled())
                {
                    buildResult = tileNavMeshBuilder.build(sample.getInputGeom(), settingsUI.getPartitioning(), m_cellSize,
                        m_cellHeight, m_agentHeight, m_agentRadius, m_agentMaxClimb, m_agentMaxSlope, m_regionMinSize,
                        m_regionMergeSize, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly, m_detailSampleDist,
                        m_detailSampleMaxError, settingsUI.isFilterLowHangingObstacles(), settingsUI.isFilterLedgeSpans(),
                        settingsUI.isFilterWalkableLowHeightSpans(), m_tileSize);
                }
                else
                {
                    buildResult = soloNavMeshBuilder.build(sample.getInputGeom(), settingsUI.getPartitioning(), m_cellSize,
                        m_cellHeight, m_agentHeight, m_agentRadius, m_agentMaxClimb, m_agentMaxSlope, m_regionMinSize,
                        m_regionMergeSize, m_edgeMaxLen, m_edgeMaxError, m_vertsPerPoly, m_detailSampleDist,
                        m_detailSampleMaxError, settingsUI.isFilterLowHangingObstacles(), settingsUI.isFilterLedgeSpans(),
                        settingsUI.isFilterWalkableLowHeightSpans());
                }

                sample.update(sample.getInputGeom(), buildResult.Item1, buildResult.Item2);
                sample.setChanged(false);
                settingsUI.setBuildTime((Stopwatch.GetTimestamp() - t) / 1_000_000);
                toolsUI.setSample(sample);
            }
        }
        else
        {
            building = false;
        }

        // if (!mouseOverMenu)
        // {
        // GLU.glhUnProjectf(mousePos[0], viewport[3] - 1 - mousePos[1], 0.0f, modelviewMatrix, projectionMatrix, viewport,
        //     rayStart);
        // GLU.glhUnProjectf(mousePos[0], viewport[3] - 1 - mousePos[1], 1.0f, modelviewMatrix, projectionMatrix, viewport,
        //     rayEnd);

        // Hit test mesh.
        // DemoInputGeomProvider inputGeom = sample.getInputGeom();
        // if (processHitTest && sample != null)
        // {
        //     float? hit = null;
        //     if (inputGeom != null)
        //     {
        //         hit = inputGeom.raycastMesh(rayStart, rayEnd);
        //     }
        //
        //     if (!hit.HasValue && sample.getNavMesh() != null)
        //     {
        //         hit = NavMeshRaycast.raycast(sample.getNavMesh(), rayStart, rayEnd);
        //     }
        //
        //     if (!hit.HasValue && sample.getRecastResults() != null)
        //     {
        //         hit = PolyMeshRaycast.raycast(sample.getRecastResults(), rayStart, rayEnd);
        //     }
        //
        //     float[] rayDir = new float[] { rayEnd[0] - rayStart[0], rayEnd[1] - rayStart[1], rayEnd[2] - rayStart[2] };
        //     Tool rayTool = toolsUI.getTool();
        //     vNormalize(rayDir);
        //     if (rayTool != null)
        //     {
        //         rayTool.handleClickRay(rayStart, rayDir, processHitTestShift);
        //     }
        //     // TODO : 잠시 주석
        //     // if (hit.HasValue) {
        //     //     float hitTime = hit.Value;
        //     //     if ((modState & GLFW_MOD_CONTROL) != 0) {
        //     //         // Marker
        //     //         markerPositionSet = true;
        //     //         markerPosition[0] = rayStart[0] + (rayEnd[0] - rayStart[0]) * hitTime;
        //     //         markerPosition[1] = rayStart[1] + (rayEnd[1] - rayStart[1]) * hitTime;
        //     //         markerPosition[2] = rayStart[2] + (rayEnd[2] - rayStart[2]) * hitTime;
        //     //     } else {
        //     //         float[] pos = new float[3];
        //     //         pos[0] = rayStart[0] + (rayEnd[0] - rayStart[0]) * hitTime;
        //     //         pos[1] = rayStart[1] + (rayEnd[1] - rayStart[1]) * hitTime;
        //     //         pos[2] = rayStart[2] + (rayEnd[2] - rayStart[2]) * hitTime;
        //     //         if (rayTool != null) {
        //     //             rayTool.handleClick(rayStart, pos, processHitTestShift);
        //     //         }
        //     //     }
        //     // } else {
        //     //     if ((modState & GLFW_MOD_CONTROL) != 0) {
        //     //         // Marker
        //     //         markerPositionSet = false;
        //     //     }
        //     // }
        // }

        //     processHitTest = false;
        // }

        if (sample.isChanged())
        {
            float[] bmin = null;
            float[] bmax = null;
            if (sample.getInputGeom() != null)
            {
                bmin = sample.getInputGeom().getMeshBoundsMin();
                bmax = sample.getInputGeom().getMeshBoundsMax();
            }
            else if (sample.getNavMesh() != null)
            {
                float[][] bounds = NavMeshUtils.getNavMeshBounds(sample.getNavMesh());
                bmin = bounds[0];
                bmax = bounds[1];
            }
            else if (0 < sample.getRecastResults().Count)
            {
                foreach (RecastBuilderResult result in sample.getRecastResults())
                {
                    if (result.getSolidHeightfield() != null)
                    {
                        if (bmin == null)
                        {
                            bmin = new float[] { float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity };
                            bmax = new float[] { float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity };
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            bmin[i] = Math.Min(bmin[i], result.getSolidHeightfield().bmin[i]);
                            bmax[i] = Math.Max(bmax[i], result.getSolidHeightfield().bmax[i]);
                        }
                    }
                }
            }

            if (bmin != null && bmax != null)
            {
                camr = (float)(Math.Sqrt(
                                   DemoMath.sqr(bmax[0] - bmin[0]) + DemoMath.sqr(bmax[1] - bmin[1]) + DemoMath.sqr(bmax[2] - bmin[2]))
                               / 2);
                cameraPos[0] = (bmax[0] + bmin[0]) / 2 + camr;
                cameraPos[1] = (bmax[1] + bmin[1]) / 2 + camr;
                cameraPos[2] = (bmax[2] + bmin[2]) / 2 + camr;
                camr *= 3;
                cameraEulers[0] = 45;
                cameraEulers[1] = -45;
            }

            sample.setChanged(false);
            toolsUI.setSample(sample);
        }


        var io = ImGui.GetIO();

        io.DisplaySize = new Vector2(width, height);
        io.DisplayFramebufferScale = Vector2.One;
        io.DeltaTime = (float)dt;

        //window.DoEvents();
        _imgui.Update((float)dt);
    }

    private unsafe void OnWindowOnRender(double dt)
    {
        // _gl.ClearColor(Color.CadetBlue);
        // _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Set the viewport.
        // glViewport(0, 0, width, height);
        //_gl.Viewport(0, 0, (uint)width, (uint)height);
        int[] viewport = new int[] { 0, 0, width, height };
        // glGetIntegerv(GL_VIEWPORT, viewport);

        // Clear the screen
        dd.clear();
        projectionMatrix = dd.projectionMatrix(50f, (float)width / (float)height, 1.0f, camr);
        modelviewMatrix = dd.viewMatrix(cameraPos, cameraEulers);

        dd.fog(camr * 0.1f, camr * 1.25f);
        renderer.render(sample);
        Tool tool = toolsUI.getTool();
        if (tool != null)
        {
            tool.handleRender(renderer);
        }

        dd.fog(false);

        mouseOverMenu = _viewSys.render(window, 0, 0, width, height, (int)mousePos[0], (int)mousePos[1]);
        _imgui.Render();

        window.SwapBuffers();
    }


    private void ErrorCallback(Silk.NET.GLFW.ErrorCode code, string message)
    {
        Console.WriteLine($"GLFW error [{code}]: {message}");
    }
}