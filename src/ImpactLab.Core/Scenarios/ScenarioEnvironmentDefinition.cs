using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Scenarios;

public sealed record ScenarioEnvironmentDefinition
{
    public string Id { get; init; } = "custom";
    public string DisplayName { get; init; } = "Custom environment";
    public Vec3 GravityMps2 { get; init; } = Vec3.Zero;
    public double AtmosphericDensityKgPerM3 { get; init; }
    public double PressurePa { get; init; }
    public double TemperatureKelvin { get; init; } = 293.15;
    public double DynamicViscosityPaS { get; init; }
    public string Reference { get; init; } = "User supplied";
    public string ReferenceUri { get; init; } = "";
    public bool AerodynamicForcesEnabled { get; init; }
    public bool IsDefaultAssumption { get; init; }
    public IReadOnlyList<EnvironmentProfilePoint> DensityByAltitude { get; init; } = [];
    public IReadOnlyList<EnvironmentProfilePoint> PressureByAltitude { get; init; } = [];
    public IReadOnlyList<EnvironmentProfilePoint> TemperatureByAltitude { get; init; } = [];

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(DisplayName))
            throw new InvalidOperationException("Environment identity is required.");
        if (AtmosphericDensityKgPerM3 < 0 || PressurePa < 0 || DynamicViscosityPaS < 0)
            throw new InvalidOperationException("Atmospheric properties cannot be negative.");
        if (TemperatureKelvin <= 0)
            throw new InvalidOperationException(
                "Environment temperature must be above absolute zero."
            );
        if (AerodynamicForcesEnabled)
            throw new NotSupportedException(
                "Aerodynamic forces are not implemented by the current impact backends. Environment atmosphere values are metadata only."
            );
        ValidateProfile(DensityByAltitude, "density");
        ValidateProfile(PressureByAltitude, "pressure");
        ValidateProfile(TemperatureByAltitude, "temperature");
    }

    public EnvironmentSample Sample(double altitudeMeters) =>
        new(
            altitudeMeters,
            GravityMps2,
            Evaluate(DensityByAltitude, altitudeMeters, AtmosphericDensityKgPerM3),
            Evaluate(PressureByAltitude, altitudeMeters, PressurePa),
            Evaluate(TemperatureByAltitude, altitudeMeters, TemperatureKelvin),
            DynamicViscosityPaS
        );

    private static void ValidateProfile(IReadOnlyList<EnvironmentProfilePoint> points, string name)
    {
        for (var i = 0; i < points.Count; i++)
        {
            if (!double.IsFinite(points[i].AltitudeMeters) || !double.IsFinite(points[i].Value))
                throw new InvalidOperationException($"Environment {name} profile must be finite.");
            if (points[i].Value < 0)
                throw new InvalidOperationException(
                    $"Environment {name} profile cannot be negative."
                );
            if (i > 0 && points[i].AltitudeMeters <= points[i - 1].AltitudeMeters)
                throw new InvalidOperationException(
                    $"Environment {name} profile altitudes must be strictly increasing."
                );
        }
    }

    private static double Evaluate(
        IReadOnlyList<EnvironmentProfilePoint> points,
        double altitude,
        double fallback
    )
    {
        if (points.Count == 0)
            return fallback;
        if (altitude <= points[0].AltitudeMeters)
            return points[0].Value;
        for (var i = 1; i < points.Count; i++)
        {
            if (altitude > points[i].AltitudeMeters)
                continue;
            var a = points[i - 1];
            var b = points[i];
            var t = (altitude - a.AltitudeMeters) / (b.AltitudeMeters - a.AltitudeMeters);
            return a.Value + (b.Value - a.Value) * t;
        }
        return points[^1].Value;
    }
}

public readonly record struct EnvironmentProfilePoint(double AltitudeMeters, double Value);

public readonly record struct EnvironmentSample(
    double AltitudeMeters,
    Vec3 GravityMps2,
    double AtmosphericDensityKgPerM3,
    double PressurePa,
    double TemperatureKelvin,
    double DynamicViscosityPaS
);

public static class ScenarioEnvironmentCatalog
{
    public static ScenarioEnvironmentDefinition EarthSeaLevelTemplate { get; } =
        new()
        {
            Id = "earth-sea-level-isa",
            DisplayName = "Earth / ISA sea level",
            GravityMps2 = new Vec3(0, 0, -9.80665),
            AtmosphericDensityKgPerM3 = 1.225,
            PressurePa = 101_325,
            TemperatureKelvin = 288.15,
            DynamicViscosityPaS = 1.7894e-5,
            Reference = "ISO 2533 standard atmosphere; conventional standard gravity",
            ReferenceUri = "https://www.iso.org/standard/7472.html",
            IsDefaultAssumption = true,
        };
}
