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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    public class DtProximityGrid
    {
        struct Item
        {
            public ushort id;
            public short x, y;
            public ushort next;
        }

        private readonly float m_cellSize;
        private readonly float m_invCellSize;

        Item[] m_pool;
        int m_poolHead;
        int m_poolSize;

        ushort[] m_buckets;
        int m_bucketsSize;

        int[] m_bounds = new int[4];

        public DtProximityGrid(int poolSize, float cellSize)
        {
            Debug.Assert(poolSize > 0);
            Debug.Assert(cellSize > 0.0f);

            m_cellSize = cellSize;
            m_invCellSize = 1.0f / cellSize;

            m_bucketsSize = (int)RcMath.dtNextPow2((uint)poolSize);
            m_buckets = new ushort[m_bucketsSize];

            m_poolSize = poolSize;
            m_poolHead = 0;
            m_pool = new Item[m_poolSize];

            Clear();
        }

        public void Clear()
        {
            m_buckets.AsSpan().Fill(0xffff);
            m_poolHead = 0;
            m_bounds[0] = 0xffff;
            m_bounds[1] = 0xffff;
            m_bounds[2] = -0xffff;
            m_bounds[3] = -0xffff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int hashPos2(int x, int y, int n)
        {
            return ((x * 73856093) ^ (y * 19349663)) & (n - 1);
        }

        public void AddItem(ushort id, float minx, float miny, float maxx, float maxy)
        {
            int iminx = (int)MathF.Floor(minx * m_invCellSize);
            int iminy = (int)MathF.Floor(miny * m_invCellSize);
            int imaxx = (int)MathF.Floor(maxx * m_invCellSize);
            int imaxy = (int)MathF.Floor(maxy * m_invCellSize);

            m_bounds[0] = Math.Min(m_bounds[0], iminx);
            m_bounds[1] = Math.Min(m_bounds[1], iminy);
            m_bounds[2] = Math.Min(m_bounds[2], imaxx);
            m_bounds[3] = Math.Min(m_bounds[3], imaxy);

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    if (m_poolHead < m_poolSize)
                    {
                        int h = hashPos2(x, y, m_bucketsSize);
                        ushort idx = (ushort)m_poolHead;
                        m_poolHead++;
                        ref var item = ref m_pool[idx];
                        item.x = (short)x;
                        item.y = (short)y;
                        item.id = id;
                        item.next = m_buckets[h];
                        m_buckets[h] = idx;
                    }
                }
            }
        }

        public int QueryItems(float minx, float miny, float maxx, float maxy, Span<ushort> ids, int maxIds)
        {
            int iminx = (int)MathF.Floor(minx * m_invCellSize);
            int iminy = (int)MathF.Floor(miny * m_invCellSize);
            int imaxx = (int)MathF.Floor(maxx * m_invCellSize);
            int imaxy = (int)MathF.Floor(maxy * m_invCellSize);

            int n = 0;

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    var h = hashPos2(x, y, m_bucketsSize);
                    ushort idx = m_buckets[h];
                    while (idx != 0xffff)
                    {
                        ref Item item = ref m_pool[idx];
                        if (item.x == x && item.y == y)
                        {
                            // Check if the id exists already.
                            ref var end = ref Unsafe.Add(ref MemoryMarshal.GetReference(ids), n);
                            ref var i = ref MemoryMarshal.GetReference(ids);
                            while (i != end && i != item.id)
                                i = ref Unsafe.Add(ref i, 1);
                            // Item not found, add it.
                            if (i == end)
                            {
                                if (n >= maxIds)
                                    return n;
                                ids[n++] = item.id;
                            }
                        }
                        idx = item.next;
                    }
                }
            }

            return n;
        }

        public int GetItemCountAt(int x, int y)
        {
            int n = 0;

            int h = hashPos2(x, y, m_bucketsSize);
            ushort idx = m_buckets[h];
            while (idx != 0xffff)
            {
                Item item = m_pool[idx];
                if (item.x == x && item.y == y)
                    n++;
                idx = item.next;
            }

            return n;
        }

        public ReadOnlySpan<int> GetBounds()
        {
            return m_bounds;
        }

        public float GetCellSize()
        {
            return m_cellSize;
        }
    }
}