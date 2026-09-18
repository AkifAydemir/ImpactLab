using ImpactLab.Core.Contact;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Backends;

public sealed record SimulationExecutionRequest(
    CompiledScenario Compiled,
    SimulationSettings Settings,
    ContactSettings ContactSettings,
    ScenarioDefinition? SourceScenario = null,
    MaterialCatalog? Materials = null,
    SimulationExecutionContext? Context = null
);
