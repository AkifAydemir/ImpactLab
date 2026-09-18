namespace ImpactLab.App.Visualization;

public sealed record ResultSelectionSummary(
    int SelectedNodes,
    int SelectedElements,
    double MinValue,
    double MaxValue,
    double MeanValue
);
