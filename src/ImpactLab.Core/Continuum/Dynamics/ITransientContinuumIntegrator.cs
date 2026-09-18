namespace ImpactLab.Core.Continuum.Dynamics;

public interface ITransientContinuumIntegrator
{
    string Id { get; }
    void Begin(ContinuumDynamicState state, double[] lumpedMass);
    void Step(
        ContinuumDynamicState state,
        ReadOnlySpan<double> force,
        double[] lumpedMass,
        double dt,
        Action<ContinuumDynamicState>? project = null
    );
}
