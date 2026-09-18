namespace ImpactLab.Core.Continuum.FiniteStrain;

public interface IObjectiveStressIntegrator
{
    Matrix3 RotateStress(Matrix3 previous, Matrix3 rotationIncrement);
    Matrix3 ObjectiveCorrection(ObjectiveStressUpdateContext context);
}
