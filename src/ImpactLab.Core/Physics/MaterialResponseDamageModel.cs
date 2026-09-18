using ImpactLab.Core.Materials;
using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Physics;

public sealed class MaterialResponseDamageModel : IDamageModel
{
    public double UpdateDamage(
        MeshSpring spring,
        double currentDamage,
        double strain,
        double timeStepSeconds
    )
    {
        var material = spring.Material;
        var effective = Math.Abs(strain);
        if (strain < 0.0)
            effective /= Math.Max(material.CompressiveStrengthScale, 1e-9);
        var yield = material.YieldStrain * (spring.Interface?.YieldStrainScale ?? 1.0);
        var failure = material.FailureStrain * (spring.Interface?.FailureStrainScale ?? 1.0);
        if (effective <= yield)
            return currentDamage;
        var normalized = Math.Clamp(
            (effective - yield) / Math.Max(failure - yield, 1e-12),
            0.0,
            1.0
        );
        var rate = material.ResponseKind switch
        {
            MaterialResponseKind.Brittle => 120_000.0,
            MaterialResponseKind.Polymer => 12_000.0,
            MaterialResponseKind.Ductile => 32_000.0,
            _ => 25_000.0,
        };
        var increment = normalized * normalized * rate * timeStepSeconds;
        return Math.Clamp(currentDamage + increment, 0.0, 1.0);
    }
}
