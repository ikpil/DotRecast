/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

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
using System.Collections.Immutable;
using DotRecast.Core;

namespace DotRecast.Recast.Geom
{
    public class SingleTrimeshInputGeomProvider : IInputGeomProvider
    {
        private readonly RcVec3f bmin;
        private readonly RcVec3f bmax;
        private readonly RcTriMesh _mesh;

        public SingleTrimeshInputGeomProvider(float[] vertices, int[] faces)
        {
            bmin = RcVec3f.Zero;
            bmax = RcVec3f.Zero;
            RcVec3f.Copy(ref bmin, vertices, 0);
            RcVec3f.Copy(ref bmax, vertices, 0);
            for (int i = 1; i < vertices.Length / 3; i++)
            {
                bmin.Min(vertices, i * 3);
                bmax.Max(vertices, i * 3);
            }

            _mesh = new RcTriMesh(vertices, faces);
        }

        public RcVec3f GetMeshBoundsMin()
        {
            return bmin;
        }

        public RcVec3f GetMeshBoundsMax()
        {
            return bmax;
        }

        public IEnumerable<RcTriMesh> Meshes()
        {
            return ImmutableArray.Create(_mesh);
        }

        public IList<ConvexVolume> ConvexVolumes()
        {
            return ImmutableArray<ConvexVolume>.Empty;
        }
    }
}