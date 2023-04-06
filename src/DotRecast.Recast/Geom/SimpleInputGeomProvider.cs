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
using System.Collections.Immutable;
using DotRecast.Core;

namespace DotRecast.Recast.Geom
{
    public class SimpleInputGeomProvider : InputGeomProvider
    {
        public readonly float[] vertices;
        public readonly int[] faces;
        public readonly float[] normals;
        private Vector3f bmin;
        private Vector3f bmax;
        private readonly List<ConvexVolume> volumes = new List<ConvexVolume>();
        private readonly TriMesh _mesh;

        public SimpleInputGeomProvider(List<float> vertexPositions, List<int> meshFaces)
            : this(mapVertices(vertexPositions), mapFaces(meshFaces))
        {
        }

        private static int[] mapFaces(List<int> meshFaces)
        {
            int[] faces = new int[meshFaces.Count];
            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = meshFaces[i];
            }

            return faces;
        }

        private static float[] mapVertices(List<float> vertexPositions)
        {
            float[] vertices = new float[vertexPositions.Count];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertexPositions[i];
            }

            return vertices;
        }

        public SimpleInputGeomProvider(float[] vertices, int[] faces)
        {
            this.vertices = vertices;
            this.faces = faces;
            normals = new float[faces.Length];
            calculateNormals();
            bmin = Vector3f.Zero;
            bmax = Vector3f.Zero;
            RecastVectors.copy(ref bmin, vertices, 0);
            RecastVectors.copy(ref bmax, vertices, 0);
            for (int i = 1; i < vertices.Length / 3; i++)
            {
                RecastVectors.min(ref bmin, vertices, i * 3);
                RecastVectors.max(ref bmax, vertices, i * 3);
            }

            _mesh = new TriMesh(vertices, faces);
        }

        public Vector3f getMeshBoundsMin()
        {
            return bmin;
        }

        public Vector3f getMeshBoundsMax()
        {
            return bmax;
        }

        public IList<ConvexVolume> convexVolumes()
        {
            return volumes;
        }

        public void addConvexVolume(float[] verts, float minh, float maxh, AreaModification areaMod)
        {
            ConvexVolume vol = new ConvexVolume();
            vol.hmin = minh;
            vol.hmax = maxh;
            vol.verts = verts;
            vol.areaMod = areaMod;
            volumes.Add(vol);
        }

        public IEnumerable<TriMesh> meshes()
        {
            return ImmutableArray.Create(_mesh);
        }

        public void calculateNormals()
        {
            for (int i = 0; i < faces.Length; i += 3)
            {
                int v0 = faces[i] * 3;
                int v1 = faces[i + 1] * 3;
                int v2 = faces[i + 2] * 3;
                Vector3f e0 = new Vector3f(), e1 = new Vector3f();
                for (int j = 0; j < 3; ++j)
                {
                    e0[j] = vertices[v1 + j] - vertices[v0 + j];
                    e1[j] = vertices[v2 + j] - vertices[v0 + j];
                }

                normals[i] = e0[1] * e1[2] - e0[2] * e1[1];
                normals[i + 1] = e0[2] * e1[0] - e0[0] * e1[2];
                normals[i + 2] = e0[0] * e1[1] - e0[1] * e1[0];
                float d = (float)Math.Sqrt(normals[i] * normals[i] + normals[i + 1] * normals[i + 1] + normals[i + 2] * normals[i + 2]);
                if (d > 0)
                {
                    d = 1.0f / d;
                    normals[i] *= d;
                    normals[i + 1] *= d;
                    normals[i + 2] *= d;
                }
            }
        }
    }
}