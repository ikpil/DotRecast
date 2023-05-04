using System;
using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class CompositeGizmo : ColliderGizmo
{
    private readonly ColliderGizmo[] gizmos;

    public CompositeGizmo(params ColliderGizmo[] gizmos)
    {
        this.gizmos = gizmos;
    }

    public void Render(RecastDebugDraw debugDraw)
    {
        gizmos.ForEach(g => g.Render(debugDraw));
    }
}