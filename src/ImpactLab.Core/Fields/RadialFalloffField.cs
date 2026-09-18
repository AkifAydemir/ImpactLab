using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Fields;

public sealed class RadialFalloffField : IScalarField3
{
    public RadialFalloffField(Vec3 center, double radiusMeters, double exponent = 2.0)
    {
        if (radiusMeters <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(radiusMeters));
        if (exponent <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(exponent));
        Center = center;
        RadiusMeters = radiusMeters;
        Exponent = exponent;
    }

    public Vec3 Center { get; }
    public double RadiusMeters { get; }
    public double Exponent { get; }

    public double Sample(in Vec3 position, double timeSeconds)
    {
        var r = (position - Center).Length;
        if (r >= RadiusMeters)
            return 0.0;
        return Math.Pow(1.0 - r / RadiusMeters, Exponent);
    }
}
