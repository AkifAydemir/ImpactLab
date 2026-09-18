namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record PolarDecompositionResult(
    Matrix3 Rotation,
    Matrix3 RightStretch,
    Matrix3 LeftStretch,
    int Iterations,
    double Residual
);
