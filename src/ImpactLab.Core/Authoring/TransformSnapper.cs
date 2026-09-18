using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Authoring;

public static class TransformSnapper
{
    public static double Snap(double value, double step, bool enabled) =>
        !enabled || step <= 0 ? value : Math.Round(value / step) * step;

    public static TransformDelta Snap(TransformDelta delta, SnapSettings settings)
    {
        if (!settings.Enabled)
            return delta;
        return new TransformDelta(
            new Vec3(
                Snap(
                    delta.Translation.X,
                    settings.TranslationStepMeters,
                    settings.TranslationEnabled
                ),
                Snap(
                    delta.Translation.Y,
                    settings.TranslationStepMeters,
                    settings.TranslationEnabled
                ),
                Snap(
                    delta.Translation.Z,
                    settings.TranslationStepMeters,
                    settings.TranslationEnabled
                )
            ),
            new Vec3(
                Snap(
                    delta.RotationDegrees.X,
                    settings.RotationStepDegrees,
                    settings.RotationEnabled
                ),
                Snap(
                    delta.RotationDegrees.Y,
                    settings.RotationStepDegrees,
                    settings.RotationEnabled
                ),
                Snap(
                    delta.RotationDegrees.Z,
                    settings.RotationStepDegrees,
                    settings.RotationEnabled
                )
            ),
            new Vec3(
                Snap(delta.Scale.X, settings.ScaleStep, settings.ScaleEnabled),
                Snap(delta.Scale.Y, settings.ScaleStep, settings.ScaleEnabled),
                Snap(delta.Scale.Z, settings.ScaleStep, settings.ScaleEnabled)
            )
        );
    }
}
