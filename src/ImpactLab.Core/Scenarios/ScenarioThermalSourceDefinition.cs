namespace ImpactLab.Core.Scenarios;

public sealed class ScenarioThermalSourceDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? PartId { get; set; }
    public double MaximumNormalizedZ { get; set; } = 1.0;
    public double PowerWatts { get; set; } = 0;
}
