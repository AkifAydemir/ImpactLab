namespace ImpactLab.Core.Materials;

public sealed class RateTemperatureAdjustedLaw : IConstitutiveLaw
{
    private readonly IConstitutiveLaw _inner;

    public RateTemperatureAdjustedLaw(
        IConstitutiveLaw inner,
        double referenceTemperatureK = 293.15,
        double thermalSofteningPerKelvin = 0.00035,
        double rateReferencePerSecond = 1.0
    )
    {
        _inner = inner;
        ReferenceTemperatureK = referenceTemperatureK;
        ThermalSofteningPerKelvin = thermalSofteningPerKelvin;
        RateReferencePerSecond = rateReferencePerSecond;
    }

    public string Id => $"rate-temp:{_inner.Id}";
    public double ReferenceTemperatureK { get; }
    public double ThermalSofteningPerKelvin { get; }
    public double RateReferencePerSecond { get; }

    public ConstitutiveEvaluation Evaluate(MaterialProfile profile, in MaterialStatePoint state)
    {
        var raw = _inner.Evaluate(profile, state);
        var tempFactor = Math.Clamp(
            1.0
                - ThermalSofteningPerKelvin
                    * Math.Max(0, state.TemperatureKelvin - ReferenceTemperatureK),
            0.05,
            1.25
        );
        var rate = Math.Max(Math.Abs(state.StrainRatePerSecond), 1e-9);
        var rateFactor =
            1.0
            + profile.Definition.RateSensitivity
                * Math.Log(1.0 + rate / Math.Max(RateReferencePerSecond, 1e-9));
        var scale = Math.Clamp(tempFactor * rateFactor, 0.02, 4.0);
        return raw with
        {
            StressPa = raw.StressPa * scale,
            TangentModulusPa = raw.TangentModulusPa * scale,
            DissipatedEnergyDensityJPerM3 = raw.DissipatedEnergyDensityJPerM3 * scale,
        };
    }
}
