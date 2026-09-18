namespace ImpactLab.Core.Experiments;

public sealed class SweepDefinition
{
    public string Name { get; set; } = "Parameter sweep";
    public List<SweepAxis> Axes { get; } = [];
    public int MaxRuns { get; set; } = 500;

    public void Validate()
    {
        if (Axes.Count == 0)
            throw new InvalidOperationException("A sweep requires at least one axis.");
        foreach (var axis in Axes)
            axis.Validate();
        if (MaxRuns <= 0)
            throw new InvalidOperationException("MaxRuns must be positive.");
    }
}
