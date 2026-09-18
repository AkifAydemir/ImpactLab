namespace ImpactLab.Core.Thermal;

public static class ThermalBoundaryAssembler
{
    public const double Sigma = 5.670374419e-8;

    public static void AddConvection(
        ConvectionBoundary b,
        ReadOnlySpan<double> t,
        Span<double> rhs,
        Span<double> diag
    )
    {
        foreach (var n in b.NodeIds)
        {
            var hA = b.HeatTransferCoefficientWPerM2K * b.AreaPerNodeM2;
            diag[n] += hA;
            rhs[n] += hA * b.AmbientTemperatureKelvin;
        }
    }

    public static void AddRadiation(
        RadiationBoundary b,
        ReadOnlySpan<double> t,
        Span<double> rhs,
        Span<double> diag
    )
    {
        foreach (var n in b.NodeIds)
        {
            var tk = Math.Max(t[n], 1);
            var h = 4 * b.Emissivity * Sigma * Math.Pow(tk, 3) * b.AreaPerNodeM2;
            diag[n] += h;
            rhs[n] += h * b.AmbientTemperatureKelvin;
        }
    }
}
