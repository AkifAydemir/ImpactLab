using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Thermal;

namespace ImpactLab.Core.Continuum;

public sealed record ContinuumCompiledScenario(
    TetrahedralMesh Mesh,
    ScenarioDefinition Source,
    ThermalScenarioPackage Thermal
);
