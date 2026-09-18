namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed record FiniteStrainMaterialUpdate(
    FiniteStrainElementState State,
    Matrix3 KirchhoffStress,
    double[,] AlgorithmicTangent,
    double ElasticEnergyJ,
    double PlasticDissipationJ,
    bool Converged
);
