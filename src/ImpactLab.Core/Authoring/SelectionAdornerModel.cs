using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Authoring;

public sealed record SelectionAdornerModel(
    string EntityId,
    BoundingBox3 Bounds,
    Vec3 Pivot,
    bool ShowTranslate = true,
    bool ShowRotate = true,
    bool ShowScale = true
);
