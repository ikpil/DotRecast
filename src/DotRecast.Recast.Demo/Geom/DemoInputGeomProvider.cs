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
using System.Linq;
using DotRecast.Core;
using DotRecast.Recast.Geom;

namespace DotRecast.Recast.Demo.Geom;

public class DemoInputGeomProvider : InputGeomProvider
{
    public readonly float[] vertices;
    public readonly int[] faces;
    public readonly float[] normals;
    private readonly Vector3f bmin;
    private readonly Vector3f bmax;
    private readonly List<ConvexVolume> _convexVolumes = new();
    private readonly List<DemoOffMeshConnection> offMeshConnections = new();
    private readonly ChunkyTriMesh chunkyTriMesh;

    public DemoInputGeomProvider(List<float> vertexPositions, List<int> meshFaces) :
        this(mapVertices(vertexPositions), mapFaces(meshFaces))
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

    public DemoInputGeomProvider(float[] vertices, int[] faces)
    {
        this.vertices = vertices;
        this.faces = faces;
        normals = new float[faces.Length];
        calculateNormals();
        bmin = new float[3];
        bmax = new float[3];
        RecastVectors.copy(bmin, vertices, 0);
        RecastVectors.copy(bmax, vertices, 0);
        for (int i = 1; i < vertices.Length / 3; i++)
        {
            RecastVectors.min(bmin, vertices, i * 3);
            RecastVectors.max(bmax, vertices, i * 3);
        }

        chunkyTriMesh = new ChunkyTriMesh(vertices, faces, faces.Length / 3, 256);
    }

    public float[] getMeshBoundsMin()
    {
        return bmin;
    }

    public float[] getMeshBoundsMax()
    {
        return bmax;
    }

    public void calculateNormals()
    {
        for (int i = 0; i < faces.Length; i += 3)
        {
            int v0 = faces[i] * 3;
            int v1 = faces[i + 1] * 3;
            int v2 = faces[i + 2] * 3;
            Vector3f e0 = new Vector3f();
            Vector3f e1 = new Vector3f();
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

    public IList<ConvexVolume> convexVolumes()
    {
        return _convexVolumes;
    }

    public IEnumerable<TriMesh> meshes()
    {
        return ImmutableArray.Create(new TriMesh(vertices, faces));
    }

    public List<DemoOffMeshConnection> getOffMeshConnections()
    {
        return offMeshConnections;
    }

    public void addOffMeshConnection(float[] start, float[] end, float radius, bool bidir, int area, int flags)
    {
        offMeshConnections.Add(new DemoOffMeshConnection(start, end, radius, bidir, area, flags));
    }

    public void removeOffMeshConnections(Predicate<DemoOffMeshConnection> filter)
    {
        //offMeshConnections.retainAll(offMeshConnections.stream().filter(c -> !filter.test(c)).collect(toList()));
        offMeshConnections.RemoveAll(filter); // TODO : 확인 필요
    }

    public float? raycastMesh(float[] src, Vector3f dst)
    {
        // Prune hit ray.
        float[] btminmax = Intersections.intersectSegmentAABB(src, dst, bmin, bmax);
        if (null == btminmax)
        {
            return null;
        }

        float btmin = btminmax[0];
        float btmax = btminmax[1];
        float[] p = new float[2], q = new float[2];
        p[0] = src[0] + (dst[0] - src[0]) * btmin;
        p[1] = src[2] + (dst[2] - src[2]) * btmin;
        q[0] = src[0] + (dst[0] - src[0]) * btmax;
        q[1] = src[2] + (dst[2] - src[2]) * btmax;

        List<ChunkyTriMeshNode> chunks = chunkyTriMesh.getChunksOverlappingSegment(p, q);
        if (0 == chunks.Count)
        {
            return null;
        }

        float tmin = 1.0f;
        bool hit = false;
        foreach (ChunkyTriMeshNode chunk in chunks)
        {
            int[] tris = chunk.tris;
            for (int j = 0; j < chunk.tris.Length; j += 3)
            {
                float[] v1 = new float[]
                {
                    vertices[tris[j] * 3], vertices[tris[j] * 3 + 1],
                    vertices[tris[j] * 3 + 2]
                };
                float[] v2 = new float[]
                {
                    vertices[tris[j + 1] * 3], vertices[tris[j + 1] * 3 + 1],
                    vertices[tris[j + 1] * 3 + 2]
                };
                float[] v3 = new float[]
                {
                    vertices[tris[j + 2] * 3], vertices[tris[j + 2] * 3 + 1],
                    vertices[tris[j + 2] * 3 + 2]
                };
                float? t = Intersections.intersectSegmentTriangle(src, dst, v1, v2, v3);
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

        return hit ? tmin : null;
    }


    public void addConvexVolume(float[] verts, float minh, float maxh, AreaModification areaMod)
    {
        ConvexVolume volume = new ConvexVolume();
        volume.verts = verts;
        volume.hmin = minh;
        volume.hmax = maxh;
        volume.areaMod = areaMod;
        _convexVolumes.Add(volume);
    }

    public void clearConvexVolumes()
    {
        _convexVolumes.Clear();
    }
}