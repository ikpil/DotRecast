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
using DotRecast.Core;
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

    private readonly OffMeshConnectionToolImpl _impl;
    private bool hitPosSet;
    private RcVec3f hitPos;

    public OffMeshConnectionSampleTool()
    {
        _impl = new();
    }

    public IRcToolable GetTool()
    {
        return _impl;
    }

    public void SetSample(DemoSample sample)
    {
        _sample = sample;
    }

    public void OnSampleChanged()
    {
        // ..
    }

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        DemoInputGeomProvider geom = _sample.GetInputGeom();
        if (geom == null)
        {
            return;
        }

        var settings = _sample.GetSettings();

        if (shift)
        {
            _impl.Remove(geom, settings, p);
        }
        else
        {
            // Create
            if (!hitPosSet)
            {
                hitPos = p;
                hitPosSet = true;
            }
            else
            {
                _impl.Add(geom, settings, hitPos, p);
                hitPosSet = false;
            }
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();

        var settings = _sample.GetSettings();
        float s = settings.agentRadius;

        if (hitPosSet)
        {
            dd.DebugDrawCross(hitPos.x, hitPos.y + 0.1f, hitPos.z, s, DuRGBA(0, 0, 0, 128), 2.0f);
        }

        DemoInputGeomProvider geom = _sample.GetInputGeom();
        if (geom != null)
        {
            renderer.DrawOffMeshConnections(geom, true);
        }
    }

    public void Layout()
    {
        var options = _impl.GetOption();
        ImGui.RadioButton("One Way", ref options.bidir, 0);
        ImGui.RadioButton("Bidirectional", ref options.bidir, 1);
    }


    public void HandleUpdate(float dt)
    {
        // TODO Auto-generated method stub
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}