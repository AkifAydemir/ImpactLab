using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Backends;

public sealed record BackendRunOutput(
    SimulationResult Result,
    IResultDomain Domain,
    object? NativeResult = null
)
{
    public TNative? Native<TNative>()
        where TNative : class => NativeResult as TNative;
}
