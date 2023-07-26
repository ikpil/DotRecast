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
        var sample = _impl.GetSample();
        if (null == sample)
            return;

        var geom = sample.GetInputGeom();
        if (null == geom)
            return;

        var dd = renderer.GetDebugDraw();
        if (_hitPosSet)
        {
            var bmin = geom.GetMeshBoundsMin();
            var bmax = geom.GetMeshBoundsMax();
            
            var settings = sample.GetSettings();
            var s = settings.agentRadius;
            float ts = settings.tileSize * settings.cellSize;
            int tx = (int)((_hitPos.x - bmin[0]) / ts);
            int ty = (int)((_hitPos.z - bmin[2]) / ts);

            RcVec3f lastBuiltTileBmin = RcVec3f.Zero;
            RcVec3f lastBuiltTileBmax = RcVec3f.Zero;
            
            lastBuiltTileBmin[0] = bmin[0] + tx*ts;
            lastBuiltTileBmin[1] = bmin[1];
            lastBuiltTileBmin[2] = bmin[2] + ty*ts;
	
            lastBuiltTileBmax[0] = bmin[0] + (tx+1)*ts;
            lastBuiltTileBmax[1] = bmax[1];
            lastBuiltTileBmax[2] = bmin[2] + (ty+1)*ts;

            dd.DebugDrawCross(_hitPos.x, _hitPos.y + 0.1f, _hitPos.z, s, DuRGBA(0, 0, 0, 128), 2.0f);
            dd.DebugDrawBoxWire(
                lastBuiltTileBmin.x, lastBuiltTileBmin.y, lastBuiltTileBmin.z,
                lastBuiltTileBmax.x, lastBuiltTileBmax.y, lastBuiltTileBmax.z,
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