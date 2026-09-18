using ImpactLab.Core.Analysis;
using ImpactLab.Core.Backends;
using ImpactLab.Core.Continuum.Results;
using ImpactLab.Core.Materials;
using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.Services;

public sealed record SimulationRunBundle(
    CompiledScenario Compiled,
    BackendRunOutput BackendOutput,
    BackendResultSummary BackendSummary,
    IReadOnlyList<ProbeSeries> Probes,
    EnergyBalanceSeries EnergyBalance,
    SimulationBackendDescriptor Backend
)
{
    public ImpactLab.Core.Simulation.SimulationResult Result => BackendOutput.Result;
    public RunResultSummary Summary => BackendSummary.Compatibility;
}

public sealed class SimulationRunnerService
{
    private readonly SimulationBackendRegistry _backends;

    public SimulationRunnerService(SimulationBackendRegistry? backends = null) =>
        _backends = backends ?? SimulationBackendRegistry.CreateDefault();

    public SimulationRunBundle Run(
        ScenarioDefinition scenario,
        MaterialCatalog materials,
        CancellationToken cancellationToken = default,
        string backendId = ExplicitLatticeBackend.BackendId
    )
    {
        var compiled = ScenarioCompiler.Compile(scenario, materials);
        var backend = _backends.Create(backendId);
        var output = backend.Run(
            new SimulationExecutionRequest(
                compiled,
                scenario.Settings,
                scenario.ContactSettings,
                scenario,
                materials
            ),
            cancellationToken
        );
        IReadOnlyList<ProbeSeries> probes = output.Native<ContinuumResult>() is { } c
            ? ContinuumProbeEvaluator.Evaluate(c, compiled.Probes)
            : ProbeEvaluator.Evaluate(compiled.Mesh, output.Result, compiled.Probes);
        return new(
            compiled,
            output,
            BackendResultAnalyzer.Analyze(backend.Descriptor.Id, output),
            probes,
            EnergyBalanceAnalyzer.Analyze(output.Result),
            backend.Descriptor
        );
    }
}
