using System.Globalization;
using ImpactLab.Core.Analysis;
using ImpactLab.Core.Materials;
using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Verification;
using ImpactLab.Core.Verification.Benchmarks;

namespace ImpactLab.Core.Reporting;

public static class EngineeringReportBuilder
{
    public static EngineeringReportDocument Build(
        string scenarioName,
        string backend,
        BackendResultSummary summary
    )
    {
        var culture = CultureInfo.InvariantCulture;
        var metrics = new List<ReportMetric>
        {
            new("Domain", summary.DomainKind),
            new(
                "Maximum displacement",
                summary.Compatibility.MaxDisplacementMeters.ToString("G6", culture),
                "m"
            ),
            new(
                "Peak contact force",
                summary.Compatibility.PeakContactForceN.ToString("G6", culture),
                "N"
            ),
        };
        if (summary.Continuum is not null)
        {
            metrics.Add(new("Tetrahedra", summary.Continuum.Elements.ToString(culture)));
            metrics.Add(
                new(
                    "Peak von Mises",
                    summary.Continuum.PeakVonMisesPa.ToString("G6", culture),
                    "Pa"
                )
            );
            metrics.Add(
                new(
                    "Maximum temperature",
                    summary.Continuum.MaxTemperatureKelvin.ToString("F2", culture),
                    "K"
                )
            );
        }
        return new(
            "ImpactLab Engineering Report",
            scenarioName,
            backend,
            DateTimeOffset.UtcNow.ToString("O", culture),
            [new("Run summary", "Generated from immutable backend results.", metrics, [])]
        );
    }

    public static EngineeringReportDocument BuildDetailed(
        ScenarioDefinition scenario,
        string backend,
        BackendResultSummary summary,
        MaterialCatalog materials,
        RunValidityAssessment? validity = null,
        MaterialProfileCatalog? materialProfiles = null,
        EnergyBalanceSeries? energyBalance = null
    )
    {
        var basic = Build(scenario.Name, backend, summary);
        var culture = CultureInfo.InvariantCulture;
        var setup = new EngineeringReportSection(
            "Simulation setup",
            "All values use SI units internally. Imported geometry units and material grade data must be confirmed by the analyst.",
            [
                new("Duration", scenario.Settings.DurationSeconds.ToString("G6", culture), "s"),
                new("Time step", scenario.Settings.TimeStepSeconds.ToString("G6", culture), "s"),
                new("Mesh cell size", scenario.CellSizeMeters.ToString("G6", culture), "m"),
                new("Deformable parts", scenario.Parts.Count.ToString(culture)),
                new("Rigid impactors", scenario.RigidBodies.Count.ToString(culture)),
                new("Damage model", scenario.Physics.DamageModelId),
                new("Lattice constitutive model", scenario.Physics.LatticeConstitutiveModelId),
                new("Fragment topology model", scenario.Physics.FragmentTopologyModelId),
                new("Rigid contact model", scenario.Physics.RigidContactModelId),
                new("Deformable contact model", scenario.Physics.DeformableContactModelId),
                new(
                    "Additional physics models",
                    scenario.Physics.AdditionalModelIds.Count == 0
                        ? "none"
                        : string.Join(", ", scenario.Physics.AdditionalModelIds)
                ),
            ],
            [
                new(
                    "Parts and materials",
                    ["Part", "Role", "Material", "Density kg/m3", "E GPa", "Yield MPa", "Standard"],
                    scenario
                        .Parts.Select(part => PartRow(part, materials, culture))
                        .Concat(
                            scenario.RigidBodies.Select(body =>
                                RigidBodyRow(body, materials, culture)
                            )
                        )
                        .ToArray()
                ),
                new(
                    "Impactor initial conditions",
                    [
                        "Impactor",
                        "Mass kg",
                        "Velocity X m/s",
                        "Velocity Y m/s",
                        "Velocity Z m/s",
                        "Speed m/s",
                    ],
                    scenario
                        .RigidBodies.Select(body =>
                            (IReadOnlyList<string>)
                                [
                                    body.Name,
                                    body.MassOverrideKg?.ToString("G6", culture)
                                        ?? "geometry-derived",
                                    body.InitialVelocity.X.ToString("G6", culture),
                                    body.InitialVelocity.Y.ToString("G6", culture),
                                    body.InitialVelocity.Z.ToString("G6", culture),
                                    body.InitialVelocity.Length.ToString("G6", culture),
                                ]
                        )
                        .ToArray()
                ),
            ]
        );
        var environmentSection = new EngineeringReportSection(
            "Physical environment",
            scenario.Environment is null
                ? "No traceable environment preset is attached. Gravity comes directly from SimulationSettings and atmospheric effects are not modeled."
                : "Gravity is applied as a uniform body acceleration. Atmospheric values are recorded for traceability; aerodynamic force is not applied unless an aerodynamic backend explicitly supports it.",
            [
                new("Environment", scenario.Environment?.DisplayName ?? "Custom / undeclared"),
                new("Gravity X", scenario.Settings.Gravity.X.ToString("G8", culture), "m/s²"),
                new("Gravity Y", scenario.Settings.Gravity.Y.ToString("G8", culture), "m/s²"),
                new("Gravity Z", scenario.Settings.Gravity.Z.ToString("G8", culture), "m/s²"),
                new(
                    "Aerodynamic forces",
                    scenario.Environment?.AerodynamicForcesEnabled == true
                        ? "ENABLED"
                        : "NOT MODELED"
                ),
                new(
                    "Altitude-dependent samples",
                    (
                        (scenario.Environment?.DensityByAltitude.Count ?? 0)
                        + (scenario.Environment?.PressureByAltitude.Count ?? 0)
                        + (scenario.Environment?.TemperatureByAltitude.Count ?? 0)
                    ).ToString(culture)
                ),
            ],
            scenario.Environment is null
                ? []
                :
                [
                    new(
                        "Environment provenance",
                        ["Density kg/m³", "Pressure Pa", "Temperature K", "Reference", "URI"],
                        [
                            [
                                scenario.Environment.AtmosphericDensityKgPerM3.ToString(
                                    "G8",
                                    culture
                                ),
                                scenario.Environment.PressurePa.ToString("G8", culture),
                                scenario.Environment.TemperatureKelvin.ToString("G8", culture),
                                scenario.Environment.Reference,
                                scenario.Environment.ReferenceUri,
                            ],
                        ]
                    ),
                    new(
                        "Altitude profiles",
                        ["Quantity", "Altitude m", "Value"],
                        scenario
                            .Environment.DensityByAltitude.Select(x =>
                                (IReadOnlyList<string>)
                                    [
                                        "Density kg/m³",
                                        x.AltitudeMeters.ToString("G8", culture),
                                        x.Value.ToString("G8", culture),
                                    ]
                            )
                            .Concat(
                                scenario.Environment.PressureByAltitude.Select(x =>
                                    (IReadOnlyList<string>)
                                        [
                                            "Pressure Pa",
                                            x.AltitudeMeters.ToString("G8", culture),
                                            x.Value.ToString("G8", culture),
                                        ]
                                )
                            )
                            .Concat(
                                scenario.Environment.TemperatureByAltitude.Select(x =>
                                    (IReadOnlyList<string>)
                                        [
                                            "Temperature K",
                                            x.AltitudeMeters.ToString("G8", culture),
                                            x.Value.ToString("G8", culture),
                                        ]
                                )
                            )
                            .ToArray()
                    ),
                ]
        );
        var limitations = new EngineeringReportSection(
            "Validity and limitations",
            "Results are numerical estimates from the selected experimental backend. They are not a certification. Mesh and time-step convergence, traceable material curves, contact calibration, and comparison with physical tests or a trusted reference solver are required before safety-critical use.",
            [],
            []
        );
        var sourceBenchmarks = VerificationBenchmarkRunner.Run();
        var effectiveProfiles = materialProfiles ?? new MaterialProfileCatalog(materials);
        var inputReadiness = ScenarioInputReadinessAssessor.Assess(scenario, effectiveProfiles);
        var assumptions = ScenarioAssumptionLedgerBuilder.Build(scenario, effectiveProfiles);
        var credibility = SimulationCredibilityAssessor.Assess(validity, sourceBenchmarks.Passed);
        var credibilitySection = new EngineeringReportSection(
            "Credibility statement",
            "ImpactLab separates a completed calculation from a validated prediction. The level below is determined only from recorded evidence; software-generated output is not its own validation source.",
            [
                new("Credibility level", credibility.Label),
                new("Permitted use", credibility.PermittedUse),
                new("Experimentally validated", credibility.IsValidated ? "YES" : "NO"),
            ],
            [
                new(
                    "Evidence status",
                    ["Status", "Evidence item"],
                    credibility
                        .SatisfiedEvidence.Select(x => (IReadOnlyList<string>)["SATISFIED", x])
                        .Concat(
                            credibility.MissingEvidence.Select(x =>
                                (IReadOnlyList<string>)["MISSING", x]
                            )
                        )
                        .ToArray()
                ),
            ]
        );
        var inputReadinessSection = new EngineeringReportSection(
            "Input-data readiness gate",
            "This gate assesses whether the supplied inputs contain enough traceable evidence for quantitative impact prediction. A completed simulation does not override missing input evidence.",
            [
                new(
                    "Quantitative impact ready",
                    inputReadiness.QuantitativeImpactReady ? "YES" : "NO"
                ),
                new("Blocking gaps", inputReadiness.BlockerCount.ToString(culture)),
                new("Warnings", inputReadiness.WarningCount.ToString(culture)),
            ],
            [
                new(
                    "Input evidence gaps",
                    ["Domain", "Severity", "Item", "Requirement"],
                    inputReadiness
                        .Findings.Select(x =>
                            (IReadOnlyList<string>)
                                [x.Domain, x.Severity.ToString(), x.Item, x.Requirement]
                        )
                        .ToArray()
                ),
            ]
        );
        var assumptionSection = new EngineeringReportSection(
            "Assumption ledger",
            "Missing evidence does not prevent exploratory simulation. ImpactLab substitutes documented defaults and records their likely effect so the estimate can be reproduced and replaced with better data later.",
            [
                new("Result class", assumptions.ResultClass),
                new("Active assumptions", assumptions.Items.Count.ToString(culture)),
                new(
                    "Uncertainty interval",
                    assumptions.Items.Any(x => x.Domain == "Uncertainty")
                        ? "NOT AVAILABLE — deterministic nominal run"
                        : "Input uncertainty supplied; propagation study still required"
                ),
            ],
            [
                new(
                    "Applied defaults and assumptions",
                    ["Domain", "Parameter", "Assumed value", "Basis", "Likely consequence"],
                    assumptions
                        .Items.Select(x =>
                            (IReadOnlyList<string>)
                                [x.Domain, x.Parameter, x.AssumedValue, x.Basis, x.Consequence]
                        )
                        .ToArray()
                ),
            ]
        );
        var provenanceSection = new EngineeringReportSection(
            "Quantity definitions and provenance",
            "Every reported field must state what was calculated, how it was derived, and what it must not be interpreted as.",
            [],
            [
                new(
                    "Reported quantities",
                    ["Quantity", "Definition / algorithm", "Source", "Interpretation limit"],
                    [
                        [
                            "Maximum displacement",
                            "Maximum Euclidean nodal distance |x(t)-x(0)| over stored simulation frames.",
                            "ImpactLab immutable solver frames / BackendResultAnalyzer",
                            "May include vibration and rigid motion; not automatically permanent deformation.",
                        ],
                        [
                            "Peak contact force",
                            "Maximum total normal penalty-contact force over recorded telemetry.",
                            "ImpactLab contact solver telemetry",
                            "A numerical penalty force, not a load-cell measurement; depends on contact calibration and time step.",
                        ],
                        [
                            "Lattice equivalent stress heatmap",
                            "Mean absolute axial stress of connected springs after tangent response and damage reduction.",
                            "LatticeDerivedFieldCalculator",
                            "Visualization proxy; it is not continuum von Mises stress.",
                        ],
                        [
                            "Heatmap colour",
                            "Linear normalization from zero to the displayed maximum. Stops: dark blue 0%, light blue 25%, cyan 50%, yellow 70%, orange 85%, red 100%.",
                            "MeshSceneBuilder / fixed 16-bin palette",
                            "Colour is relative to the displayed scale; the same colour can mean a different value when the maximum changes.",
                        ],
                    ]
                ),
                new(
                    "Method references",
                    ["Reference", "Role", "Location"],
                    [
                        [
                            "NASA-STD-7009B, Standard for Models and Simulations (2024)",
                            "Credibility evidence, reporting and intended-use discipline.",
                            "https://standards.nasa.gov/standard/NASA/NASA-STD-7009",
                        ],
                        [
                            "ASME V&V 10-2019 (R2025), Computational Solid Mechanics",
                            "Verification, validation and uncertainty-quantification framework.",
                            "https://www.asme.org/codes-standards/find-codes-standards/standard-for-verification-and-validation-in-computational-solid-mechanics/2019/pdf",
                        ],
                    ]
                ),
            ]
        );
        var sourceVerificationSection = new EngineeringReportSection(
            "Software source verification",
            "These deterministic component benchmarks compare selected algorithms with known reference responses. They verify implementation components; they do not validate this impact scenario or prove agreement with physical testing.",
            [
                new("Benchmark gate", sourceBenchmarks.Passed ? "PASSED" : "FAILED"),
                new(
                    "Benchmarks passed",
                    $"{sourceBenchmarks.Benchmarks.Count(x => x.Passed)}/{sourceBenchmarks.Benchmarks.Count}"
                ),
            ],
            [
                new(
                    "Component benchmarks",
                    ["Benchmark", "Category", "Status", "Evidence"],
                    sourceBenchmarks
                        .Benchmarks.Select(x =>
                            (IReadOnlyList<string>)
                                [
                                    x.Name,
                                    x.Category.ToString(),
                                    x.Passed ? "PASS" : "FAIL",
                                    x.Error ?? x.Evidence,
                                ]
                        )
                        .ToArray()
                ),
            ]
        );
        var materialEvidenceSection = new EngineeringReportSection(
            "Material-data evidence",
            "Built-in records are nominal reference values, not batch certificates. A standard designation identifies the grade family but does not prove the properties of the simulated product.",
            [],
            [
                new(
                    "Material traceability",
                    ["Material", "Evidence class", "Declared basis", "Engineering status"],
                    scenario
                        .Parts.Select(x => x.MaterialId)
                        .Concat(scenario.RigidBodies.Select(x => x.MaterialId))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Select(id =>
                            materials.Entries.Single(x =>
                                string.Equals(
                                    x.Definition.Id,
                                    id,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                        )
                        .Select(entry =>
                            (IReadOnlyList<string>)
                                [
                                    entry.Definition.DisplayName,
                                    entry.IsBuiltIn
                                        ? "Built-in nominal reference"
                                        : "User supplied",
                                    string.IsNullOrWhiteSpace(entry.Definition.Standard)
                                        ? "No standard/source declared"
                                        : entry.Definition.Standard,
                                    entry.IsBuiltIn
                                        ? "UNVERIFIED — replace with traceable certificate/curve"
                                        : "UNVERIFIED — user must attach source and calibration evidence",
                                ]
                        )
                        .ToArray()
                ),
            ]
        );
        var validitySection = new EngineeringReportSection(
            "Run validity screening",
            "Automated checks identify obvious numerical or data-quality problems. Passing these checks does not establish predictive validation.",
            validity is null
                ? []
                :
                [
                    new("Passed checks", validity.PassedCount.ToString(culture)),
                    new("Warnings", validity.WarningCount.ToString(culture)),
                    new("Failed checks", validity.FailedCount.ToString(culture)),
                ],
            validity is null
                ? []
                :
                [
                    new(
                        "Validity checks",
                        ["Check", "Status", "Observed", "Limit", "Unit", "Detail"],
                        validity
                            .Checks.Select(check =>
                                (IReadOnlyList<string>)
                                    [
                                        check.Name,
                                        check.Status.ToString(),
                                        check.Observed?.ToString("G6", culture) ?? "n/a",
                                        check.Limit?.ToString("G6", culture) ?? "n/a",
                                        check.Unit,
                                        check.Detail,
                                    ]
                            )
                            .ToArray()
                    ),
                ]
        );
        var energySamples = energyBalance?.Samples ?? [];
        var finalEnergy = energySamples.LastOrDefault();
        var peakResidual =
            energySamples.Count == 0
                ? default
                : energySamples.MaxBy(x => Math.Abs(x.ResidualPercent));
        var energyLedgerSection = new EngineeringReportSection(
            "Energy ledger",
            "The ledger separates recoverable mechanical/contact energy from accumulated dissipation and applied work. The explicit lattice backend tracks contact, structural/global damping, scalar-damage fracture energy and gravity work. Moving-boundary constraint work and non-gravity load work require explicit backend support. A zero in an unsupported channel is not evidence that the physical mechanism is absent.",
            energySamples.Count == 0
                ? [new("Ledger status", "NOT AVAILABLE")]
                :
                [
                    new(
                        "Reference energy",
                        energyBalance!.ReferenceEnergyJ.ToString("G8", culture),
                        "J"
                    ),
                    new(
                        "Final accounted energy",
                        finalEnergy.AccountedEnergyJ.ToString("G8", culture),
                        "J"
                    ),
                    new(
                        "Maximum absolute residual",
                        Math.Abs(peakResidual.ResidualPercent).ToString("G6", culture),
                        "%"
                    ),
                ],
            energySamples.Count == 0
                ? []
                :
                [
                    new(
                        "Final energy channels",
                        ["Channel", "Energy J", "Tracking status"],
                        [
                            [
                                "Mechanical (including contact potential)",
                                finalEnergy.MechanicalEnergyJ.ToString("G8", culture),
                                "Tracked",
                            ],
                            [
                                "Contact penalty potential",
                                finalEnergy.ContactPotentialEnergyJ.ToString("G8", culture),
                                "Tracked (instantaneous)",
                            ],
                            [
                                "Friction dissipation",
                                finalEnergy.FrictionEnergyJ.ToString("G8", culture),
                                "Tracked (cumulative)",
                            ],
                            [
                                "Normal contact damping",
                                finalEnergy.ContactDampingEnergyJ.ToString("G8", culture),
                                "Tracked (cumulative)",
                            ],
                            [
                                "Structural viscous damping",
                                finalEnergy.StructuralDampingEnergyJ.ToString("G8", culture),
                                "Tracked by explicit lattice (cumulative)",
                            ],
                            [
                                "Global numerical damping",
                                finalEnergy.GlobalDampingEnergyJ.ToString("G8", culture),
                                "Tracked by explicit lattice (cumulative)",
                            ],
                            [
                                "Plastic dissipation",
                                finalEnergy.PlasticDissipationEnergyJ.ToString("G8", culture),
                                "Tracked when the selected constitutive model supplies plastic work",
                            ],
                            [
                                "Fracture dissipation",
                                finalEnergy.FractureDissipationEnergyJ.ToString("G8", culture),
                                "Scalar-damage estimate in explicit lattice",
                            ],
                            [
                                "Thermal energy",
                                finalEnergy.ThermalEnergyJ.ToString("G8", culture),
                                "Schema reserved; backend-dependent",
                            ],
                            [
                                "External work",
                                finalEnergy.ExternalWorkJ.ToString("G8", culture),
                                "Gravity tracked; other loads backend-dependent",
                            ],
                            [
                                "Constraint work",
                                finalEnergy.ConstraintWorkJ.ToString("G8", culture),
                                "Schema reserved; moving-boundary work not yet tracked",
                            ],
                            [
                                "Diagnostic residual",
                                finalEnergy.DiagnosticResidualJ.ToString("G8", culture),
                                "Unaccounted numerical/model energy",
                            ],
                        ]
                    ),
                ]
        );
        return basic with
        {
            Sections =
            [
                credibilitySection,
                inputReadinessSection,
                assumptionSection,
                environmentSection,
                setup,
                .. basic.Sections,
                provenanceSection,
                sourceVerificationSection,
                materialEvidenceSection,
                energyLedgerSection,
                validitySection,
                limitations,
            ],
        };
    }

    private static IReadOnlyList<string> PartRow(
        ScenarioPartDefinition part,
        MaterialCatalog materials,
        CultureInfo culture
    )
    {
        var material = materials.Get(part.MaterialId);
        return
        [
            part.Name,
            part.Behavior.ToString(),
            material.DisplayName,
            material.DensityKgPerM3.ToString("G6", culture),
            (material.YoungModulusPa / 1e9).ToString("G6", culture),
            (material.EffectiveYieldStrengthPa / 1e6).ToString("G6", culture),
            material.Standard,
        ];
    }

    private static IReadOnlyList<string> RigidBodyRow(
        ScenarioRigidBodyDefinition body,
        MaterialCatalog materials,
        CultureInfo culture
    )
    {
        var material = materials.Get(body.MaterialId);
        return
        [
            body.Name,
            "Rigid impactor",
            material.DisplayName,
            material.DensityKgPerM3.ToString("G6", culture),
            (material.YoungModulusPa / 1e9).ToString("G6", culture),
            (material.EffectiveYieldStrengthPa / 1e6).ToString("G6", culture),
            material.Standard,
        ];
    }
}
