using ImpactLab.Core.Thermal;

namespace ImpactLab.Core.Verification.Benchmarks;

public sealed class ThermalBoundaryBalanceBenchmark : IVerificationBenchmark
{
    public string Id => "thermal.convection-balance";
    public string Name => "Convection boundary assembly";
    public VerificationBenchmarkCategory Category => VerificationBenchmarkCategory.Thermal;

    public VerificationBenchmarkResult Evaluate()
    {
        var rhs = new double[1];
        var diag = new double[1];
        ThermalBoundaryAssembler.AddConvection(new([0], 10, 300, 2), [293d], rhs, diag);
        return new(
            Id,
            Name,
            Category,
            [
                new("diagonal", "W/K", diag[0], 20, 1e-12, 1e-12),
                new("rhs", "W", rhs[0], 6000, 1e-9, 1e-12),
            ],
            "Analytical h*A and h*A*T_infinity convection contribution."
        );
    }
}
