using ImpactLab.Core.Continuum.FiniteStrain;

namespace ImpactLab.Core.Verification.Benchmarks;

public sealed class PureRotationObjectivityBenchmark : IVerificationBenchmark
{
    public string Id => "finite-strain.pure-rotation";
    public string Name => "Pure rotation objectivity";
    public VerificationBenchmarkCategory Category => VerificationBenchmarkCategory.Kinematics;

    public VerificationBenchmarkResult Evaluate()
    {
        const double a = 1.0471975511965976;
        var c = Math.Cos(a);
        var s = Math.Sin(a);
        var f = new Matrix3(c, -s, 0, s, c, 0, 0, 0, 1);
        var k = FiniteStrainKinematics.FromGradient(f, new Matrix3());
        var e = Matrix3Math.Frobenius(k.GreenLagrange);
        var det = k.F.J;
        return new(
            Id,
            Name,
            Category,
            [
                new("green-lagrange-norm", "-", e, 0, 1e-10, 1e-10),
                new("jacobian", "-", det, 1, 1e-12, 1e-12),
            ],
            "Analytical rigid-rotation invariant: R^T R = I and det(R)=1."
        );
    }
}
