using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Projects;

public sealed record ImpactLabProject(
    ImpactLabProjectManifest Manifest,
    ScenarioDefinition Scenario,
    IReadOnlyList<MaterialProfile> Materials,
    string AssetDirectory
);
