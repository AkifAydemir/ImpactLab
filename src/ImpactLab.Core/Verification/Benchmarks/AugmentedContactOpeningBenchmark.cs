using ImpactLab.Core.Continuum.Contact;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Verification.Benchmarks;

public sealed class AugmentedContactOpeningBenchmark : IVerificationBenchmark
{
    public string Id => "contact.augmented-open-close";
    public string Name => "Augmented contact open/close";
    public VerificationBenchmarkCategory Category => VerificationBenchmarkCategory.Contact;

    public VerificationBenchmarkResult Evaluate()
    {
        var law = new AugmentedLagrangianContactLaw();
        var closed = new ContactManifoldPoint(
            new(1, 2, 0),
            Vec3.Zero,
            Vec3.Zero,
            Vec3.UnitZ,
            -1e-4,
            1,
            0,
            0
        );
        var open = new ContactManifoldPoint(
            new(1, 2, 0),
            Vec3.Zero,
            Vec3.UnitZ * 1e-4,
            Vec3.UnitZ,
            1e-4,
            1,
            0,
            0
        );
        var c = law.Evaluate(closed, new(0, 0, 0, true, 0, 0), 1e8, Vec3.Zero, 1e-3);
        var o = law.Evaluate(open, new(0, 0, 0, true, 0, 0), 1e8, Vec3.Zero, 1e-3);
        return new(
            Id,
            Name,
            Category,
            [
                new("closed-compression-positive", "bool", c.normal > 0 ? 1 : 0, 1, 0, 0),
                new("open-compression-zero", "N", o.normal, 0, 1e-12, 1e-12),
            ],
            "Complementarity sanity check for zero-history open and penetrated contact states."
        );
    }
}
