namespace ImpactLab.Core.PostProcessing;

public sealed record EnergyBalanceSeries(
    double ReferenceEnergyJ,
    IReadOnlyList<EnergyBalanceSample> Samples
);
