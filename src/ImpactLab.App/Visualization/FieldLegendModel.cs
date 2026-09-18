using ImpactLab.Core.Results.Fields;

namespace ImpactLab.App.Visualization;

public sealed record FieldLegendModel(
    ResultFieldDescriptor Field,
    double Minimum,
    double Maximum,
    IReadOnlyList<double> Ticks,
    string Format = "G4",
    bool LogScale = false
);
