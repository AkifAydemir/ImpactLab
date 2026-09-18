using ImpactLab.Core.Continuum.Contact;
using ImpactLab.Core.Continuum.FiniteStrain;
using ImpactLab.Core.Sparse;
using ImpactLab.Core.Thermal.Coupled;

namespace ImpactLab.Core.Continuum;

public sealed record AdvancedContinuumSettings
{
    public bool EnableFiniteStrain { get; init; } = false;
    public string FiniteStrainLawId { get; init; } = "finite-j2";
    public ObjectiveStressRateKind ObjectiveRate { get; init; } =
        ObjectiveStressRateKind.GreenNaghdi;
    public AdvancedContactMethod ContactMethod { get; init; } =
        AdvancedContactMethod.AugmentedLagrangian;
    public AugmentedContactSettings AugmentedContact { get; init; } = new();
    public MonolithicCouplingSettings MonolithicCoupling { get; init; } = new();
    public ScalableLinearSolverSettings LinearSolver { get; init; } = new();
}
