using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Authoring;

public readonly record struct GeometryHit(
    string PartId,
    double Distance,
    Vec3 Point,
    Vec3 ApproximateNormal
);
