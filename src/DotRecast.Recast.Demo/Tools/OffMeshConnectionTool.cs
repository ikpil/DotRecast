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
using Silk.NET.Windowing;
using DotRecast.Core;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;
using ImGuiNET;
using static DotRecast.Recast.Demo.Draw.DebugDraw;

namespace DotRecast.Recast.Demo.Tools;

public class OffMeshConnectionTool : Tool
{
    private Sample sample;
    private bool hitPosSet;
    private float[] hitPos;
    private int bidir;

    public override void setSample(Sample m_sample)
    {
        sample = m_sample;
    }

    public override void handleClick(float[] s, float[] p, bool shift)
    {
        DemoInputGeomProvider geom = sample.getInputGeom();
        if (geom == null)
        {
            return;
        }

        if (shift)
        {
            // Delete
            // Find nearest link end-point
            float nearestDist = float.MaxValue;
            DemoOffMeshConnection nearestConnection = null;
            foreach (DemoOffMeshConnection offMeshCon in geom.getOffMeshConnections())
            {
                float d = Math.Min(RecastMath.vDistSqr(p, offMeshCon.verts, 0), RecastMath.vDistSqr(p, offMeshCon.verts, 3));
                if (d < nearestDist && Math.Sqrt(d) < sample.getSettingsUI().getAgentRadius())
                {
                    nearestDist = d;
                    nearestConnection = offMeshCon;
                }
            }

            if (nearestConnection != null)
            {
                geom.getOffMeshConnections().Remove(nearestConnection);
            }
        }
        else
        {
            // Create
            if (!hitPosSet)
            {
                hitPos = ArrayUtils.CopyOf(p, p.Length);
                hitPosSet = true;
            }
            else
            {
                int area = SampleAreaModifications.SAMPLE_POLYAREA_TYPE_JUMP;
                int flags = SampleAreaModifications.SAMPLE_POLYFLAGS_JUMP;
                geom.addOffMeshConnection(hitPos, p, sample.getSettingsUI().getAgentRadius(), 0 == bidir, area, flags);
                hitPosSet = false;
            }
        }
    }

    public override void handleRender(NavMeshRenderer renderer)
    {
        if (sample == null)
        {
            return;
        }

        RecastDebugDraw dd = renderer.getDebugDraw();
        float s = sample.getSettingsUI().getAgentRadius();

        if (hitPosSet)
        {
            dd.debugDrawCross(hitPos[0], hitPos[1] + 0.1f, hitPos[2], s, duRGBA(0, 0, 0, 128), 2.0f);
        }

        DemoInputGeomProvider geom = sample.getInputGeom();
        if (geom != null)
        {
            renderer.drawOffMeshConnections(geom, true);
        }
    }

    public override void layout()
    {
        ImGui.RadioButton("One Way", ref bidir, 0);
        ImGui.RadioButton("Bidirectional", ref bidir, 1);
    }

    public override string getName()
    {
        return "Create Off-Mesh Links";
    }

    public override void handleUpdate(float dt)
    {
        // TODO Auto-generated method stub
    }
}