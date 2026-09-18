using ImpactLab.Core.Geometry;

namespace ImpactLab.Core.Materials;

public sealed record MaterialRegion(
    string Id,
    GeometrySpec Geometry,
    MaterialDefinition Material,
    int Priority = 0
);
