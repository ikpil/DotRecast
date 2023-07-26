using System.Reflection.Metadata;
using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.DemoTool;
using DotRecast.Recast.DemoTool.Tools;
using ImGuiNET;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class TileTool : IRcTool
{
    private readonly TileToolImpl _impl;

    private bool _hitPosSet;
    private RcVec3f _hitPos;

    public TileTool()
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
        if (ImGui.Button("Create All Tile"))
        {
            // if (m_sample)
            //     m_sample->buildAllTiles();
        }

        if (ImGui.Button("Remove All Tile"))
        {
            // if (m_sample)
            //     m_sample->removeAllTiles();
        }
    }

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        _hitPosSet = true;
        _hitPos = p;

        var sample = _impl.GetSample();
        if (null != sample)
        {
            if (shift)
            {
            }
            else
            {
            }
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        if (_hitPosSet)
        {
            RecastDebugDraw dd = renderer.GetDebugDraw();
            var s = _impl.GetSample().GetSettings().agentRadius;
            dd.Begin(LINES, 2.0f);
            dd.Vertex(_hitPos[0] - s, _hitPos[1] + 0.1f, _hitPos[2], DuRGBA(0, 0, 0, 128));
            dd.Vertex(_hitPos[0] + s, _hitPos[1] + 0.1f, _hitPos[2], DuRGBA(0, 0, 0, 128));
            dd.Vertex(_hitPos[0], _hitPos[1] - s + 0.1f, _hitPos[2], DuRGBA(0, 0, 0, 128));
            dd.Vertex(_hitPos[0], _hitPos[1] + s + 0.1f, _hitPos[2], DuRGBA(0, 0, 0, 128));
            dd.Vertex(_hitPos[0], _hitPos[1] + 0.1f, _hitPos[2] - s, DuRGBA(0, 0, 0, 128));
            dd.Vertex(_hitPos[0], _hitPos[1] + 0.1f, _hitPos[2] + s, DuRGBA(0, 0, 0, 128));
            dd.End();
        }
    }

    public void HandleUpdate(float dt)
    {
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}