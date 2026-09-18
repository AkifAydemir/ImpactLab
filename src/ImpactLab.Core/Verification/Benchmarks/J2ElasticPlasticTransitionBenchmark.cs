using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Plasticity;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Verification.Benchmarks;

public sealed class J2ElasticPlasticTransitionBenchmark : IVerificationBenchmark
{
    public string Id => "plasticity.j2-transition";
    public string Name => "J2 elastic/plastic transition";
    public VerificationBenchmarkCategory Category => VerificationBenchmarkCategory.Plasticity;

    public VerificationBenchmarkResult Evaluate()
    {
        var m = MaterialLibrary.All[0];
        var law = new LinearIsotropicHardening(250e6, 1e9);
        var mapper = new J2ReturnMapper();
        var elastic = mapper.Update(
            m,
            new StrainTensor6(1e-5, 0, 0, 0, 0, 0),
            PlasticState.Zero,
            law
        );
        var plastic = mapper.Update(
            m,
            new StrainTensor6(.02, 0, 0, 0, 0, 0),
            PlasticState.Zero,
            law
        );
        return new(
            Id,
            Name,
            Category,
            [
                new("elastic-yield-flag", "bool", elastic.Yielded ? 1 : 0, 0, 0, 0),
                new("plastic-yield-flag", "bool", plastic.Yielded ? 1 : 0, 1, 0, 0),
                new(
                    "plastic-eqp-positive",
                    "bool",
                    plastic.State.EquivalentPlasticStrain > 0 ? 1 : 0,
                    1,
                    0,
                    0
                ),
            ],
            "Internal constitutive transition check against prescribed below/above-yield strain states."
        );
    }
}
