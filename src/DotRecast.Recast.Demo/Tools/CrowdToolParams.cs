/*
recast4j copyright (c) 2020-2021 Piotr Piastucki piotr@jtilia.org

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

namespace DotRecast.Recast.Demo.Tools;

public class CrowdToolParams
{
    public readonly int[] m_expandSelectedDebugDraw = new[] { 1 };
    public bool m_showCorners;
    public bool m_showCollisionSegments;
    public bool m_showPath;
    public bool m_showVO;
    public bool m_showOpt;
    public bool m_showNeis;

    public readonly int[] m_expandDebugDraw = new[] { 0 };
    public bool m_showLabels;
    public bool m_showGrid;
    public bool m_showNodes;
    public bool m_showPerfGraph;
    public bool m_showDetailAll;

    public readonly int[] m_expandOptions = new[] { 1 };
    public bool m_anticipateTurns = true;
    public bool m_optimizeVis = true;
    public bool m_optimizeTopo = true;
    public bool m_obstacleAvoidance = true;
    public readonly int[] m_obstacleAvoidanceType = new[] { 3 };
    public bool m_separation;
    public readonly float[] m_separationWeight = new[] { 2f };
}