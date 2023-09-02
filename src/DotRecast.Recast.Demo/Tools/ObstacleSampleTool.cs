using DotRecast.Core;
using DotRecast.Detour.TileCache;
using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Tools;
using ImGuiNET;
using Serilog;

namespace DotRecast.Recast.Demo.Tools;

public class ObstacleSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<ObstacleSampleTool>();

    private DemoSample _sample;

    private readonly RcObstacleTool _tool;
    private bool _hitPosSet;
    private RcVec3f _hitPos;

    public ObstacleSampleTool()
    {
        _tool = new(DtTileCacheCompressorFactory.Shared);
    }

    public IRcToolable GetTool()
    {
        return _tool;
    }

    public void SetSample(DemoSample sample)
    {
        _sample = sample;
    }

    public void OnSampleChanged()
    {
    }

    public void Layout()
    {
        if (ImGui.Button("Build Tile Cache"))
        {
            var geom = _sample.GetInputGeom();
            var settings = _sample.GetSettings();

            var buildResult =_tool.Build(geom, settings, RcByteOrder.LITTLE_ENDIAN, true);
            if (buildResult.Success)
            {
                _sample.Update(_sample.GetInputGeom(), buildResult.RecastBuilderResults, buildResult.NavMesh);
                _sample.SetChanged(false);
            }
                
        }

        if (ImGui.Button("Remove All Temp Obstacles"))
        {
            _tool.ClearAllTempObstacles();
        }

        ImGui.Separator();

        ImGui.Text("Click LMB to create an obstacle.");
        ImGui.Text("Shift+LMB to remove an obstacle.");
    }

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        _hitPosSet = true;
        _hitPos = p;

        if (shift)
        {
            _tool.RemoveTempObstacle(s, p);
        }
        else
        {
            _tool.AddTempObstacle(_hitPos);
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        DrawObstacles(renderer.GetDebugDraw());
    }


    private void DrawObstacles(RecastDebugDraw dd)
    {
        var tc = _tool.GetTileCache();
        if (null == tc)
            return;

        // Draw obstacles
        for (int i = 0; i < tc.GetObstacleCount(); ++i)
        {
            var ob = tc.GetObstacle(i);
            if (ob.state == DtObstacleState.DT_OBSTACLE_EMPTY)
                continue;

            RcVec3f bmin = RcVec3f.Zero;
            RcVec3f bmax = RcVec3f.Zero;
            tc.GetObstacleBounds(ob, ref bmin, ref bmax);

            int col = 0;
            if (ob.state == DtObstacleState.DT_OBSTACLE_PROCESSING)
                col = DebugDraw.DuRGBA(255, 255, 0, 128);
            else if (ob.state == DtObstacleState.DT_OBSTACLE_PROCESSED)
                col = DebugDraw.DuRGBA(255, 192, 0, 192);
            else if (ob.state == DtObstacleState.DT_OBSTACLE_REMOVING)
                col = DebugDraw.DuRGBA(220, 0, 0, 128);

            dd.DebugDrawCylinder(bmin[0], bmin[1], bmin[2], bmax[0], bmax[1], bmax[2], col);
            dd.DebugDrawCylinderWire(bmin[0], bmin[1], bmin[2], bmax[0], bmax[1], bmax[2], DebugDraw.DuDarkenCol(col), 2);
        }
    }

    public void HandleUpdate(float dt)
    {
        var tc = _tool.GetTileCache();
        if (null != tc)
            tc.Update();
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}