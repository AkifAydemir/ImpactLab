namespace ImpactLab.Core.Verification;

public enum SimulationCredibilityLevel
{
    NotAssessed,
    NumericalScreeningFailed,
    NumericalScreeningWithWarnings,
    NumericalScreeningPassed,
    Benchmarked,
    ExperimentallyValidated,
}

public sealed record SimulationCredibilityAssessment(
    SimulationCredibilityLevel Level,
    string Label,
    string PermittedUse,
    IReadOnlyList<string> SatisfiedEvidence,
    IReadOnlyList<string> MissingEvidence
)
{
    public bool IsValidated => Level == SimulationCredibilityLevel.ExperimentallyValidated;
}

public static class SimulationCredibilityAssessor
{
    public static SimulationCredibilityAssessment Assess(
        RunValidityAssessment? validity,
        bool sourceBenchmarksPassed = false
    )
    {
        IReadOnlyList<string> Evidence(params string[] items) =>
            sourceBenchmarksPassed
                ? [.. items, "Built-in component/source benchmarks passed for this software build."]
                : items;

        if (validity is null)
            return Build(
                SimulationCredibilityLevel.NotAssessed,
                "NOT ASSESSED",
                "No engineering decision; run validity was not assessed.",
                Evidence()
            );

        if (validity.HasFailure)
            return Build(
                SimulationCredibilityLevel.NumericalScreeningFailed,
                "NUMERICAL SCREENING FAILED — NOT VALIDATED",
                "Qualitative visualization and debugging only.",
                Evidence("Automated validity checks executed.")
            );

        if (validity.WarningCount > 0)
            return Build(
                SimulationCredibilityLevel.NumericalScreeningWithWarnings,
                "NUMERICAL SCREENING HAS WARNINGS — NOT VALIDATED",
                "Exploratory comparison only; do not use as a design allow/deny criterion.",
                Evidence("No automated validity check failed.")
            );

        return Build(
            SimulationCredibilityLevel.NumericalScreeningPassed,
            "NUMERICAL SCREENING PASSED — NOT VALIDATED",
            "Screening and relative option comparison only, within the modeled domain.",
            Evidence("All configured automated validity checks passed.")
        );
    }

    private static SimulationCredibilityAssessment Build(
        SimulationCredibilityLevel level,
        string label,
        string permittedUse,
        IReadOnlyList<string> satisfied
    ) =>
        new(
            level,
            label,
            permittedUse,
            satisfied,
            [
                "Mesh and time-step convergence with recorded response deltas.",
                "Material certificates or calibrated rate-dependent curves for the actual batch and temperature.",
                "Contact/friction calibration and complete energy-dissipation accounting.",
                "Application-relevant benchmark of the selected backend against an analytical solution or independently verified reference solver.",
                "Physical impact-test correlation with measurement uncertainty and acceptance limits.",
                "Sensitivity and uncertainty quantification for decision-driving inputs.",
            ]
        );
}
