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
using DotRecast.Core;
using DotRecast.Core.Collections;
using System.Numerics;

namespace DotRecast.Recast.Geom
{
    public class SimpleInputGeomProvider : IInputGeomProvider
    {
        public readonly float[] vertices;
        public readonly int[] faces;
        public readonly float[] normals;
        private Vector3 bmin;
        private Vector3 bmax;

        private readonly List<RcConvexVolume> volumes = new List<RcConvexVolume>();
        private readonly RcTriMesh _mesh;

        public static SimpleInputGeomProvider LoadFile(string objFilePath)
        {
            byte[] chunk = RcIO.ReadFileIfFound(objFilePath);
            var context = RcObjImporter.LoadContext(chunk);
            return new SimpleInputGeomProvider(context.vertexPositions, context.meshFaces);
        }

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
            bmin = new Vector3(vertices);
            bmax = new Vector3(vertices);
            for (int i = 1; i < vertices.Length / 3; i++)
            {
                bmin = Vector3.Min(bmin, RcVec.Create(vertices, i * 3));
                bmax = Vector3.Max(bmax, RcVec.Create(vertices, i * 3));
            }

            _mesh = new RcTriMesh(vertices, faces);
        }

        public RcTriMesh GetMesh()
        {
            return _mesh;
        }

        public Vector3 GetMeshBoundsMin()
        {
            return bmin;
        }

        public Vector3 GetMeshBoundsMax()
        {
            return bmax;
        }

        public IList<RcConvexVolume> ConvexVolumes()
        {
            return volumes;
        }

        public void AddConvexVolume(float[] verts, float minh, float maxh, RcAreaModification areaMod)
        {
            RcConvexVolume vol = new RcConvexVolume();
            vol.hmin = minh;
            vol.hmax = maxh;
            vol.verts = verts;
            vol.areaMod = areaMod;
        }

        public void AddConvexVolume(RcConvexVolume convexVolume)
        {
            volumes.Add(convexVolume);
        }

        public IEnumerable<RcTriMesh> Meshes()
        {
            return RcImmutableArray.Create(_mesh);
        }

        public int OffMeshConCount => throw new NotImplementedException();

        public float[] OffMeshConVerts => throw new NotImplementedException();

        public float[] OffMeshConRads => throw new NotImplementedException();

        public bool[] OffMeshConDirs => throw new NotImplementedException();

        public int[] OffMeshConAreas => throw new NotImplementedException();

        public int[] OffMeshConFlags => throw new NotImplementedException();

        public int[] OffMeshConId => throw new NotImplementedException();

        public void AddOffMeshConnection(Vector3 start, Vector3 end, float radius, bool bidir, int area, int flags)
        {
            throw new NotImplementedException();
        }

        public void CalculateNormals()
        {
            for (int i = 0; i < faces.Length; i += 3)
            {
                int v0 = faces[i] * 3;
                int v1 = faces[i + 1] * 3;
                int v2 = faces[i + 2] * 3;

                var e0 = new Vector3();
                var e1 = new Vector3();
                e0.X = vertices[v1 + 0] - vertices[v0 + 0];
                e0.Y = vertices[v1 + 1] - vertices[v0 + 1];
                e0.Z = vertices[v1 + 2] - vertices[v0 + 2];

                e1.X = vertices[v2 + 0] - vertices[v0 + 0];
                e1.Y = vertices[v2 + 1] - vertices[v0 + 1];
                e1.Z = vertices[v2 + 2] - vertices[v0 + 2];

                normals[i] = e0.Y * e1.Z - e0.Z * e1.Y;
                normals[i + 1] = e0.Z * e1.X - e0.X * e1.Z;
                normals[i + 2] = e0.X * e1.Y - e0.Y * e1.X;
                float d = MathF.Sqrt(normals[i] * normals[i] + normals[i + 1] * normals[i + 1] + normals[i + 2] * normals[i + 2]);
                if (d > 0)
                {
                    d = 1.0f / d;
                    normals[i] *= d;
                    normals[i + 1] *= d;
                    normals[i + 2] *= d;
                }
            }
        }

        public void RemoveOffMeshConnection(int idx)
        {
            throw new NotImplementedException();
        }
    }
}