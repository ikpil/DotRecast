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
using DotRecast.Core.Numerics;

namespace DotRecast.Recast.Geom
{
    public class RcPartitionedMesh : IRcTriMesh
    {
        private readonly float[] _verts;
        private readonly int[] _tris;
        private readonly List<RcPartitionedMeshNode> _nodes;

        private class RcPartitionedMeshNode
        {
            public RcVec2f bmin;
            public RcVec2f bmax;
            public int i;
            public int[] tris;
        }

        public RcPartitionedMesh(float[] verts, int[] tris) :
            this(verts, tris, 32)
        {
        }

        public RcPartitionedMesh(float[] verts, int[] tris, int trisPerChunk)
        {
            _verts = verts;
            _tris = tris;

            int ntris = tris.Length / 3;
            int nchunks = (ntris + trisPerChunk - 1) / trisPerChunk;
            _nodes = new List<RcPartitionedMeshNode>(nchunks);

            RcBoundsItem[] items = new RcBoundsItem[ntris];
            for (int i = 0; i < ntris; ++i)
            {
                items[i] = new RcBoundsItem();
            }

            for (int i = 0; i < ntris; i++)
            {
                int t = i * 3;
                RcBoundsItem item = items[i];
                item.i = i;

                item.bmin.X = item.bmax.X = verts[tris[t] * 3];
                item.bmin.Y = item.bmax.Y = verts[tris[t] * 3 + 2];
                for (int j = 1; j < 3; ++j)
                {
                    int v = tris[t + j] * 3;
                    item.bmin.X = Math.Min(item.bmin.X, verts[v]);
                    item.bmin.Y = Math.Min(item.bmin.Y, verts[v + 2]);
                    item.bmax.X = Math.Max(item.bmax.X, verts[v]);
                    item.bmax.Y = Math.Max(item.bmax.Y, verts[v + 2]);
                }
            }

            Subdivide(items, 0, ntris, trisPerChunk, _nodes, tris);
        }

        public int[] GetTris()
        {
            return _tris;
        }

        public float[] GetVerts()
        {
            return _verts;
        }

        public List<int[]> GetChunksOverlappingRect(RcVec2f bmin, RcVec2f bmax)
        {
            List<int[]> chunks = new List<int[]>();
            int i = 0;
            while (i < _nodes.Count)
            {
                RcPartitionedMeshNode node = _nodes[i];
                bool overlap = CheckOverlapRect(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    chunks.Add(node.tris);
                }

                if (overlap || isLeafNode)
                {
                    i++;
                }
                else
                {
                    i = -node.i;
                }
            }

            return chunks;
        }

        public List<int[]> GetChunksOverlappingSegment(RcVec2f p, RcVec2f q)
        {
            List<int[]> chunks = new List<int[]>();
            int i = 0;
            while (i < _nodes.Count)
            {
                RcPartitionedMeshNode node = _nodes[i];
                bool overlap = CheckOverlapSegment(p, q, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    chunks.Add(node.tris);
                }

                if (overlap || isLeafNode)
                {
                    i++;
                }
                else
                {
                    i = -node.i;
                }
            }

            return chunks;
        }

        private static void CalcExtends(RcBoundsItem[] items, int imin, int imax, ref RcVec2f bmin, ref RcVec2f bmax)
        {
            bmin = items[imin].bmin;
            bmax = items[imin].bmax;

            for (int i = imin + 1; i < imax; ++i)
            {
                RcBoundsItem item = items[i];
                bmin.X = Math.Min(bmin.X, item.bmin.X);
                bmin.Y = Math.Min(bmin.Y, item.bmin.Y);
                bmax.X = Math.Max(bmax.X, item.bmax.X);
                bmax.Y = Math.Max(bmax.Y, item.bmax.Y);
            }
        }

        private static int LongestAxis(float x, float y)
        {
            return y > x ? 1 : 0;
        }

        private static void Subdivide(
            RcBoundsItem[] items, int imin, int imax, int trisPerChunk,
            List<RcPartitionedMeshNode> nodes, int[] inTris)
        {
            int inum = imax - imin;
            RcPartitionedMeshNode node = new RcPartitionedMeshNode();
            nodes.Add(node);

            if (inum <= trisPerChunk)
            {
                CalcExtends(items, imin, imax, ref node.bmin, ref node.bmax);

                node.i = nodes.Count;
                node.tris = new int[inum * 3];
                int dst = 0;
                for (int i = imin; i < imax; ++i)
                {
                    int src = items[i].i * 3;
                    node.tris[dst++] = inTris[src];
                    node.tris[dst++] = inTris[src + 1];
                    node.tris[dst++] = inTris[src + 2];
                }
            }
            else
            {
                CalcExtends(items, imin, imax, ref node.bmin, ref node.bmax);
                int axis = LongestAxis(node.bmax.X - node.bmin.X, node.bmax.Y - node.bmin.Y);
                int isplit = imin + inum / 2;
                IComparer<RcBoundsItem> comparer = axis == 0
                    ? (IComparer<RcBoundsItem>)RcBoundsItemXComparer.Shared
                    : RcBoundsItemYComparer.Shared;
                RcNthElement.NthElement(items, imin, isplit, imax, comparer);

                Subdivide(items, imin, isplit, trisPerChunk, nodes, inTris);
                Subdivide(items, isplit, imax, trisPerChunk, nodes, inTris);
                node.i = -nodes.Count;
            }
        }

        private static bool CheckOverlapRect(RcVec2f amin, RcVec2f amax, RcVec2f bmin, RcVec2f bmax)
        {
            return amin.X <= bmax.X && amax.X >= bmin.X &&
                   amin.Y <= bmax.Y && amax.Y >= bmin.Y;
        }

        private static bool CheckOverlapSegment(RcVec2f p, RcVec2f q, RcVec2f bmin, RcVec2f bmax)
        {
            const float epsilon = 1e-6f;
            float tmin = 0;
            float tmax = 1;
            RcVec2f d = new RcVec2f(q.X - p.X, q.Y - p.Y);

            for (int i = 0; i < 2; i++)
            {
                if (MathF.Abs(d.Get(i)) < epsilon)
                {
                    if (p.Get(i) < bmin.Get(i) || p.Get(i) > bmax.Get(i))
                    {
                        return false;
                    }
                }
                else
                {
                    float inverseDirection = 1.0f / d.Get(i);
                    float t1 = (bmin.Get(i) - p.Get(i)) * inverseDirection;
                    float t2 = (bmax.Get(i) - p.Get(i)) * inverseDirection;
                    if (t1 > t2)
                    {
                        (t1, t2) = (t2, t1);
                    }

                    tmin = Math.Max(tmin, t1);
                    tmax = Math.Min(tmax, t2);
                    if (tmin > tmax)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
