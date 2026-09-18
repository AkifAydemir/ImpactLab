namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record UpdatedLagrangianElementKinematics(
    FiniteStrainKinematics Kinematics,
    double CurrentVolume,
    double ReferenceVolume
);
