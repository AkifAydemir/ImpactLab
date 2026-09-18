using ImpactLab.Core.Mathematics;

namespace ImpactLab.App.Authoring;

public sealed record GizmoInteractionState(
    GizmoHandleKind Handle,
    Vec3 StartWorld,
    Vec3 CurrentWorld,
    bool IsActive
);
