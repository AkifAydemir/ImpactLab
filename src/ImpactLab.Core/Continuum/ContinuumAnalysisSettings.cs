using System.Text.Json.Serialization;
using ImpactLab.Core.Continuum.Dynamics;
using ImpactLab.Core.Continuum.Nonlinear;

namespace ImpactLab.Core.Continuum;

public sealed record ContinuumAnalysisSettings
{
    public ContinuumSolveMode Mode { get; init; } = ContinuumSolveMode.LinearStatic;
    public double TargetEdgeLengthMeters { get; init; } = 0.015;
    public int MaxCellsPerAxis { get; init; } = 160;
    public bool PreferNativeSparse { get; init; } = false;
    public int LoadSteps { get; init; } = 10;
    public int MaxNewtonIterations { get; init; } = 12;
    public ContinuumDynamicSettings Dynamics { get; init; } = new();
    public NonlinearTransientSettings ImplicitNonlinear { get; init; } = new();
    public AdvancedContinuumSettings Advanced { get; init; } = new();
    public bool EnableThermalCoupling { get; init; } = false;
    public bool EnableSelfContact { get; init; } = false;

    [JsonIgnore]
    public ContinuumDynamicSettings Dynamic => Dynamics;

    public void Validate()
    {
        if (
            TargetEdgeLengthMeters <= 0
            || MaxCellsPerAxis < 2
            || LoadSteps < 1
            || MaxNewtonIterations < 1
        )
            throw new InvalidOperationException("Invalid continuum settings.");
        if (Mode == ContinuumSolveMode.TransientDynamic || Dynamics.Enabled)
            Dynamics.Validate();
        if (Mode == ContinuumSolveMode.ImplicitNonlinearTransient)
            ImplicitNonlinear.Validate();
    }
}
