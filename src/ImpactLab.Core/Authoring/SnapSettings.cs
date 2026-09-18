namespace ImpactLab.Core.Authoring;

public sealed record SnapSettings(
    bool Enabled = true,
    double TranslationStepMeters = 0.001,
    double RotationStepDegrees = 5.0,
    double ScaleStep = 0.05,
    bool TranslationEnabled = true,
    bool RotationEnabled = true,
    bool ScaleEnabled = true
)
{
    public double RotationStepRadians => RotationStepDegrees * Math.PI / 180.0;
}
