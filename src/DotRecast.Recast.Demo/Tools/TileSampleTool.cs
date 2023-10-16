using System;
using DotRecast.Core.Numerics;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Toolset.Tools;
using ImGuiNET;
using Serilog;
using static DotRecast.Recast.Demo.Draw.DebugDraw;

namespace DotRecast.Recast.Demo.Tools;

public class TileSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<TileSampleTool>();

    private DemoSample _sample;
    private readonly RcTileTool _tool;

    private bool _hitPosSet;
    private RcVec3f _hitPos;

    public TileSampleTool()
    {
        _tool = new();
    }

    public void Layout()
    {
        var geom = _sample.GetInputGeom();
        var settings = _sample.GetSettings();
        var navMesh = _sample.GetNavMesh();

        if (ImGui.Button("Create All Tile"))
        {
            _tool.BuildAllTiles(geom, settings, navMesh);
        }

        if (ImGui.Button("Remove All Tile"))
        {
            _tool.RemoveAllTiles(geom, settings, navMesh);
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        var geom = _sample.GetInputGeom();
        var settings = _sample.GetSettings();

        if (null == geom)
            return;

        var dd = renderer.GetDebugDraw();
        if (_hitPosSet)
        {
            var bmin = geom.GetMeshBoundsMin();
            var bmax = geom.GetMeshBoundsMax();

            var s = settings.agentRadius;

            float ts = settings.tileSize * settings.cellSize;
            int tx = (int)((_hitPos.X - bmin[0]) / ts);
            int ty = (int)((_hitPos.Z - bmin[2]) / ts);

            RcVec3f lastBuiltTileBmin = RcVec3f.Zero;
            RcVec3f lastBuiltTileBmax = RcVec3f.Zero;

            lastBuiltTileBmin[0] = bmin[0] + tx * ts;
            lastBuiltTileBmin[1] = bmin[1];
            lastBuiltTileBmin[2] = bmin[2] + ty * ts;

            lastBuiltTileBmax[0] = bmin[0] + (tx + 1) * ts;
            lastBuiltTileBmax[1] = bmax[1];
            lastBuiltTileBmax[2] = bmin[2] + (ty + 1) * ts;

            dd.DebugDrawCross(_hitPos.X, _hitPos.Y + 0.1f, _hitPos.Z, s, DuRGBA(0, 0, 0, 128), 2.0f);
            dd.DebugDrawBoxWire(
                lastBuiltTileBmin.X, lastBuiltTileBmin.Y, lastBuiltTileBmin.Z,
                lastBuiltTileBmax.X, lastBuiltTileBmax.Y, lastBuiltTileBmax.Z,
                DuRGBA(255, 255, 255, 64), 1.0f);

            // 표기
        }
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


    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        _hitPosSet = true;
        _hitPos = p;

        var geom = _sample.GetInputGeom();
        var settings = _sample.GetSettings();
        var navMesh = _sample.GetNavMesh();

        if (shift)
        {
            _tool.RemoveTile(geom, settings, navMesh, _hitPos);
        }
        else
        {
            bool built = _tool.BuildTile(geom, settings, navMesh, _hitPos, out var tileBuildTicks, out var tileTriCount, out var tileMemUsage);
            if (!built)
            {
                Logger.Error($"failed to build tile - check!");
            }
            else
            {
                Logger.Information($"{tileBuildTicks / (float)TimeSpan.TicksPerMillisecond}ms / {tileTriCount}Tris / {tileMemUsage}kB ");
            }
        }
    }


    public void HandleUpdate(float dt)
    {
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}