using ImpactLab.Core.Geometry;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Scene;

public sealed record ScenePart(
    string Id,
    string Name,
    GeometrySpec Geometry,
    MaterialDefinition Material,
    ScenePartBehavior Behavior,
    IReadOnlyList<MaterialRegion>? MaterialRegions = null
);
