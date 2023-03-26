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

using DotRecast.Detour.Extras.Jumplink;

namespace DotRecast.Recast.Demo.Tools;

public class JumpLinkBuilderToolParams
{
    public const int DRAW_WALKABLE_SURFACE = 1 << 0;
    public const int DRAW_WALKABLE_BORDER = 1 << 1;
    public const int DRAW_SELECTED_EDGE = 1 << 2;
    public const int DRAW_ANIM_TRAJECTORY = 1 << 3;
    public const int DRAW_LAND_SAMPLES = 1 << 4;
    public const int DRAW_COLLISION_SLICES = 1 << 5;
    public const int DRAW_ANNOTATIONS = 1 << 6;

    public int flags = DRAW_WALKABLE_SURFACE | DRAW_WALKABLE_BORDER | DRAW_SELECTED_EDGE | DRAW_ANIM_TRAJECTORY | DRAW_LAND_SAMPLES | DRAW_ANNOTATIONS;
    public float groundTolerance = 0.3f;
    public float climbDownDistance = 0.4f;
    public float climbDownMaxHeight = 3.2f;
    public float climbDownMinHeight = 1.5f;
    public float edgeJumpEndDistance = 2f;
    public float edgeJumpHeight = 0.4f;
    public float edgeJumpDownMaxHeight = 2.5f;
    public float edgeJumpUpMaxHeight = 0.3f;
    public int buildTypes = JumpLinkType.EDGE_CLIMB_DOWN.Bit | JumpLinkType.EDGE_JUMP.Bit;
}