using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Verification;

public static class RunValidityAssessor
{
    public static RunValidityAssessment Assess(
        ScenarioDefinition scenario,
        CompiledScenario compiled,
        SimulationResult result,
        EnergyBalanceSeries energy
    )
    {
        var checks = new List<RunValidityCheck>();
        var materials = compiled.Mesh.Nodes.Select(x => x.Material).DistinctBy(x => x.Id).ToArray();
        var maximumWaveSpeed =
            materials.Length == 0
                ? 0
                : materials.Max(x => Math.Sqrt(x.YoungModulusPa / x.DensityKgPerM3));
        var stableStep =
            maximumWaveSpeed <= 0
                ? double.PositiveInfinity
                : 0.5 * compiled.Mesh.CellSize / maximumWaveSpeed;
        checks.Add(
            new(
                "explicit-time-step",
                "Explicit time-step estimate",
                scenario.Settings.TimeStepSeconds <= stableStep
                    ? RunValidityStatus.Passed
                    : RunValidityStatus.Failed,
                scenario.Settings.TimeStepSeconds,
                stableStep,
                "s",
                "Conservative cell-size / elastic-wave-speed estimate. A backend-specific convergence study remains required."
            )
        );

        var lastActiveContact = -1;
        for (var i = 0; i < result.Telemetry.Samples.Count; i++)
        {
            if (result.Telemetry.Samples[i].ContactCount > 0)
                lastActiveContact = i;
        }
        var contactStillActive =
            lastActiveContact >= 0 && lastActiveContact == energy.Samples.Count - 1;
        var energyWindow =
            lastActiveContact >= 0 && lastActiveContact + 1 < energy.Samples.Count
                ? energy.Samples.Skip(lastActiveContact + 1).ToArray()
                : energy.Samples;
        var maxEnergyResidual =
            energyWindow.Count == 0
                ? double.NaN
                : energyWindow.Max(x => Math.Abs(x.ResidualPercent));
        var energyStatus =
            double.IsNaN(maxEnergyResidual) ? RunValidityStatus.Warning
            : contactStillActive ? RunValidityStatus.Warning
            : maxEnergyResidual <= 5 ? RunValidityStatus.Passed
            : maxEnergyResidual <= 15 ? RunValidityStatus.Warning
            : RunValidityStatus.Failed;
        checks.Add(
            new(
                "energy-balance",
                "Mechanical energy residual",
                energyStatus,
                double.IsNaN(maxEnergyResidual) ? null : maxEnergyResidual,
                5,
                "%",
                contactStillActive
                    ? "Contact remains active at the final sample, so a settled post-impact residual cannot be assessed. The ledger includes contact potential and the dissipation channels supplied by the backend."
                    : "Evaluated after the final active-contact sample to avoid transient force/work phase lag. The explicit lattice ledger includes kinetic, rotational, elastic and contact-potential energy; friction, contact, structural and global damping; scalar-damage fracture energy; and gravity work. Plastic, thermal, non-gravity load and moving-boundary work remain backend-dependent."
            )
        );

        var penetrationRatio =
            result.MaxContactPenetrationMeters / Math.Max(compiled.Mesh.CellSize, 1e-15);
        checks.Add(
            new(
                "contact-penetration",
                "Peak contact penetration / cell size",
                penetrationRatio <= 0.10 ? RunValidityStatus.Passed
                    : penetrationRatio <= 0.25 ? RunValidityStatus.Warning
                    : RunValidityStatus.Failed,
                penetrationRatio,
                0.10,
                "ratio",
                "Large penalty penetration can make contact force and stress results unreliable."
            )
        );

        var untraceable = materials.Where(x => string.IsNullOrWhiteSpace(x.Standard)).ToArray();
        checks.Add(
            new(
                "material-traceability",
                "Material grade traceability",
                untraceable.Length == 0 ? RunValidityStatus.Passed : RunValidityStatus.Warning,
                materials.Length - untraceable.Length,
                materials.Length,
                "materials",
                untraceable.Length == 0
                    ? "Every used material has a specification note."
                    : $"Missing grade/specification metadata: {string.Join(", ", untraceable.Select(x => x.DisplayName))}."
            )
        );

        var nonFinite = result.Frames.Sum(frame =>
            frame.Position.Count(x => !Finite(x.X) || !Finite(x.Y) || !Finite(x.Z))
            + frame.Velocity.Count(x => !Finite(x.X) || !Finite(x.Y) || !Finite(x.Z))
        );
        checks.Add(
            new(
                "finite-state",
                "Finite numerical state",
                nonFinite == 0 ? RunValidityStatus.Passed : RunValidityStatus.Failed,
                nonFinite,
                0,
                "values",
                "NaN or infinite state values invalidate the run."
            )
        );

        var minimumPartNodes =
            compiled.Mesh.Parts.Count == 0 ? 0 : compiled.Mesh.Parts.Min(x => x.NodeCount);
        checks.Add(
            new(
                "mesh-resolution",
                "Minimum nodes per part",
                minimumPartNodes >= 125 ? RunValidityStatus.Passed
                    : minimumPartNodes >= 27 ? RunValidityStatus.Warning
                    : RunValidityStatus.Failed,
                minimumPartNodes,
                125,
                "nodes",
                "This is a screening gate, not a substitute for a mesh-convergence study."
            )
        );
        return new(checks);
    }

    private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
