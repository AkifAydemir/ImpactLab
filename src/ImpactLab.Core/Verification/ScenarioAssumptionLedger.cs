using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Verification;

public sealed record ScenarioAssumption(
    string Domain,
    string Parameter,
    string AssumedValue,
    string Basis,
    string Consequence
);

public sealed record ScenarioAssumptionLedger(IReadOnlyList<ScenarioAssumption> Items)
{
    public bool UsesAssumptions => Items.Count > 0;
    public string ResultClass =>
        UsesAssumptions ? "ASSUMPTION-BASED ESTIMATE" : "TRACEABLE INPUT RUN";
}

public static class ScenarioAssumptionLedgerBuilder
{
    public static ScenarioAssumptionLedger Build(
        ScenarioDefinition scenario,
        MaterialProfileCatalog profiles
    )
    {
        var items = new List<ScenarioAssumption>();
        if (scenario.Environment?.IsDefaultAssumption == true)
            items.Add(
                new(
                    "Environment",
                    "Gravity and atmosphere",
                    "Earth / ISA sea-level template",
                    scenario.Environment.Reference,
                    "Suitable as a documented default near sea level; replace for altitude, weather or another environment."
                )
            );

        foreach (
            var id in scenario
                .Parts.Select(x => x.MaterialId)
                .Concat(scenario.RigidBodies.Select(x => x.MaterialId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
        )
        {
            var profile = profiles.Get(id);
            if (profile.TensileCurve is null)
                items.Add(
                    new(
                        "Material",
                        profile.Definition.DisplayName,
                        "Nominal scalar/bilinear material response",
                        profile.Provenance.SourceName,
                        "Does not represent batch scatter, full plastic curve, dynamic fracture or flaw population."
                    )
                );
            if (profile.Uncertainty.Properties.Count == 0)
                items.Add(
                    new(
                        "Uncertainty",
                        profile.Definition.DisplayName,
                        "Deterministic nominal value",
                        "No uncertainty band supplied",
                        "Reported result has no statistical confidence interval."
                    )
                );
        }

        if (string.IsNullOrWhiteSpace(scenario.ContactSettings.CalibrationSource))
            items.Add(
                new(
                    "Contact",
                    "Penalty stiffness, damping and friction",
                    "ImpactLab default contact parameters",
                    "Engineering software default; not test-calibrated for this material pair",
                    "Peak force, rebound, penetration and dissipated energy may shift materially."
                )
            );
        return new(items);
    }
}
