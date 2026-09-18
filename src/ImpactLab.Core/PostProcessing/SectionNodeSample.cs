using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.PostProcessing;

public readonly record struct SectionNodeSample(
    int NodeId,
    string PartId,
    Vec3 Position,
    double DisplacementMeters,
    double SpeedMps,
    double Damage
);
