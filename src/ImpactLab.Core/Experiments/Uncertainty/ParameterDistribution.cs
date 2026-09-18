namespace ImpactLab.Core.Experiments.Uncertainty;

public sealed record ParameterDistribution(
    string ParameterId,
    DistributionKind Kind,
    double A,
    double B,
    double C = 0,
    IReadOnlyList<double>? DiscreteValues = null
)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ParameterId))
            throw new InvalidOperationException("Parameter id required.");
        if (
            Kind == DistributionKind.Discrete
            && (DiscreteValues is null || DiscreteValues.Count == 0)
        )
            throw new InvalidOperationException("Discrete distribution needs values.");
    }
}
