namespace ImpactLab.Core.Materials;

public static class MaterialMixer
{
    private static readonly Dictionary<(string A, string B), MaterialDefinition> Cache = new();
    private static readonly object Gate = new();

    public static MaterialDefinition Mix(MaterialDefinition a, MaterialDefinition b)
    {
        if (a.Id == b.Id)
            return a;
        var key = string.CompareOrdinal(a.Id, b.Id) <= 0 ? (a.Id, b.Id) : (b.Id, a.Id);
        lock (Gate)
        {
            if (Cache.TryGetValue(key, out var cached))
                return cached;
            var mixed = new MaterialDefinition(
                $"mix:{key.Item1}|{key.Item2}",
                $"Interface {a.DisplayName} / {b.DisplayName}",
                Harmonic(a.DensityKgPerM3, b.DensityKgPerM3),
                Harmonic(a.YoungModulusPa, b.YoungModulusPa),
                (a.PoissonRatio + b.PoissonRatio) * 0.5,
                Math.Min(a.YieldStrain, b.YieldStrain),
                Math.Min(a.FailureStrain, b.FailureStrain),
                Math.Max(a.DampingRatio, b.DampingRatio),
                MaterialResponseKind.Generic,
                Math.Min(a.TangentModulusPa, b.TangentModulusPa),
                Math.Min(a.CompressiveStrengthScale, b.CompressiveStrengthScale),
                Math.Max(a.RateSensitivity, b.RateSensitivity)
            );
            Cache[key] = mixed;
            return mixed;
        }
    }

    private static double Harmonic(double a, double b) => 2.0 * a * b / (a + b);
}
