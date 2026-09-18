namespace ImpactLab.App.Layout;

public sealed record WorkspaceLayoutEnvelope(
    int SchemaVersion,
    WorkspaceLayoutState Layout,
    DateTimeOffset SavedUtc
);
