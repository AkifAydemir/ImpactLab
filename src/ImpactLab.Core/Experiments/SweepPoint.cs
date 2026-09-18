namespace ImpactLab.Core.Experiments;

public sealed record SweepPoint(int Index, IReadOnlyDictionary<string, double> Values)
{
    public string Label =>
        $"Run {Index + 1}: " + string.Join(", ", Values.Select(x => $"{x.Key}={x.Value:g6}"));
}
