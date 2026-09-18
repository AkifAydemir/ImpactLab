using ImpactLab.Core.Mathematics;

namespace ImpactLab.App.Visualization;

public sealed record FieldProbeCursor(
    Vec3 WorldPosition,
    int? NodeId,
    int? ElementId,
    string FieldId,
    double? Value
);
