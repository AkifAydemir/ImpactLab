namespace ImpactLab.Core.Finalization;

public static class FeatureCompletenessReview
{
    public static FeatureCompletenessMatrix CreateV16SourceBaseline()
    {
        var m = new FeatureCompletenessMatrix();
        foreach (var area in Enum.GetValues<FeatureArea>())
            m.Set(
                new(
                    area,
                    FeatureAreaStatus.Implemented,
                    "Canonical source exists through v16.",
                    "Repository execution gate has not run.",
                    true
                )
            );
        m.Set(
            new(
                FeatureArea.Extraction,
                FeatureAreaStatus.Ready,
                "Snapshot-only OpenXML extraction, path safety and reproducible repository manifest calculation are canonical.",
                null
            )
        );
        m.Set(
            new(
                FeatureArea.Meshing,
                FeatureAreaStatus.ExternalDependency,
                "External CAD/tetra protocol, validation, sample executable and fixture coverage are canonical.",
                "Production CAD kernel/tetrahedralizer binaries and measured quality baselines remain external/runtime dependencies."
            )
        );
        m.Set(
            new(
                FeatureArea.Reporting,
                FeatureAreaStatus.Ready,
                "Deterministic report packages, SHA-256 manifests and golden source fixtures are canonical.",
                null
            )
        );
        m.Set(
            new(
                FeatureArea.FiniteStrain,
                FeatureAreaStatus.VerificationNeeded,
                "Finite-strain/objectivity/plasticity benchmark source is canonical.",
                "Compiled/calibrated benchmark execution remains pending."
            )
        );
        m.Set(
            new(
                FeatureArea.Contact,
                FeatureAreaStatus.VerificationNeeded,
                "Augmented/mortar/contact verification source is canonical.",
                "Large-sliding compiled benchmark execution remains pending."
            )
        );
        m.Set(
            new(
                FeatureArea.Thermal,
                FeatureAreaStatus.VerificationNeeded,
                "Thermal and coupled verification source is canonical.",
                "Compiled coupled-energy verification remains pending."
            )
        );
        m.Set(
            new(
                FeatureArea.Experiments,
                FeatureAreaStatus.VerificationNeeded,
                "ImpactLab.Worker protocol/process orchestration is canonical.",
                "Supported-OS runtime process/quota/crash-recovery tests remain pending."
            )
        );
        m.Set(
            new(
                FeatureArea.Results,
                FeatureAreaStatus.VerificationNeeded,
                "Indexed/chunked results, async prefetch and large-result WPF source workflow are canonical.",
                "Very-large-result performance execution remains pending."
            )
        );
        m.Set(
            new(
                FeatureArea.Persistence,
                FeatureAreaStatus.VerificationNeeded,
                "Versioned migration registries/matrices and layout schema recovery are canonical.",
                "Compiled round-trip/malformed-file execution remains pending."
            )
        );
        m.Set(
            new(
                FeatureArea.Extensions,
                FeatureAreaStatus.Ready,
                "Authenticated HMAC/replay-protected capability IPC, isolated extension-host executable, publisher/trust policy and OS isolation provider boundary are canonical.",
                null
            )
        );
        m.Set(
            new(
                FeatureArea.Accessibility,
                FeatureAreaStatus.Ready,
                "Keyboard traversal, live announcements, workflow audit catalog and accessible WPF source chrome are canonical.",
                null
            )
        );
        m.Set(
            new(
                FeatureArea.Documentation,
                FeatureAreaStatus.Ready,
                "Architecture, security, accessibility and source-feature review documentation is canonical.",
                null
            )
        );
        return m;
    }

    public static FeatureCompletenessMatrix CreateV15SourceBaseline() => CreateV16SourceBaseline();

    public static FeatureCompletenessMatrix CreateV14SourceBaseline() => CreateV16SourceBaseline();

    public static FeatureCompletenessMatrix CreateV12Baseline() => CreateV16SourceBaseline();
}
