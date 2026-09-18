namespace ImpactLab.Core.Continuum.Dynamics;

public readonly record struct DynamicStepDiagnostics(
    double TimeSeconds,
    double KineticEnergyJ,
    double InternalEnergyJ,
    double PlasticDissipationJ,
    double ContactEnergyJ,
    double ExternalWorkJ,
    double EnergyResidualJ,
    int ContactPairs,
    int PlasticElements
);
