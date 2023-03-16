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
using System.Linq;

namespace DotRecast.Detour.Crowd
{


public class ProximityGrid {

    private readonly float m_cellSize;
    private readonly float m_invCellSize;
    private readonly Dictionary<ItemKey, List<CrowdAgent>> items;

    public ProximityGrid(float m_cellSize) {
        this.m_cellSize = m_cellSize;
        m_invCellSize = 1.0f / m_cellSize;
        items = new Dictionary<ItemKey, List<CrowdAgent>>();
    }

    void clear() {
        items.Clear();
    }

    public void addItem(CrowdAgent agent, float minx, float miny, float maxx, float maxy) {
        int iminx = (int) Math.Floor(minx * m_invCellSize);
        int iminy = (int) Math.Floor(miny * m_invCellSize);
        int imaxx = (int) Math.Floor(maxx * m_invCellSize);
        int imaxy = (int) Math.Floor(maxy * m_invCellSize);

        for (int y = iminy; y <= imaxy; ++y) {
            for (int x = iminx; x <= imaxx; ++x) {
                ItemKey key = new ItemKey(x, y);
                if (!items.TryGetValue(key, out var ids)) {
                    ids = new List<CrowdAgent>();
                    items.Add(key, ids);
                }
                ids.Add(agent);
            }
        }
    }

    public HashSet<CrowdAgent> queryItems(float minx, float miny, float maxx, float maxy) {
        int iminx = (int) Math.Floor(minx * m_invCellSize);
        int iminy = (int) Math.Floor(miny * m_invCellSize);
        int imaxx = (int) Math.Floor(maxx * m_invCellSize);
        int imaxy = (int) Math.Floor(maxy * m_invCellSize);

        HashSet<CrowdAgent> result = new HashSet<CrowdAgent>();
        for (int y = iminy; y <= imaxy; ++y) {
            for (int x = iminx; x <= imaxx; ++x) {
                ItemKey key = new ItemKey(x, y);
                if (items.TryGetValue(key, out var ids)) {
                    result.UnionWith(ids);
                }
            }
        }

        return result;
    }

    public List<int[]> getItemCounts() {
        return items
            .Where(e => e.Value.Count > 0)
            .Select(e => new int[] { e.Key.x, e.Key.y, e.Value.Count })
            .ToList();
    }

    public float getCellSize() {
        return m_cellSize;
    }

    private class ItemKey {

        public readonly int x;
        public readonly int y;

        public ItemKey(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public override int GetHashCode() {
            int prime = 31;
            int result = 1;
            result = prime * result + x;
            result = prime * result + y;
            return result;
        }

        public override bool Equals(object? obj) {
            if (this == obj)
                return true;
            
            if (obj == null)
                return false;
            
            if (GetType() != obj.GetType())
                return false;
            
            ItemKey other = (ItemKey) obj;
            if (x != other.x)
                return false;
            
            if (y != other.y)
                return false;
            
            return true;
        }

    };
}

}