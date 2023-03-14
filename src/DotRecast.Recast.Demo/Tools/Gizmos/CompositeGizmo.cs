using DotRecast.Core;
using DotRecast.Recast.Demo.Draw;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class CompositeGizmo : ColliderGizmo {

    private readonly ColliderGizmo[] gizmos;

    public CompositeGizmo(params ColliderGizmo[] gizmos) {
        this.gizmos = gizmos;
    }

    public void render(RecastDebugDraw debugDraw) {
        gizmos.forEach(g => g.render(debugDraw));
    }
}