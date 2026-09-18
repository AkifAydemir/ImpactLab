using ImpactLab.Core.Mathematics;

namespace ImpactLab.App.Authoring.Gizmo;

public sealed record GizmoDragState(
    GizmoOperation Operation,
    GizmoAxis Axis,
    Vec3 StartWorld,
    Vec3 CurrentWorld,
    bool Snapping,
    double SnapStep
);
