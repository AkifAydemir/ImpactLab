namespace ImpactLab.App.Layout;

public sealed record WorkspaceLayoutRecoveryResult(
    WorkspaceLayoutState Layout,
    bool Recovered,
    IReadOnlyList<string> Diagnostics
);
