namespace ImpactLab.Core.Continuum.FiniteStrain;

public interface IFiniteStrainConstitutiveLaw
{
    string Id { get; }
    FiniteStrainMaterialUpdate Update(FiniteStrainMaterialContext context);
}
