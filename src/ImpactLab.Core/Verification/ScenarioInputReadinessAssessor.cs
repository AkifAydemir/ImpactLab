using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Verification;

public enum InputReadinessSeverity
{
    Information,
    Warning,
    Blocker,
}

public sealed record InputReadinessFinding(
    string Domain,
    InputReadinessSeverity Severity,
    string Item,
    string Requirement
);

public sealed record ScenarioInputReadinessAssessment(IReadOnlyList<InputReadinessFinding> Findings)
{
    public bool QuantitativeImpactReady =>
        Findings.All(x => x.Severity != InputReadinessSeverity.Blocker);
    public int BlockerCount => Findings.Count(x => x.Severity == InputReadinessSeverity.Blocker);
    public int WarningCount => Findings.Count(x => x.Severity == InputReadinessSeverity.Warning);
}

public static class ScenarioInputReadinessAssessor
{
    public static ScenarioInputReadinessAssessment Assess(
        ScenarioDefinition scenario,
        MaterialProfileCatalog profiles
    )
    {
        var findings = new List<InputReadinessFinding>();
        var materialIds = scenario
            .Parts.Select(x => x.MaterialId)
            .Concat(scenario.RigidBodies.Select(x => x.MaterialId))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var id in materialIds)
        {
            var profile = profiles.Get(id);
            var name = profile.Definition.DisplayName;
            if (profile.TensileCurve is null)
                Block("Material", name, "Traceable true stress–plastic strain curve is missing.");
            if (string.IsNullOrWhiteSpace(profile.Provenance.BatchOrLot))
                Block("Material", name, "Actual batch/lot or specimen identity is missing.");
            if (profile.Calibration is null)
                Block("Material", name, "Constitutive-model calibration metrics are missing.");
            if (profile.Uncertainty.Properties.Count == 0)
                Warn(
                    "Material",
                    name,
                    "No uncertainty bands are declared for material properties."
                );
            if (!profile.Provenance.IsTraceable)
                Block(
                    "Material",
                    name,
                    "A traceable source URI, specification or batch is required."
                );
        }

        if (string.IsNullOrWhiteSpace(scenario.ContactSettings.CalibrationSource))
            Block(
                "Contact",
                "Penalty contact and friction",
                "Contact stiffness, damping and friction require a calibration source/test."
            );
        if (scenario.Environment is null)
            Warn(
                "Environment",
                "Physical environment",
                "Environment is undeclared; confirm that zero gravity/vacuum is intentional."
            );
        else if (string.IsNullOrWhiteSpace(scenario.Environment.Reference))
            Warn(
                "Environment",
                scenario.Environment.DisplayName,
                "Environment values have no declared source."
            );

        foreach (
            var part in scenario.Parts.Where(x =>
                x.GeometrySource == ScenarioGeometrySourceKind.ImportedMesh
            )
        )
            Warn(
                "Geometry",
                part.Name,
                "Imported geometry has a path and unit but no dimensional inspection record or source hash."
            );

        return new(findings);

        void Block(string domain, string item, string requirement) =>
            findings.Add(new(domain, InputReadinessSeverity.Blocker, item, requirement));
        void Warn(string domain, string item, string requirement) =>
            findings.Add(new(domain, InputReadinessSeverity.Warning, item, requirement));
    }
}
