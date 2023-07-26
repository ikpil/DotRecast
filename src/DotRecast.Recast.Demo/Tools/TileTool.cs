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
        var dd = renderer.GetDebugDraw();
        if (_hitPosSet)
        {
            var s = _impl.GetSample().GetSettings().agentRadius;
            RcVec3f m_lastBuiltTileBmin = _hitPos - RcVec3f.One;
            RcVec3f m_lastBuiltTileBmax = _hitPos + RcVec3f.One;
            
            dd.DebugDrawCross(_hitPos.x, _hitPos.y + 0.1f, _hitPos.z, s, DuRGBA(0, 0, 0, 128), 2.0f);
            dd.DebugDrawBoxWire(
                m_lastBuiltTileBmin.x, m_lastBuiltTileBmin.y, m_lastBuiltTileBmin.z,
                m_lastBuiltTileBmax.x, m_lastBuiltTileBmax.y, m_lastBuiltTileBmax.z,
                DuRGBA(255, 255, 255, 64), 1.0f);
        }
    }

    public void HandleUpdate(float dt)
    {
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}