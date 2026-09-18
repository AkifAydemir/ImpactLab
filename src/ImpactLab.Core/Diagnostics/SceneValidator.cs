using ImpactLab.Core.Scene;

namespace ImpactLab.Core.Diagnostics;

public static class SceneValidator
{
    public static IReadOnlyList<ValidationIssue> Validate(SimulationScene scene)
    {
        var issues = new List<ValidationIssue>();
        if (scene.Parts.Count == 0)
            issues.Add(new(ValidationSeverity.Error, "SCENE_EMPTY", "Scene has no parts."));
        foreach (
            var duplicate in scene
                .Parts.GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1)
        )
            issues.Add(
                new(
                    ValidationSeverity.Error,
                    "PART_ID_DUPLICATE",
                    $"Duplicate part id '{duplicate.Key}'.",
                    duplicate.Key
                )
            );
        foreach (var part in scene.Parts)
        {
            try
            {
                part.Material.Validate();
            }
            catch (Exception ex)
            {
                issues.Add(new(ValidationSeverity.Error, "MATERIAL_INVALID", ex.Message, part.Id));
            }
            if (part.Geometry.Bounds.Size.LengthSquared <= 1e-18)
                issues.Add(
                    new(
                        ValidationSeverity.Error,
                        "GEOMETRY_DEGENERATE",
                        "Geometry bounds are degenerate.",
                        part.Id
                    )
                );
        }
        return issues;
    }
}
