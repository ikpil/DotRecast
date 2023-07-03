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
using DotRecast.Core;
using DotRecast.Recast.Geom;

namespace DotRecast.Recast.DemoTool.Geom
{
    public class DemoInputGeomProvider : IInputGeomProvider
    {
        public readonly float[] vertices;
        public readonly int[] faces;
        public readonly float[] normals;
        private readonly RcVec3f bmin;
        private readonly RcVec3f bmax;
        private readonly List<ConvexVolume> _convexVolumes = new List<ConvexVolume>();
        private readonly List<DemoOffMeshConnection> offMeshConnections = new List<DemoOffMeshConnection>();
        private readonly RcTriMesh _mesh;

        public DemoInputGeomProvider(List<float> vertexPositions, List<int> meshFaces) :
            this(MapVertices(vertexPositions), MapFaces(meshFaces))
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

        public DemoInputGeomProvider(float[] vertices, int[] faces)
        {
            this.vertices = vertices;
            this.faces = faces;
            normals = new float[faces.Length];
            CalculateNormals();
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

        public void CalculateNormals()
        {
            for (int i = 0; i < faces.Length; i += 3)
            {
                int v0 = faces[i] * 3;
                int v1 = faces[i + 1] * 3;
                int v2 = faces[i + 2] * 3;
                RcVec3f e0 = new RcVec3f();
                RcVec3f e1 = new RcVec3f();
                for (int j = 0; j < 3; ++j)
                {
                    e0[j] = vertices[v1 + j] - vertices[v0 + j];
                    e1[j] = vertices[v2 + j] - vertices[v0 + j];
                }

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

        public IList<ConvexVolume> ConvexVolumes()
        {
            return _convexVolumes;
        }

        public IEnumerable<RcTriMesh> Meshes()
        {
            return new []{_mesh};
        }

        public List<DemoOffMeshConnection> GetOffMeshConnections()
        {
            return offMeshConnections;
        }

        public void AddOffMeshConnection(RcVec3f start, RcVec3f end, float radius, bool bidir, int area, int flags)
        {
            offMeshConnections.Add(new DemoOffMeshConnection(start, end, radius, bidir, area, flags));
        }

        public void RemoveOffMeshConnections(Predicate<DemoOffMeshConnection> filter)
        {
            //offMeshConnections.RetainAll(offMeshConnections.Stream().Filter(c -> !filter.Test(c)).Collect(ToList()));
            offMeshConnections.RemoveAll(filter); // TODO : 확인 필요
        }

        public float? RaycastMesh(RcVec3f src, RcVec3f dst)
        {
            // Prune hit ray.
            if (!Intersections.IsectSegAABB(src, dst, bmin, bmax, out var btmin, out var btmax))
            {
                return null;
            }

            float[] p = new float[2];
            float[] q = new float[2];
            p[0] = src.x + (dst.x - src.x) * btmin;
            p[1] = src.z + (dst.z - src.z) * btmin;
            q[0] = src.x + (dst.x - src.x) * btmax;
            q[1] = src.z + (dst.z - src.z) * btmax;

            List<RcChunkyTriMeshNode> chunks = _mesh.chunkyTriMesh.GetChunksOverlappingSegment(p, q);
            if (0 == chunks.Count)
            {
                return null;
            }

            float? tmin = 1.0f;
            bool hit = false;
            foreach (RcChunkyTriMeshNode chunk in chunks)
            {
                int[] tris = chunk.tris;
                for (int j = 0; j < chunk.tris.Length; j += 3)
                {
                    RcVec3f v1 = RcVec3f.Of(
                        vertices[tris[j] * 3],
                        vertices[tris[j] * 3 + 1],
                        vertices[tris[j] * 3 + 2]
                    );
                    RcVec3f v2 = RcVec3f.Of(
                        vertices[tris[j + 1] * 3],
                        vertices[tris[j + 1] * 3 + 1],
                        vertices[tris[j + 1] * 3 + 2]
                    );
                    RcVec3f v3 = RcVec3f.Of(
                        vertices[tris[j + 2] * 3],
                        vertices[tris[j + 2] * 3 + 1],
                        vertices[tris[j + 2] * 3 + 2]
                    );
                    float? t = Intersections.IntersectSegmentTriangle(src, dst, v1, v2, v3);
                    if (null != t)
                    {
                        if (t.Value < tmin)
                        {
                            tmin = t.Value;
                        }

                        hit = true;
                    }
                }
            }

            return hit 
                ? tmin
                : null;
        }


        public void AddConvexVolume(float[] verts, float minh, float maxh, AreaModification areaMod)
        {
            ConvexVolume volume = new ConvexVolume();
            volume.verts = verts;
            volume.hmin = minh;
            volume.hmax = maxh;
            volume.areaMod = areaMod;
            _convexVolumes.Add(volume);
        }

        public void ClearConvexVolumes()
        {
            _convexVolumes.Clear();
        }
    }
}