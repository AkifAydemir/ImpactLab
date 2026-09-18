using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Results.Queries;

public sealed record SpatialQueryRegion(Vec3 Min, Vec3 Max, string? PartId = null);
