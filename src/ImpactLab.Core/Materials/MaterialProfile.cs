namespace ImpactLab.Core.Materials;

public sealed class MaterialProfile
{
    public MaterialProfile(MaterialDefinition definition) => Definition = definition;

    public MaterialDefinition Definition { get; init; }
    public ConstitutiveCurve? TensileCurve { get; init; }
    public ConstitutiveCurve? CompressiveCurve { get; init; }
    public MaterialUncertainty Uncertainty { get; init; } = new();
    public MaterialProvenance Provenance { get; init; } = new("Built-in / unspecified");
    public MaterialCalibrationMetrics? Calibration { get; init; }
    public string Notes { get; init; } = "";

    public void Validate()
    {
        Definition.Validate();
        foreach (var band in Uncertainty.Properties.Values)
            band.Validate();
    }
}
