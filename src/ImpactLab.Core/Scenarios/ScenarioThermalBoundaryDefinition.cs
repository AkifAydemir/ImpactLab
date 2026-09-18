namespace ImpactLab.Core.Scenarios;

public sealed class ScenarioThermalBoundaryDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? PartId { get; set; }
    public double MaximumNormalizedZ { get; set; } = 0.05;
    public double TemperatureKelvin { get; set; } = 293.15;
}
