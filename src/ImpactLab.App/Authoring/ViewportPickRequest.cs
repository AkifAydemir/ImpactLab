using ImpactLab.Core.Geometry.Imported;

namespace ImpactLab.App.Authoring;

public sealed record ViewportPickRequest(
    double ScreenX,
    double ScreenY,
    Ray3 Ray,
    bool Additive = false,
    bool Toggle = false
);
