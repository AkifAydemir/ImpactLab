using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Verification;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ScenarioInputReadinessAssessorTests
{
    [Fact]
    public void NominalBuiltInsAreNotQuantitativeImpactReady()
    {
        var materials = new MaterialCatalog();
        var result = ScenarioInputReadinessAssessor.Assess(
            ScenarioPresetLibrary.CreatePlateImpact(),
            new MaterialProfileCatalog(materials)
        );

        Assert.False(result.QuantitativeImpactReady);
        Assert.True(result.BlockerCount > 0);
        Assert.Contains(
            result.Findings,
            x => x.Domain == "Material" && x.Requirement.Contains("batch/lot")
        );

        var assumptions = ScenarioAssumptionLedgerBuilder.Build(
            ScenarioPresetLibrary.CreatePlateImpact(),
            new MaterialProfileCatalog(materials)
        );
        Assert.Equal("ASSUMPTION-BASED ESTIMATE", assumptions.ResultClass);
        Assert.Contains(
            assumptions.Items,
            x => x.Domain == "Environment" && x.AssumedValue.Contains("Earth")
        );
        Assert.Contains(assumptions.Items, x => x.Domain == "Contact");
    }

    [Fact]
    public void TraceableCalibratedInputsCanPassReadinessGate()
    {
        var materials = new MaterialCatalog();
        var profiles = new MaterialProfileCatalog(materials);
        var uncertainty = new MaterialUncertainty();
        uncertainty.Set("yield-strength", new(355e6, 345e6, 365e6, "Pa"));
        profiles.Upsert(
            new MaterialProfile(materials.Get("generic-steel"))
            {
                TensileCurve = new ConstitutiveCurve([new(0, 0), new(0.1, 400e6)]),
                Provenance = new(
                    "Accredited impact-material campaign",
                    "https://example.invalid/evidence/steel-lot-42",
                    "Lab procedure IMP-01",
                    "LOT-42"
                ),
                Calibration = new(1e6, 0.8e6, 0.99, 40, "tabulated-profile"),
                Uncertainty = uncertainty,
            }
        );
        var scenario = ScenarioPresetLibrary.CreatePlateImpact();
        scenario.ContactSettings = scenario.ContactSettings with
        {
            CalibrationSource = "Instrumented drop test IMP-CONTACT-01",
            CalibrationTestId = "IMP-CONTACT-01",
        };
        scenario.Environment = ScenarioEnvironmentCatalog.EarthSeaLevelTemplate;

        var result = ScenarioInputReadinessAssessor.Assess(scenario, profiles);

        Assert.True(result.QuantitativeImpactReady);
        Assert.Equal(0, result.BlockerCount);
    }
}
