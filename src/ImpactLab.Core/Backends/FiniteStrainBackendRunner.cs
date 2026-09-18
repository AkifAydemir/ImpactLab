using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.FiniteStrain;
using ImpactLab.Core.Continuum.Results;

namespace ImpactLab.Core.Backends;

public static class FiniteStrainBackendRunner
{
    public static BackendRunOutput Run(
        SimulationExecutionRequest request,
        CancellationToken ct = default
    )
    {
        var scenario =
            request.SourceScenario
            ?? throw new InvalidOperationException("Source scenario required.");
        var materials =
            request.Materials ?? throw new InvalidOperationException("Material catalog required.");
        var compiled = ContinuumScenarioCompiler.Compile(scenario, materials);
        var law = FiniteStrainLawRegistry
            .CreateDefault()
            .Create(scenario.Continuum.Advanced.FiniteStrainLawId);
        var native = new FiniteStrainContinuumDriver(law).Run(compiled, scenario.Continuum, ct);
        return ContinuumBackendAdapters.FromFiniteStrain(compiled, native);
    }
}
