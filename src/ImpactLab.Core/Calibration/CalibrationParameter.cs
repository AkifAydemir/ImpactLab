namespace ImpactLab.Core.Calibration;

public sealed record CalibrationParameter(string Path, double Minimum, double Maximum, int Samples)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new InvalidOperationException("Calibration parameter path cannot be empty.");
        if (!double.IsFinite(Minimum) || !double.IsFinite(Maximum) || Minimum > Maximum)
            throw new InvalidOperationException("Invalid calibration parameter bounds.");
        if (Samples < 2)
            throw new InvalidOperationException(
                "Calibration parameter requires at least two samples."
            );
    }
}
