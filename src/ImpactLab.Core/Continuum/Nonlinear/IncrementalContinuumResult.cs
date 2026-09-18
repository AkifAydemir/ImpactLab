using ImpactLab.Core.Continuum.Static;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record IncrementalContinuumResult(
    IReadOnlyList<ContinuumStaticResult> Steps,
    IReadOnlyList<NonlinearIteration> Iterations
);
