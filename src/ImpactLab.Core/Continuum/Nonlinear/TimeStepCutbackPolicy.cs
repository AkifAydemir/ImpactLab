namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed record TimeStepCutbackPolicy(
    double Factor = 0.5,
    double Growth = 1.25,
    int SuccessfulStepsBeforeGrowth = 3
)
{
    public double Cut(double dt, double min) => Math.Max(min, dt * Factor);

    public double Grow(double dt, double max) => Math.Min(max, dt * Growth);
}
