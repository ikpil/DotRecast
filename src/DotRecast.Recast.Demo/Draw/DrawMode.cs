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

using DotRecast.Core.Collections;

namespace DotRecast.Recast.Demo.Draw;

public class DrawMode
{
    public static readonly DrawMode DRAWMODE_MESH = new(0, "Input Mesh");
    public static readonly DrawMode DRAWMODE_NAVMESH = new(1, "Navmesh");
    public static readonly DrawMode DRAWMODE_NAVMESH_INVIS = new(2, "Navmesh Invis");
    public static readonly DrawMode DRAWMODE_NAVMESH_TRANS = new(3, "Navmesh Trans");
    public static readonly DrawMode DRAWMODE_NAVMESH_BVTREE = new(4, "Navmesh BVTree");
    public static readonly DrawMode DRAWMODE_NAVMESH_NODES = new(5, "Navmesh Nodes");
    public static readonly DrawMode DRAWMODE_NAVMESH_PORTALS = new(6, "Navmesh Portals");
    public static readonly DrawMode DRAWMODE_VOXELS = new(7, "Voxels");
    public static readonly DrawMode DRAWMODE_VOXELS_WALKABLE = new(8, "Walkable Voxels");
    public static readonly DrawMode DRAWMODE_COMPACT = new(9, "Compact");
    public static readonly DrawMode DRAWMODE_COMPACT_DISTANCE = new(10, "Compact Distance");
    public static readonly DrawMode DRAWMODE_COMPACT_REGIONS = new(11, "Compact Regions");
    public static readonly DrawMode DRAWMODE_REGION_CONNECTIONS = new(12, "Region Connections");
    public static readonly DrawMode DRAWMODE_RAW_CONTOURS = new(13, "Raw Contours");
    public static readonly DrawMode DRAWMODE_BOTH_CONTOURS = new(14, "Both Contours");
    public static readonly DrawMode DRAWMODE_CONTOURS = new(15, "Contours");
    public static readonly DrawMode DRAWMODE_POLYMESH = new(16, "Poly Mesh");
    public static readonly DrawMode DRAWMODE_POLYMESH_DETAIL = new(17, "Poly Mesh Detils");

    public static readonly RcImmutableArray<DrawMode> Values = RcImmutableArray.Create(
        DRAWMODE_MESH,
        DRAWMODE_NAVMESH,
        DRAWMODE_NAVMESH_INVIS,
        DRAWMODE_NAVMESH_TRANS,
        DRAWMODE_NAVMESH_BVTREE,
        DRAWMODE_NAVMESH_NODES,
        DRAWMODE_NAVMESH_PORTALS,
        DRAWMODE_VOXELS,
        DRAWMODE_VOXELS_WALKABLE,
        DRAWMODE_COMPACT,
        DRAWMODE_COMPACT_DISTANCE,
        DRAWMODE_COMPACT_REGIONS,
        DRAWMODE_REGION_CONNECTIONS,
        DRAWMODE_RAW_CONTOURS,
        DRAWMODE_BOTH_CONTOURS,
        DRAWMODE_CONTOURS,
        DRAWMODE_POLYMESH,
        DRAWMODE_POLYMESH_DETAIL
    );

    public readonly int Idx;
    public readonly string Text;

    private DrawMode(int idx, string text)
    {
        Idx = idx;
        Text = text;
    }

    public static DrawMode OfIdx(int idx)
    {
        return Values[idx];
    }

    public override string ToString()
    {
        return Text;
    }
}