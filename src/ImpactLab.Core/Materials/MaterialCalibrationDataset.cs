namespace ImpactLab.Core.Materials;

public sealed class MaterialCalibrationDataset
{
    private readonly List<MaterialTestSample> _samples = [];
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Material calibration dataset";
    public MaterialProvenance Provenance { get; set; } = new("Unspecified");
    public IReadOnlyList<MaterialTestSample> Samples => _samples;

    public void Add(MaterialTestSample sample)
    {
        if (
            !double.IsFinite(sample.Strain)
            || !double.IsFinite(sample.StressPa)
            || sample.Weight <= 0.0
        )
            throw new InvalidOperationException("Invalid material test sample.");
        _samples.Add(sample);
    }

    public void Validate()
    {
        if (_samples.Count < 3)
            throw new InvalidOperationException(
                "Calibration dataset requires at least three samples."
            );
        if (_samples.Select(x => x.Strain).Distinct().Count() < 3)
            throw new InvalidOperationException(
                "Calibration dataset requires distinct strain values."
            );
    }
}
