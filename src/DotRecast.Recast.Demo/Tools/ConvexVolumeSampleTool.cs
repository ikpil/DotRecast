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
using DotRecast.Core.Numerics;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Tools;
using ImGuiNET;
using Serilog;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class ConvexVolumeSampleTool : ISampleTool
{
    private static readonly ILogger Logger = Log.ForContext<ConvexVolumeSampleTool>();

    private DemoSample _sample;
    private readonly RcConvexVolumeTool _tool;

    private float _boxHeight = 6f;
    private float _boxDescent = 1f;
    private float _polyOffset = 0f;

    private int _areaTypeValue = SampleAreaModifications.SAMPLE_AREAMOD_GRASS.Value;
    private RcAreaModification _areaType = SampleAreaModifications.SAMPLE_AREAMOD_GRASS;


    public ConvexVolumeSampleTool()
    {
        _tool = new RcConvexVolumeTool();
    }

    public void Layout()
    {
        ImGui.SliderFloat("Shape Height", ref _boxHeight, 0.1f, 20f, "%.1f");
        ImGui.SliderFloat("Shape Descent", ref _boxDescent, 0.1f, 20f, "%.1f");
        ImGui.SliderFloat("Poly Offset", ref _polyOffset, 0.1f, 10f, "%.1f");
        ImGui.NewLine();

        int prevAreaTypeValue = _areaTypeValue;

        ImGui.Text("Area Type");
        ImGui.Separator();
        ImGui.RadioButton("Ground", ref _areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_GROUND.Value);
        ImGui.RadioButton("Water", ref _areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_WATER.Value);
        ImGui.RadioButton("Road", ref _areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_ROAD.Value);
        ImGui.RadioButton("Door", ref _areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_DOOR.Value);
        ImGui.RadioButton("Grass", ref _areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_GRASS.Value);
        ImGui.RadioButton("Jump", ref _areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_JUMP.Value);
        ImGui.NewLine();

        if (prevAreaTypeValue != _areaTypeValue)
        {
            _areaType = SampleAreaModifications.OfValue(_areaTypeValue);
        }

        if (ImGui.Button("Clear Shape"))
        {
            _tool.ClearShape();
        }

        if (ImGui.Button("Remove All"))
        {
            _tool.ClearShape();

            var geom = _sample.GetInputGeom();
            if (geom != null)
            {
                geom.ClearConvexVolumes();
            }
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();

        var pts = _tool.GetShapePoint();
        var hull = _tool.GetShapeHull();

        // Find height extent of the shape.
        float minh = float.MaxValue, maxh = 0;
        for (int i = 0; i < pts.Count; ++i)
        {
            minh = Math.Min(minh, pts[i].Y);
        }

        minh -= _boxDescent;
        maxh = minh + _boxHeight;

        dd.Begin(POINTS, 4.0f);
        for (int i = 0; i < pts.Count; ++i)
        {
            int col = DuRGBA(255, 255, 255, 255);
            if (i == pts.Count - 1)
            {
                col = DuRGBA(240, 32, 16, 255);
            }

            dd.Vertex(pts[i].X, pts[i].Y + 0.1f, pts[i].Z, col);
        }

        dd.End();

        dd.Begin(LINES, 2.0f);
        for (int i = 0, j = hull.Count - 1; i < hull.Count; j = i++)
        {
            int vi = hull[j];
            int vj = hull[i];
            dd.Vertex(pts[vj].X, minh, pts[vj].Z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vi].X, minh, pts[vi].Z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vj].X, maxh, pts[vj].Z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vi].X, maxh, pts[vi].Z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vj].X, minh, pts[vj].Z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vj].X, maxh, pts[vj].Z, DuRGBA(255, 255, 255, 64));
        }

        dd.End();
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

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        var geom = _sample.GetInputGeom();
        if (shift)
        {
            _tool.RemoveByPos(geom, p);
        }
        else
        {
            if (_tool.PlottingShape(p, out var pts, out var hull))
            {
                var vol = RcConvexVolumeTool.CreateConvexVolume(pts, hull, _areaType, _boxDescent, _boxHeight, _polyOffset);
                _tool.Add(geom, vol);
            }
        }
    }


    public void HandleUpdate(float dt)
    {
        // TODO Auto-generated method stub
    }

    public void HandleClickRay(RcVec3f start, RcVec3f direction, bool shift)
    {
    }
}