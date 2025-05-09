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
using DotRecast.Core.Numerics;

namespace DotRecast.Detour
{
    /// Provides information about raycast hit
    /// filled by dtNavMeshQuery::raycast
    /// @ingroup detour
    public ref struct DtRaycastHit
    {
        /// The hit parameter. (FLT_MAX if no wall hit.)
        public float t;

        /// hitNormal	The normal of the nearest wall hit. [(x, y, z)]
        public RcVec3f hitNormal;

        /// The index of the edge on the final polygon where the wall was hit.
        public int hitEdgeIndex;

        /// Pointer to an array of reference ids of the visited polygons. [opt]
        public Span<long> path;
        
        /// The number of visited polygons. [opt]
        public int pathCount;

        /// The maximum number of polygons the @p path array can hold.
        public int maxPath;

        ///  The cost of the path until hit.
        public float pathCost;
    }
}