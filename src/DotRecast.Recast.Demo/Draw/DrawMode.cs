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
namespace DotRecast.Recast.Demo.Draw;

public class DrawMode {
    public static readonly DrawMode DRAWMODE_MESH = new("Input Mesh");
    public static readonly DrawMode DRAWMODE_NAVMESH = new("Navmesh");
    public static readonly DrawMode DRAWMODE_NAVMESH_INVIS = new("Navmesh Invis");
    public static readonly DrawMode DRAWMODE_NAVMESH_TRANS = new("Navmesh Trans");
    public static readonly DrawMode DRAWMODE_NAVMESH_BVTREE = new("Navmesh BVTree");
    public static readonly DrawMode DRAWMODE_NAVMESH_NODES = new("Navmesh Nodes");
    public static readonly DrawMode DRAWMODE_NAVMESH_PORTALS = new("Navmesh Portals");
    public static readonly DrawMode DRAWMODE_VOXELS = new("Voxels");
    public static readonly DrawMode DRAWMODE_VOXELS_WALKABLE = new("Walkable Voxels");
    public static readonly DrawMode DRAWMODE_COMPACT = new("Compact");
    public static readonly DrawMode DRAWMODE_COMPACT_DISTANCE = new("Compact Distance");
    public static readonly DrawMode DRAWMODE_COMPACT_REGIONS = new("Compact Regions");
    public static readonly DrawMode DRAWMODE_REGION_CONNECTIONS = new("Region Connections");
    public static readonly DrawMode DRAWMODE_RAW_CONTOURS = new("Raw Contours");
    public static readonly DrawMode DRAWMODE_BOTH_CONTOURS = new("Both Contours");
    public static readonly DrawMode DRAWMODE_CONTOURS = new("Contours");
    public static readonly DrawMode DRAWMODE_POLYMESH = new("Poly Mesh");
    public static readonly DrawMode DRAWMODE_POLYMESH_DETAIL = new("Poly Mesh Detils");
    
    private readonly string text;

    private DrawMode(string text) {
        this.text = text;
    }

    public override string ToString() {
        return text;
    }
}
