namespace ImpactLab.Core.Materials;

public interface IConstitutiveLaw
{
    string Id { get; }
    ConstitutiveEvaluation Evaluate(MaterialProfile profile, in MaterialStatePoint state);
}
