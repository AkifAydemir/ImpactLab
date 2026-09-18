namespace ImpactLab.App.Visualization;

public sealed record ContourLegendModel(
    string FieldId,
    string Unit,
    double Minimum,
    double Maximum,
    IReadOnlyList<ContourLegendTick> Ticks
)
{
    public static ContourLegendModel Create(
        string id,
        string unit,
        double min,
        double max,
        int tickCount = 6
    )
    {
        var t = Enumerable
            .Range(0, Math.Max(2, tickCount))
            .Select(i =>
            {
                var q = (double)i / (Math.Max(2, tickCount) - 1);
                var v = min + (max - min) * q;
                return new ContourLegendTick(
                    v,
                    v.ToString("G4", System.Globalization.CultureInfo.InvariantCulture),
                    q
                );
            })
            .ToArray();
        return new(id, unit, min, max, t);
    }
}
