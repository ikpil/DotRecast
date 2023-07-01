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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotRecast.Core;

namespace DotRecast.Detour.TileCache
{
    public abstract class AbstractTileLayersBuilder
    {
        protected List<byte[]> Build(RcByteOrder order, bool cCompatibility, int threads, int tw, int th)
        {
            if (threads == 1)
            {
                return BuildSingleThread(order, cCompatibility, tw, th);
            }

            return BuildMultiThread(order, cCompatibility, tw, th, threads);
        }

        private List<byte[]> BuildSingleThread(RcByteOrder order, bool cCompatibility, int tw, int th)
        {
            List<byte[]> layers = new List<byte[]>();
            for (int y = 0; y < th; ++y)
            {
                for (int x = 0; x < tw; ++x)
                {
                    layers.AddRange(Build(x, y, order, cCompatibility));
                }
            }

            return layers;
        }


        private List<byte[]> BuildMultiThread(RcByteOrder order, bool cCompatibility, int tw, int th, int threads)
        {
            var results = new List<DtTileCacheBuildResult>();
            for (int y = 0; y < th; ++y)
            {
                for (int x = 0; x < tw; ++x)
                {
                    int tx = x;
                    int ty = y;
                    var task = Task.Run(() => Build(tx, ty, order, cCompatibility));
                    results.Add(new DtTileCacheBuildResult(tx, ty, task));
                }
            }

            return results
                .SelectMany(x => x.task.Result)
                .ToList();
        }

        protected abstract List<byte[]> Build(int tx, int ty, RcByteOrder order, bool cCompatibility);
    }
}