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
using System.Runtime.CompilerServices;

namespace DotRecast.Detour.Crowd
{
    // Spatial hash mirroring recastnavigation's dtProximityGrid: a flat item
    // pool chained from power-of-two hash buckets. Clearing resets the pool
    // head and bucket heads, so a steady-state update allocates nothing.
    public class DtProximityGrid
    {
        private struct Item
        {
            public int id;
            public int x;
            public int y;
            public int next;
        }

        private readonly float _cellSize;
        private readonly float _invCellSize;

        private int[] _buckets; // index of the first pool item per bucket, -1 if empty
        private Item[] _pool;
        private int _poolHead;

        public DtProximityGrid(float cellSize)
        {
            _cellSize = cellSize;
            _invCellSize = 1.0f / cellSize;
            _pool = new Item[256];
            _buckets = new int[NextPow2(_pool.Length)];
            Array.Fill(_buckets, -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CombineKey(int x, int y)
        {
            uint ux = (uint)x;
            uint uy = (uint)y;
            return ((long)ux << 32) | uy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecomposeKey(long key, out int x, out int y)
        {
            uint ux = (uint)(key >> 32);
            uint uy = (uint)key;
            x = (int)ux;
            y = (int)uy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int HashPos2(int x, int y, int n)
        {
            return ((x * 73856093) ^ (y * 19349663)) & (n - 1);
        }

        private static int NextPow2(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        public void Clear()
        {
            Array.Fill(_buckets, -1);
            _poolHead = 0;
        }

        // Unlike upstream, the pool grows instead of dropping items: DtCrowd
        // does not cap the agent count, so the grid cannot size itself up front.
        private void Grow()
        {
            Array.Resize(ref _pool, _pool.Length * 2);
            _buckets = new int[NextPow2(_pool.Length)];
            Array.Fill(_buckets, -1);
            for (int i = 0; i < _poolHead; ++i)
            {
                ref Item item = ref _pool[i];
                int h = HashPos2(item.x, item.y, _buckets.Length);
                item.next = _buckets[h];
                _buckets[h] = i;
            }
        }

        public void AddItem(DtCrowdAgent agent, float minx, float miny, float maxx, float maxy)
        {
            int iminx = (int)MathF.Floor(minx * _invCellSize);
            int iminy = (int)MathF.Floor(miny * _invCellSize);
            int imaxx = (int)MathF.Floor(maxx * _invCellSize);
            int imaxy = (int)MathF.Floor(maxy * _invCellSize);

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    if (_poolHead >= _pool.Length)
                    {
                        Grow();
                    }

                    int h = HashPos2(x, y, _buckets.Length);
                    int idx = _poolHead;
                    _poolHead++;

                    ref Item item = ref _pool[idx];
                    item.x = x;
                    item.y = y;
                    item.id = agent.idx;
                    item.next = _buckets[h];
                    _buckets[h] = idx;
                }
            }
        }

        public int QueryItems(float minx, float miny, float maxx, float maxy, Span<int> ids, int maxIds)
        {
            int iminx = (int)MathF.Floor(minx * _invCellSize);
            int iminy = (int)MathF.Floor(miny * _invCellSize);
            int imaxx = (int)MathF.Floor(maxx * _invCellSize);
            int imaxy = (int)MathF.Floor(maxy * _invCellSize);

            int n = 0;

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    int idx = _buckets[HashPos2(x, y, _buckets.Length)];
                    while (idx != -1)
                    {
                        ref Item item = ref _pool[idx];
                        if (item.x == x && item.y == y)
                        {
                            // Check if the id exists already.
                            int end = n;
                            int i = 0;
                            while (i != end && ids[i] != item.id)
                            {
                                ++i;
                            }

                            // Item not found, add it.
                            if (i == n)
                            {
                                ids[n++] = item.id;

                                if (n >= maxIds)
                                    return n;
                            }
                        }

                        idx = item.next;
                    }
                }
            }

            return n;
        }

        public IEnumerable<(long, int)> GetItemCounts()
        {
            // Debug/visualization only - allocation here is fine.
            Dictionary<long, int> counts = new Dictionary<long, int>();
            for (int i = 0; i < _poolHead; ++i)
            {
                long key = CombineKey(_pool[i].x, _pool[i].y);
                counts.TryGetValue(key, out var count);
                counts[key] = count + 1;
            }

            foreach (var e in counts)
            {
                yield return (e.Key, e.Value);
            }
        }

        public float GetCellSize()
        {
            return _cellSize;
        }
    }
}
