using ImpactLab.Core.Analysis;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Reporting;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Verification;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class EngineeringReportBuilderTests
{
    [Fact]
    public void BuildsRunSummarySection()
    {
        var r = new RunResultSummary(1, 0, 0, .01, 0, 0, 0, 0, []);
        var d = EngineeringReportBuilder.Build(
            "S",
            "B",
            new BackendResultSummary("B", "Lattice", r, null)
        );
        Assert.NotEmpty(d.Sections);
    }

    [Fact]
    public void DetailedReportDoesNotClaimValidationFromPassingAutomatedChecks()
    {
        var summary = new RunResultSummary(1, 0, 0, .01, 0, 0, 0, 0, []);
        var validity = new RunValidityAssessment([
            new("check", "Check", RunValidityStatus.Passed, 1, 2, "-", "ok"),
        ]);

        var report = EngineeringReportBuilder.BuildDetailed(
            ScenarioPresetLibrary.CreatePlateImpact(),
            "Explicit lattice",
            new BackendResultSummary("B", "Lattice", summary, null),
            new MaterialCatalog(),
            validity
        );

        var credibility = Assert.Single(report.Sections, x => x.Title == "Credibility statement");
        Assert.Contains(
            credibility.Metrics,
            x => x.Name == "Experimentally validated" && x.Value == "NO"
        );
        Assert.Contains(report.Sections, x => x.Title == "Quantity definitions and provenance");
        Assert.Contains(report.Sections, x => x.Title == "Material-data evidence");
        var readiness = Assert.Single(report.Sections, x => x.Title == "Input-data readiness gate");
        Assert.Contains(
            readiness.Metrics,
            x => x.Name == "Quantitative impact ready" && x.Value == "NO"
        );
        var assumptions = Assert.Single(report.Sections, x => x.Title == "Assumption ledger");
        Assert.Contains(
            assumptions.Metrics,
            x => x.Name == "Result class" && x.Value == "ASSUMPTION-BASED ESTIMATE"
        );
    }

    [Fact]
    public void FailedValidityCheckRestrictsResultToQualitativeUse()
    {
        var assessment = SimulationCredibilityAssessor.Assess(
            new RunValidityAssessment([
                new("energy", "Energy", RunValidityStatus.Failed, 32, 5, "%", "bad"),
            ])
        );

        Assert.Equal(SimulationCredibilityLevel.NumericalScreeningFailed, assessment.Level);
        Assert.Contains("Qualitative", assessment.PermittedUse);
        Assert.False(assessment.IsValidated);
    }

    [Fact]
    public void UserEnvironmentDrivesSolverGravityProfilesAndReport()
    {
        var scenario = ScenarioPresetLibrary.CreatePlateImpact();
        scenario.Environment = new ScenarioEnvironmentDefinition
        {
            Id = "user-environment",
            DisplayName = "User-defined low-gravity environment",
            GravityMps2 = new(0, 0, -3.73),
            AtmosphericDensityKgPerM3 = 0.016,
            PressurePa = 636,
            TemperatureKelvin = 214,
            Reference = "User supplied",
            DensityByAltitude = [new(0, 0.016), new(10_000, 0.006)],
        };
        scenario.Validate();

        Assert.Equal(-3.73, scenario.Settings.Gravity.Z, 5);
        Assert.Equal(0.011, scenario.Environment.Sample(5_000).AtmosphericDensityKgPerM3, 6);
        var restored = ScenarioCloner.Clone(scenario);
        Assert.Equal(2, restored.Environment!.DensityByAltitude.Count);
        Assert.Equal(0.011, restored.Environment.Sample(5_000).AtmosphericDensityKgPerM3, 6);

        var summary = new RunResultSummary(1, 0, 0, .01, 0, 0, 0, 0, []);
        var report = EngineeringReportBuilder.BuildDetailed(
            scenario,
            "Explicit lattice",
            new BackendResultSummary("B", "Lattice", summary, null),
            new MaterialCatalog()
        );
        var environment = Assert.Single(report.Sections, x => x.Title == "Physical environment");
        Assert.Contains(
            environment.Metrics,
            x => x.Name == "Aerodynamic forces" && x.Value == "NOT MODELED"
        );
        Assert.Contains(
            environment.Metrics,
            x => x.Name == "Altitude-dependent samples" && x.Value == "2"
        );
    }
}
