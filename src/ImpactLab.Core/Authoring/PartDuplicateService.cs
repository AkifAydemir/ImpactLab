using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Authoring;

public static class PartDuplicateService
{
    public static ScenarioPartDefinition Duplicate(
        ScenarioPartDefinition source,
        string? id = null,
        string? name = null
    ) =>
        new()
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            Name = name ?? source.Name + " Copy",
            GeometrySource = source.GeometrySource,
            GeometryKind = source.GeometryKind,
            GeometryParameters = source.GeometryParameters.Clone(),
            ImportedMeshPath = source.ImportedMeshPath,
            MaterialId = source.MaterialId,
            Behavior = source.Behavior,
            FixMinimumZPlane = source.FixMinimumZPlane,
            Transform = source.Transform,
        };
}
