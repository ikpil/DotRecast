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

namespace DotRecast.Recast
{
    /// Represents a set of heightfield layers.
    /// @ingroup recast
    /// @see rcAllocHeightfieldLayerSet, rcFreeHeightfieldLayerSet
    public class HeightfieldLayerSet
    {
        /// Represents a heightfield layer within a layer set.
        /// @see rcHeightfieldLayerSet
        public class HeightfieldLayer
        {
            public readonly float[] bmin = new float[3];

            /// < The minimum bounds in world space. [(x, y, z)]
            public readonly float[] bmax = new float[3];

            /// < The maximum bounds in world space. [(x, y, z)]
            public float cs;

            /// < The size of each cell. (On the xz-plane.)
            public float ch;

            /// < The height of each cell. (The minimum increment along the y-axis.)
            public int width;

            /// < The width of the heightfield. (Along the x-axis in cell units.)
            public int height;

            /// < The height of the heightfield. (Along the z-axis in cell units.)
            public int minx;

            /// < The minimum x-bounds of usable data.
            public int maxx;

            /// < The maximum x-bounds of usable data.
            public int miny;

            /// < The minimum y-bounds of usable data. (Along the z-axis.)
            public int maxy;

            /// < The maximum y-bounds of usable data. (Along the z-axis.)
            public int hmin;

            /// < The minimum height bounds of usable data. (Along the y-axis.)
            public int hmax;

            /// < The maximum height bounds of usable data. (Along the y-axis.)
            public int[] heights;

            /// < The heightfield. [Size: width * height]
            public int[] areas;

            /// < Area ids. [Size: Same as #heights]
            public int[] cons; /// < Packed neighbor connection information. [Size: Same as #heights]
        }

        public HeightfieldLayer[] layers; /// < The layers in the set. [Size: #nlayers]
    }
}