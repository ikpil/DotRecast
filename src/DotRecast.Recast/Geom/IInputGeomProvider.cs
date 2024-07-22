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

using System;
using System.Collections.Generic;
using System.Numerics;

namespace DotRecast.Recast.Geom
{
    public interface IInputGeomProvider
    {
        RcTriMesh GetMesh();
        Vector3 GetMeshBoundsMin();

        Vector3 GetMeshBoundsMax();

        IEnumerable<RcTriMesh> Meshes();

        // convex volume
        void AddConvexVolume(RcConvexVolume convexVolume);
        IList<RcConvexVolume> ConvexVolumes();

        // off mesh connections
        int OffMeshConCount { get; }
        float[] OffMeshConVerts { get; }
        float[] OffMeshConRads { get; }
        bool[] OffMeshConDirs { get; }
        int[] OffMeshConAreas { get; }
        int[] OffMeshConFlags { get; }
        int[] OffMeshConId { get; }
        public void AddOffMeshConnection(Vector3 start, Vector3 end, float radius, bool bidir, int area, int flags);
        public void RemoveOffMeshConnection(int idx);
    }
}