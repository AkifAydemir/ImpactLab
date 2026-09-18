using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.PostProcessing;

public sealed record ResultQuery(
    int FrameIndex,
    ResultFieldKind Field,
    string? PartId = null,
    BoundingBox3? Bounds = null,
    double? MinimumValue = null,
    double? MaximumValue = null,
    int Limit = 100000
);
