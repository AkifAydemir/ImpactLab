namespace ImpactLab.Core.Verification.Benchmarks;

public static class VerificationBenchmarkCatalog
{
    public static IReadOnlyList<IVerificationBenchmark> BuiltIns { get; } =
    [
        new PureRotationObjectivityBenchmark(),
        new NeoHookeanUniaxialBenchmark(),
        new J2ElasticPlasticTransitionBenchmark(),
        new AugmentedContactOpeningBenchmark(),
        new ThermalBoundaryBalanceBenchmark(),
    ];
}
