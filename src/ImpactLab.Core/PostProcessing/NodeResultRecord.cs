using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.PostProcessing;

public readonly record struct NodeResultRecord(
    int NodeId,
    string PartId,
    Vec3 Position,
    double Value,
    double Damage
);
