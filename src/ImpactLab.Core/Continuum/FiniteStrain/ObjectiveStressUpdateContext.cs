namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record ObjectiveStressUpdateContext(
    Matrix3 CauchyStress,
    Matrix3 RateOfDeformation,
    Matrix3 Spin,
    Matrix3 RotationIncrement,
    double TimeStepSeconds
);
