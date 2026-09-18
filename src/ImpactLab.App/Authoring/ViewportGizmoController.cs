using ImpactLab.Core.Mathematics;

namespace ImpactLab.App.Authoring;

public sealed class ViewportGizmoController
{
    public GizmoInteractionState? State { get; private set; }

    public void Begin(GizmoHandleKind h, Vec3 p) => State = new(h, p, p, true);

    public Vec3 Update(Vec3 p)
    {
        if (State is null)
            return Vec3.Zero;
        var d = p - State.StartWorld;
        State = State with { CurrentWorld = p };
        return d;
    }

    public void End() => State = null;
}
