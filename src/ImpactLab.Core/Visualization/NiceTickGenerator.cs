using System.Globalization;

namespace ImpactLab.Core.Visualization;

public static class NiceTickGenerator
{
    public static IReadOnlyList<AxisTick> Generate(double min, double max, int target = 6)
    {
        if (max <= min)
            return [new(min, min.ToString("G4", CultureInfo.InvariantCulture))];
        var raw = (max - min) / Math.Max(2, target);
        var mag = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        var n = raw / mag;
        var step =
            (
                n < 1.5 ? 1
                : n < 3 ? 2
                : n < 7 ? 5
                : 10
            ) * mag;
        var start = Math.Ceiling(min / step) * step;
        var list = new List<AxisTick>();
        for (var v = start; v <= max + step * 1e-9; v += step)
            list.Add(new(v, v.ToString("G4", CultureInfo.InvariantCulture)));
        return list;
    }
}
