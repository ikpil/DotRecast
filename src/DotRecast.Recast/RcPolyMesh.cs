/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
Recast4J Copyright (c) 2015 Piotr Piastucki piotr@jtilia.org
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

namespace DotRecast.Recast
{
    /// Represents a polygon mesh suitable for use in building a navigation mesh. 
    /// @ingroup recast
    public class RcPolyMesh
    {
        public int[] verts; // The mesh vertices. [Form: (x, y, z) coordinates * #nverts]
        public int[] polys; // Polygon and neighbor data. [Length: #maxpolys * 2 * #nvp]
        public int[] regs; // The region id assigned to each polygon. [Length: #maxpolys]
        public int[] areas; // The area id assigned to each polygon. [Length: #maxpolys]
        public int nverts; // The number of vertices.
        public int npolys; // The number of polygons.
        public int nvp; // The maximum number of vertices per polygon.
        public int maxpolys; // The number of allocated polygons.
        public int[] flags; // The user defined flags for each polygon. [Length: #maxpolys]
        public Vector3 bmin = new Vector3(); // The minimum bounds in world space. [(x, y, z)]
        public Vector3 bmax = new Vector3(); // The maximum bounds in world space. [(x, y, z)]

        public float cs; // The size of each cell. (On the xz-plane.)
        public float ch; // The height of each cell. (The minimum increment along the y-axis.)

        public int borderSize; // The AABB border size used to generate the source data from which the mesh was derived.
        public float maxEdgeError; // The max error of the polygon edges in the mesh.
    }
}