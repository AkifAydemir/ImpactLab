using ImpactLab.Core.Geometry;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Authoring;

public readonly record struct GizmoDragResult(
    GeometryTransform3 Transform,
    Vec3 Delta,
    double ScalarDelta
);
