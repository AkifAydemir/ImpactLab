namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record NonlinearConvergenceSettings(
    double ResidualRelative = 1e-6,
    double IncrementRelative = 1e-7,
    double EnergyRelative = 1e-8,
    int MaxIterations = 30
)
{
    public void Validate()
    {
        if (
            ResidualRelative <= 0
            || IncrementRelative <= 0
            || EnergyRelative <= 0
            || MaxIterations < 1
        )
            throw new InvalidOperationException("Invalid nonlinear convergence settings.");
    }
}
