using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Physics;

public sealed class StrainDamageModel : IDamageModel
{
    public StrainDamageModel(double growthRatePerSecond = 25_000.0)
    {
        if (growthRatePerSecond <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(growthRatePerSecond));
        GrowthRatePerSecond = growthRatePerSecond;
    }

    public double GrowthRatePerSecond { get; }

    public double UpdateDamage(
        MeshSpring spring,
        double currentDamage,
        double strain,
        double timeStepSeconds
    )
    {
        var material = spring.Material;
        var yieldScale = spring.Interface?.YieldStrainScale ?? 1.0;
        var failureScale = spring.Interface?.FailureStrainScale ?? 1.0;
        var yield = material.YieldStrain * yieldScale;
        var failure = material.FailureStrain * failureScale;
        var magnitude = Math.Abs(strain);
        if (magnitude <= yield)
            return currentDamage;
        if (magnitude >= failure)
            return 1.0;
        var normalized = (magnitude - yield) / Math.Max(1e-12, failure - yield);
        var increment = normalized * GrowthRatePerSecond * timeStepSeconds;
        return Math.Clamp(currentDamage + increment, 0.0, 1.0);
    }
}
