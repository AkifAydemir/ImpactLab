namespace ImpactLab.App.Authoring;

public sealed record SelectionAdornerState(
    IReadOnlyList<string> SelectedIds,
    bool ShowBoundingBoxes = true,
    bool ShowPivot = true,
    bool ShowGizmo = true
);
