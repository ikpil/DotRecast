using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.DemoTool;
using DotRecast.Recast.DemoTool.Tools;
using Serilog;

namespace DotRecast.Recast.Demo.Tools;

public class ObstacleTool : IRcTool
{
    private static readonly ILogger Logger = Log.ForContext<ObstacleTool>();
    private readonly ObstacleToolImpl _impl;

    public ObstacleTool()
    {
        _impl = new();
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
    }

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
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