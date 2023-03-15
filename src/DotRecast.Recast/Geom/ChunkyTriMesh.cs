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

namespace DotRecast.Recast.Geom;

public class ChunkyTriMesh
{
    private class BoundsItem
    {
        public readonly float[] bmin = new float[2];
        public readonly float[] bmax = new float[2];
        public int i;
    }

    private class CompareItemX : IComparer<BoundsItem>
    {
        public int Compare(BoundsItem? a, BoundsItem? b)
        {
            return a.bmin[0].CompareTo(b.bmin[0]);
        }
    }

    private class CompareItemY : IComparer<BoundsItem>
    {
        public int Compare(BoundsItem? a, BoundsItem? b)
        {
            return a.bmin[1].CompareTo(b.bmin[1]);
        }
    }

    List<ChunkyTriMeshNode> nodes;
    int ntris;
    int maxTrisPerChunk;

    private void calcExtends(BoundsItem[] items, int imin, int imax, float[] bmin, float[] bmax)
    {
        bmin[0] = items[imin].bmin[0];
        bmin[1] = items[imin].bmin[1];

        bmax[0] = items[imin].bmax[0];
        bmax[1] = items[imin].bmax[1];

        for (int i = imin + 1; i < imax; ++i)
        {
            BoundsItem it = items[i];
            if (it.bmin[0] < bmin[0])
            {
                bmin[0] = it.bmin[0];
            }

            if (it.bmin[1] < bmin[1])
            {
                bmin[1] = it.bmin[1];
            }

            if (it.bmax[0] > bmax[0])
            {
                bmax[0] = it.bmax[0];
            }

            if (it.bmax[1] > bmax[1])
            {
                bmax[1] = it.bmax[1];
            }
        }
    }

    private int longestAxis(float x, float y)
    {
        return y > x ? 1 : 0;
    }

    private void subdivide(BoundsItem[] items, int imin, int imax, int trisPerChunk, List<ChunkyTriMeshNode> nodes,
        int[] inTris)
    {
        int inum = imax - imin;

        ChunkyTriMeshNode node = new ChunkyTriMeshNode();
        nodes.Add(node);

        if (inum <= trisPerChunk)
        {
            // Leaf
            calcExtends(items, imin, imax, node.bmin, node.bmax);

            // Copy triangles.
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
            // Split
            calcExtends(items, imin, imax, node.bmin, node.bmax);

            int axis = longestAxis(node.bmax[0] - node.bmin[0], node.bmax[1] - node.bmin[1]);

            if (axis == 0)
            {
                Array.Sort(items, imin, imax - imin, new CompareItemX());
                // Sort along x-axis
            }
            else if (axis == 1)
            {
                Array.Sort(items, imin, imax - imin, new CompareItemY());
                // Sort along y-axis
            }

            int isplit = imin + inum / 2;

            // Left
            subdivide(items, imin, isplit, trisPerChunk, nodes, inTris);
            // Right
            subdivide(items, isplit, imax, trisPerChunk, nodes, inTris);

            // Negative index means escape.
            node.i = -nodes.Count;
        }
    }

    public ChunkyTriMesh(float[] verts, int[] tris, int ntris, int trisPerChunk)
    {
        int nchunks = (ntris + trisPerChunk - 1) / trisPerChunk;

        nodes = new(nchunks);
        this.ntris = ntris;

        // Build tree
        BoundsItem[] items = new BoundsItem[ntris];

        for (int i = 0; i < ntris; i++)
        {
            int t = i * 3;
            BoundsItem it = items[i] = new BoundsItem();
            it.i = i;
            // Calc triangle XZ bounds.
            it.bmin[0] = it.bmax[0] = verts[tris[t] * 3 + 0];
            it.bmin[1] = it.bmax[1] = verts[tris[t] * 3 + 2];
            for (int j = 1; j < 3; ++j)
            {
                int v = tris[t + j] * 3;
                if (verts[v] < it.bmin[0])
                {
                    it.bmin[0] = verts[v];
                }

                if (verts[v + 2] < it.bmin[1])
                {
                    it.bmin[1] = verts[v + 2];
                }

                if (verts[v] > it.bmax[0])
                {
                    it.bmax[0] = verts[v];
                }

                if (verts[v + 2] > it.bmax[1])
                {
                    it.bmax[1] = verts[v + 2];
                }
            }
        }

        subdivide(items, 0, ntris, trisPerChunk, nodes, tris);

        // Calc max tris per node.
        maxTrisPerChunk = 0;
        foreach (ChunkyTriMeshNode node in nodes)
        {
            bool isLeaf = node.i >= 0;
            if (!isLeaf)
            {
                continue;
            }

            if (node.tris.Length / 3 > maxTrisPerChunk)
            {
                maxTrisPerChunk = node.tris.Length / 3;
            }
        }
    }

    private bool checkOverlapRect(float[] amin, float[] amax, float[] bmin, float[] bmax)
    {
        bool overlap = true;
        overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
        overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
        return overlap;
    }

    public List<ChunkyTriMeshNode> getChunksOverlappingRect(float[] bmin, float[] bmax)
    {
        // Traverse tree
        List<ChunkyTriMeshNode> ids = new();
        int i = 0;
        while (i < nodes.Count)
        {
            ChunkyTriMeshNode node = nodes[i];
            bool overlap = checkOverlapRect(bmin, bmax, node.bmin, node.bmax);
            bool isLeafNode = node.i >= 0;

            if (isLeafNode && overlap)
            {
                ids.Add(node);
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

        return ids;
    }
}