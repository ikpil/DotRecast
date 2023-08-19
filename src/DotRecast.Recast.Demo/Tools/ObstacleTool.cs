using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Tools;
using ImGuiNET;
using Serilog;

namespace DotRecast.Recast.Demo.Tools;

public class ObstacleTool : IRcTool
{
    private static readonly ILogger Logger = Log.ForContext<ObstacleTool>();
    private readonly ObstacleToolImpl _impl;
    private bool _hitPosSet;
    private RcVec3f _hitPos;

    public ObstacleTool()
    {
        _impl = new(DtTileCacheCompressorDemoFactory.Shared);
    }

    public ISampleTool GetTool()
    {
        return _impl;
    }

    public void OnSampleChanged()
    {
    }

    public void Layout()
    {
        if (ImGui.Button("Remove All Temp Obstacles"))
        {
            _impl.ClearAllTempObstacles();
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
            _impl.RemoveTempObstacle(s, p);
        }
        else
        {
            _impl.AddTempObstacle(_hitPos);
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