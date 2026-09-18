namespace ImpactLab.App.Layout;

public sealed record WorkspaceLayoutState(
    string Name,
    IReadOnlyList<DockPaneState> Panes,
    string ActiveWorkspaceId,
    string? ActiveDocumentId = null
);
