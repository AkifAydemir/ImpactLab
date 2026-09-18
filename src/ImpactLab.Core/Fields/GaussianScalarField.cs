using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fields;

public sealed class GaussianScalarField : IScalarField3
{
    public GaussianScalarField(Vec3 center, double sigmaMeters, double amplitude = 1.0)
    {
        if (sigmaMeters <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(sigmaMeters));
        Center = center;
        SigmaMeters = sigmaMeters;
        Amplitude = amplitude;
    }

    public Vec3 Center { get; }
    public double SigmaMeters { get; }
    public double Amplitude { get; }

    public double Sample(in Vec3 position, double timeSeconds)
    {
        var r2 = (position - Center).LengthSquared;
        return Amplitude * Math.Exp(-0.5 * r2 / (SigmaMeters * SigmaMeters));
    }
}
