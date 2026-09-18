namespace ImpactLab.Core.PostProcessing;

public sealed record ContactHistorySeries(
    string RigidBodyId,
    string TargetPartId,
    IReadOnlyList<ContactHistorySample> Samples
);
