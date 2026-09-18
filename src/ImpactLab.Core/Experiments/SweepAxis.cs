namespace ImpactLab.Core.Experiments;

public sealed record SweepAxis(string Path, IReadOnlyList<double> Values)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new InvalidOperationException("Sweep axis path is required.");
        if (Values.Count == 0)
            throw new InvalidOperationException($"Sweep axis '{Path}' has no values.");
    }
}
