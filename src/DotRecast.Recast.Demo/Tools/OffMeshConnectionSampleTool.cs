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

using System.Numerics;
using DotRecast.Core.Numerics;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Geom;
using DotRecast.Recast.Toolset.Tools;
using ImGuiNET;
using Serilog;
using static DotRecast.Recast.Demo.Draw.DebugDraw;

namespace DotRecast.Recast.Demo.Tools;

public class OffMeshConnectionSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<OffMeshConnectionSampleTool>();

    private DemoSample _sample;

    private readonly RcOffMeshConnectionTool _tool;

    private int _bidir;
    private bool _hasStartPt;
    private Vector3 _startPt;

    public OffMeshConnectionSampleTool()
    {
        _tool = new();
    }

    public void Layout()
    {
        ImGui.RadioButton("One Way", ref _bidir, 0);
        ImGui.RadioButton("Bidirectional", ref _bidir, 1);
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();

        var settings = _sample.GetSettings();
        float s = settings.agentRadius;

        if (_hasStartPt)
        {
            dd.DebugDrawCross(_startPt.X, _startPt.Y + 0.1f, _startPt.Z, s, DuRGBA(0, 0, 0, 128), 2.0f);
        }

        DemoInputGeomProvider geom = _sample.GetInputGeom();
        if (geom != null)
        {
            renderer.DrawOffMeshConnections(geom, true);
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
        // ..
    }

    public void HandleClick(Vector3 s, Vector3 p, bool shift)
    {
        DemoInputGeomProvider geom = _sample.GetInputGeom();
        if (geom == null)
        {
            return;
        }

        var settings = _sample.GetSettings();

        if (shift)
        {
            _tool.Remove(geom, settings, p);
        }
        else
        {
            // Create
            if (!_hasStartPt)
            {
                _startPt = p;
                _hasStartPt = true;
            }
            else
            {
                _tool.Add(geom, settings, _startPt, p, 1 == _bidir);
                _hasStartPt = false;
            }
        }
    }


    public void HandleUpdate(float dt)
    {
        // TODO Auto-generated method stub
    }

    public void HandleClickRay(Vector3 start, Vector3 direction, bool shift)
    {
    }
}