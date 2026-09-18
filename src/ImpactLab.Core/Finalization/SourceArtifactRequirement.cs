namespace ImpactLab.Core.Finalization;

public sealed record SourceArtifactRequirement(
    string Id,
    SourceArtifactKind Kind,
    string Path,
    FeatureArea Area,
    bool CompletionRelevant = true
);
