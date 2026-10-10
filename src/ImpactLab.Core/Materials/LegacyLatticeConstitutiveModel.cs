using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Materials;

public sealed class LegacyLatticeConstitutiveModel : ILatticeConstitutiveModel
{
    public LatticeConstitutiveResponse Evaluate(
        MeshSpring spring,
        double strain,
        double strainRatePerSecond,
        double timeStepSeconds
    )
    {
        var material = spring.Material;
        var yielded = Math.Abs(strain) > material.EffectiveYieldStrain;
        var tangent =
            material.TangentModulusPa > 0.0
                ? material.TangentModulusPa
                : material.YoungModulusPa * 0.02;
        var activeModulus = yielded ? tangent : material.YoungModulusPa;
        return new(
            activeModulus * strain,
            activeModulus,
            0.5 * material.YoungModulusPa * strain * strain,
            0.0
        );
    }
}
