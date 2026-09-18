namespace ImpactLab.App.Layout;

public sealed record DockPaneState(
    string Id,
    bool Visible,
    double Width,
    double Height,
    string Dock = "Left",
    int Order = 0
);
