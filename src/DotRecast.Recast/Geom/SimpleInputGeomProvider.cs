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
            : this(MapVertices(vertexPositions), MapFaces(meshFaces))
        {
        }

        private static int[] MapFaces(List<int> meshFaces)
        {
            int[] faces = new int[meshFaces.Count];
            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = meshFaces[i];
            }

            return faces;
        }

        private static float[] MapVertices(List<float> vertexPositions)
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
            CalculateNormals();
            bmin = Vector3f.Zero;
            bmax = Vector3f.Zero;
            RecastVectors.Copy(ref bmin, vertices, 0);
            RecastVectors.Copy(ref bmax, vertices, 0);
            for (int i = 1; i < vertices.Length / 3; i++)
            {
                RecastVectors.Min(ref bmin, vertices, i * 3);
                RecastVectors.Max(ref bmax, vertices, i * 3);
            }

            _mesh = new TriMesh(vertices, faces);
        }

        public Vector3f GetMeshBoundsMin()
        {
            return bmin;
        }

        public Vector3f GetMeshBoundsMax()
        {
            return bmax;
        }

        public IList<ConvexVolume> ConvexVolumes()
        {
            return volumes;
        }

        public void AddConvexVolume(float[] verts, float minh, float maxh, AreaModification areaMod)
        {
            ConvexVolume vol = new ConvexVolume();
            vol.hmin = minh;
            vol.hmax = maxh;
            vol.verts = verts;
            vol.areaMod = areaMod;
            volumes.Add(vol);
        }

        public IEnumerable<TriMesh> Meshes()
        {
            return ImmutableArray.Create(_mesh);
        }

        public void CalculateNormals()
        {
            for (int i = 0; i < faces.Length; i += 3)
            {
                int v0 = faces[i] * 3;
                int v1 = faces[i + 1] * 3;
                int v2 = faces[i + 2] * 3;

                var e0 = new Vector3f();
                var e1 = new Vector3f();
                e0.x = vertices[v1 + 0] - vertices[v0 + 0];
                e0.y = vertices[v1 + 1] - vertices[v0 + 1];
                e0.z = vertices[v1 + 2] - vertices[v0 + 2];

                e1.x = vertices[v2 + 0] - vertices[v0 + 0];
                e1.y = vertices[v2 + 1] - vertices[v0 + 1];
                e1.z = vertices[v2 + 2] - vertices[v0 + 2];

                normals[i] = e0.y * e1.z - e0.z * e1.y;
                normals[i + 1] = e0.z * e1.x - e0.x * e1.z;
                normals[i + 2] = e0.x * e1.y - e0.y * e1.x;
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