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
using DotRecast.Recast.Geom;

namespace DotRecast.Recast.Toolset.Geom
{
    public class DemoInputGeomProvider : IInputGeomProvider
    {
        public readonly float[] vertices;
        public readonly int[] faces;
        public readonly float[] normals;
        private readonly Vector3 bmin;
        private readonly Vector3 bmax;

        private readonly List<RcConvexVolume> _convexVolumes = new List<RcConvexVolume>();
        private readonly List<RcOffMeshConnection> _offMeshConnections = new List<RcOffMeshConnection>();
        private readonly RcTriMesh _mesh;

        public static DemoInputGeomProvider LoadFile(string objFilePath)
        {
            byte[] chunk = RcIO.ReadFileIfFound(objFilePath);
            var context = RcObjImporter.LoadContext(chunk);
            return new DemoInputGeomProvider(context.vertexPositions, context.meshFaces);
        }

        public DemoInputGeomProvider(List<float> vertexPositions, List<int> meshFaces) :
            this(MapVertices(vertexPositions), MapFaces(meshFaces))
        {
        }

        public DemoInputGeomProvider(float[] vertices, int[] faces)
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

        public void CalculateNormals()
        {
            for (int i = 0; i < faces.Length; i += 3)
            {
                Vector3 v0 = RcVec.Create(vertices, faces[i] * 3);
                Vector3 v1 = RcVec.Create(vertices, faces[i + 1] * 3);
                Vector3 v2 = RcVec.Create(vertices, faces[i + 2] * 3);
                Vector3 e0 = v1 - v0;
                Vector3 e1 = v2 - v0;

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

        public IList<RcConvexVolume> ConvexVolumes()
        {
            return _convexVolumes;
        }

        public IEnumerable<RcTriMesh> Meshes()
        {
            return RcImmutableArray.Create(_mesh);
        }

        public List<RcOffMeshConnection> GetOffMeshConnections()
        {
            return _offMeshConnections;
        }

        public void AddOffMeshConnection(Vector3 start, Vector3 end, float radius, bool bidir, int area, int flags)
        {
            _offMeshConnections.Add(new RcOffMeshConnection(start, end, radius, bidir, area, flags));
        }

        public void RemoveOffMeshConnections(Predicate<RcOffMeshConnection> filter)
        {
            //offMeshConnections.RetainAll(offMeshConnections.Stream().Filter(c -> !filter.Test(c)).Collect(ToList()));
            _offMeshConnections.RemoveAll(filter); // TODO : 확인 필요
        }

        public bool RaycastMesh(Vector3 src, Vector3 dst, out float tmin)
        {
            tmin = 1.0f;

            // Prune hit ray.
            if (!RcIntersections.IsectSegAABB(src, dst, bmin, bmax, out var btmin, out var btmax))
            {
                return false;
            }

            var p = new Vector2();
            var q = new Vector2();
            p.X = src.X + (dst.X - src.X) * btmin;
            p.Y = src.Z + (dst.Z - src.Z) * btmin;
            q.X = src.X + (dst.X - src.X) * btmax;
            q.Y = src.Z + (dst.Z - src.Z) * btmax;

            List<RcChunkyTriMeshNode> chunks = RcChunkyTriMeshs.GetChunksOverlappingSegment(_mesh.chunkyTriMesh, p, q);
            if (0 == chunks.Count)
            {
                return false;
            }

            tmin = 1.0f;
            bool hit = false;
            foreach (RcChunkyTriMeshNode chunk in chunks)
            {
                int[] tris = chunk.tris;
                for (int j = 0; j < chunk.tris.Length; j += 3)
                {
                    Vector3 v1 = new Vector3(
                        vertices[tris[j] * 3],
                        vertices[tris[j] * 3 + 1],
                        vertices[tris[j] * 3 + 2]
                    );
                    Vector3 v2 = new Vector3(
                        vertices[tris[j + 1] * 3],
                        vertices[tris[j + 1] * 3 + 1],
                        vertices[tris[j + 1] * 3 + 2]
                    );
                    Vector3 v3 = new Vector3(
                        vertices[tris[j + 2] * 3],
                        vertices[tris[j + 2] * 3 + 1],
                        vertices[tris[j + 2] * 3 + 2]
                    );
                    if (RcIntersections.IntersectSegmentTriangle(src, dst, v1, v2, v3, out var t))
                    {
                        if (t < tmin)
                        {
                            tmin = t;
                        }

                        hit = true;
                    }
                }
            }

            return hit;
        }


        public void AddConvexVolume(float[] verts, float minh, float maxh, RcAreaModification areaMod)
        {
            RcConvexVolume volume = new RcConvexVolume();
            volume.verts = verts;
            volume.hmin = minh;
            volume.hmax = maxh;
            volume.areaMod = areaMod;
            AddConvexVolume(volume);
        }

        public void AddConvexVolume(RcConvexVolume volume)
        {
            _convexVolumes.Add(volume);
        }

        public void ClearConvexVolumes()
        {
            _convexVolumes.Clear();
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
    }
}