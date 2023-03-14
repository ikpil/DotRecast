/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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
using Silk.NET.Windowing;

using DotRecast.Core;
using DotRecast.Recast.Demo.Builder;
using DotRecast.Recast.Demo.Draw;
using DotRecast.Recast.Demo.Geom;

using static DotRecast.Recast.Demo.Draw.DebugDraw;
using static DotRecast.Recast.Demo.Draw.DebugDrawPrimitives;

namespace DotRecast.Recast.Demo.Tools;

public class ConvexVolumeTool : Tool {

    private Sample sample;
    private AreaModification areaType = SampleAreaModifications.SAMPLE_AREAMOD_GRASS;
    private readonly float[] boxHeight = new[] { 6f };
    private readonly float[] boxDescent = new[] { 1f };
    private readonly float[] polyOffset = new[] { 0f };
    private readonly List<float> pts = new();
    private readonly List<int> hull = new();

    public override void setSample(Sample m_sample) {
        sample = m_sample;
    }

    public override void handleClick(float[] s, float[] p, bool shift) {
        DemoInputGeomProvider geom = sample.getInputGeom();
        if (geom == null) {
            return;
        }

        if (shift) {
            // Delete
            int nearestIndex = -1;
            IList<ConvexVolume> vols = geom.convexVolumes();
            for (int i = 0; i < vols.Count; ++i) {
                if (PolyUtils.pointInPoly(vols[i].verts, p) && p[1] >= vols[i].hmin
                        && p[1] <= vols[i].hmax) {
                    nearestIndex = i;
                }
            }
            // If end point close enough, delete it.
            if (nearestIndex != -1) {
                geom.convexVolumes().RemoveAt(nearestIndex);
            }
        } else {
            // Create

            // If clicked on that last pt, create the shape.
            if (pts.Count > 0 && DemoMath.vDistSqr(p,
                    new float[] { pts[pts.Count - 3], pts[pts.Count - 2], pts[pts.Count - 1] },
                    0) < 0.2f * 0.2f) {
                if (hull.Count > 2) {
                    // Create shape.
                    float[] verts = new float[hull.Count * 3];
                    for (int i = 0; i < hull.Count; ++i) {
                        verts[i * 3] = pts[hull[i] * 3];
                        verts[i * 3 + 1] = pts[hull[i] * 3 + 1];
                        verts[i * 3 + 2] = pts[hull[i] * 3 + 2];
                    }

                    float minh = float.MaxValue, maxh = 0;
                    for (int i = 0; i < hull.Count; ++i) {
                        minh = Math.Min(minh, verts[i * 3 + 1]);
                    }
                    minh -= boxDescent[0];
                    maxh = minh + boxHeight[0];

                    if (polyOffset[0] > 0.01f) {
                        float[] offset = new float[verts.Length * 2];
                        int noffset = PolyUtils.offsetPoly(verts, hull.Count, polyOffset[0], offset,
                                offset.Length);
                        if (noffset > 0) {
                            geom.addConvexVolume(ArrayUtils.CopyOf(offset, 0, noffset * 3), minh, maxh, areaType);
                        }
                    } else {
                        geom.addConvexVolume(verts, minh, maxh, areaType);
                    }
                }
                pts.Clear();
                hull.Clear();
            } else {
                // Add new point
                pts.Add(p[0]);
                pts.Add(p[1]);
                pts.Add(p[2]);
                // Update hull.
                if (pts.Count > 3) {
                    hull.Clear();
                    hull.AddRange(ConvexUtils.convexhull(pts));
                } else {
                    hull.Clear();
                }
            }
        }

    }

    public override void handleRender(NavMeshRenderer renderer) {
        RecastDebugDraw dd = renderer.getDebugDraw();
        // Find height extent of the shape.
        float minh = float.MaxValue, maxh = 0;
        for (int i = 0; i < pts.Count; i += 3) {
            minh = Math.Min(minh, pts[i + 1]);
        }
        minh -= boxDescent[0];
        maxh = minh + boxHeight[0];

        dd.begin(POINTS, 4.0f);
        for (int i = 0; i < pts.Count; i += 3) {
            int col = duRGBA(255, 255, 255, 255);
            if (i == pts.Count - 3) {
                col = duRGBA(240, 32, 16, 255);
            }
            dd.vertex(pts[i + 0], pts[i + 1] + 0.1f, pts[i + 2], col);
        }
        dd.end();

        dd.begin(LINES, 2.0f);
        for (int i = 0, j = hull.Count - 1; i < hull.Count; j = i++) {
            int vi = hull[j] * 3;
            int vj = hull[i] * 3;
            dd.vertex(pts[vj + 0], minh, pts[vj + 2], duRGBA(255, 255, 255, 64));
            dd.vertex(pts[vi + 0], minh, pts[vi + 2], duRGBA(255, 255, 255, 64));
            dd.vertex(pts[vj + 0], maxh, pts[vj + 2], duRGBA(255, 255, 255, 64));
            dd.vertex(pts[vi + 0], maxh, pts[vi + 2], duRGBA(255, 255, 255, 64));
            dd.vertex(pts[vj + 0], minh, pts[vj + 2], duRGBA(255, 255, 255, 64));
            dd.vertex(pts[vj + 0], maxh, pts[vj + 2], duRGBA(255, 255, 255, 64));
        }
        dd.end();
    }

    public override void layout(IWindow ctx) {
        // nk_layout_row_dynamic(ctx, 20, 1);
        // nk_property_float(ctx, "Shape Height", 0.1f, boxHeight, 20f, 0.1f, 0.1f);
        // nk_layout_row_dynamic(ctx, 20, 1);
        // nk_property_float(ctx, "Shape Descent", 0.1f, boxDescent, 20f, 0.1f, 0.1f);
        // nk_layout_row_dynamic(ctx, 20, 1);
        // nk_property_float(ctx, "Poly Offset", 0.1f, polyOffset, 10f, 0.1f, 0.1f);
        // nk_label(ctx, "Area Type", NK_TEXT_ALIGN_LEFT);
        // nk_layout_row_dynamic(ctx, 20, 1);
        // if (nk_option_label(ctx, "Ground", areaType == SampleAreaModifications.SAMPLE_AREAMOD_GROUND)) {
        //     areaType = SampleAreaModifications.SAMPLE_AREAMOD_GROUND;
        // }
        // if (nk_option_label(ctx, "Water", areaType == SampleAreaModifications.SAMPLE_AREAMOD_WATER)) {
        //     areaType = SampleAreaModifications.SAMPLE_AREAMOD_WATER;
        // }
        // if (nk_option_label(ctx, "Road", areaType == SampleAreaModifications.SAMPLE_AREAMOD_ROAD)) {
        //     areaType = SampleAreaModifications.SAMPLE_AREAMOD_ROAD;
        // }
        // if (nk_option_label(ctx, "Door", areaType == SampleAreaModifications.SAMPLE_AREAMOD_DOOR)) {
        //     areaType = SampleAreaModifications.SAMPLE_AREAMOD_DOOR;
        // }
        // if (nk_option_label(ctx, "Grass", areaType == SampleAreaModifications.SAMPLE_AREAMOD_GRASS)) {
        //     areaType = SampleAreaModifications.SAMPLE_AREAMOD_GRASS;
        // }
        // if (nk_option_label(ctx, "Jump", areaType == SampleAreaModifications.SAMPLE_AREAMOD_JUMP)) {
        //     areaType = SampleAreaModifications.SAMPLE_AREAMOD_JUMP;
        // }
        // if (nk_button_text(ctx, "Clear Shape")) {
        //     hull.clear();
        //     pts.clear();
        // }
        // if (nk_button_text(ctx, "Remove All")) {
        //     hull.clear();
        //     pts.clear();
        //     DemoInputGeomProvider geom = sample.getInputGeom();
        //     if (geom != null) {
        //         geom.clearConvexVolumes();
        //     }
        // }
    }

    public override string getName() {
        return "Create Convex Volumes";
    }

    public override void handleUpdate(float dt) {
        // TODO Auto-generated method stub
    }

}
