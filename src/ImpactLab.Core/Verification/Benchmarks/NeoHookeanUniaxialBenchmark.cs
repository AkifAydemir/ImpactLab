using ImpactLab.Core.Continuum.FiniteStrain;
using ImpactLab.Core.Materials;

namespace ImpactLab.Core.Verification.Benchmarks;

public sealed class NeoHookeanUniaxialBenchmark : IVerificationBenchmark
{
    public string Id => "finite-strain.neo-hookean-uniaxial";
    public string Name => "Isochoric Neo-Hookean uniaxial state";
    public VerificationBenchmarkCategory Category => VerificationBenchmarkCategory.Hyperelasticity;

    public VerificationBenchmarkResult Evaluate()
    {
        var material = MaterialLibrary.All[0];
        const double stretch = 1.2;
        var lateral = 1 / Math.Sqrt(stretch);
        var F = new Matrix3(stretch, 0, 0, 0, lateral, 0, 0, 0, lateral);
        var k = FiniteStrainKinematics.FromGradient(F, new Matrix3());
        var update = new NeoHookeanLaw().Update(
            new(material, FiniteStrainElementState.Initial(), k, 1e-3, 293.15)
        );
        var mu = material.YoungModulusPa / (2 * (1 + material.PoissonRatio));
        var expected = mu * (stretch * stretch - 1);
        return new(
            Id,
            Name,
            Category,
            [
                new(
                    "kirchhoff-xx",
                    "Pa",
                    update.KirchhoffStress.M11,
                    expected,
                    Math.Max(1e-6, Math.Abs(expected) * 1e-10),
                    1e-10
                ),
                new("jacobian", "-", k.Jacobian, 1, 1e-12, 1e-12),
            ],
            "Closed-form compressible Neo-Hookean law evaluated on an isochoric diagonal deformation."
        );
    }
}
