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
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Recast.DemoTool.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.DemoTool;
using DotRecast.Recast.DemoTool.Geom;
using DotRecast.Recast.DemoTool.Tools;
using ImGuiNET;
using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class ConvexVolumeTool : IRcTool
{
    private readonly ConvexVolumeToolImpl _impl;

    private int areaTypeValue = SampleAreaModifications.SAMPLE_AREAMOD_GRASS.Value;
    private RcAreaModification areaType = SampleAreaModifications.SAMPLE_AREAMOD_GRASS;
    private float boxHeight = 6f;
    private float boxDescent = 1f;
    private float polyOffset = 0f;
    private readonly List<RcVec3f> pts = new();
    private readonly List<int> hull = new();

    public ConvexVolumeTool()
    {
        _impl = new ConvexVolumeToolImpl();
    }

    public ISampleTool GetTool()
    {
        return _impl;
    }

    public void OnSampleChanged()
    {
        // ..
    }

    public void HandleClick(RcVec3f s, RcVec3f p, bool shift)
    {
        DemoInputGeomProvider geom = _impl.GetSample().GetInputGeom();
        if (geom == null)
        {
            return;
        }

        if (shift)
        {
            _impl.RemoveByPos(p);
        }
        else
        {
            // Create

            // If clicked on that last pt, create the shape.
            if (pts.Count > 0 && RcVec3f.DistSqr(p, pts[pts.Count - 1]) < 0.2f * 0.2f)
            {
                var vol = ConvexVolumeToolImpl.CreateConvexVolume(pts, hull, areaType, boxDescent, boxHeight, polyOffset);
                if (null != vol)
                    _impl.Add(vol);

                pts.Clear();
                hull.Clear();
            }
            else
            {
                // Add new point
                pts.Add(p);

                // Update hull.
                if (pts.Count > 3)
                {
                    hull.Clear();
                    hull.AddRange(RcConvexUtils.Convexhull(pts));
                }
                else
                {
                    hull.Clear();
                }
            }
        }
    }

    public void HandleRender(NavMeshRenderer renderer)
    {
        RecastDebugDraw dd = renderer.GetDebugDraw();
        // Find height extent of the shape.
        float minh = float.MaxValue, maxh = 0;
        for (int i = 0; i < pts.Count; ++i)
        {
            minh = Math.Min(minh, pts[i].y);
        }

        minh -= boxDescent;
        maxh = minh + boxHeight;

        dd.Begin(POINTS, 4.0f);
        for (int i = 0; i < pts.Count; ++i)
        {
            int col = DuRGBA(255, 255, 255, 255);
            if (i == pts.Count - 1)
            {
                col = DuRGBA(240, 32, 16, 255);
            }

            dd.Vertex(pts[i].x, pts[i].y + 0.1f, pts[i].z, col);
        }

        dd.End();

        dd.Begin(LINES, 2.0f);
        for (int i = 0, j = hull.Count - 1; i < hull.Count; j = i++)
        {
            int vi = hull[j];
            int vj = hull[i];
            dd.Vertex(pts[vj].x, minh, pts[vj].z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vi].x, minh, pts[vi].z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vj].x, maxh, pts[vj].z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vi].x, maxh, pts[vi].z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vj].x, minh, pts[vj].z, DuRGBA(255, 255, 255, 64));
            dd.Vertex(pts[vj].x, maxh, pts[vj].z, DuRGBA(255, 255, 255, 64));
        }

        dd.End();
    }

    public void Layout()
    {
        ImGui.SliderFloat("Shape Height", ref boxHeight, 0.1f, 20f, "%.1f");
        ImGui.SliderFloat("Shape Descent", ref boxDescent, 0.1f, 20f, "%.1f");
        ImGui.SliderFloat("Poly Offset", ref polyOffset, 0.1f, 10f, "%.1f");
        ImGui.NewLine();

        ImGui.Text("Area Type");
        ImGui.Separator();
        int prevAreaTypeValue = areaTypeValue;
        ImGui.RadioButton("Ground", ref areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_GROUND.Value);
        ImGui.RadioButton("Water", ref areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_WATER.Value);
        ImGui.RadioButton("Road", ref areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_ROAD.Value);
        ImGui.RadioButton("Door", ref areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_DOOR.Value);
        ImGui.RadioButton("Grass", ref areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_GRASS.Value);
        ImGui.RadioButton("Jump", ref areaTypeValue, SampleAreaModifications.SAMPLE_AREAMOD_JUMP.Value);
        ImGui.NewLine();

        if (prevAreaTypeValue != areaTypeValue)
        {
            areaType = SampleAreaModifications.OfValue(areaTypeValue);
        }

        if (ImGui.Button("Clear Shape"))
        {
            hull.Clear();
            pts.Clear();
        }

        if (ImGui.Button("Remove All"))
        {
            hull.Clear();
            pts.Clear();

            DemoInputGeomProvider geom = _impl.GetSample().GetInputGeom();
            if (geom != null)
            {
                geom.ClearConvexVolumes();
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