namespace ImpactLab.Core.Mobility;

public readonly record struct GridCoordinate(int Row, int Column);

public sealed record MobilityRoute(
    bool Found,
    IReadOnlyList<GridCoordinate> Cells,
    double TotalCost,
    string Message
);
