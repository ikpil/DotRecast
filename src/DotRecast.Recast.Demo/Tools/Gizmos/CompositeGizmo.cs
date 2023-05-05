using System;
using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class CompositeGizmo : IColliderGizmo
{
    private readonly IColliderGizmo[] gizmos;

    public CompositeGizmo(params IColliderGizmo[] gizmos)
    {
        this.gizmos = gizmos;
    }

    public void Render(RecastDebugDraw debugDraw)
    {
        gizmos.ForEach(g => g.Render(debugDraw));
    }
}