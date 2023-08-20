using DotRecast.Core;
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

            _tool.Build(geom, settings, RcByteOrder.LITTLE_ENDIAN, true);
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
    }

    public void HandleUpdate(float dt)
    {
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}